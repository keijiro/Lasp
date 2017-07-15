using UnityEngine;
using System.Collections.Generic;

public class Tester : MonoBehaviour
{
    [SerializeField] Lasp.FilterType _filterType;
    [SerializeField] float _amplify = 1;
    [SerializeField] Transform _peakIndicator;
    [SerializeField] Transform _rmsIndicator;
    [SerializeField] Material _lineMaterial;

    float[] _waveform;
    List<Vector3> _vertices;
    Mesh _mesh;

    static int _instanceCount;

    void Start()
    {
        if (_instanceCount == 0) Lasp.PluginEntry.Initialize();
        _instanceCount++;

        _waveform = new float[512];

        _vertices = new List<Vector3>(_waveform.Length);
        for (var i = 0; i < _waveform.Length; i++) _vertices.Add(Vector3.zero);

        CreateMesh();
    }

    void OnDestroy()
    {
        _instanceCount--;
        if (_instanceCount == 0) Lasp.PluginEntry.Terminate();

        Destroy(_mesh);
    }

    void Update()
    {
        var peak = Lasp.PluginEntry.GetPeakLevel(_filterType, Time.deltaTime);
        var rms = Lasp.PluginEntry.CalculateRMS(_filterType, Time.deltaTime);
        Lasp.PluginEntry.CopyWaveform(_filterType, _waveform, _waveform.Length);

        _peakIndicator.localScale = new Vector3(1, peak * _amplify, 1);
        _rmsIndicator.localScale = new Vector3(1, rms * _amplify, 1);

        UpdateMeshWithWaveform();
        Graphics.DrawMesh(_mesh, transform.localToWorldMatrix, _lineMaterial, gameObject.layer);
    }

    void CreateMesh()
    {
        var indices = new int[2 * (_waveform.Length - 1)];

        for (var i = 0; i < _waveform.Length - 1; i++)
        {
            indices[2 * i + 0] = i;
            indices[2 * i + 1] = i + 1;
        }

        _mesh = new Mesh();
        _mesh.MarkDynamic();
        _mesh.SetVertices(_vertices);
        _mesh.SetIndices(indices, MeshTopology.Lines, 0);
    }

    void UpdateMeshWithWaveform()
    {
        var scale = 2.0f / _waveform.Length;

        for (var i = 0; i < _waveform.Length; i++)
            _vertices[i] = new Vector3(scale * i - 1, _waveform[i] * _amplify, 0);

        _mesh.SetVertices(_vertices);
    }
}
