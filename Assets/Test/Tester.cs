using UnityEngine;
using System.Collections.Generic;

public class Tester : MonoBehaviour
{
    [SerializeField] Lasp.FilterType _filterType;
    [SerializeField] Transform _peakIndicator;
    [SerializeField] Transform _rmsIndicator;
    [SerializeField] Material _lineMaterial;

    const float kSilence = -40; // -40 dBFS = silence

    float[] _waveform;
    List<Vector3> _vertices;
    Mesh _mesh;

    void Start()
    {
        _waveform = new float[512];

        _vertices = new List<Vector3>(_waveform.Length);
        for (var i = 0; i < _waveform.Length; i++) _vertices.Add(Vector3.zero);

        CreateMesh();
    }

    void OnDestroy()
    {
        Destroy(_mesh);
    }

    void Update()
    {
        var peak = Lasp.AudioInput.GetPeakLevelDecibel(_filterType);
        var rms = Lasp.AudioInput.CalculateRMSDecibel(_filterType);
        Lasp.AudioInput.RetrieveWaveform(_filterType, _waveform);

        peak = Mathf.Clamp01(1 - peak / kSilence);
        rms = Mathf.Clamp01(1 - rms / kSilence);

        _peakIndicator.localScale = new Vector3(1, peak, 1);
        _rmsIndicator.localScale = new Vector3(1, rms, 1);

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
            _vertices[i] = new Vector3(scale * i - 1, _waveform[i], 0);

        _mesh.SetVertices(_vertices);
    }
}
