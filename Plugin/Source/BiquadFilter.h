#pragma once

// A simple implementation of a biquad IIR filter
// based on the EarLevel Engineering blog article.
// http://www.earlevel.com/main/2012/11/26/biquad-c-source-code/

#include <cmath>

namespace Lasp
{
    class BiquadFilter
    {
    public:

        BiquadFilter() : z1_(0), z2_(0) {}

        void setLowpass(float Fc, float Q)
        {
            auto K = tanf(PI_ * Fc);
            auto norm = 1 / (1 + K / Q + K * K);
            a0_ = K * K * norm;
            a1_ = 2 * a0_;
            a2_ = a0_;
            b1_ = 2 * (K * K - 1) * norm;
            b2_ = (1 - K / Q + K * K) * norm;
        }

        void setBandpass(float Fc, float Q)
        {
            auto K = tanf(PI_ * Fc);
            auto norm = 1 / (1 + K / Q + K * K);
            a0_ = K / Q * norm;
            a1_ = 0;
            a2_ = -a0_;
            b1_ = 2 * (K * K - 1) * norm;
            b2_ = (1 - K / Q + K * K) * norm;
        }

        void setHighpass(float Fc, float Q)
        {
            auto K = tanf(PI_ * Fc);
            auto norm = 1 / (1 + K / Q + K * K);
            a0_ = norm;
            a1_ = -2 * a0_;
            a2_ = a0_;
            b1_ = 2 * (K * K - 1) * norm;
            b2_ = (1 - K / Q + K * K) * norm;
        }

        float feedSample(float i)
        {
            auto o = i * a0_ + z1_;
            z1_ = i * a1_ + z2_ - b1_ * o;
            z2_ = i * a2_ - b2_ * o;
            return o;
        }

    private:

        const float PI_ = 3.14159265358979323846f;

        float a0_, a1_, a2_, b1_, b2_;
        float z1_, z2_;
    };
}
