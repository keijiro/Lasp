#pragma once

// A simplified version of the LASP driver specialized for loopback.

#include "Debug.h"
#include "RingBuffer.h"
#include "BiquadFilter.h"

namespace Lasp
{
    class Driver
    {
    public:

        // Thru driver implementation

        Driver() {}
        ~Driver() {}

        bool OpenStream() { return true; }
        void CloseStream() {}

        float getSampleRate() const
        {
            return getAndUpdateSampleRate();
        }

        const RingBuffer& getBuffer(int index) const
        {
            return getSharedBuffer(index);
        }

        // Loopback buffer interface

        static void updateSampleRate(float rate)
        {
            if (rate == getAndUpdateSampleRate(rate)) return;

            // Reinitialize the filter bank.
            auto fc = 960.0f / rate;
            getSharedFilter(0).setLowpass(fc, 0.15f);
            getSharedFilter(1).setLowpass(fc, 0.15f);
            getSharedFilter(2).setBandpass(fc, 0.15f);
            getSharedFilter(3).setBandpass(fc, 0.15f);
            getSharedFilter(4).setHighpass(fc, 0.15f);
            getSharedFilter(5).setHighpass(fc, 0.15f);
        }

        static void feedSample(float input)
        {
            auto& buffer_raw = getSharedBuffer(0);
            auto& buffer_lpf = getSharedBuffer(1);
            auto& buffer_bpf = getSharedBuffer(2);
            auto& buffer_hpf = getSharedBuffer(3);

            auto& lpf1 = getSharedFilter(0);
            auto& lpf2 = getSharedFilter(1);
            auto& bpf1 = getSharedFilter(2);
            auto& bpf2 = getSharedFilter(3);
            auto& hpf1 = getSharedFilter(4);
            auto& hpf2 = getSharedFilter(5);

            buffer_raw.pushFrame(input);
            buffer_lpf.pushFrame(lpf2.feedSample(lpf1.feedSample(input)));
            buffer_bpf.pushFrame(bpf2.feedSample(bpf1.feedSample(input)));
            buffer_hpf.pushFrame(hpf2.feedSample(hpf1.feedSample(input)));
        }

    private:

        static float getAndUpdateSampleRate(float updateValue = 0)
        {
            static float currentValue;
            auto ret = currentValue;
            if (updateValue != 0 && updateValue != currentValue)
                currentValue = updateValue;
            return ret;
        }

        // Three-band filter bank.
        // We use two filters per band to get 24db/oct slope.
        // The filters are assigned in this order: [LPF1, LPF2, BPF1, BPF2, HPF1, HPF2]
        static BiquadFilter& getSharedFilter(int index)
        {
            static std::array<BiquadFilter, 6> filters;
            return filters[index];
        }

        // Ring buffers used for storing filtered results.
        // The buffers are assigned in this order: [non-filtered, low, middle, high]
        static RingBuffer& getSharedBuffer(int index)
        {
            static std::array<RingBuffer, 4> buffers;
            return buffers[index];
        }
    };
}
