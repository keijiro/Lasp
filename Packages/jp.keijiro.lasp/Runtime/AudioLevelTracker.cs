using UnityEngine;

namespace Lasp
{
    //
    // Unity component used to track audio input level and drive other
    // components via UnityEvent
    //
    [AddComponentMenu("LASP/Audio Level Tracker")]
    public sealed class AudioLevelTracker : MonoBehaviour
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

        // Filter type selection
        [SerializeField] FilterType _filterType = FilterType.Bypass;
        public FilterType filterType
          { get => _filterType;
            set => _filterType = value; }

        // Auto gain control switch
        [SerializeField] bool _autoGain = true;
        public bool autoGain
          { get => _autoGain;
            set => _autoGain = value; }

        // Manual input gain (only used when auto gain is off)
        [SerializeField, Range(-10, 40)] float _gain = 6;
        public float gain
          { get => _gain;
            set => _gain = value; }

        // Dynamic range in dB
        [SerializeField, Range(1, 40)] float _dynamicRange = 12;
        public float dynamicRange
          { get => _dynamicRange;
            set => _dynamicRange = value; }

        // Smooth fall animation switch
        [SerializeField] bool _smoothFall = true;
        public bool smoothFall
          { get => _smoothFall;
            set => _smoothFall = value; }

        // Fall animation speed
        [SerializeField, Range(0, 1)] float _fallSpeed = 0.3f;
        public float fallSpeed
          { get => _fallSpeed;
            set => _fallSpeed = value; }

        // Property binders
        [SerializeReference] PropertyBinder[] _propertyBinders = null;
        public PropertyBinder[] propertyBinders
          { get => (PropertyBinder[])_propertyBinders.Clone();
            set => _propertyBinders = value; }

        #endregion

        #region Runtime public properties and methods

        // Current input gain (dB)
        public float currentGain => _autoGain ? -_head : _gain;

        // Unprocessed input level (dBFS)
        public float inputLevel
          => Stream?.GetChannelLevel(_channel, _filterType) ?? kSilence;

        // Curent level in the normalized scale
        public float normalizedLevel => _normalizedLevel;

        // Raw wave audio data as NativeSlice
        public Unity.Collections.NativeSlice<float> audioDataSlice
          => Stream?.GetChannelDataSlice(channel)
             ?? default(Unity.Collections.NativeSlice<float>);

        // Reset the auto gain state.
        public void ResetAutoGain() => _head = kSilence;

        #endregion

        #region Private members

        // Silence: Locally defined noise floor level (dBFS)
        const float kSilence = -60;

        // Current normalized level value
        float _normalizedLevel = 0;

        // Nominal level of auto gain (recent maximum level)
        float _head = kSilence;

        // Hold and fall down animation parameter
        float _fall = 0;

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

        #endregion

        #region MonoBehaviour implementation

        void Update()
        {
            var input = inputLevel;
            var dt = Time.deltaTime;

            // Auto gain control
            if (_autoGain)
            {
                // Slowly return to the noise floor.
                const float kDecaySpeed = 0.6f;
                _head = Mathf.Max(_head - kDecaySpeed * dt, kSilence);

                // Pull up by input with a small headroom.
                var room = _dynamicRange * 0.05f;
                _head = Mathf.Clamp(input - room, _head, 0);
            }

            // Normalize the input value.
            var normalizedInput
              = Mathf.Clamp01((input + currentGain) / _dynamicRange + 1);

            if (_smoothFall)
            {
                // Hold and fall down animation
                _fall += Mathf.Pow(10, 1 + _fallSpeed * 2) * dt;
                _normalizedLevel -= _fall * dt;

                // Pull up by input.
                if (_normalizedLevel < normalizedInput)
                {
                    _normalizedLevel = normalizedInput;
                    _fall = 0;
                }
            }
            else
            {
                _normalizedLevel = normalizedInput;
            }

            // Output
            if (_propertyBinders != null)
                foreach (var b in _propertyBinders) b.Level = _normalizedLevel;
        }

        #endregion
    }
}
