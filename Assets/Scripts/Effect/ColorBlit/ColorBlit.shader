Shader "Hidden/PostProcess/ColorBlit"
{
    Properties
    {

//        [HideInInspector]_MainTex ("Base (RGB)", 2D) = "white" {}

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
            Name"ColorBlitPass"

            HLSLPROGRAM
            #include "Assets/Shader/PostProcessing.hlsl"


            #pragma vertex Vert
            #pragma fragment frag

            CBUFFER_START(UnityPerMaterial)
                float _Intensity;
            CBUFFER_END

            float4 frag(Varyings input) :SV_Target
            {

                float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, input.texcoord);
                return color * float4(0,_Intensity,0,1);
            }
            ENDHLSL
        }

    }
}