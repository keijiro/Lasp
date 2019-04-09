// LASP - Low-latency Audio Signal Processing plugin for Unity
// https://github.com/keijiro/Lasp

using System;

namespace Lasp
{
    // LASP input stream class
    //
    // NOTE: Even though the unmanaged resources are automatically disposed
    // by the finalizer, it's strongly recommended to explicitly call Dispose()
    // from owner because the finalizer is not necessarily invoked in the main
    // thread and the driver resources may not be correctly released from non-
    // main threads. Calling Dispose() is the safest way to clean things up.
    public class LaspStream : IDisposable
    {
        public LaspStream()
        {
            PluginEntry.SetupLogger();
            _driver = PluginEntry.CreateDriver();
        }

        ~LaspStream()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_driver != IntPtr.Zero)
            {
                PluginEntry.DeleteDriver(_driver);
                _driver = IntPtr.Zero;
            }
        }

        public bool Open()
        {
            return PluginEntry.OpenStream(_driver);
        }

        public void Close()
        {
            PluginEntry.CloseStream(_driver);
        }

        public float GetPeakLevel(FilterType filter, float duration)
        {
            return PluginEntry.GetPeakLevel(_driver, filter, duration);
        }

        public float CalculateRMS(FilterType filter, float duration)
        {
            return PluginEntry.CalculateRMS(_driver, filter, duration);
        }

        public int RetrieveWaveform(FilterType filter, float[] dest, int length)
        {
            return PluginEntry.RetrieveWaveform(_driver, filter, dest, length);
        }

        System.IntPtr _driver;
    }
}
