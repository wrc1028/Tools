Shader "Unlit/ShowVolume"
{
    Properties
    {
        _VolumeTex ("Volume Tex", 3D) = "black"{}
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
                half3 worldPos : TEXCOORD1;
                half4 vertex : SV_POSITION;
            };

            Texture3D<float4> _VolumeTex;
            SamplerState  sampler_VolumeTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half3 uv3D = half3(i.uv.x, i.worldPos.y, i.uv.y);
                half4 col = _VolumeTex.SampleLevel(sampler_VolumeTex, uv3D, 0);;
                return half4(col);
            }
            ENDCG
        }
    }
}
