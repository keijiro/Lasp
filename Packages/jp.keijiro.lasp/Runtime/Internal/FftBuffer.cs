using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Lasp
{
    //
    // Burst-optimized variant of the Cooley–Tukey FFT
    //
    sealed class FftBuffer : System.IDisposable
    {
        #region Public properties

        public int Width => _N;
        public NativeArray<float> Spectrum => _O;

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            if (_I.IsCreated) _I.Dispose();
            if (_O.IsCreated) _O.Dispose();
            if (_W.IsCreated) _W.Dispose();
            if (_P.IsCreated) _P.Dispose();
            if (_T.IsCreated) _T.Dispose();
        }

        #endregion

        #region Public methods

        public FftBuffer(int width)
        {
            _N = width;
            _logN = (int)math.log2(width);

            _I = PersistentMemory.New<float>(_N);
            _O = PersistentMemory.New<float>(_N / 2);

            InitializeWindow();
            BuildPermutationTable();
            BuildTwiddleFactors();
        }

        // Push audio data to the FIFO buffer.
        public void Push(NativeSlice<float> data)
        {
            var length = data.Length;

            if (length == 0) return;

            if (length < _N)
            {
                // The data is smaller than the buffer: Dequeue and copy
                var part = _N - length;
                NativeArray<float>.Copy(_I, _N - part, _I, 0, part);
                data.CopyTo(_I.GetSubArray(part, length));
            }
            else
            {
                // The data is larger than the buffer: Simple fill
                data.Slice(length - _N).CopyTo(_I);
            }
        }

        // Analyze the input buffer to calculate spectrum data.
        public void Analyze(float floor, float head)
        {
            using (var X = TempJobMemory.New<float4>(_N / 2))
            {
                // Bit-reversal permutation and first DFT pass
                new FirstPassJob { I = _I, W = _W, P = _P, X = X }.Run(_N / 2);

                // 2nd and later DFT passes
                for (var i = 0; i < _logN - 1; i++)
                {
                    var T_slice = new NativeSlice<TFactor>(_T, _N / 4 * i);
                    new DftPassJob { T = T_slice, X = X }.Run(_N / 4);
                }

                // Postprocess (power spectrum calculation)
                var O2 = _O.Reinterpret<float2>(sizeof(float));
                new PostprocessJob
                  { X = X, O = O2, DivN = 2.0f / _N,
                    DivR = 1 / (head - floor), F = floor}.Run(_N / 4);
            }
        }

        #endregion

        #region Hanning window function

        NativeArray<float> _W;

        void InitializeWindow()
        {
            _W = PersistentMemory.New<float>(_N);
            for (var i = 0; i < _N; i++)
                _W[i] = (1 - math.cos(2 * math.PI * i / (_N - 1))) / 2;
        }

        #endregion

        #region Private members

        readonly int _N;
        readonly int _logN;
        NativeArray<float> _I;
        NativeArray<float> _O;

        #endregion

        #region Bit-reversal permutation table

        NativeArray<int2> _P;

        void BuildPermutationTable()
        {
            _P = PersistentMemory.New<int2>(_N / 2);
            for (var i = 0; i < _N; i += 2)
                _P[i / 2] = math.int2(Permutate(i), Permutate(i + 1));
        }

        int Permutate(int x)
          => Enumerable.Range(0, _logN)
             .Aggregate(0, (a, i) => a += ((x >> i) & 1) << (_logN - 1 - i));

        #endregion

        #region Precalculated twiddle factors

        struct TFactor
        {
            public int2 I;
            public float2 W;

            public int i1 => I.x;
            public int i2 => I.y;

            public float4 W4
              => math.float4(W.x, math.sqrt(1 - W.x * W.x),
                             W.y, math.sqrt(1 - W.y * W.y));
        }

        NativeArray<TFactor> _T;

        void BuildTwiddleFactors()
        {
            _T = PersistentMemory.New<TFactor>((_logN - 1) * (_N / 4));

            var i = 0;
            for (var m = 4; m <= _N; m <<= 1)
            {
                var alpha = -2 * math.PI / m;
                for (var k = 0; k < _N; k += m)
                    for (var j = 0; j < m / 2; j += 2)
                        _T[i++] = new TFactor
                          { I = math.int2((k + j) / 2, (k + j + m / 2) / 2),
                            W = math.cos(alpha * math.float2(j, j + 1)) };
            }
        }

        #endregion

        #region First pass job

        [Unity.Burst.BurstCompile(CompileSynchronously = true)]
        struct FirstPassJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> I;
            [ReadOnly] public NativeArray<float> W;
            [ReadOnly] public NativeArray<int2> P;
            [WriteOnly] public NativeArray<float4> X;

            public void Execute(int i)
            {
                var i1 = P[i].x;
                var i2 = P[i].y;
                var a1 = I[i1] * W[i1];
                var a2 = I[i2] * W[i2];
                X[i] = math.float4(a1 + a2, 0, a1 - a2, 0);
            }
        }

        #endregion

        #region DFT pass job

        [Unity.Burst.BurstCompile(CompileSynchronously = true)]
        struct DftPassJob : IJobParallelFor
        {
            [ReadOnly] public NativeSlice<TFactor> T;
            [NativeDisableParallelForRestriction] public NativeArray<float4> X;

            static float4 Mulc(float4 a, float4 b)
              => a.xxzz * b.xyzw + math.float4(-1, 1, -1, 1) * a.yyww * b.yxwz;

            public void Execute(int i)
            {
                var t = T[i];
                var e = X[t.i1];
                var o = Mulc(t.W4, X[t.i2]);
                X[t.i1] = e + o;
                X[t.i2] = e - o;
            }
        }

        #endregion

        #region Postprocess Job

        [Unity.Burst.BurstCompile(CompileSynchronously = true)]
        struct PostprocessJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float4> X;
            [WriteOnly] public NativeArray<float2> O;
            public float DivN;
            public float DivR;
            public float F;

            public void Execute(int i)
            {
                var x = X[i];
                var l = math.float2(math.length(x.xy), math.length(x.zw));
                O[i] = (MathUtils.dBFS(l * DivN) - F) * DivR;
            }
        }

        #endregion
    }
}
