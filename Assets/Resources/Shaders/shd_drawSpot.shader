Shader "CFLTool/shd_drawSpot"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _CursorPos("Cursor Pos", Vector) = (0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" } 
        LOD 100

        BLEND SrcAlpha OneMinusSrcAlpha

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
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 fragPos = i.vertex;
                if (abs(length(fragPos - _CursorPos.xy)) < 4.0f)
                {
                    return _Color;
                }
                return float4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
