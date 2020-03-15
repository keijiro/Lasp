using System;
using System.Runtime.InteropServices;
using InvalidOp = System.InvalidOperationException;
using PInvokeCallbackAttribute = AOT.MonoPInvokeCallbackAttribute;

namespace Lasp
{
    //
    // Internal input device handle class
    //
    // This is the one big monolithic class for managing a pair of an audio
    // input device and its input stream.
    //
    // Initially, it only manages a device object, then it starts streaming
    // when someone tries to read the audio data. The stream will be
    // automatically closed when the data hasn't been accessed for several
    // frames.
    //
    // It not only manages these objects, but also calculates audio levels of
    // each channel. They're managed in an on-demand fashion too: It starts
    // calculating them when accessed, and stops calculation when it's not
    // accessed.
    //
    sealed class InputDeviceHandle : IDisposable
    {
        #region SoundIO device object

        public SoundIO.Device SioDevice => _device;
        public string ID => _device.ID;
        public bool IsValid => _device != null;

        SoundIO.Device _device;

        #endregion

        #region SoundIO stream object

        public bool IsStreamActive
          => _stream != null && !_stream.IsInvalid && !_stream.IsClosed;

        SoundIO.InStream _stream;

        #endregion

        #region Basic stream properties

        public int StreamChannelCount => PreparedStream.Layout.ChannelCount;
        public int StreamSampleRate => PreparedStream.SampleRate;

        #endregion

        #region Per-channel audio levels

        public Unity.Mathematics.float4 GetChannelLevel(int channel)
          => Prepare() ? _audioLevels.GetLevel(channel) : 0;

        LevelMeter _audioLevels;

        #endregion

        #region Interleaved audio data

        public ReadOnlySpan<float> LastFrameWindow
          => MemoryMarshal.Cast<byte, float>(LastFrameWindowRaw);

        ReadOnlySpan<byte> LastFrameWindowRaw
          => Prepare() ?
             new ReadOnlySpan<byte>(_window, 0, _windowSize) :
             ReadOnlySpan<byte>.Empty;

        byte[] _window;
        int _windowSize;

        #endregion

        #region "Prepare" method

        bool Prepare()
        {
            if (!IsValid) throw new InvalidOp("Invalid device");

            _sleepTimer = 0;

            if (IsStreamActive) return true;

            OpenStream();
            return false;
        }

        SoundIO.InStream PreparedStream { get { Prepare(); return _stream; } }

        int _sleepTimer;

        #endregion

        #region Allocation/deallocation

        // Factory method
        public static InputDeviceHandle CreateAndOwn(SoundIO.Device device)
          => new InputDeviceHandle(device);

        // Private constructor
        InputDeviceHandle(SoundIO.Device device)
        {
            _self = GCHandle.Alloc(this);
            _device = device;
        }

        // IDisposable implementation
        public void Dispose()
        {
            _stream?.Dispose();
            _stream = null;

            _device?.Dispose();
            _device = null;

            if (_self.IsAllocated) _self.Free();
        }

        // A GC handle used to share 'this' pointer with unmanaged code
        GCHandle _self;

        #endregion

        #region State update methods

        public void Update(float deltaTime)
        {
            // Nothing to do when the stream is inactive.
            if (!IsStreamActive) return;

            // Close the stream when the sleep timer hits the threshold.
            if (++_sleepTimer > DelayToSleep)
            {
                CloseStream();
                return;
            }

            // Calculate the size of the last-frame window.
            _windowSize =
                Math.Min(_window.Length, CalculateBufferSize(deltaTime));

            lock (_ring)
            {
                // Copy the last frame data into the window buffer.
                if (_ring.FillCount >= _windowSize)
                    _ring.Read(new Span<byte>(_window, 0, _windowSize));
                else
                    _windowSize = 0; // Underflow

                // Reset the buffer when it detects an overflow.
                // TODO: Is this the best strategy to deal with overflow?
                if (_ring.OverflowCount > 0) _ring.Clear();
            }

            // Process the audio data.
            _audioLevels.ProcessAudioData
              (MemoryMarshal.Cast<byte, float>
                (new ReadOnlySpan<byte>(_window, 0, _windowSize)));
        }

        const int DelayToSleep = 10;

        #endregion

        #region Stream initialization/finalization

