// LASP - Low-latency Audio Signal Processing plugin for Unity
// https://github.com/keijiro/Lasp

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
#define LASP_ENABLED
#endif

using System;
using System.Runtime.InteropServices;

namespace Lasp
{
    internal static class PluginEntry
    {
        #if LASP_ENABLED

        #region Plugin interface

        [DllImport("Lasp", EntryPoint="LaspCreateDriver")]
        public static extern IntPtr CreateDriver();

        [DllImport("Lasp", EntryPoint="LaspDeleteDriver")]
        public static extern void DeleteDriver(IntPtr driver);

        [DllImport("Lasp", EntryPoint="LaspOpenStream")]
        public static extern bool OpenStream(IntPtr driver);

        [DllImport("Lasp", EntryPoint="LaspCloseStream")]
        public static extern void CloseStream(IntPtr driver);

        [DllImport("Lasp", EntryPoint="LaspGetSampleRate")]
        public static extern float GetSampleRate(IntPtr driver);

        [DllImport("Lasp", EntryPoint="LaspGetPeakLevel")]
        public static extern float GetPeakLevel(IntPtr driver, FilterType filter, float duration);

        [DllImport("Lasp", EntryPoint="LaspCalculateRMS")]
        public static extern float CalculateRMS(IntPtr driver, FilterType filter, float duration);

        [DllImport("Lasp", EntryPoint="LaspRetrieveWaveform")]
        public static extern int RetrieveWaveform(IntPtr driver, FilterType filter, float[] dest, int length);

        [DllImport("Lasp", EntryPoint="LaspRetrieveFft")]
        public static extern int RetrieveFft(IntPtr driver, FftAveragingType type, float[] dest, int length);

        #endregion

        #region Debug helpers

        public static void SetupLogger()
        {
            var del = (PrintDelegate)Log;
            var ptr = Marshal.GetFunctionPointerForDelegate(del);
            ReplaceLogger(ptr);
        }

        [DllImport("Lasp", EntryPoint="LaspReplaceLogger")]
        public static extern void ReplaceLogger(IntPtr logger);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PrintDelegate(string message);

        static void Log(string message)
        {
        #if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
        #else
            System.Console.WriteLine(message);
        #endif
        }

        #endregion

        #else

        public static IntPtr CreateDriver() { return IntPtr.Zero; }
        public static void DeleteDriver(IntPtr driver) {}
        public static bool OpenStream(IntPtr driver) { return false; }
        public static void CloseStream(IntPtr driver) {}
        public static float GetSampleRate(IntPtr driver) { return 0; }
        public static float GetPeakLevel(IntPtr driver, FilterType filter, float duration) { return 0; }
        public static float CalculateRMS(IntPtr driver, FilterType filter, float duration) { return 0; }
        public static int RetrieveWaveform(IntPtr driver, FilterType filter, float[] dest, int length) { return 0; }
        public static void SetupLogger() {}
        public static int RetrieveFft(IntPtr driver, float[] dest, int length) { return 0; }

        #endif
    }
}
