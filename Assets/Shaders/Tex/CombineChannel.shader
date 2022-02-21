Shader "Unlit/CombineChannel"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _AddTex ("Add Tex", 2D) = "white" {}
        _GammaValue ("Gamma Value", float) = 0.45454545
        _ChannelMask ("Channel Mask", vector) = (0, 0, 0, 0)

        _RChannelTex ("R Channel Tex", 2D) = "white" {}
        _GChannelTex ("G Channel Tex", 2D) = "white" {}
        _BChannelTex ("B Channel Tex", 2D) = "white" {}
        _AChannelTex ("A Channel Tex", 2D) = "white" {}
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
        sampler2D _AddTex;
        float _GammaValue;
        float4 _ChannelMask;

        sampler2D _RChannelTex;
        sampler2D _GChannelTex;
        sampler2D _BChannelTex;
        sampler2D _AChannelTex;

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
            return o;
        }
        half4 frag_ChannelCombine (v2f i) : SV_Target
        {
            half4 mainColor = tex2D(_MainTex, i.uv) * (1 - _ChannelMask);
            half4 addColor = tex2D(_AddTex, i.uv) * _ChannelMask;
            half3 finalColor = min(half3(1, 1, 1), (mainColor + addColor).rgb);
            half finalAlpha = min(1, mainColor.a + addColor.a);
            return half4(finalColor, finalAlpha);
        }
        half4 frag_Gamma (v2f i) : SV_Target
        {
            half4 mainColor = tex2D(_MainTex, i.uv);
            return half4(pow(mainColor.rgb, _GammaValue), mainColor.a);
        }
        half4 frag_RChannel (v2f i) : SV_Target
        {
            half4 mainColor = tex2D(_MainTex, i.uv);
            return half4(mainColor.rrr, 1);
        }
        half4 frag_GChannel (v2f i) : SV_Target
        {
            half4 mainColor = tex2D(_MainTex, i.uv);
            return half4(mainColor.ggg, 1);
        }
        half4 frag_BChannel (v2f i) : SV_Target
        {
            half4 mainColor = tex2D(_MainTex, i.uv);
            return half4(mainColor.bbb, 1);
        }
        half4 frag_AChannel (v2f i) : SV_Target
        {
            half4 mainColor = tex2D(_MainTex, i.uv);
            return half4(mainColor.aaa, 1);
        }
        half4 frag_ExchangeChannel (v2f i) : SV_Target
        {
            half RColor = tex2D(_RChannelTex, i.uv).r;
            half GColor = tex2D(_GChannelTex, i.uv).r;
            half BColor = tex2D(_BChannelTex, i.uv).r;
            half Alpha = tex2D(_AChannelTex, i.uv).r;
            return half4(RColor, GColor, BColor, Alpha);
        }
        ENDCG
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_ChannelCombine
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_RChannel
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_GChannel
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_BChannel
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_AChannel
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_ExchangeChannel
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_Gamma
            ENDCG
        }
    }
}
