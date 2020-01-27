// LASP - Low-latency Audio Signal Processing plugin for Unity
// https://github.com/keijiro/Lasp

using System;

namespace Lasp
{
    // LASP input stream class
    //
    // NOTE: Even though the unmanaged resources are automatically disposed
    // by the finalizer, it's strongly recommended to explicitly call Dispose()
    // from its owner because the finalizer may be invoked from a non-main
    // thread that can introduce threading issues. Calling Dispose() is the
    // safest way to clean things up.
    internal sealed class LaspStream : IDisposable
    {
        public LaspStream()
        {
            PluginEntry.SetupLogger();
            _driver = PluginEntry.CreateDriver();
        }

        ~LaspStream()
        {
            ReleaseUnmanagedResources();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        void ReleaseUnmanagedResources()
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

        public int RetrieveFft(float[] dest, int length)
        {
            return PluginEntry.RetrieveFft(_driver, dest, length);
        }

        System.IntPtr _driver;
    }
}
