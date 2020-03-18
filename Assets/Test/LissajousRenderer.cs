using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
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
sealed class LissajousRenderer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Material _material = null;
    [SerializeField, Range(0, 10)] float _amplitude = 1;

    public float amplitude { get => _amplitude; set => _amplitude = value; }

    #endregion

    #region MonoBehaviour implementation

    //
    // Input stream object
    //
    // LASP manages input streams in an automatic fashion. You can create an
    // input stream any time, and it starts streaming when you access the data.
    // It's automatically released when you stop using it.
    //
    Lasp.InputStream _stream;

    void Start()
    {
        //
        // Create an input stream for the default audio input device.
        //
        _stream = Lasp.AudioSystem.GetDefaultInputStream();

        // Check if it's stereo device (Lissajous only works with stereo).
        if (_stream.ChannelCount != 2)
        {
            Debug.LogError("This example only supports a stereo device.");
            Destroy(gameObject);
            return;
        }

        // Line strip mesh initialization
        InitializeMesh();
    }

    void Update()
    {
        //
        // Retrieve interleaved waveform data from the stream. The left and
        // right channel data is interleaved in this single slice.
        //
        var slice = _stream.InterleavedDataSlice;

        // Update the line strip mesh.
        UpdateMesh(slice);

        // Draw the line strip mesh.
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

    #region Line strip mesh operations

    Mesh _mesh;

    // The number of vertices.
    // 2048 is enough for handling 48,000Hz audio at 30fps.
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
            var l = source[sidx++];
            var r = source[sidx++];
            vertices[vidx++] = math.float3(l, r, 0) * _amplitude;
        }

        var last = vidx == 0 ? float3.zero : vertices[vidx - 1];
        while (vidx < VertexCount) vertices[vidx++] = last;

        return vertices;
    }

    #endregion
}
