Shader "Unlit/VertexColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                half4 color : COLOR;
            };

            struct v2f
            {
                half2 uv : TEXCOORD0;
                half4 vertex : SV_POSITION;
                half4 vertexColor : TEXCOORD1;
            };

            sampler2D _MainTex;
            half4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.vertexColor = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv) * i.vertexColor;
                return half4(col);
            }
            ENDCG
        }
    }
}