        void OpenStream()
        {
            if (IsStreamActive)
                throw new InvalidOp("Stream alreadly opened");

            try
            {
                _stream = SoundIO.InStream.Create(_device);

                if (_stream.IsInvalid)
                    throw new InvalidOp("Stream allocation error");

                if (_device.Layouts.Length == 0)
                    throw new InvalidOp("No channel layout");

                // Calculate the best latency.
                // TODO: Should we use the target frame rate instead of 1/60?
                var bestLatency = Math.Max(1.0 / 60, _device.SoftwareLatencyMin);

                // Stream properties
                _stream.Format = SoundIO.Format.Float32LE;
                _stream.Layout = _device.Layouts[0];
                _stream.SoftwareLatency = bestLatency;
                _stream.ReadCallback = _readCallback;
                _stream.OverflowCallback = _overflowCallback;
                _stream.ErrorCallback = _errorCallback;
                _stream.UserData = GCHandle.ToIntPtr(_self);

                var err = _stream.Open();

                if (err != SoundIO.Error.None)
                    throw new InvalidOp($"Stream initialization error ({err})");

                // We want the buffers to meet the following requirements:
                // - Doesn't overflow if the main thread pauses for 4 frames.
                // - Doesn't overflow if the callback is invoked 4 times a frame.
                var latency = Math.Max(_stream.SoftwareLatency, bestLatency);
                var bufferSize = CalculateBufferSize((float)(latency * 4));

                // Ring/window buffer allocation
                _ring = new RingBuffer(bufferSize);
                _window = new byte[bufferSize];

                // Start streaming.
                _stream.Start();
            }
            catch
            {
                // Dispose the stream on an exception.
                _stream?.Dispose();
                _stream = null;
                throw;
            }

            _audioLevels = new LevelMeter(_stream.Layout.ChannelCount)
              { SampleRate = _stream.SampleRate };
        }

        void CloseStream()
        {
            if (!IsStreamActive)
                throw new InvalidOp("Stream not opened");

            _stream?.Dispose();
            _stream = null;
        }

        #endregion

        #region Private members

        // Input stream ring buffer
        // This object will be accessed from both the main/callback thread.
        // Must be locked when accessing it.
        RingBuffer _ring;

        // Calculate a buffer size based on a duration.
        int CalculateBufferSize(float second)
          => (int)(_stream.SampleRate * second) *
             _stream.Layout.ChannelCount * sizeof(float);

        #endregion

        #region SoundIO callback delegates

        static SoundIO.InStream.ReadCallbackDelegate
          _readCallback = OnReadInStream;

        static SoundIO.InStream.OverflowCallbackDelegate
          _overflowCallback = OnOverflowInStream;

        static SoundIO.InStream.ErrorCallbackDelegate
          _errorCallback = OnErrorInStream;

        [PInvokeCallback(typeof(SoundIO.InStream.ReadCallbackDelegate))]
        unsafe static void OnReadInStream
          (ref SoundIO.InStreamData stream, int min, int left)
        {
            // Recover the 'this' reference from the UserData pointer.
            var self = (InputDeviceHandle)
              GCHandle.FromIntPtr(stream.UserData).Target;

            while (left > 0)
            {
                // Start reading the buffer.
                var count = left;
                SoundIO.ChannelArea* areas;
                stream.BeginRead(out areas, ref count);

                // When getting count == 0, we must stop reading
                // immediately without calling InStream.EndRead.
                if (count == 0) break;

                if (areas == null)
                {
                    // We must do zero-fill when receiving a null pointer.
                    lock (self._ring)
                      self._ring.WriteEmpty(stream.BytesPerFrame * count);
                }
                else
                {
                    // Determine the memory span of the input data with
                    // assuming the data is tightly packed.
                    // TODO: Is this assumption always true?
                    var span = new ReadOnlySpan<Byte>
                      ((void*)areas[0].Pointer, areas[0].Step * count);

                    // Transfer the data to the ring buffer.
                    lock (self._ring) self._ring.Write(span);
                }

                stream.EndRead();

                left -= count;
            }
        }

        [PInvokeCallback(typeof(SoundIO.InStream.OverflowCallbackDelegate))]
        static void OnOverflowInStream(ref SoundIO.InStreamData stream)
          => UnityEngine.Debug.LogWarning("InStream overflow");

        [PInvokeCallback(typeof(SoundIO.InStream.ErrorCallbackDelegate))]
        static void OnErrorInStream
          (ref SoundIO.InStreamData stream, SoundIO.Error error)
          => UnityEngine.Debug.LogWarning($"InStream error ({error})");

        #endregion
    }
}
