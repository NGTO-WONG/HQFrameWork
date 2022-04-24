Shader "Custom/LSpinner"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.position.xy / _ScreenParams.y;

                float2 p = uv - float2(.87, .5);
                float time = _Time.y * 1.5;

                float angle = -(time - sin(time + UNITY_PI) * cos(time)) - time * .95;
                float2x2 rot = float2x2(cos(angle), sin(angle), -sin(angle), cos(angle));
                p = mul(p,rot);

                float3 col = float3(0,0,0);
                float L = length(p);
                float f = 0.;

                f = smoothstep(L - .005, L, .35);
                f -= smoothstep(L, L + 0.005, .27);
                //f = step(sin(L * 200. + iTime * p.x)*.5+.5,.25); // uncomment for a headache

                float t = fmod(time, UNITY_TWO_PI) - UNITY_PI;
                float t1 = -UNITY_PI;
                float t2 = sin(t) * (UNITY_PI - .25);

                float a = atan2(p.x,p.y);
                f = f * step(a, t2) * (1. - step(a, t1));


                col = lerp(col, float3(cos(time), cos(time + UNITY_TWO_PI / 3.), cos(time + 2. * UNITY_TWO_PI / 3.)), f);


                float4 fragColor = float4(col, 1.0);
                return fragColor;
            }
            ENDCG
        }
    }
}