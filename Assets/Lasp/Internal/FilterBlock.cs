// LASP - Low-latency Audio Signal Processing plugin for Unity
// https://github.com/keijiro/Lasp

using UnityEngine;

namespace Lasp
{
    // FilterBlock class that is used to cache results from a specific filter
    // block (single band in a filter bank).
    internal sealed class FilterBlock
    {
        FilterType _filter;
        LaspStream _stream;

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

        public FilterBlock(FilterType filter, LaspStream stream)
        {
            _filter = filter;
            _stream = stream;
        }

        public void InvalidateState()
        {
            _peakUpdated = _rmsUpdated = false;
        }
    }
}
