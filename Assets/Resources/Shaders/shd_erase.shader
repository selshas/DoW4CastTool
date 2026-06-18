Shader "CFLTool/shd_eraser"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CursorPos("Cursor Pos", Vector) = (0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" } 
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float3 _CursorPos;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 fragPos = i.vertex;
                if (abs(length(fragPos - _CursorPos.xy)) > 32.0f)
                {
                    discard;
                }
                return float4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
