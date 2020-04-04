using UnityEngine;
using UnityEditor;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Lasp.Editor
{
    //
    // A utility class for drawing a spectrum graph
    //
    static class SpectrumDrawer
    {
        public static void DrawGraph(NativeArray<float> spectrum)
        {
            EditorGUILayout.Space();

            var rect = GUILayoutUtility.GetRect(128, 64);

            // Spectrum curve construction
            using (var temp = NewTempVertices())
            {
                // Reinterpretation as x4 packed vertices
                var packed = temp.Reinterpret<float3x4>(sizeof(float) * 3);

                // Construction job on the main thread.
                new SpectrumCurveConstructionJob
                  { Input = spectrum, Output = packed, Rect = rect,
                    Log2Ni = math.log2(spectrum.Length),
                    DivNo = 1.0f / packed.Length }
                  .Run(_vertices.Length / 4);

                // Job result retrieval
                temp.CopyTo(_vertices);
            }

            // Background
            Handles.DrawSolidRectangleWithOutline
              (rect, new Color(0.1f, 0.1f, 0.1f, 1), Color.clear);

            // Curve
            Handles.color = Color.white;
            Handles.DrawAAPolyLine(_vertices);
        }

        // Static vertex array (used to avoid GC alloc)
        static Vector3 [] _vertices = new Vector3 [192];

        // Temporary native array used to update the vertex array
        static NativeArray<Vector3> NewTempVertices()
          => new NativeArray<Vector3>(_vertices.Length, Allocator.TempJob,
                                      NativeArrayOptions.UninitializedMemory);

        // SIMD optimized spectrum curve construction job
        [Unity.Burst.BurstCompile(CompileSynchronously = true)]
        struct SpectrumCurveConstructionJob : IJobFor
        {
            [ReadOnly] public NativeArray<float> Input;
            [WriteOnly] public NativeArray<float3x4> Output;

            public Rect Rect;
            public float Log2Ni, DivNo;

            public void Execute(int i)
            {
                var offsets = math.float4(0, 1, 2, 3) / 4;

                // Log scale by inverse projection
                var x = (offsets + i) * DivNo;
                var p = math.pow(2, math.lerp(0.1f, 1, x) * Log2Ni);
                var y = math.saturate(SmoothSample(p));

                // Transform the point into the editor rect.
                x = x * Rect.width + Rect.xMin;
                y = Rect.yMax - y * Rect.height;

                // Output
                Output[i] = math.transpose(math.float4x3(x, y, 0));
            }

            // 4 point sampling
            float4 Sample4(int4 i)
              => math.float4(Input[i.x], Input[i.y], Input[i.z], Input[i.w]);

            // 4 point sampling with smoothing
            float4 SmoothSample(float4 p)
            {
                var i = (int4)p;

                var i0 = math.max(0, i - 1);
                var i1 = i;
                var i2 = math.min(i + 1, Input.Length - 1);
                var i3 = math.min(i + 2, Input.Length - 1);

                var y0 = Sample4(i0);
                var y1 = Sample4(i1);
                var y2 = Sample4(i2);
                var y3 = Sample4(i3);

                return Cubic(y0, y1, y2, y3, p - i);
            }

            // Cubic interpolation
            static float4 Cubic
              (float4 y0, float4 y1, float4 y2, float4 y3, float4 p)
            {
                var a0 = y3 - y2 - y0 + y1;
                var a1 = y0 - y1 - a0;
                var a2 = y2 - y0;
                var a3 = y1;
                var p2 = p * p;
                return a0 * p * p2 + a1 * p2 + a2 * p + a3;
            }
        }
    }
}
