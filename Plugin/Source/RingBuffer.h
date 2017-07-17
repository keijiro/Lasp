#pragma once

// Ring buffer class used for storing filtered waveform.
//
// Note on thread safety: This class doesn't do any thread synchronization.
// Although the audio thread may write to the buffer while Unity thread is
// reading it, it's okay if the size of the buffer is longer enough than
// read/write length.

#include <array>
#include <algorithm>
#include <cmath>

namespace Lasp
{
    class RingBuffer
    {
    public:

        RingBuffer()
        {
            static_assert((bufferSize_ & indexMask_) == 0, "Buffer size should be a power of two.");
            clear();
        }

        constexpr size_t getSize() const
        {
            return bufferSize_;
        }

        void pushFrame(float value)
        {
            buffer_[index_] = value;
            index_ = (index_ + 1) & indexMask_;
        }

        float getPeakLevel(std::size_t range) const
        {
            range = std::min(range, getSize());
            auto base = index_ + bufferSize_ - range;
            auto peak = .0f;
            for (auto offs = 0u; offs < range; offs++)
                peak = std::fmaxf(peak, std::fabsf(buffer_[(base + offs) & indexMask_]));
            return peak;
        }

        float calculateRMS(std::size_t range) const
        {
            range = std::min(range, getSize());
            auto base = index_ + bufferSize_ - range;
            auto sq = .0f;
            for (auto offs = 0u; offs < range; offs++)
            {
                auto v = buffer_[(base + offs) & indexMask_];
                sq += v * v;
            }
            return std::sqrtf(sq / range);
        }

        void clear()
        {
            buffer_.fill(0);
            index_ = 0;
        }

        void copyRecentFrames(float* dest, std::size_t length) const
        {
            length = std::min(length, getSize());
            auto base = index_ + bufferSize_ - length;
            for (auto offs = 0u; offs < length; offs++)
                dest[offs] = buffer_[(base + offs) & indexMask_];
        }

    private:

        enum { bufferSize_ = 2048, indexMask_ = bufferSize_ - 1 };

        std::array<float, bufferSize_> buffer_;
        std::size_t index_;
    };
}
