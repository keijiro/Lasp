using System;
using System.Runtime.InteropServices;

namespace Lasp
{
    public enum FilterType { Bypass, LowPass, BandPass, HighPass }

    public static class PluginEntry
    {
        [DllImport("Lasp", EntryPoint="LaspCreateDriver")]
        public static extern IntPtr CreateDriver();

        [DllImport("Lasp", EntryPoint="LaspDeleteDriver")]
        public static extern void DeleteDriver(IntPtr driver);

        [DllImport("Lasp", EntryPoint="LaspOpenStream")]
        public static extern bool OpenStream(IntPtr driver);

        [DllImport("Lasp", EntryPoint="LaspCloseStream")]
        public static extern void CloseStream(IntPtr driver);

        [DllImport("Lasp", EntryPoint="LaspGetPeakLevel")]
        public static extern float GetPeakLevel(IntPtr driver, FilterType filter, float duration);

        [DllImport("Lasp", EntryPoint="LaspCalculateRMS")]
        public static extern float CalculateRMS(IntPtr driver, FilterType filter, float duration);

        [DllImport("Lasp", EntryPoint="LaspRetrieveWaveform")]
        public static extern int RetrieveWaveform(IntPtr driver, FilterType filter, float[] dest, int length);
    }
}
