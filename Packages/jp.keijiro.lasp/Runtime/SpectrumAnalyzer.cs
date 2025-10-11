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

        // Spectrum resolution
        [SerializeField] int _resolution = 512;
        public int resolution
          { get => _resolution;
            set => _resolution = ValidateResolution(value); }

        // Auto gain control switch
        [SerializeField] bool _autoGain = true;
        public bool autoGain
          { get => _autoGain;
            set => _autoGain = value; }

        // Manual input gain (only used when auto gain is off)
        [SerializeField, Range(-10, 120)] float _gain = 0;
        public float gain
          { get => _gain;
            set => _gain = value; }

        // Dynamic range in dB
        [SerializeField, Range(1, 120)] float _dynamicRange = 80;
        public float dynamicRange
          { get => _dynamicRange;
            set => _dynamicRange = value; }

        #endregion

        #region Attribute validators

        static int ValidateResolution(int x)
        {
            if (x > 0 && (x & (x - 1)) == 0) return x;
            Debug.LogError("Spectrum resolution must be a power of 2.");
            return 1 << (int)math.max(1, math.round(math.log2(x)));
        }

        #endregion

        #region Runtime public properties and methods

        // Current input gain (dB)
        public float currentGain => _autoGain ? -_head : _gain;

        // Spectrum data as NativeArray
        public Unity.Collections.NativeArray<float> spectrumArray
          => Fft.Spectrum;

        // X-axis log scaled spectrum data as NativeArray
        public Unity.Collections.NativeArray<float> logSpectrumArray
          => LogScaler.Resample(Fft.Spectrum);

        // Spectrum data as ReadOnlySpan
        public System.ReadOnlySpan<float> spectrumSpan
          => Fft.Spectrum.GetReadOnlySpan();

        // X-axis log scaled spectrum data as ReadOnlySpan
        public System.ReadOnlySpan<float> logSpectrumSpan
          => logSpectrumArray.GetReadOnlySpan();

        // Reset the auto gain state.
        public void ResetAutoGain() => _head = kSilence;

        #endregion

        #region Private members

        // Silence: Locally defined noise floor level (dBFS)
        const float kSilence = -240;

        // Nominal level of auto gain (recent maximum level)
        float _head = kSilence;

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
        public FftBuffer Fft => _fft ?? (_fft = new FftBuffer(_resolution * 2));
        FftBuffer _fft;

        // Log scale resampler with lazy initialization
        LogScaler LogScaler => _logScaler ?? (_logScaler = new LogScaler());
        LogScaler _logScaler;

        #endregion

        #region MonoBehaviour implementation

        void OnDisable()
        {
            _fft?.Dispose();
            _fft = null;

            _logScaler?.Dispose();
            _logScaler = null;
        }

        void Update()
        {
            var input = Stream?.GetChannelLevel(_channel) ?? kSilence;
            var dt = Time.deltaTime;

            // Auto gain control
            if (_autoGain)
            {
                // Slowly return to the noise floor.
                const float kDecaySpeed = 0.6f;
                _head -= kDecaySpeed * dt;
                _head = Mathf.Max(_head, kSilence + _dynamicRange);

                // Pull up by input with a small headroom.
                var room = _dynamicRange * 0.05f;
                _head = Mathf.Clamp(input - room, _head, 0);
            }

            // FFT
            _fft?.Push(Stream.GetChannelDataSlice(_channel));
            _fft?.Analyze(-currentGain - _dynamicRange, -currentGain);
        }

        #endregion
    }
}
