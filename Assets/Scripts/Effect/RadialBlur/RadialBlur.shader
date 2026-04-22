Shader "Hidden/PostProcess/RadialBlur"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200


        Pass
        {
            Name "RadialBlurPass"

            HLSLPROGRAM
            #include "Assets/Shader/PostProcessing.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            CBUFFER_START(UnityPerMaterial)
                float4 _RadialBlurParams;
            CBUFFER_END

            float4 Frag(Varyings input):SV_Target
            {
                float2 uv = input.texcoord;
                
                float BlurRadius = _RadialBlurParams.x;
                int Iteration = (int)floor(_RadialBlurParams.y);
                float2 RadialCenter = _RadialBlurParams.zw;
                
                float2 blurVector = (RadialCenter - uv) * BlurRadius;

                real4 acumulateColor = half4(0, 0, 0, 0);

                UNITY_UNROLLX(30)
                for (int j = 0; j < Iteration; j++)
                {
                    acumulateColor += SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, uv);
                    uv += blurVector;
                }
                
                real4 color = acumulateColor/Iteration;

                return color;
            }
            ENDHLSL
        }

    }
}