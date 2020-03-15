using Unity.Mathematics;

namespace Lasp
{
    //
    // Three-band (+ bypass) filter with a biquad IIR filter
    // based on the EarLevel Engineering blog article.
    // http://www.earlevel.com/main/2012/11/26/biquad-c-source-code/
    //
    struct MultibandFilter
    {
        float4 _a0, _a1, _a2;
        float  _b1, _b2;
        float4 _z1, _z2;

        static readonly float4 _xmask = math.float4(0, 1, 1, 1);

        public void SetParameter(float Fc, float Q)
        {
            var K = math.tan((float)math.PI * Fc);
            var norm = 1 / (1 + K / Q + K * K);

            // x: Bypass
            _a0.x = 1;
            _a1.x = 0;
            _a2.x = 0;

            // y: Lowpass
            _a0.y = K * K * norm;
            _a1.y = 2 * _a0.y;
            _a2.y = _a0.y;

            // z: Bandpass
            _a0.z = K / Q * norm;
            _a1.z = 0;
            _a2.z = -_a0.z;

            // w: Highpass
            _a0.w = norm;
            _a1.w = -2 * _a0.w;
            _a2.w = _a0.w;

            _b1 = 2 * (K * K - 1) * norm;
            _b2 = (1 - K / Q + K * K) * norm;
        }

        public float4 FeedSample(float i)
        {
            var o = _a0 * i + _z1 * _xmask;
            _z1 = _a1 * i + _z2 - o * _b1;
            _z2 = _a2 * i - o * _b2;
            return o;
        }
    }
}
