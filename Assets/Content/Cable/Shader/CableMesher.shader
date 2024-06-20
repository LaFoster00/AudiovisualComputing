// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CableMesher"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _CableRadius ("Radius", Float) = 0.1
        _CableSegments ("Segments", Int) = 8
    }
    SubShader
    {
        Name "ForwardLit"
        Tags
        {
            "RenderType"="Opaque"
            "LightMode" = "UniversalForward"
        }
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 vertex : POSITION;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float4 _BaseColor;
            float _CableRadius;
            int _CableSegments;

            v2g vert(appdata v)
            {
                v2g o;
                o.vertex = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(2)]
            void geom(triangle v2g p[3], inout LineStream<g2f> outputStream)
            {
                float4x4 identity = float4x4(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1);
             
                g2f OUT;
             
                float4 pos0 = mul(UNITY_MATRIX_VP, p[0].vertex);
                float4 pos1 = mul(UNITY_MATRIX_VP, p[1].vertex);
                float4 pos2 = mul(UNITY_MATRIX_VP, p[2].vertex);

                // Check if there is a next element to connect to
                if (all(pos1 == pos2))
                    return;
                
                OUT.vertex = pos1;
                OUT.color = half4(1,0,0,1);  
                outputStream.Append(OUT);
             
                OUT.vertex = pos2;
                OUT.color = half4(0,1,0,1);
                outputStream.Append(OUT);
             
                //outputStream.RestartStrip();
                
            }

            float4 frag(g2f i) : SV_Target
            {
                return float4(i.color.rgb, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}