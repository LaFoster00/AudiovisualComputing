Shader "Custom/PianoRoll"
{
    Properties
    {
        BgColor ("Background Color", Color) = (0.2, 0.2, 0.2)
        LineColor ("Line Color", Color) = (1, 1, 1, 1)
        TimeLineColor ("Time Line Color", Color) = (0, 1, 0, 1)

        CellHeight ("Cell Height", Float) = 0.1
        CellWidth ("Cell Width", Float) = 0.1
        LineWidth ("Line Width", Float) = 0.2
        PositionX ("Position X", Float) = 0
        PositionY ("Position Y", Float) = 0

        CursorTime ("Time (Beat position of Marker)", Float) = 0

        NumberOfNotes ("Number of Lines", Integer) = 128
        NumberOfBars ("Number of Bars", Integer) = 4

        StepsPerBar ("Steps per Bar", Integer) = 16

    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalRenderPipeline"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"

            
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 BgColor;
            float4 LineColor;
            float4 TimeLineColor;

            float CellHeight;
            float CellWidth;
            float LineWidth;
            float PositionX;
            float PositionY;

            float CursorTime;

            int NumberOfNotes;
            int NumberOfBars;
            int StepsPerBar;

            float3 ObjectScale()
            {
                return float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                v.uv = float2(1 - v.uv.x, v.uv.y);
                o.uv = v.uv * 10 * ObjectScale().xy * (1 / float2(CellWidth, CellHeight));
                o.uv += float2(PositionX, PositionY);
                return o;
            }

            inline int2 CellCoordinate(float2 uv)
            {
                return int2(floor(uv.x), NumberOfNotes - 1 - floor(uv.y));
            }


            inline float GetVerticalLineWidth(float2 uv)
            {
                int2 lineCellCoordinate = CellCoordinate(uv + 0.5f);
                bool isQuaterLine = lineCellCoordinate.x % (StepsPerBar / 4) == 0;
                float verticalLineWidth = lerp(LineWidth * 0.4, LineWidth * 1, isQuaterLine);
                bool isBarLine = lineCellCoordinate.x % StepsPerBar == 0;
                verticalLineWidth = lerp(verticalLineWidth, LineWidth * 3, isBarLine);
                return verticalLineWidth;
            }

            inline bool NotInsideActiveGrid(int2 cellCoordinate)
            {
                return any(cellCoordinate >= int2(NumberOfBars * StepsPerBar, NumberOfNotes)) || any(
                    cellCoordinate < int2(0, 0));
            }

            half4 frag(v2f i) : SV_Target
            {
                const int2 cellCoordinate = CellCoordinate(i.uv);

                const float2 tiledCoordinate = (frac(i.uv) - 0.5) * 2;

                const float2 actualLineWidth = float2(GetVerticalLineWidth(i.uv)/CellWidth, LineWidth * 0.5 / CellHeight);
                float2 gridLine = smoothstep(
                    1 - actualLineWidth, 1 - actualLineWidth * 0.5,
                    abs(tiledCoordinate));
                const float isLine = max(gridLine.x, gridLine.y);

                float4 gridColor = lerp(BgColor, LineColor * NotInsideActiveGrid(cellCoordinate) ? 0.1 : 1, isLine);

                const float isTimeLine = (1 - smoothstep(0.05, 0.1, abs(i.uv.x - CursorTime)));
                return lerp(gridColor, TimeLineColor, isTimeLine);
            }
            ENDHLSL
        }
    }
}