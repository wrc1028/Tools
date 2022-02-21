Shader "Unlit/AlphaToDither"
{
    Properties
    {
        _NormalSampleTex ("Normal Sample Tex", 2D) = "white" {}
        _LowSampleTex ("Low Sample Tex", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                half2 texcoord : TEXCOORD0;
                half4 vertex : POSITION;
            };

            struct v2f
            {
                half2 uv : TEXCOORD0;
                half4 vertex : SV_POSITION;
            };

            sampler2D _NormalSampleTex;
            sampler2D _LowSampleTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 color = tex2D(_NormalSampleTex, i.uv);
                half4 dither = tex2D(_LowSampleTex, i.uv);
                half alpha = 0;
                if (1 >= color.a && color.a > 0.8) alpha = 1;
                else if (0.8 >= color.a && color.a > 0.6) alpha = dither.a;
                else if (0.6 >= color.a && color.a > 0.4) alpha = dither.b;
                else if (0.4 >= color.a && color.a > 0.2) alpha = dither.g;
                else if (0.2 >= color.a && color.a > 0) alpha = dither.r;

                alpha = dither.r;
                clip(alpha - 0.5);
                return half4(color.rgb, alpha);
            }
            ENDCG
        }
    }
}
