#pragma once

#include <array>

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

        size_t size()
        {
            return bufferSize_;
        }

        void pushFrame(float value)
        {
            buffer_[index_] = value;
            index_ = (index_ + 1) & indexMask_;
        }

        float getPeak(std::size_t range)
        {
            range = min(range, bufferSize_);
            auto i = index_ + bufferSize_ - range;
            auto peak = .0f;
            for (auto offs = 0u; offs < range; offs++)
                peak = std::fmaxf(peak, std::fabsf(buffer_[(i + offs) & indexMask_]));
            return peak;
        }

        float getRMS(std::size_t range)
        {
            range = min(range, bufferSize_);
            auto i = index_ + bufferSize_ - range;
            auto sq = .0f;
            for (auto offs = 0u; offs < range; offs++)
            {
                auto v = buffer_[(i + offs) & indexMask_];
                sq += v * v;
            }
            return std::sqrtf(sq / range);
        }

        void clear()
        {
            buffer_.fill(0);
            index_ = 0;
        }

        void copyRecentFrames(float* dest, std::size_t length)
        {
            length = min(length, bufferSize_);
            auto i = index_ + bufferSize_ - length;
            for (auto offs = 0u; offs < length; offs++)
                dest[offs] = buffer_[(i + offs) & indexMask_];
        }

    private:

        static const size_t bufferSize_ = 1024;
        static const size_t indexMask_ = bufferSize_ - 1;

        std::array<float, bufferSize_> buffer_;
        std::size_t index_;
    };
}