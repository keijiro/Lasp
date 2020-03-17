using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

//
// Raw waveform rendering example
//
// There are two approaches to retrieve raw waveform data from LASP.
//
// - AudioLevelTracker.AudioDataSlice: This property returns a strided native
//   slice that represents a raw waveform received at a specified channel of a
//   specified device. The length of the slice is the same as the duration of
//   the last frame, so you can continuously retrieve waveform data every frame
//   without bothering to buffer it.
//
// - The InputStream class provides some properties and methods for raw
//   waveform retrieval: InterleavedDataSpan, InterleavedDataSlice, and
//   GetChannelDataSlice. The former two properties return N-channel
//   interleaved data span/slice. You have to read them in a strided way if you
//   want individual channel data.
//
// This renderer script supports the former approach. It simply convert the
// waveform data into a vertex array and renders as a line strip mesh.
//
sealed class WaveformRenderer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Lasp.AudioLevelTracker _input = null;
    [SerializeField, Range(16, 1024)] int _resolution = 512;
    [SerializeField] Material _material = null;

    #endregion

    #region MonoBehaviour implementation

    void Update()
    {
        // Waveform retrieval
        var slice = _input.AudioDataSlice;
        if (slice.Length == 0) return;

        // Line strip mesh update and rendering
        UpdateMesh(slice);
        Graphics.DrawMesh
          (_mesh, transform.localToWorldMatrix,
           _material, gameObject.layer);
    }

    void OnDestroy()
    {
        if (_mesh != null) Destroy(_mesh);
    }

    #endregion

    #region Mesh generator

    Mesh _mesh;

    void UpdateMesh(NativeSlice<float> source)
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10);

            // Initial vertices
            using (var vertices = CreateVertexArray(source))
            {
                var pos = new VertexAttributeDescriptor
                  (VertexAttribute.Position,
                   VertexAttributeFormat.Float32, 3);

                _mesh.SetVertexBufferParams(vertices.Length, pos);
                _mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
            }

            // Initial indices
            using (var indices = CreateIndexArray())
            {
                _mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
                _mesh.SetIndexBufferData(indices, 0, 0, indices.Length);

                var lines = new SubMeshDescriptor
                  (0, indices.Length, MeshTopology.LineStrip);

                _mesh.SetSubMesh(0, lines);
            }
        }
        else
        {
            // Vertex update
            using (var vertices = CreateVertexArray(source))
              _mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
        }
    }

    NativeArray<int> CreateIndexArray()
    {
        var indices = Enumerable.Range(0, _resolution);
        return new NativeArray<int>(indices.ToArray(), Allocator.Temp);
    }

    NativeArray<Vector3> CreateVertexArray(NativeSlice<float> source)
    {
        var buffer = new NativeArray<Vector3>
          (_resolution, Allocator.Temp,
           NativeArrayOptions.UninitializedMemory);

        for (var vi = 0; vi < _resolution; vi++)
        {
            var x = (float)vi / _resolution;
            var i = vi * source.Length / _resolution;
            buffer[vi] = new Vector3(x, source[i], 0);
        }

        return buffer;
    }

    #endregion
}
