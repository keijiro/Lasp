using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

//
// InputStream usage example (Lissajous curve renderer)
//
// LASP provides not only MonoBehaviour-based class but also a raw input stream
// class (InputStream) that allows an application to access pre-normalized
// audio levels and waveform data (per-channel or interleaved). It's convenient
// to access audio data when you don't need the level tracking algorithm or the
// property binding mechanism.
//
// LASP automatically manages InputStream instances. You can open an input
// stream at any time, and it starts streaming when you access the actual data.
// It's automatically released when you stop using it.
//
// This example shows how to use InputStream to retrieve multiple channel data.
//
sealed class LissajousRenderer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Material _material = null;
    [SerializeField, Range(0, 10)] float _amplitude = 1;

    // Public accessor (needed to be controlled by UI)
    public float amplitude { get => _amplitude; set => _amplitude = value; }

    #endregion

    #region MonoBehaviour implementation

    Lasp.InputStream _stream;

    void Start()
    {
        //
        // Create an input stream from the system default input device.
        //
        _stream = Lasp.AudioSystem.GetDefaultInputStream();

        // Check if it's a stereo device (Lissajous only works with stereo).
        if (_stream.ChannelCount != 2)
        {
            Debug.LogError("This example only supports a stereo device.");
            Destroy(gameObject);
            return;
        }

        // Line mesh initialization
        InitializeMesh();
    }

    void Update()
    {
        //
        // Retrieve interleaved waveform data from the stream. The left and
        // right channel data is interleaved in this slice.
        //
        var slice = _stream.InterleavedDataSlice;

        // Update the line mesh.
        UpdateMesh(slice);

        // Draw the line mesh.
        Graphics.DrawMesh
          (_mesh, transform.localToWorldMatrix,
           _material, gameObject.layer);
    }

    void OnDestroy()
    {
        if (_mesh != null) Destroy(_mesh);

        //
        // We don't need to do anything here with the input stream. It will be
        // automatically released.
        //
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

        var sidx = 0;
        var vidx = 0;

        while (sidx < source.Length && vidx < VertexCount)
        {
            // Decompose the interleaved channel data.
            var l = source[sidx++];
            var r = source[sidx++];

            // Calculate the ertex position from the L-R channel values.
            vertices[vidx++] = math.float3(l, r, 0) * _amplitude;
        }

        // Fill the rest of the array with the last vertex.
        var last = (vidx == 0) ? float3.zero : vertices[vidx - 1];
        while (vidx < VertexCount) vertices[vidx++] = last;

        return vertices;
    }

    #endregion
}
