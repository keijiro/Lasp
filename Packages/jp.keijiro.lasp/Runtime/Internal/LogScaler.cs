using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Lasp
{
    //
    // X-axis log-scale resampler for spectrum analysis
    //
    public sealed class LogScaler : System.IDisposable
    {
        public NativeArray<float> Resample(NativeArray<float> source)
        {
            var length_4 = source.Length / 4;

            // Dispose the output buffer if the size doesn't match.
            if (_buffer.IsCreated && _buffer.Length != length_4)
                _buffer.Dispose();

            // Lazy initialization of the output buffer
            if (!_buffer.IsCreated)
                _buffer = PersistentMemory.New<float4>(length_4);

            // Run the resampling job on the main thread.
            new ResamplingJob
              { Input = source, Output = _buffer,
                Log2Ni = math.log2(source.Length), DivNo = 1.0f / length_4 }
              .Run(length_4);

            // Return the output buffer as a float array.
            return _buffer.Reinterpret<float>(sizeof(float) * 4);
        }

        public void Dispose()
        {
            if (_buffer.IsCreated) _buffer.Dispose();
        }

        NativeArray<float4> _buffer;

        [Unity.Burst.BurstCompile(CompileSynchronously = true)]
        struct ResamplingJob : IJobFor
        {
            [ReadOnly] public NativeArray<float> Input;
            [WriteOnly] public NativeArray<float4> Output;

            public float Log2Ni, DivNo;

            public void Execute(int i)
            {
                var offsets = math.float4(0, 1, 2, 3) / 4;

                // Log scale by inverse projection
                var x = (offsets + i) * DivNo;
                var p = math.pow(2, math.lerp(0.1f, 1, x) * Log2Ni);
                var y = math.saturate(SmoothSample(p));

                // Output
                Output[i] = y;
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
