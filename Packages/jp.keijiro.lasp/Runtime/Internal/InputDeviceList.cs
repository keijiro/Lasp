using System.Collections;
using System.Collections.Generic;

namespace Lasp
{
    //
    // A collection class used for enumerating available input devices
    //
    sealed class InputDeviceList : IEnumerable<InputDeviceHandle>
    {
        #region IEnumerable implementation

        public IEnumerator<InputDeviceHandle> GetEnumerator()
          => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
          => _list.GetEnumerator();

        #endregion

        #region Storage object

        List<InputDeviceHandle> _list = new List<InputDeviceHandle>(); 

        #endregion

        #region Public method

        // Scan and update the input device list.
        // It reuses object handles if their bound devices are still there.
        public void ScanAvailable(SoundIO.Context context)
        {
            var deviceCount = context.InputDeviceCount;
            var defaultIndex = context.DefaultInputDeviceIndex;

            var founds = new List<InputDeviceHandle>();

            for (var i = 0; i < deviceCount; i++)
            {
                var dev = context.GetInputDevice(i);

                // Check if the device is useful. Reject it if not.
                if (dev.IsRaw || dev.Layouts.Length < 1)
                {
                    dev.Dispose();
                    continue;
                }

                // Find the same device in the current list.
                var handle = _list.FindAndRemove(h => h.SioDevice.ID == dev.ID);

                if (handle != null)
                {
                    // We reuse the handle, so this libsoundio device object
                    // should be disposed.
                    dev.Dispose();
                }
                else
                {
                    // Create a new handle with transferring the ownership of
                    // this libsoundio device object.
                    handle = InputDeviceHandle.CreateAndOwn(dev);
                }

                // Default device: Insert it at the head of the list.
                // Others: Simply append it to the list.
                if (i == defaultIndex)
                    founds.Insert(0, handle);
                else
                    founds.Add(handle);
            }

            // Dispose the remained handles (disconnected devices).
            foreach (var dev in _list) dev.Dispose();

            // Replace the list with the new one.
            _list = founds;
        }

        public void UpdateAll(float deltaTime)
        {
            foreach (var h in _list) h.Update(deltaTime);
        }

        #endregion
    }
}
