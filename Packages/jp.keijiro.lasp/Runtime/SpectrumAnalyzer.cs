using UnityEngine;
using Unity.Mathematics;

namespace Lasp
{
    //
    // Unity component used to provide spectrum data from a specific audio
    // channel.
    //
    [AddComponentMenu("LASP/Spectrum Analyzer")]
    public sealed class SpectrumAnalyzer : MonoBehaviour
    {
        #region Editor attributes and public properties

        // System default device switch
        [SerializeField] bool _useDefaultDevice = true;
        public bool useDefaultDevice
          { get => _useDefaultDevice;
            set => TrySelectDevice(null); }

        // Device ID to use
        [SerializeField] string _deviceID = "";
        public string deviceID
          { get => _deviceID;
            set => TrySelectDevice(value); }

        // Channel Selection
        [SerializeField, Range(0, 15)] int _channel = 0;
        public int channel
          { get => _channel;
            set => _channel = value; }

        [SerializeField] int _resolution = 512;
        public int resolution
          { get => _resolution;
            set => _resolution = ValidateResolution(value); }

        #endregion

        #region Attribute validators

        static int ValidateResolution(int x)
        {
            if (x > 0 && (x & (x - 1)) == 0) return x;
            Debug.LogError("Spectrum resolution must be a power of 2.");
            return 1 << (int)math.max(1, math.round(math.log2(x)));
        }

        void OnValidate()
        {
            _resolution = ValidateResolution(_resolution);
        }

        #endregion

        #region Runtime public properties and methods

        // Spectrum data as NativeArray
        public Unity.Collections.NativeArray<float> SpectrumArray
          => Fft.Spectrum;

        // Spectrum data as ReadOnlySpan
        public System.ReadOnlySpan<float> SpectrumSpan
          => Fft.Spectrum.GetReadOnlySpan();

        // Raw wave audio data as NativeSlice
        public Unity.Collections.NativeSlice<float> AudioDataSlice
          => Stream?.GetChannelDataSlice(channel)
             ?? default(Unity.Collections.NativeSlice<float>);

        #endregion

        #region Private members

        // Check the status and try selecting the device.
        void TrySelectDevice(string id)
        {
            // At the moment, we only supports selecting a device before the
            // stream is initialized.
            if (_stream != null)
                throw new System.InvalidOperationException
                  ("Stream is already open");

            _useDefaultDevice = string.IsNullOrEmpty(id);
            _deviceID = id;
        }

        // Input stream object with local cache
        InputStream Stream
          => (_stream != null && _stream.IsValid) ? _stream : CacheStream();

        InputStream CacheStream()
          => (_stream = _useDefaultDevice ?
               AudioSystem.GetDefaultInputStream() :
               AudioSystem.GetInputStream(_deviceID));

        InputStream _stream;

        // FFT buffer object with lazy initialization
        FftBuffer Fft => _fft ?? (_fft = new FftBuffer(_resolution * 2));

        FftBuffer _fft;

        #endregion

        #region MonoBehaviour implementation

        void OnDisable()
        {
            _fft?.Dispose();
            _fft = null;
        }

        void Update()
        {
            _fft?.Push(Stream.GetChannelDataSlice(_channel));
            _fft?.Analyze();
        }

        #endregion
    }
}
