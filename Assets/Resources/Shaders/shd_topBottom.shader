Shader "CFLTool/shd_topBottom"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BottomTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        CULL OFF
        BLEND SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex_clip : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float3 vertex_world : TEXCOORD1;
                UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _BottomTex;
            float4 _BottomTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex_clip = UnityObjectToClipPos(v.vertex);
                o.normal = mul(transpose(unity_WorldToObject), v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertex_world = mul(unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 viewDir = normalize(_WorldSpaceCameraPos - i.vertex_world.xyz);
                // sample the texture
                fixed4 col;
                fixed ndotv = dot(i.normal, viewDir);
                if (ndotv > 0)
                {
                    col = tex2D(_MainTex, i.uv);
                }
                else
                {
                    col = tex2D(_BottomTex, fixed2(-i.uv.x, i.uv.y));
                }

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
