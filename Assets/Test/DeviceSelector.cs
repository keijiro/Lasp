using UnityEngine;
using UnityEngine.UI;
using System.Linq;

//
// Runtime device selection and AudioLevelTracker instantiation example
//
class DeviceSelector : MonoBehaviour
{
    #region Scene object references

    [SerializeField] Dropdown _deviceList = null;
    [SerializeField] Dropdown _channelList = null;
    [SerializeField] Transform _targetTransform = null;

    #endregion

    #region Custom dropdown item class

    //
    // We want the dropdown items to have device identifier, so we extend the
    // OptionData class and add the id field. Also we add a constructor that
    // initializes the data based on a LASP device descriptor.
    //
    class DeviceItem : Dropdown.OptionData
    {
        public string id;
        public DeviceItem(in Lasp.DeviceDescriptor device)
          => (text, id) = (device.Name, device.ID);
    }

    #endregion

    #region MonoBehaviour implementation

    // Reference to the level tracker object that is created by this script.
    Lasp.AudioLevelTracker _tracker;

    void Start()
    {
        _deviceList.ClearOptions();
        _channelList.ClearOptions();

        //
        // Construct the device selection dropdown list.
        //
        // LASP provides IEnumerable of available audio input devices with
        // AudioSystem.InputDevices. Here we construct the dropdown list from
        // it using LINQ.
        //
        _deviceList.options.AddRange
          (Lasp.AudioSystem.InputDevices.Select(dev => new DeviceItem(dev)));

        _deviceList.RefreshShownValue();

        //
        // If there is any input device, select the first one.
        //
        if (Lasp.AudioSystem.InputDevices.Any()) OnDeviceSelected(0);
    }

    #endregion

    #region UI callback

    public void OnDeviceSelected(int index)
    {
        // Retrieve the device ID from the dropdown item data.
        var id = ((DeviceItem)_deviceList.options[index]).id;

        //
        // In LASP, we can specify an input device using ID (not a display
        // name). We also can use AudioSystem.DefaultDevice that returns the
        // system default input device.
        //
        var dev = Lasp.AudioSystem.GetInputDevice(id);

        //
        // The DeviceDescriptor struct contains several information, like
        // the number of the channels and the sampling rate. Here we construct
        // the channel selection dropdown list from the descriptor.
        //
        _channelList.options =
          Enumerable.Range(0, dev.ChannelCount).
          Select(i => $"Channel {i + 1}").
          Select(text => new Dropdown.OptionData(){ text = text }).ToList();

        _channelList.RefreshShownValue();

        // Destroy the previously created level tracker object.
        if (_tracker != null) Destroy(_tracker.gameObject);

        // Create a new level tracker game object.
        var gameObject = new GameObject("Level Tracker");

        //
        // Add the LASP audio level tracker component to the game object.
        // Then make the tracker use the chosen device.
        //
        _tracker = gameObject.AddComponent<Lasp.AudioLevelTracker>();
        _tracker.deviceID = dev.ID;

        //
        // Add a property binder to the tracker that controls the scale of the
        // target transform based on the audio level.
        //
        _tracker.propertyBinders =
          new [] {
            new Lasp.Vector3PropertyBinder {
              Target = _targetTransform,
              PropertyName = "localScale",
              Value0 = Vector3.zero,
              Value1 = Vector3.one
            }
          };
    }

    #endregion
}
