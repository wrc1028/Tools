Shader "Unlit/CombineTex"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _AddTex ("Add Tex", 2D) = "white" {}
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
            sampler2D _AddTex;
            float _GammaValue;
            float4 _ChannelMask;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            // 普通合并
            half4 frag_Normal (v2f i) : SV_Target
            {
                half4 mainColor = tex2D(_MainTex, i.uv);
                half4 addColor = tex2D(_AddTex, i.uv) * _ChannelMask;
                half3 finalColor = pow(lerp(mainColor, addColor, addColor.a), _GammaValue);
                half finalAlpha = min(1, mainColor.a + addColor.a);
                return half4(finalColor, finalAlpha);
            }
            half4 frag_Multiply (v2f i) : SV_Target
            {
                half4 mainColor = tex2D(_MainTex, i.uv);
                half4 addColor = tex2D(_AddTex, i.uv) * _ChannelMask;
                half3 finalColor = pow((mainColor * addColor).rgb, _GammaValue);
                half finalAlpha = 1;
                return half4(finalColor, finalAlpha);
            }
            half4 frag_ChannelCombine (v2f i) : SV_Target
            {
                half4 mainColor = tex2D(_MainTex, i.uv);
                half4 addColor = tex2D(_AddTex, i.uv) * _ChannelMask;
                half3 finalColor = pow((mainColor + addColor).rgb, _GammaValue);
                half finalAlpha = 1;
                return half4(finalColor, finalAlpha);
            }
        ENDCG
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_Normal
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_Multiply
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_ChannelCombine
            ENDCG
        }
    }
}
