namespace Lasp
{
    //
    // A descriptor class that identifies a device (audio interface endpoint)
    // and describes its basic specifications.
    //
    public struct DeviceDescriptor
    {
        #region Property accessors

        public string ID => _handle.SioDevice.ID;
        public string Name => _handle.SioDevice.Name;
        public int ChannelCount => _handle.SioDevice.Layouts[0].ChannelCount;
        public int SampleRate => _handle.SioDevice.SampleRates[0];

        #endregion

        #region Internal members (initialized by DeviceManager)

        internal InputDeviceHandle _handle;

        #endregion
    }
}
