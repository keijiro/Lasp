// LASP - Low-latency Audio Signal Processing plugin for Unity
// https://github.com/keijiro/Lasp

using UnityEngine;
using UnityEngine.Serialization;

namespace Lasp
{
    // Unity component used to track audio input level and drive other
    // components via UnityEvent
    [AddComponentMenu("LASP/Audio Level Tracker")]
    public sealed class AudioLevelTracker : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] Lasp.FilterType _filterType = Lasp.FilterType.LowPass;

        public Lasp.FilterType filterType {
            get { return _filterType; }
            set { _filterType = value; }
        }

        [UnityEngine.Serialization.FormerlySerializedAs("_autoGain")]
        [SerializeField] bool _peakTracking = true;

        public bool peakTracking {
            get { return _peakTracking; }
            set { _peakTracking = value; }
        }

        [SerializeField, Range(-10, 40)] float _gain = 6;

        public float gain {
            get { return _gain; }
            set { _gain = value; }
        }

        [SerializeField, Range(1, 40)] float _dynamicRange = 12;

        public float dynamicRange {
            get { return _dynamicRange; }
            set { _dynamicRange = value; }
        }

        [SerializeField] bool _holdAndFallDown = true;

        public bool holdAndFallDown {
            get { return _holdAndFallDown; }
            set { _holdAndFallDown = value; }
        }

        [SerializeField, Range(0, 1)] float _fallDownSpeed = 0.3f;

        public float fallDownSpeed {
            get { return _fallDownSpeed; }
            set { _fallDownSpeed = value; }
        }

        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_outputEvent")]
        AudioLevelEvent _normalizedLevelEvent = new AudioLevelEvent();

        public AudioLevelEvent normalizedLevelEvent {
            get { return _normalizedLevelEvent; }
            set { _normalizedLevelEvent = value; }
        }

        #endregion

        #region Runtime public properties and methods

        public float calculatedGain {
            get { return _peakTracking ? -_peak : _gain; }
        }

        public float inputAmplitude {
            get { return Lasp.MasterInput.CalculateRMSDecibel(_filterType); }
        }

        public float normalizedLevel {
            get { return _amplitude; }
        }

        public void ResetPeak()
        {
            _peak = kSilence;
        }

        #endregion

        #region Private members

        // Silence: Minimum amplitude value
        const float kSilence = -60;

        // Current amplitude value.
        float _amplitude = kSilence;

        // Variables for automatic gain control.
        float _peak = kSilence;
        float _fall = 0;

        #endregion

        #region MonoBehaviour implementation

        void Update()
        {
            var input = inputAmplitude;
            var dt = Time.deltaTime;

            // Automatic gain control
            if (_peakTracking)
            {
                // Gradually falls down to the minimum amplitude.
                const float peakFallSpeed = 0.6f;
                _peak = Mathf.Max(_peak - peakFallSpeed * dt, kSilence);

                // Pull up by input with allowing a small amount of clipping.
                var clip = _dynamicRange * 0.05f;
                _peak = Mathf.Clamp(input - clip, _peak, 0);
            }

            // Normalize the input value.
            input = Mathf.Clamp01((input + calculatedGain) / _dynamicRange + 1);

            if (_holdAndFallDown)
            {
                // Hold-and-fall-down animation.
                _fall += Mathf.Pow(10, 1 + _fallDownSpeed * 2) * dt;
                _amplitude -= _fall * dt;

                // Pull up by input.
                if (_amplitude < input)
                {
                    _amplitude = input;
                    _fall = 0;
                }
            }
            else
            {
                _amplitude = input;
            }

            // Output
            _normalizedLevelEvent.Invoke(_amplitude);
        }

        #endregion
    }
}
