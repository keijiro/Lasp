using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

//
// Spectrum graph rendering example
//
// You can retrieve the spectrum data from SpectrumAnalyzer using the following
// properties.
//
// - spectrumSpan / spectrumArray: Return raw spectrum data, which preserves the
//   full resolution of the FFT. It's useful for audio signal processing but not
//   very convenient for visual effects because it's not log-scaled.
//
//   spectrumSpan returns ReadOnlySpan<float>, and spectrumArray returns
//   NativeArray<float>. There is no performance difference between them.
//
// - logSpectrumSpan / logSpectrumArray: Returns log-scaled spectrum data, which
//   is useful for creating visual effects. Note that it causes inconsistencies
//   in the visual resolution. The low frequency band gets softened, and the
//   high frequency band gets sharpened.
//
// This script retrieves the spectrum data from a given SpectrumAnalyzer and
// renders it as a line strip mesh.
//
sealed class SpectrumGraph : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Lasp.SpectrumAnalyzer _input = null;
    [SerializeField] bool _logScale = true;
    [SerializeField] Material _material = null;

    #endregion

    #region MonoBehaviour implementation

    void Update()
    {
        //
        // Retrieve the spectrum data (linear or log-scaled) and then construct
        // a line strip mesh with it.
        //
        var span = _logScale ? _input.logSpectrumSpan : _input.spectrumSpan;
        if (_mesh == null) InitializeMesh(span); else UpdateMesh(span);

        // Draw the line strip mesh.
        Graphics.DrawMesh
          (_mesh, transform.localToWorldMatrix,
           _material, gameObject.layer);
    }

    void OnDestroy()
    {
        if (_mesh != null) Destroy(_mesh);
    }

    #endregion

    #region Line mesh operations

    Mesh _mesh;

    void InitializeMesh(System.ReadOnlySpan<float> source)
    {
        _mesh = new Mesh();
        _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10);

        // Initial vertices
        using (var vertices = CreateVertexArray(source))
        {
            var desc = new VertexAttributeDescriptor
              (VertexAttribute.Position, VertexAttributeFormat.Float32, 3);

            _mesh.SetVertexBufferParams(vertices.Length, desc);
            _mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
        }

        // Initial indices
        using (var indices = CreateIndexArray(source.Length))
        {
            var desc = new SubMeshDescriptor
              (0, indices.Length, MeshTopology.LineStrip);

            _mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
            _mesh.SetIndexBufferData(indices, 0, 0, indices.Length);
            _mesh.SetSubMesh(0, desc);
        }
    }

    void UpdateMesh(System.ReadOnlySpan<float> source)
    {
        using (var vertices = CreateVertexArray(source))
            _mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
    }

    NativeArray<int> CreateIndexArray(int vertexCount)
    {
        return new NativeArray<int>
          (Enumerable.Range(0, vertexCount).ToArray(), Allocator.Temp);
    }

    NativeArray<float3> CreateVertexArray(System.ReadOnlySpan<float> source)
    {
        var vertices = new NativeArray<float3>
          (source.Length, Allocator.Temp,
           NativeArrayOptions.UninitializedMemory);

        // Transfer spectrum data to the vertex array.
        var xscale = 1.0f / (source.Length - 1);
        for (var i = 0; i < source.Length; i++)
            vertices[i] = math.float3(i * xscale, source[i], 0);

        return vertices;
    }

    #endregion
}
