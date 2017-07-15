using UnityEngine;
using System.Runtime.InteropServices;

namespace Lasp
{
    public enum FilterType { None, LowPass, BandPass, HighPass }

    public static class PluginEntry
    {
        [DllImport("Lasp", EntryPoint="LaspInitialize")]
        public static extern bool Initialize();

        [DllImport("Lasp", EntryPoint="LaspFinalize")]
        public static extern void Terminate();

        [DllImport("Lasp", EntryPoint="LaspGetPeakLevel")]
        public static extern float GetPeakLevel(FilterType filter, float duration);

        [DllImport("Lasp", EntryPoint="LaspCalculateRMS")]
        public static extern float CalculateRMS(FilterType filter, float duration);

        [DllImport("Lasp", EntryPoint="LaspCopyWaveform")]
        public static extern int CopyWaveform(FilterType filter, float[] dest, int length);
    }
}
