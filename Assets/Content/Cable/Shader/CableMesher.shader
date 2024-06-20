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
                float3 normal : NORMAL;
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

            float4x4 rotationMatrix(float3 axis, float angle)
            {
                axis = normalize(axis);
                float s = sin(angle);
                float c = cos(angle);
                float oc = 1.0 - c;

                return float4x4(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s,
                                 oc * axis.z * axis.x + axis.y * s, 0.0,
                                 oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c,
                                 oc * axis.y * axis.z - axis.x * s, 0.0,
                                 oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s,
                                 oc * axis.z * axis.z + c, 0.0,
                                 0.0, 0.0, 0.0, 1.0);
            }

            float3 rotateVector(float3 direction, float angle, float3 axis)
            {
                float4x4 rotation = rotationMatrix(axis, angle);
                return mul(rotation, float4(direction, 1.0)).xyz;
            }

            uint repeat(int value, uint min, uint max)
            {
                value %= max - min;
                value += min;
                return value;
            }

            [maxvertexcount(48)]
            void geom(triangle v2g p[3], inout TriangleStream<g2f> outputStream)
            {
                float4x4 vp = UNITY_MATRIX_VP;

                g2f OUT;
                OUT.color = float4(1, 1, 1, 0);

                float4x4 identity = float4x4(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1);

                float4 pos0 = p[0].vertex;
                float4 pos1 = p[1].vertex;
                float4 pos2 = p[2].vertex;

                float4 halfPos0 = (pos0 + pos1) / 2;
                float4 halfPos1 = (pos1 + pos2) / 2;

                #if false
                OUT.vertex = UnityObjectToClipPos(mul(unity_WorldToObject, halfPos0));
                outputStream.Append(OUT);
                OUT.vertex = UnityObjectToClipPos(mul(unity_WorldToObject, pos1));
                outputStream.Append(OUT);
                //outputStream.Append(OUT);
                OUT.vertex = UnityObjectToClipPos(mul(unity_WorldToObject, halfPos1));
                outputStream.Append(OUT);
                return;
                #endif

                float3 lineDir0 = normalize(pos1.xyz - pos0.xyz);
                float3 lineDir1 = normalize(pos2.xyz - pos1.xyz);
                float3 lineDir = normalize(lineDir0 + lineDir1);

                float3 tangent0 = normalize(cross(lineDir0, float3(0, 1, 0)));

                float3 tangent1 = normalize(cross(lineDir1, float3(0, 1, 0)));

                float3 tangent = normalize(tangent0 + tangent1);

                float cableRadius = 0.002;

                float3 normals[12];
                const float angleStep = UNITY_TWO_PI / 4;
                // Normals for vertices around line 1
                for (int i = 0; i < 12; i++)
                {
                    float3 direction;
                    float3 axis;
                    if (i < 4)
                    {
                        direction = tangent0;
                        axis = lineDir0;
                    }
                    else if (i < 8)
                    {
                        direction = tangent;
                        axis = lineDir;
                    }
                    else
                    {
                        direction = tangent1;
                        axis = lineDir1;
                    }

                    normals[i] = rotateVector(direction, angleStep * (i + 1), axis);
                }

                float4 vertices[12];
                for (int i = 0; i < 12; i++)
                {
                    float3 startPos;
                    if (i < 4)
                    {
                        startPos = halfPos0;
                    }
                    else if (i < 8)
                    {
                        startPos = pos1;
                    }
                    else
                    {
                        startPos = halfPos1;
                    }
                    vertices[i] = float4(startPos + normals[i] * cableRadius, 1.0);
                }

                for (int i = 0; i < 12; ++i)
                {
                    vertices[i] = UnityObjectToClipPos(mul(unity_WorldToObject, vertices[i]));
                }

                for (int i = 0; i < 12; ++i)
                {
                    normals[i] = UnityObjectToClipPos(normals[i]);
                }

                //// Triangulation

                // The index of the first triangle on the bottom triangle row
                int bottomIndex = 0;
                // The index of the first triangle on the top triangle row
                int topIndex = 4;
                // Triangles for first part of the cable sleeve
                for (int s1 = 0; s1 < 4; ++s1)
                {
                    // First triangle
                    OUT.vertex = vertices[bottomIndex];
                    OUT.normal = normals[bottomIndex++];
                    OUT.color = float4(1, 0, 0, 1);
                    bottomIndex = repeat(bottomIndex, 0, 4);
                    outputStream.Append(OUT);

                    OUT.vertex = vertices[bottomIndex];
                    OUT.normal = normals[bottomIndex];
                    OUT.color = float4(0, 1, 0, 1);
                    outputStream.Append(OUT);

                    OUT.vertex = vertices[topIndex];
                    OUT.normal = normals[topIndex];
                    OUT.color = float4(0, 0, 1, 1);
                    outputStream.Append(OUT);

                    outputStream.RestartStrip();

                    // Second triangle
                    OUT.vertex = vertices[bottomIndex];
                    OUT.normal = normals[bottomIndex];
                    OUT.color = float4(0, 1, 0, 1);
                    outputStream.Append(OUT);
                    OUT.vertex = vertices[repeat(topIndex + 1, 4, 8)];
                    OUT.normal = normals[repeat(topIndex + 1, 4, 8)];
                    OUT.color = float4(0, 1, 1, 1);
                    outputStream.Append(OUT);
                    OUT.vertex = vertices[topIndex];
                    OUT.normal = normals[topIndex++];
                    OUT.color = float4(0, 0, 1, 1);
                    outputStream.Append(OUT);
                    topIndex = repeat(topIndex, 4, 8);

                    outputStream.RestartStrip();
                }

                // The index of the first triangle on the bottom triangle row
                bottomIndex = 4;
                // The index of the first triangle on the top triangle row
                topIndex = 8;
                // Triangles for second part of the cable sleeve
                for (int s2 = 0; s2 < 4; ++s2)
                {
                    // First triangle
                    OUT.vertex = vertices[bottomIndex];
                    OUT.normal = normals[bottomIndex++];
                    OUT.color = float4(1, 0, 0, 1);
                    bottomIndex = repeat(bottomIndex, 4, 8);
                    outputStream.Append(OUT);

                    OUT.vertex = vertices[bottomIndex];
                    OUT.normal = normals[bottomIndex];
                    OUT.color = float4(0, 1, 0, 1);
                    outputStream.Append(OUT);

                    OUT.vertex = vertices[topIndex];
                    OUT.normal = normals[topIndex];
                    OUT.color = float4(0, 0, 1, 1);
                    outputStream.Append(OUT);

                    outputStream.RestartStrip();

                    // First triangle
                    OUT.vertex = vertices[bottomIndex];
                    OUT.normal = normals[bottomIndex];
                    OUT.color = float4(0, 1, 0, 1);
                    outputStream.Append(OUT);
                    OUT.vertex = vertices[repeat(topIndex + 1, 8, 12)];
                    OUT.normal = normals[repeat(topIndex + 1, 8, 12)];
                    OUT.color = float4(0, 1, 1, 1);
                    outputStream.Append(OUT);
                    OUT.vertex = vertices[topIndex];
                    OUT.normal = normals[topIndex++];
                    OUT.color = float4(0, 0, 1, 1);
                    outputStream.Append(OUT);
                    topIndex = repeat(topIndex, 8, 12);

                    outputStream.RestartStrip();
                }
            }

            float4 frag(g2f i) : SV_Target
            {
                return float4(i.normal.xyz, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}