Shader "Custom/SceneFade"
{
    Properties
    {
        _TransTex ("Texture", 2D) = "white" {}
        _Color("Color",Color)=(1,1,1,1)
        _Weight ("_Weight",Range(0,1))=0

        [ToggleOff] _HARDFADE("_HARDFADE", Int) = 1

    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        ZWrite Off //关闭深度写入
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma shader_feature _HARDFADE_OFF

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _TransTex;

            float4 _TransTex_ST;
            float4 _Color;
            float _Weight;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _TransTex);
                return o;
            }

            float4 circle(float2 uv, float2 pos, float rad, float3 color)
            {
                float d = length(pos - uv) - rad;
                float t = clamp(d, 0.0, 1.0);
                return float4(color, 1.0 - t);
            }
            

            float4 frag(v2f i) : SV_Target
            {
                float4 transCol = tex2D(_TransTex, i.uv);

                #if defined(_HARDFADE_OFF)

                return _Color * Smootherstep(transCol.r - _Weight, transCol.r, Pow4(_Weight));
                #else
                    if (_Weight<=0)
                    {
                        return 0;
                    }
                    return _Color * step(transCol.r, Pow4(_Weight));
                #endif
            }
            ENDHLSL
        }
    }
}