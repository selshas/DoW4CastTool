Shader "CFLTool/shd_multiplyOpacity"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _DestTex("Texture", 2D) = "white" {}
        _Opacity ("Opacity", float) = 1.0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _DestTex;
            float4 _DestTex_ST;

            float _Opacity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 src = tex2D(_MainTex, i.uv);
                src.a *= _Opacity;

                fixed4 dest = tex2D(_DestTex, i.uv);

                return float4(
                    src.rgb * src.a + dest.rgb * (1 - src.a), 
                    1.0f
                );
            }
            ENDCG
        }
    }
}
