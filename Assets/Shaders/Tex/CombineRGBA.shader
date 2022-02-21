Shader "Unlit/CombineRGBA"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RTex ("R Channel", 2D) = "white" {}
        _GTex ("G Channel", 2D) = "white" {}
        _BTex ("B Channel", 2D) = "white" {}
        _ATex ("A Channel", 2D) = "white" {}
        _GammaValue ("Gamma Value", float) = 0.45454545
        _ChannelMask ("Channel Mask", vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        CGINCLUDE
        #include "UnityCG.cginc"

            struct appdata
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _RTex;
            sampler2D _GTex;
            sampler2D _BTex;
            sampler2D _ATex;
            float _GammaValue;
            float4 _ChannelMask;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            half4 frag_FourTex (v2f i) : SV_Target
            {
                
                half RColor = tex2D(_RTex, i.uv).r;
                half GColor = tex2D(_GTex, i.uv).g;
                half BColor = tex2D(_BTex, i.uv).b;
                half Alpha = tex2D(_ATex, i.uv).a;
                half3 color = pow(half3(RColor, GColor, BColor), _GammaValue);
                return half4(color, Alpha);
            }
            half4 frag_TwoTex(v2f i) : SV_Target
            {
                half4 firstTex = tex2D(_RTex, i.uv) * _ChannelMask;
                half4 secondTex = tex2D(_GTex, i.uv) * (1 - _ChannelMask);
                half3 finalColor = pow((firstTex + secondTex).rgb, _GammaValue);
                half finalAlpha = firstTex.a + secondTex.a;
                return half4(finalColor, finalAlpha);
            }
            half4 frag_OneTex(v2f i) : SV_Target
            {
                half4 finalColor = pow(tex2D(_RTex, i.uv), _GammaValue) * _ChannelMask;
                return finalColor;
            }
        ENDCG
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_FourTex
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_TwoTex
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_OneTex
            ENDCG
        }
    }
}
