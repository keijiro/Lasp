using System;
using Unity.Collections;

namespace Lasp
{
    // Filter type enums used in audio input processing
    public enum FilterType { Bypass, LowPass, BandPass, HighPass }

    //
    // Input stream class
    //
    // This class provides a weak reference to an internal device handler
    // object. You can access the device information and the stream data
    // without manually managing the actual device object. The information will
    // be calculated when it's needed. The stream will be opened when you
    // access the data, and it will be closed when you stop accessing.
    // Everything is done in an on-demand fashion.
    //
    public sealed class InputStream
    {
        #region Stream settings

        public bool IsValid => _deviceHandle.IsValid;
        public int ChannelCount => _deviceHandle.StreamChannelCount;
        public int SampleRate => _deviceHandle.StreamSampleRate;

        #endregion

        #region Per-channel audio levels

        public float GetChannelLevel(int channel)
          => MathUtils.dBFS(_deviceHandle.GetChannelLevel(channel).x);

        public float GetChannelLevel(int channel, FilterType filter)
          => MathUtils.dBFS
             (_deviceHandle.GetChannelLevel(channel)[(int)filter]);

        #endregion

        #region Audio data (waveform)

        public ReadOnlySpan<float> InterleavedDataSpan
          => _deviceHandle.LastFrameWindow;

        public NativeSlice<float> InterleavedDataSlice
          => _deviceHandle.LastFrameWindow.GetNativeSlice();

        public NativeSlice<float> GetChannelDataSlice(int channel)
          => _deviceHandle.LastFrameWindow.GetNativeSlice
             (channel, ChannelCount);

        #endregion

        #region Private and internal members

        InputDeviceHandle _deviceHandle;

        InputStream() {} // Hidden constructor

        internal static InputStream Create(InputDeviceHandle deviceHandle)
          => (deviceHandle != null && deviceHandle.IsValid) ?
             new InputStream { _deviceHandle = deviceHandle } : null;

        #endregion
    }
}
