Shader "Unlit/SliceShader"
{
    Properties
    {
        _FrontColor ("Front Color", Color) = (0, 0, 0, 1)
        _BackColor ("Back Color", Color) = (1, 1, 1, 1)
        _SliceValue ("Slice Value", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull off
            AlphaToMask on
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
                half3 worldPos : TEXCOORD1;
                half4 vertex : SV_POSITION;
            };

            float4 _FrontColor;
            float4 _BackColor;
            float _SliceValue;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            half4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                half clipValue = -i.worldPos.y + _SliceValue;
                clip(clipValue);
                half4 finalColor = step(0, facing) ? _FrontColor : _BackColor;
                return half4(finalColor);
            }
            ENDCG
        }
    }
}
