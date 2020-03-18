using UnityEngine;
using UnityEngine.UI;
using System.Linq;

//
// Runtime device selection and AudioLevelTracker instantiation example
//
// Usually, autio level trackers are configured on Editor, but in some use
// cases, you may want to instantiate and configure them at run time. This
// example shows how to select an input device using the AudioSystem properties
// and instantiate an audio level tracker from it at run time. It also shows
// how to construct property binders programmatically.
//
sealed class DeviceSelector : MonoBehaviour
{
    #region Scene object references

    [SerializeField] Dropdown _deviceList = null;
    [SerializeField] Dropdown _channelList = null;
    [SerializeField] Transform _targetTransform = null;

    #endregion

    #region Custom dropdown item class

    //
    // We want the dropdown items to have device identifier, so we extend the
    // OptionData class to add an ID field. Also we add a constructor that
    // initializes the data from a device descriptor.
    //
    class DeviceItem : Dropdown.OptionData
    {
        public string id;
        public DeviceItem(in Lasp.DeviceDescriptor device)
          => (text, id) = (device.Name, device.ID);
    }

    #endregion

    #region MonoBehaviour implementation

    Lasp.AudioLevelTracker _tracker;

    void Start()
    {
        _deviceList.ClearOptions();
        _channelList.ClearOptions();

        //
        // Construct the device selection dropdown list.
        //
        // LASP provides IEnumerable of currently available audio input devices
        // via AudioSystem.InputDevices. Here we construct a dropdown list from
        // it using LINQ.
        //
        _deviceList.options.AddRange
          (Lasp.AudioSystem.InputDevices.Select(dev => new DeviceItem(dev)));

        _deviceList.RefreshShownValue();

        //
        // If there is any input device, select the first one (the system
        // default input device).
        //
        if (Lasp.AudioSystem.InputDevices.Any()) OnDeviceSelected(0);
    }

    void Update()
    {
        //
        // Apply the channel selection to the audio level tracker.
        //
        if (_tracker != null) _tracker.channel = _channelList.value;
    }

    #endregion

    #region UI callback

    public void OnDeviceSelected(int index)
    {
        // Retrieve the device ID from the dropdown item data.
        var id = ((DeviceItem)_deviceList.options[index]).id;

        //
        // Retrieve a descriptor of the selected device using the ID.
        //
        var dev = Lasp.AudioSystem.GetInputDevice(id);

        //
        // The device descriptor struct has several attributes, like the number
        // of the channels, the sampling rate, etc. Here we construct the
        // channel selection dropdown list from the descriptor.
        //
        _channelList.options =
          Enumerable.Range(0, dev.ChannelCount).
          Select(i => $"Channel {i + 1}").
          Select(text => new Dropdown.OptionData(){ text = text }).ToList();

        _channelList.value = 0;
        _channelList.RefreshShownValue();

        // Destroy the previously created level tracker object...
        if (_tracker != null) Destroy(_tracker.gameObject);

        // ...then create a new one.
        var gameObject = new GameObject("Level Tracker");

        //
        // Add the LASP audio level tracker component to the game object and
        // make it use the selected device.
        //
        _tracker = gameObject.AddComponent<Lasp.AudioLevelTracker>();
        _tracker.deviceID = dev.ID;

        //
        // Add a property binder to the tracker that controls the scale of the
        // target transform based on a normalize audio level.
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
