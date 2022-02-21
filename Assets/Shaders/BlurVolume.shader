Shader "Unlit/BlurVolume"
{
   Properties
    {
        _VolumeTex ("3D Texture", 3D) = "black"{}
        _BlurRadius ("Blur Radius", range(0, 2)) = 0.01
        _SliceValue ("Slice Value", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Zwrite off
            Blend One OneMinusSrcAlpha
		    Cull Off
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

            Texture3D<float4> _VolumeTex;
            SamplerState  sampler_VolumeTex;
            float _BlurRadius;
            float _SliceValue;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }
            half3 UV3DOffset(half3 _uv3D, half3 _offset, half _radius)
            {
                return _uv3D + _offset * _radius;
            }

            half4 frag (v2f i) : SV_Target
            {
                half3 uv3D = half3(i.uv.x, _SliceValue, i.uv.y);
                half4 finalColor = half4(0, 0, 0, 1);
                for (int x = -1; x < 2; x++)
                        for (int y = -1; y < 2; y++)
                            for (int z = -1; z < 2; z++)
                            {
                                half3 uv = UV3DOffset(uv3D, half3(x, y, z), _BlurRadius);
                                finalColor += _VolumeTex.SampleLevel(sampler_VolumeTex, uv, 0);
                            }
                // half3 colorValue = _VolumeTex.SampleLevel(sampler_VolumeTex, uv3D, 0);
                finalColor /= 27.f;
                return half4(finalColor.xyz, 1);
            }
            ENDCG
        }
    }
}
