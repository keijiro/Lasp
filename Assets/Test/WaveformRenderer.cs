using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

//
// Raw waveform rendering example
//
// There are two approaches to retrieve raw waveforms from LASP.
//
// - AudioLevelTracker.audioDataSlice: This property returns a strided native
//   slice that represents a raw waveform received at a specified channel of a
//   specified device. The length of the slice is the same as the duration of
//   the last frame, so you can continuously retrieve waveform data every frame
//   without bothering to buffer it.
//
// - The InputStream class provides properties and methods for raw waveform
//   retrieval: InterleavedDataSpan, InterleavedDataSlice, and
//   GetChannelDataSlice. The former two properties return N-channel
//   interleaved data span/slice. You have to read them in a strided way if you
//   want individual channel data.
//
// This renderer script uses the former approach. It converts a waveform into a
// vertex array and renders it as a line strip mesh.
//
sealed class WaveformRenderer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Lasp.AudioLevelTracker _input = null;
    [SerializeField] Material _material = null;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Line mesh initialization
        InitializeMesh();
    }

    void Update()
    {
        //
        // Retrieve waveform data as a channel-strided data slice and then
        // update the line mesh with it.
        //
        UpdateMesh(_input.audioDataSlice);

        // Draw the line mesh.
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

    // The number of vertices.
    // 2048 is enough for rendering 48,000Hz audio at 30fps.
    const int VertexCount = 2048;

    void InitializeMesh()
    {
        _mesh = new Mesh();
        _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10);

        // Initial vertices
        using (var vertices = CreateVertexArray(default(NativeSlice<float>)))
        {
            var desc = new VertexAttributeDescriptor
              (VertexAttribute.Position, VertexAttributeFormat.Float32, 3);

            _mesh.SetVertexBufferParams(vertices.Length, desc);
            _mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
        }

        // Initial indices
        using (var indices = CreateIndexArray())
        {
            var desc = new SubMeshDescriptor
              (0, indices.Length, MeshTopology.LineStrip);

            _mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
            _mesh.SetIndexBufferData(indices, 0, 0, indices.Length);
            _mesh.SetSubMesh(0, desc);
        }
    }

    void UpdateMesh(NativeSlice<float> source)
    {
        using (var vertices = CreateVertexArray(source))
            _mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
    }

    NativeArray<int> CreateIndexArray()
    {
        return new NativeArray<int>
          (Enumerable.Range(0, VertexCount).ToArray(), Allocator.Temp);
    }

    NativeArray<float3> CreateVertexArray(NativeSlice<float> source)
    {
        var vertices = new NativeArray<float3>
          (VertexCount, Allocator.Temp,
           NativeArrayOptions.UninitializedMemory);

        var vcount = math.min(source.Length, VertexCount);

        // Transfer waveform data to the vertex array.
        for (var i = 0; i < vcount; i++)
        {
            var x = (float)i / (vcount - 1);
            vertices[i] = math.float3(x, source[i], 0);
        }

        // Fill the rest of the array with the last vertex.
        var last = (vcount == 0) ? float3.zero : vertices[vcount - 1];
        for (var i = vcount; i < VertexCount; i++) vertices[i] = last;

        return vertices;
    }

    #endregion
}
