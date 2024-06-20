Shader "Custom/Cable"
{
    Properties
    {
        _BaseColor("Base Color", color) = (1, 1, 1, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0
        _Metallic("Metallic", Range(0, 1)) = 0
        _CableRadius("Cable Radius", Float) = 0.002
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
            #pragma geometry geom
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/BSDF.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 texcoord1 : TEXCOORD1;
            };

            struct v2g
            {
                float4 vertex : POSITION;
                float4 texcoord1 : TEXCOORD1;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 4);
            };

            float4 _BaseColor;
            float _Smoothness, _Metallic, _CableRadius;

            float4x4 rotationMatrix(float3 axis, float angle)
            {
                axis = normalize(axis);
                float s = sin(angle);
                float c = cos(angle);
                float oc = 1.0 - c;

                return float4x4(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s,
                                oc * axis.z * axis.x + axis.y * s, 0.0,
                                oc * axis.x * axis.y + axis.z * s,
                                oc * axis.y * axis.y + c,
                                oc * axis.y * axis.z - axis.x * s, 0.0,
                                oc * axis.z * axis.x - axis.y * s,
                                oc * axis.y * axis.z + axis.x * s,
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

            v2g vert(appdata v)
            {
                v2g o;
                o.vertex = float4(TransformObjectToWorld(v.vertex.xyz), 1.0);
                //o.positionWS = TransformObjectToWorld(v.vertex.xyz);
                //o.normalWS = TransformObjectToWorldNormal(v.normal.xyz);
                //o.viewDir = normalize(_WorldSpaceCameraPos - o.positionWS);
                o.texcoord1 = v.texcoord1;
                return o;
            }

            g2f SetupVert(float4 positionWS, float2 uv, float3 normalWS, float4 texcoord1)
            {
                g2f Out;
                Out.vertex = TransformObjectToHClip(mul(unity_WorldToObject, positionWS).xyz);
                Out.uv = uv;
                Out.positionWS = positionWS;
                Out.normalWS = normalWS;
                Out.viewDir = normalize(_WorldSpaceCameraPos - positionWS);
                OUTPUT_LIGHTMAP_UV(texcoord1, unity_LightmapST, Out.lightmapUV);
                OUTPUT_SH(Out.normalWS, Out.vertexSH);
                return Out;
            }

            [maxvertexcount(48)]
            void geom(triangle v2g p[3], inout TriangleStream<g2f> outputStream)
            {
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

                const float3 lineDir0 = normalize(pos1.xyz - pos0.xyz);
                float3 lineDir1 = normalize(pos2.xyz - pos1.xyz);
                #define STABLE_TANGENT false
                #if STABLE_TANGENT

                // Bring the vectors slightly out of alignment so that the form a plane
                if (dot(lineDir0, lineDir1) >= 0.999f || dot(lineDir0, lineDir1) <= 0.001f)
                {
                    lineDir1 = normalize(lineDir1 + float3(0, 0.000001, 0));
                }
                const float3 lineDir = normalize(lineDir0 + lineDir1);
                const float3 planeNormal = normalize(cross(lineDir0, lineDir1));

                const float3 tangent0 = normalize(cross(planeNormal, lineDir0));

                const float3 tangent1 = normalize(cross(planeNormal, lineDir1));
                
                #else

                const float3 lineDir = normalize(lineDir0 + lineDir1);

                float3 tangent0 = normalize(cross(lineDir0, float3(0, 1, 0)));
                if (abs(dot(lineDir0, float3(0, 1, 0))) >= 0.99f)
                    tangent0 = normalize(cross(lineDir0, normalize(float3(1, 1, 1))));
                
                float3 tangent1 = normalize(cross(lineDir1, float3(0, 1, 0)));
                if (abs(dot(lineDir1, float3(0, 1, 0))) >= 0.99f)
                    tangent1 = normalize(cross(lineDir1, normalize(float3(1, 1, 1))));

                #endif

                const float3 tangent = normalize(tangent0 + tangent1);

                float3 normals[12];
                const float angleStep = TWO_PI / 4;
                // Normals for vertices around line 1
                for (int normal = 0; normal < 12; normal++)
                {
                    float3 direction;
                    float3 axis;
                    if (normal < 4)
                    {
                        direction = tangent0;
                        axis = lineDir0;
                    }
                    else if (normal < 8)
                    {
                        direction = tangent;
                        axis = lineDir;
                    }
                    else
                    {
                        direction = tangent1;
                        axis = lineDir1;
                    }

                    normals[normal] = rotateVector(direction, angleStep * (normal + 1), axis);
                }

                float4 vertices[12];
                for (int vertex = 0; vertex < 12; vertex++)
                {
                    float3 startPos;
                    if (vertex < 4)
                    {
                        startPos = halfPos0.xyz;
                    }
                    else if (vertex < 8)
                    {
                        startPos = pos1.xyz;
                    }
                    else
                    {
                        startPos = halfPos1.xyz;
                    }
                    vertices[vertex] = float4(startPos + normals[vertex] * _CableRadius, 1.0f);
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
                    outputStream.Append(
                        SetupVert(
                            vertices[bottomIndex],
                            0,
                            normals[bottomIndex],
                            p[1].texcoord1));
                    bottomIndex = repeat(bottomIndex + 1, 0, 4);

                    outputStream.Append(
                        SetupVert(
                            vertices[bottomIndex],
                            0,
                            normals[bottomIndex],
                            p[1].texcoord1));

                    outputStream.Append(
                        SetupVert(
                            vertices[topIndex],
                            0,
                            normals[topIndex],
                            p[1].texcoord1));

                    outputStream.RestartStrip();

                    // Second triangle
                    outputStream.Append(
                        SetupVert(
                            vertices[bottomIndex],
                            0,
                            normals[bottomIndex],
                            p[1].texcoord1));

                    outputStream.Append(
                        SetupVert(
                            vertices[repeat(topIndex + 1, 4, 8)],
                            0,
                            normals[repeat(topIndex + 1, 4, 8)],
                            p[1].texcoord1));

                    outputStream.Append(
                        SetupVert(
                            vertices[topIndex],
                            0,
                            normals[topIndex],
                            p[1].texcoord1));
                    topIndex = repeat(topIndex + 1, 4, 8);

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
                    outputStream.Append(
                        SetupVert(
                            vertices[bottomIndex],
                            0,
                            normals[bottomIndex],
                            p[1].texcoord1));
                    bottomIndex = repeat(bottomIndex + 1, 4, 8);

                    outputStream.Append(
                        SetupVert(
                            vertices[bottomIndex],
                            0,
                            normals[bottomIndex],
                            p[1].texcoord1));

                    outputStream.Append(
                        SetupVert(
                            vertices[topIndex],
                            0,
                            normals[topIndex],
                            p[1].texcoord1));

                    outputStream.RestartStrip();

                    // First triangle
                    outputStream.Append(
                        SetupVert(
                            vertices[bottomIndex],
                            0,
                            normals[bottomIndex],
                            p[1].texcoord1));

                    outputStream.Append(
                        SetupVert(
                            vertices[repeat(topIndex + 1, 8, 12)],
                            0,
                            normals[repeat(topIndex + 1, 8, 12)],
                            p[1].texcoord1));

                    outputStream.Append(
                        SetupVert(
                            vertices[topIndex],
                            0,
                            normals[topIndex],
                            p[1].texcoord1));
                    topIndex = repeat(topIndex + 1, 8, 12);

                    outputStream.RestartStrip();
                }
            }

            half4 frag(g2f i) : SV_Target
            {
                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.normalWS = normalize(i.normalWS);
                inputData.viewDirectionWS = i.viewDir;
                inputData.bakedGI = SAMPLE_GI(i.lightmapUV, i.vertexSH, inputData.normalWS);

                SurfaceData surfaceData;
                surfaceData.albedo = _BaseColor;
                surfaceData.specular = 0;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = 0;
                surfaceData.emission = 0;
                surfaceData.occlusion = 1;
                surfaceData.alpha = 0;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 0;

                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }
    }
}