using UnityEngine;
using System.Linq;

class DeviceList : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Text _label = null;

    string ChannelMode(int count)
      => count == 1 ? "Mono" : (count == 2 ? "Stereo" : $"{count} channels");

    string Describe(in Lasp.DeviceDescriptor dev)
      => $"Name: {dev.Name}\nID: {dev.ID}\n" +
         $"{ChannelMode(dev.ChannelCount)}, {dev.SampleRate} Hz\n";

    void Update()
    {
        var desc = Lasp.AudioSystem.InputDevices.Select(dev => Describe(dev));
        _label.text = string.Join("\n", desc);
    }
}
