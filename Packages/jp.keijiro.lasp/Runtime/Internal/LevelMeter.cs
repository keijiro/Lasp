using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Lasp
{
    //
    // Audio level meter class with low/mid/high filter bank
    //
    sealed class LevelMeter
    {
        #region Public properties

        public float4 GetLevel(int channel) => _levels[channel];
        public float SampleRate { get; set; }

        #endregion

        #region Internal state

        float4 [] _levels;
        MultibandFilter [] _filters;

        #endregion

        #region Public methods

        public LevelMeter(int channels)
        {
            _levels = new float4 [channels];
            _filters = new MultibandFilter [channels];
        }

        public unsafe void ProcessAudioData(ReadOnlySpan<float> input)
        {
            if (input.Length == 0) return;

            // This function is jobified only for the purpose of using the
            // Burst compiler. We don't need to parallelize it because it will
            // end up with adding extra cost without making any performance
            // gain. We rather do it on the main thread as a one-shot job.

            fixed (float* pInput = &input.GetPinnableReference())
            fixed (MultibandFilter* pFilter = &_filters[0])
            fixed (float4* pOutput = &_levels[0])
            {
                new AudioProcessJob
                  { Input    = pInput,
                    Length   = input.Length,
                    Channels = _levels.Length,
                    Filters  = pFilter,
                    FilterFc = 960.0f / SampleRate,
                    FilterQ  = 0.15f,
                    Output   = pOutput }.Run();
            }
        }

        #endregion

        #region Signal processing job

        [Unity.Burst.BurstCompile]
        unsafe struct AudioProcessJob : IJob
        {
            [NativeDisableUnsafePtrRestriction]
            public float* Input;
            public int Length, Channels;

            [NativeDisableUnsafePtrRestriction]
            public MultibandFilter* Filters;
            public float FilterFc, FilterQ;

            [NativeDisableUnsafePtrRestriction]
            public float4* Output;

            public void Execute()
            {
                var sum = stackalloc float4 [Channels];

                // Pre-loop initialization
                for (var ch = 0; ch < Channels; ch++)
                {
                    Filters[ch].SetParameter(FilterFc, FilterQ);
                    sum[ch] = float4.zero;
                }

                // Squared sum
                for (var offs = 0; offs < Length;)
                    for (var ch = 0; ch < Channels; ch++, offs++)
                    {
                        var vf = Filters[ch].FeedSample(Input[offs]);
                        sum[ch] += vf * vf;
                    }

                // RMS
                var rsteps = math.rcp(Length / Channels);
                for (var ch = 0; ch < Channels; ch++)
                    Output[ch] = math.sqrt(sum[ch] * rsteps);
            }
        }

        #endregion
    }
}
