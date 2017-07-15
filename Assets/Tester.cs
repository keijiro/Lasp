using UnityEngine;

public class Tester : MonoBehaviour
{
    float[] waveform_;

    void Start()
    {
        Lasp.PluginEntry.Initialize();
        waveform_ = new float[512];
    }

    void OnDestroy()
    {
        Lasp.PluginEntry.Terminate();
    }

    void Update()
    {
        var peak = Lasp.PluginEntry.GetPeakLevel(Time.deltaTime);
        Lasp.PluginEntry.CopyWaveform(waveform_, waveform_.Length);
        transform.localScale = Vector3.one * peak;
    }
}
