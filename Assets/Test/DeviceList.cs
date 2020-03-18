using UnityEngine;
using System.Linq;

//
// Audio input device enumeration example
//
// LASP provides IEnumerable of currently available audio input devices via
// AudioSystem.InputDevices. This example creates a device list from it using
// LINQ.
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

    // Device description string
    string Describe(in Lasp.DeviceDescriptor dev)
      => $"Name: {dev.Name}\nID: {dev.ID}\n" +
         $"{ChannelMode(dev.ChannelCount)}, {dev.SampleRate} Hz\n";

    #endregion

    #region MonoBehaviour implementation

    void Update()
    {
        // Create a device list using LINQ.
        var descs = Lasp.AudioSystem.InputDevices.Select(dev => Describe(dev));
        _label.text = string.Join("\n", descs);
    }

    #endregion
}
