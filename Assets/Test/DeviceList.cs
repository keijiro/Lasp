using UnityEngine;
using System.Linq;

//
// Audio input device enumeration example
//
sealed class DeviceList : MonoBehaviour
{
    #region Scene object reference

    [SerializeField] UnityEngine.UI.Text _label = null;

    #endregion

    #region Formatter functions

    // Channel count to string
    string ChannelMode(int count)
      => count == 1 ? "Mono" : (count == 2 ? "Stereo" : $"{count} channels");

    //
    // Generate a description of a given device.
    //
    string Describe(in Lasp.DeviceDescriptor dev)
      => $"Name: {dev.Name}\nID: {dev.ID}\n" +
         $"{ChannelMode(dev.ChannelCount)}, {dev.SampleRate} Hz\n";

    #endregion

    #region MonoBehaviour implementation

    void Update()
    {
        //
        // LASP provides IEnumerable of available audio input devices with
        // AudioSystem.InputDevices. Here we create a device list using LINQ.
        //
        var descs = Lasp.AudioSystem.InputDevices.Select(dev => Describe(dev));
        _label.text = string.Join("\n", descs);
    }

    #endregion
}
