Shader "Custom/PianoRoll_UI"
{
    Properties
    {
        _MainTex("Main Tex (Unused)", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        Aspect ("Aspect Ratio", float) = 1

        BgColor ("Background Color", Color) = (0.2, 0.2, 0.2, 1)
        LineColor ("Line Color", Color) = (1, 1, 1, 1)
        TimeLineColor ("Time Line Color", Color) = (0, 1, 0, 1)

        CellHeight ("Cell Height", Float) = 0.1
        CellWidth ("Cell Width", Float) = 0.1
        LineWidth ("Line Width", Float) = 0.2
        PositionX ("Position X", Float) = 0
        PositionY ("Position Y", Float) = 0

        CursorTime ("Time (Beat position of Marker)", Float) = 0

        NumberOfNotes ("Number of Lines", Int) = 128
        NumberOfBars ("Number of Bars", Int) = 4

        StepsPerBar ("Steps per Bar", Int) = 16

        // UI-specific properties
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            
            fixed4 _Color;
            float4 _ClipRect;

            float4 BgColor;
            float4 LineColor;
            float4 TimeLineColor;

            float Aspect;
            
            float CellHeight;
            float CellWidth;
            float LineWidth;
            float PositionX;
            float PositionY;

            float CursorTime;

            int NumberOfNotes;
            int NumberOfBars;
            int StepsPerBar;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                
                // Bring uv into right orientation
                OUT.texcoord = float2((IN.texcoord.x) * Aspect, 1 - IN.texcoord.y);
                // Scale it according to cell sizes
                OUT.texcoord *= (1 / float2(CellWidth, CellHeight));
                // Add the position offset
                OUT.texcoord += float2(PositionX, PositionY);
                
                OUT.color = IN.color * _Color;
                return OUT;
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

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                const int2 cellCoordinate = CellCoordinate(uv);
                const float2 tiledCoordinate = (frac(uv) - 0.5) * 2;

                const float2 actualLineWidth = float2(GetVerticalLineWidth(uv) / CellWidth,
                                                                  LineWidth * 0.5 / CellHeight);
                float2 gridLine = smoothstep(1 - actualLineWidth, 1 - actualLineWidth * 0.5, abs(tiledCoordinate));
                const float isLine = max(gridLine.x, gridLine.y);

                float4 gridColor = lerp(BgColor, LineColor * (NotInsideActiveGrid(cellCoordinate) ? 0.1 : 1), isLine);

                const float isTimeLine = (1 - smoothstep(0.05, 0.1, abs(uv.x - CursorTime)));
                float4 color = float4(lerp(gridColor, TimeLineColor, isTimeLine).xyz, 1);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color * IN.color;
            }
            ENDCG
        }
    }
}