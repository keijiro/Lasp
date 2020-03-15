using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class WaveformRenderer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Lasp.AudioLevelTracker _input = null;
    [SerializeField, Range(16, 1024)] int _resolution = 512;
    [SerializeField] Material _material = null;

    #endregion

    #region MonoBehaviour implementation

    void Update()
    {
        var slice = _input.AudioDataSlice;
        if (slice.Length == 0) return;

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
