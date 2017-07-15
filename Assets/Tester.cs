using UnityEngine;
using System.Collections.Generic;

public class Tester : MonoBehaviour
{
    [SerializeField] Transform _peakIndicator;
    [SerializeField] Transform _rmsIndicator;
    [SerializeField] Material _lineMaterial;

    float[] _waveform;
    List<Vector3> _vertices;
    Mesh _mesh;

    void Start()
    {
        _waveform = new float[512];

        _mesh = new Mesh();
        _mesh.MarkDynamic();

        _vertices = new List<Vector3>(_waveform.Length);
        for (var i = 0; i < _waveform.Length; i++) _vertices.Add(Vector3.zero);
        _mesh.SetVertices(_vertices);

        var indices = new int[2 * (_waveform.Length - 1)];
        for (var i = 0; i < _waveform.Length - 1; i++)
        {
            indices[2 * i + 0] = i;
            indices[2 * i + 1] = i + 1;
        }
        _mesh.SetIndices(indices, MeshTopology.Lines, 0);

        Lasp.PluginEntry.Initialize();
    }

    void OnDestroy()
    {
        Destroy(_mesh);
        Lasp.PluginEntry.Terminate();
    }

    void Update()
    {
        var peak = Lasp.PluginEntry.GetPeakLevel(Time.deltaTime);
        var rms = Lasp.PluginEntry.CalculateRMS(Time.deltaTime);
        Lasp.PluginEntry.CopyWaveform(_waveform, _waveform.Length);

        _peakIndicator.localScale = new Vector3(1, peak, 1);
        _rmsIndicator.localScale = new Vector3(1, rms, 1);

        var scale = 2.0f / _waveform.Length;
        for (var i = 0; i < _waveform.Length; i++)
            _vertices[i] = new Vector3(scale * i - 1, _waveform[i], 0);
        _mesh.SetVertices(_vertices);

        Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _lineMaterial, gameObject.layer);
    }
}
