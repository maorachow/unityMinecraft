    #ifndef REALTIME_RAYTRACING_COMMON_SHADERS
    #define REALTIME_RAYTRACING_COMMON_SHADERS
    #include "./RealtimeRayTracingUtility.hlsl"

            #include "./RealtimePhysicallyLightingUtility.hlsl"
      
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            Texture2D _MainTex;
            Texture2D _BumpMap;
            sampler sampler_BumpMap;
            sampler sampler_MainTex;
            Texture2D _MetallicRoughness;
            sampler sampler_MetallicRoughness;
            float _AlphaClip;
 #define COMMON_ALPHATEST_ANYHIT_SHADER
                [shader("anyhit")]\
                void AnyHit(inout RealtimeRaytracingRayPayload rayPayload : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes){\
                    IntersectionVertex curVertex;\
                    GetCurrentIntersectionVertex(attributeData, curVertex);\
                    float2 uv = curVertex.texCoord0;\
                    if(_MainTex.SampleLevel(sampler_MainTex, uv, 0).a<0.0001&&_AlphaClip>0){\
                        IgnoreHit();\
                    }
                    if(rayPayload.TracingDepth<=0&&RayTCurrent()<0.5){\
                        IgnoreHit();\
                    }} 
                      


    #define INITIALIZE_ADDITIONAL_SHADOWRAY(RAY_DESC,RAY_PAYLOAD,RAY_ORIGIN,RAY_DIRECTION,MIN_T,MAX_T,EXPECTED_DIST,CURRENT_TRACING_DEPTH,NORMAL,RANDSEED){\
                            RAY_DESC.Origin =RAY_ORIGIN;\
				            RAY_DESC.Direction =RAY_DIRECTION;\
				            RAY_DESC.TMin =MIN_T;\
				            RAY_DESC.TMax =MAX_T;\
                            RAY_PAYLOAD.TracingDepth =CURRENT_TRACING_DEPTH;\
                            RAY_PAYLOAD.HitDistance=0;\
                            RAY_PAYLOAD.MetallicRoughness=0;\
                            RAY_PAYLOAD.Normal=NORMAL;\
                            RAY_PAYLOAD.Albedo=0;\
                            RAY_PAYLOAD.ShadowRayDirection=RAY_DIRECTION;\
                            RAY_PAYLOAD.TracingRayType=-1;\
                            RAY_PAYLOAD.HitMaterialType=-1;\
                            RAY_PAYLOAD.Radiance=0;\
                            RAY_PAYLOAD.expectedShadowRayHitDistance=EXPECTED_DIST;\
                            RAY_PAYLOAD.randomSeed=RANDSEED;\
    }
    #define INITIALIZE_DIRECTIONAL_SHADOWRAY(RAY_DESC,RAY_PAYLOAD,RAY_ORIGIN,RAY_DIRECTION,MIN_T,MAX_T,CURRENT_TRACING_DEPTH,NORMAL,RANDSEED){\
                    RAY_DESC.Origin = RAY_ORIGIN;\
				    RAY_DESC.Direction = RAY_DIRECTION;\
				    RAY_DESC.TMin =  MIN_T;\
				    RAY_DESC.TMax = MAX_T;\
                    RAY_PAYLOAD.TracingDepth =CURRENT_TRACING_DEPTH+1;\
                    RAY_PAYLOAD.HitDistance=0;\
                    RAY_PAYLOAD.MetallicRoughness=0;\
                    RAY_PAYLOAD.Normal=NORMAL;\
                    RAY_PAYLOAD.Albedo=0;\
                    RAY_PAYLOAD.TracingRayType=-1;\
                    RAY_PAYLOAD.HitMaterialType=-1;\
                    RAY_PAYLOAD.ShadowRayDirection=RAY_DIRECTION;\
                    RAY_PAYLOAD.expectedShadowRayHitDistance=-1;\
                    RAY_PAYLOAD.randomSeed=RANDSEED;\
    }



   #define INITIALIZE_INDIRECT_RAY(RAY_DESC,RAY_PAYLOAD,RAY_ORIGIN,RAY_DIRECTION,MIN_T,MAX_T,CURRENT_TRACING_DEPTH,NORMAL,ALBEDO,METALLIC_ROUGHNESS,TRACING_RAY_TYPE,RANDSEED){\
                    RAY_DESC.Origin =RAY_ORIGIN;\
				    RAY_DESC.Direction = RAY_DIRECTION;\
				    RAY_DESC.TMin =MIN_T;\
				    RAY_DESC.TMax = MAX_T;\
                    RAY_PAYLOAD.TracingDepth =CURRENT_TRACING_DEPTH + 1;\
                    RAY_PAYLOAD.HitDistance=0;\
                    RAY_PAYLOAD.MetallicRoughness=METALLIC_ROUGHNESS;\
                    RAY_PAYLOAD.Normal=NORMAL;\
                    RAY_PAYLOAD.Albedo=ALBEDO;\
                    RAY_PAYLOAD.HitMaterialType=0;\
                    RAY_PAYLOAD.TracingRayType=TRACING_RAY_TYPE;\
                    RAY_PAYLOAD.HitMaterialType=-1;  \
                    RAY_PAYLOAD.randomSeed=RANDSEED;\
    }
    #define PROCESS_RAY_RANDOM_SEED(RAY_PAYLOAD,RANDOMVAL1,RANDOMVAL2){\
             GenerateRandomAndNextSeed(RAY_PAYLOAD.randomSeed,RANDOMVAL1,RANDOMVAL2);\
    }
      #define PROCESS_RAY_RANDOM_SEED(RAY_PAYLOAD,RANDOMVAL1,RANDOMVAL2,RANDOMVAL3,RANDOMVAL4){\
             GenerateRandomAndNextSeed(RAY_PAYLOAD.randomSeed,RANDOMVAL1,RANDOMVAL2,RANDOMVAL3,RANDOMVAL4);\
    }

    #endif