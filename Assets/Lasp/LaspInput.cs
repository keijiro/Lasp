using UnityEngine;

namespace Lasp
{
    public static class LaspInput
    {
        #region Public methods

        public static float GetPeakLevel(FilterType filter)
        {
            UpdateState();
            return _filterBank[(int)filter].peak;
        }

        public static float CalculateRMS(FilterType filter)
        {
            UpdateState();
            return _filterBank[(int)filter].rms;
        }

        public static void RetrieveWaveform(FilterType filter, float[] dest)
        {
            UpdateState();
            _filterBank[(int)filter].CopyWaveform(dest);
        }

        public static void Terminate()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
                _stream = null;
                _filterBank = null;
            }
        }

        #endregion

        #region Internal methods

        static LaspStream _stream;
        static Filter[] _filterBank;
        static int _lastUpdateFrame;

        static void UpdateState()
        {
            if (_stream == null) Initialize();

            if (_lastUpdateFrame < Time.frameCount)
            {
                foreach (var f in _filterBank) f.InvalidateState();
                _lastUpdateFrame = Time.frameCount;
            }
        }

        static void Initialize()
        {
            _stream = new Lasp.LaspStream();
            _stream.Open();

            LaspUpdater.Create();

            _filterBank = new[] {
                new Filter(_stream, FilterType.Bypass),
                new Filter(_stream, FilterType.LowPass),
                new Filter(_stream, FilterType.BandPass),
                new Filter(_stream, FilterType.HighPass)
            };

            _lastUpdateFrame = -1;
        }

        #endregion

        #region Filter buffer class

        sealed class Filter
        {
            LaspStream _stream;
            FilterType _filter;

            float _peak;
            bool _peakUpdated;

            public float peak
            {
                get
                {
                    if (!_peakUpdated)
                    {
                        _peak = _stream.GetPeakLevel(_filter, Time.deltaTime);
                        _peakUpdated = true;
                    }
                    return _peak;
                }
            }

            float _rms;
            bool _rmsUpdated;

            public float rms
            {
                get
                {
                    if (!_rmsUpdated)
                    {
                        _rms = _stream.CalculateRMS(_filter, Time.deltaTime);
                        _rmsUpdated = true;
                    }
                    return _rms;
                }
            }

            float[] _waveform;
            bool _waveformUpdated;

            public void CopyWaveform(float[] dest)
            {
                if (!_waveformUpdated)
                {
                    _stream.RetrieveWaveform(_filter, _waveform, _waveform.Length);
                    _waveformUpdated = true;
                }
                System.Array.Copy(_waveform, dest, _waveform.Length);
            }

            public Filter(LaspStream stream, FilterType filter)
            {
                _stream = stream;
                _filter = filter;
                _waveform = new float [512];
            }

            public void InvalidateState()
            {
                _peakUpdated = _rmsUpdated = _waveformUpdated = false;
            }
        }

        #endregion
    }
}
