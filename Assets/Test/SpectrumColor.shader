Shader "Lasp/SpectrumColor"
{
    Properties
    {
        _MainTex("Texture", 2D) = "black" {}
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "UnityCG.cginc"

            float3 Hue2RGB(float hue)
            {
                float h = frac(hue) * 6 - 2;
                float r = abs(h - 1) - 1;
                float g = 2 - abs(h);
                float b = 2 - abs(h - 2);
                return saturate(float3(r, g, b));
            }

            sampler2D _MainTex;

            void Vertex(
              float4 vertex : POSITION,
              float2 uv : TEXCOORD,
              out float4 out_vertex : SV_Position,
              out float2 out_uv : TEXCOORD
            )
            {
                out_vertex = UnityObjectToClipPos(vertex);
                out_uv = uv;
            }

            float4 Fragment(
              float4 vertex : SV_Position,
              float2 uv : TEXCOORD
            ) : SV_Target
            {
                float r = tex2D(_MainTex, uv).r;
                float3 rgb = Hue2RGB(lerp(-0.3, 0.3, r));
                rgb = rgb * saturate(r * 2) + saturate(r * 2 - 1);
                return float4(rgb, 1);
            }

            ENDCG
        }
    }
}
