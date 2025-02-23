Shader "Custom/RealtimeDeletricDiffuseShader"
{
    Properties
    {
            _MainTex ("Texture", 2D) = "white" {}
            _BumpMap("Normal Map", 2D) = "bump" {}
            _MetallicRoughness ("Metallic Sommthness Map", 2D) = "grey" {}

            _EmissionMultiplier("Emission", Range(0.0, 100000.0)) = 0
            _RoughnessMultiplier("Smoothness Multiplier", Range(0.0, 1.0)) = 0
            [ToggleUI] _AlphaClip("__clip", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass{
          Name "RealtimeDxrPass"
			Tags{ "LightMode" = "RealtimeDxrPass" }
            HLSLPROGRAM
            #include "./RealtimeRayTracingUtility.hlsl"
            #include "./RealtimePhysicallyLightingUtility.hlsl"
                  #include "./RealtimeRayTracingLightSamplingUtility.hlsl"
          #include "./RealtimeRayTracingCommonShaders.hlsl"
            #pragma raytracing test
        
    
            float _EmissionMultiplier;
            float _RoughnessMultiplier;
              
          COMMON_ALPHATEST_ANYHIT_SHADER 


                  [shader("closesthit")]
            void ClosestHit(inout RealtimeRaytracingRayPayload rayPayload : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
            {
              float rayDist=RayTCurrent();
                rayPayload.HitDistance = rayDist;
                 if(rayPayload.TracingRayType==SHADOWRAY){
                    return;
                }
                 
                    float2 randVal1;
                    float2 randVal2;
                    float2 randVal3;
                    float2 randVal4;
                    PROCESS_RAY_RANDOM_SEED(rayPayload,randVal1,randVal2,randVal3,randVal4);
              
                IntersectionVertex curVertex;
                GetCurrentIntersectionVertex(attributeData, curVertex);
                float2 uv = curVertex.texCoord0;
              
              
                rayPayload.HitMaterialType=0;
                float3 worldPosOrigin=WorldRayOrigin();
                float3 rayDir = WorldRayDirection();
                float3 worldPos= worldPosOrigin +rayDist* rayDir;
              
              
                float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
				float3 worldNormalVertex = normalize(mul(objectToWorld, curVertex.normalOS));
                float3 worldTangentVertex= normalize(mul(objectToWorld, curVertex.tangentOS));

                float3 bitangent=cross(worldNormalVertex, worldTangentVertex);
               
                float3 worldNormalInTexture= UnpackNormalCustom(_BumpMap.SampleLevel(sampler_BumpMap, uv, 0).xyzw);
                float3 worldNormalInTextureOS= normalize(worldTangentVertex*worldNormalInTexture.x+bitangent*worldNormalInTexture.y+worldNormalVertex*worldNormalInTexture.z );
             
                float3 worldNormal=worldNormalInTextureOS;// normalize(mul(objectToWorld, worldNormalInTextureOS));
                 
                if(dot(-rayDir,worldNormalVertex)<-0.01){
                    worldNormal=-worldNormal;
                }
                rayPayload.Normal=worldNormal;
              
              
                float2 metallicRoughness=_MetallicRoughness.SampleLevel(sampler_MetallicRoughness, uv, 0).xw;
                metallicRoughness.y*=_RoughnessMultiplier;
                metallicRoughness.y=1- metallicRoughness.y;
                
                metallicRoughness.xy=clamp(metallicRoughness.xy,0.03,0.97);
                rayPayload.MetallicRoughness=metallicRoughness;
                //   metallicRoughness.y=0.3;
                #ifdef SAMPLING_TEXTURE_GAMMA_CORRECTION_ENABLED
                float3 albedo=pow(_MainTex.SampleLevel(sampler_MainTex, uv, 0).xyz,2.2);
                #else
                float3 albedo=_MainTex.SampleLevel(sampler_MainTex, uv, 0).xyz;
                #endif
               
              
                worldPos+=normalize(worldNormal)*MIN_RAY_TRACING_DIST;
              /*  randVal1=saturate(Random2DTo2D(float2(abs(worldPos.x)*100+worldPos.y*100+ _Time.w*43,abs(worldPos.z)*100-_Time.w*100)));
                randVal2=saturate(Random2DTo2D(float2(abs(worldPos.x)*120+worldPos.y*180+ _Time.w*53,abs(worldPos.z)*-100-_Time.w*170)));*/
                float3 F0=NON_METALLIC_F0;
               
                rayPayload.Albedo=albedo;

                Light mainLight=GetMainLight();
                float3 lightDir=normalize(mainLight.direction);
                float3 viewDir=normalize(-rayDir);
                float3 lightPos=worldPos+lightDir;

                
              
        //        float2 randVal=saturate(Random2DTo2D(float2(abs(worldPos.x)*100+worldPos.y*100+ _Time.x*10,abs(worldPos.z)*100-_Time.x*100)));


                float3 Lo=0;
                float mainLightPDF=0;
                float3 shadowRayDir=GetDirectionalLightRandomDirection(randVal1,lightDir,mainLightPDF);//phi=0.01
                
                
                 
                    RayDesc rayDescMainShadow;
                    RealtimeRaytracingRayPayload testRayResultShadow;
                    INITIALIZE_DIRECTIONAL_SHADOWRAY(rayDescMainShadow,
                    testRayResultShadow,
                    worldPos+normalize(worldNormalVertex)*MIN_RAY_TRACING_DIST,
                    shadowRayDir,
                    MIN_RAY_TRACING_DIST*2,
                    MAX_RAY_TRACING_DIST,
                    rayPayload.TracingDepth,
                    worldNormal,
                    rayPayload.randomSeed

                    )
                     
				   /* rayDescMainShadow.Origin = worldPos+normalize(worldNormalVertex)*MIN_RAY_TRACING_DIST;
				    rayDescMainShadow.Direction = shadowRayDir;
				    rayDescMainShadow.TMin = MIN_RAY_TRACING_DIST*2;
				    rayDescMainShadow.TMax = MAX_RAY_TRACING_DIST;
                 
                    
                    testRayResultShadow.TracingDepth =0;	
                    testRayResultShadow.HitDistance=0;
                    testRayResultShadow.MetallicRoughness=0;
                    testRayResultShadow.Normal=worldNormal;
                    testRayResultShadow.Albedo=0;
                    testRayResultShadow.ShadowRayDirection=shadowRayDir;
                    testRayResultShadow.TracingRayType=-1;
                    testRayResultShadow.HitMaterialType=-1;  
                     testRayResultShadow.Radiance=0;
                         testRayResultShadow.expectedShadowRayHitDistance=-1;*/
                    TraceRay(SceneRaytracingAccelerationStructure, RAY_FLAG_FORCE_NON_OPAQUE, 0xFF, 0, 1, 0, rayDescMainShadow, testRayResultShadow);
                    bool isInShadow=testRayResultShadow.HitDistance>0&&testRayResultShadow.HitDistance<MAX_RAY_TRACING_DIST;
                   float3 directRad=0;
                     if(!isInShadow){
                     
                       
                        float3 wi=normalize(testRayResultShadow.ShadowRayDirection);
                        float3 wo=normalize(-rayDir);
                    
                        float3 brdf1=(FrBRDF(worldPos,wi,wo,albedo,worldNormal,metallicRoughness.y,F0,0.0,true));
                        Lo +=brdf1*testRayResultShadow.Radiance*max(dot(worldNormal,wi),0)/mainLightPDF;
                     //Lo +=CalculateLightPhysically(worldPos,lightPos,worldNormal,wo,albedo,mainLight.color,metallicRoughness.y,F0,true);
                    }
                 
                  //直接采样光源
                    //直接光照
                        
               
                 
                    for (uint lightIndex = 0; lightIndex <8; lightIndex++)
                    {
                    RealtimeRayTracingPointLight light = RayTracingGetAdditionalPerObjectLight(lightIndex);
                    if(length(light.color)<=0.000001||length(light.positionWS)<=0.000001){
                    continue;
                    }
                            float2 randVal=saturate(Random2DTo2D(float2(randVal1.x+(float)lightIndex/8.0,randVal2.y-(float)lightIndex/8.0)));
                            float3 sampleDir=0;
                            float additionalLightPDF;
                            float expectedDist;
                            float lightIntensity=max(light.color.x,max(light.color.y,light.color.z));
                            bool isLightCulled=false;
                            SamplePointLightRandomDirection(randVal,worldPos,light.radius,light.positionWS,sampleDir,expectedDist,additionalLightPDF,lightIntensity,isLightCulled);
                            RayDesc rayDescAdditionalShadow;
                            RealtimeRaytracingRayPayload testRayResultAdditionalShadow ;
                            INITIALIZE_ADDITIONAL_SHADOWRAY(rayDescAdditionalShadow,testRayResultAdditionalShadow,worldPos+normalize(worldNormal)*MIN_RAY_TRACING_DIST,sampleDir, MIN_RAY_TRACING_DIST*2,MAX_RAY_TRACING_DIST,expectedDist,0,worldNormal,randVal)
				          /*  rayDescAdditionalShadow.Origin = worldPos+normalize(sampleDir)*MIN_RAY_TRACING_DIST;
				            rayDescAdditionalShadow.Direction = sampleDir;
				            rayDescAdditionalShadow.TMin = MIN_RAY_TRACING_DIST*2;
				            rayDescAdditionalShadow.TMax = MAX_RAY_TRACING_DIST;
                           
                    
                            testRayResultAdditionalShadow.TracingDepth =0;	
                            testRayResultAdditionalShadow.HitDistance=0;
                            testRayResultAdditionalShadow.MetallicRoughness=0;
                            testRayResultAdditionalShadow.Normal=worldNormal;
                            testRayResultAdditionalShadow.Albedo=0;
                            testRayResultAdditionalShadow.ShadowRayDirection=sampleDir;
                            testRayResultAdditionalShadow.TracingRayType=-1;
                            testRayResultAdditionalShadow.HitMaterialType=-1;  
                            testRayResultAdditionalShadow.Radiance=0;
                            testRayResultAdditionalShadow.expectedShadowRayHitDistance=expectedDist;*/
                            if(isLightCulled==true){
                             
                            continue;
                            }
                            TraceRay(SceneRaytracingAccelerationStructure, RAY_FLAG_FORCE_NON_OPAQUE, 0xFF, 0, 1, 0, rayDescAdditionalShadow, testRayResultAdditionalShadow);
                            bool additionalLightIsInShadow=testRayResultAdditionalShadow.HitDistance>MIN_RAY_TRACING_DIST&&testRayResultAdditionalShadow.HitDistance<expectedDist;
                            if(!additionalLightIsInShadow){
                            float3 wi=normalize(testRayResultAdditionalShadow.ShadowRayDirection);
                            float3 wo=normalize(-rayDir);
                    
                            float3 brdf=(FrBRDF(worldPos,wi,wo,albedo,worldNormal,metallicRoughness.y,F0,0,true));
                            Lo +=brdf*light.color*max(dot(worldNormal,wi),0)/additionalLightPDF;
                            }
                           
                       
                    }

                   
               
				   
                    
                   
                    
              
                    float3 indirectDiffuseDir=0;
                    float cosTheta=0;
                    float sinTheta=0;
                    float3 h = ImportanceSampleGGX(randVal3, worldNormal,metallicRoughness.y,cosTheta,sinTheta);
                    float3 lightDirGlossy= normalize(2.0 * dot(-rayDir, h) * h -(-rayDir));
                     float3 F = fresnelSchlickRoughness(max(dot(h,normalize(-rayDir)), 0.0), F0,metallicRoughness.y);
    
                        float3 kS = F;
                        float3 kD = 1.0 - kS;
                        kD *= 1.0;

                 //   float2 randVal3=saturate(Random2DTo2D(float2(abs(worldPos.x)*140+worldPos.y*120+ _Time.x*16,abs(worldPos.z)*150-_Time.x*130))).xy;
                    bool isGlossyReflections=randVal2.x<kS;//metallicRoughness.y;
                    
                    float cosWeightedtheta=0;
              
                    indirectDiffuseDir =normalize(SampleCosineWeightedHemisphere(worldNormal,randVal4,cosWeightedtheta));

                    if(isGlossyReflections){
                    indirectDiffuseDir= lightDirGlossy;
                    }

                    if(rayPayload.TracingDepth + 1>=MAX_RAY_RECURSION_DEPTH){
                        rayPayload.Radiance=0;

                        return;
                    }
 
                       
                    // indirectDiffuseDir = indirectDiffuseDirDiffuse;//normalize(reflect(h,normalize(worldNormal)));
                   //   indirectDiffuseDir=normalize(lerp(indirectDiffuseDir,indirectDiffuseDirDiffuse,metallicRoughness.y));
                    RayDesc rayDescIndirect;
                    RealtimeRaytracingRayPayload testRayResultIndirect;
                    INITIALIZE_INDIRECT_RAY(rayDescIndirect,testRayResultIndirect,worldPos+normalize(indirectDiffuseDir)*MIN_RAY_TRACING_DIST,indirectDiffuseDir,
                     MIN_RAY_TRACING_DIST*2,
                     MAX_RAY_TRACING_DIST,
                     rayPayload.TracingDepth,
                     worldNormal,
                     albedo,
                     metallicRoughness,
                     INDIRECT_DIFFUSE,
                     rayPayload.randomSeed
                    )
                 /*   rayDescIndirect.Origin = worldPos+normalize(indirectDiffuseDir)*MIN_RAY_TRACING_DIST;
				    rayDescIndirect.Direction = indirectDiffuseDir;
				    rayDescIndirect.TMin = MIN_RAY_TRACING_DIST*2;
				    rayDescIndirect.TMax = MAX_RAY_TRACING_DIST;
                    testRayResultIndirect.TracingDepth =rayPayload.TracingDepth + 1;	
                    testRayResultIndirect.HitDistance=0;
                    testRayResultIndirect.MetallicRoughness=metallicRoughness;
                    testRayResultIndirect.Normal=worldNormal;
                    testRayResultIndirect.Albedo=albedo;
                    testRayResultIndirect.HitMaterialType=0;
                    testRayResultIndirect.TracingRayType=INDIRECT_DIFFUSE;
                    testRayResultIndirect.HitMaterialType=-1;  
                    testRayResultIndirect.randomSeed=rayPayload.randomSeed;*/
                    TraceRay(SceneRaytracingAccelerationStructure, RAY_FLAG_FORCE_NON_OPAQUE, 0xFF, 0, 1, 0, rayDescIndirect, testRayResultIndirect);

                    
                
                 
                
                    float3 wi=normalize(indirectDiffuseDir);
                    float3 wo=normalize(-rayDir);
                    float pdfCosWeighted=(cos(cosWeightedtheta))/PI;
                     
                    float pdfGGXImportanceSampling= GetGGXImportanceSamplingPDFUE4(cosTheta,metallicRoughness.y,wi,h); ;

                    float pdf=kD*pdfGGXImportanceSampling+kS*pdfCosWeighted;
                    float3 brdf=FrBRDF(worldPos,wi,wo,albedo,worldNormal,metallicRoughness.y,F0,0.0,true);
                  
                    float3 indirectCol= brdf*testRayResultIndirect.Radiance*max(dot(worldNormal,wi),0)/(isGlossyReflections?pdfGGXImportanceSampling:pdfCosWeighted) ;//间接光照
                    indirectCol+=albedo*_EmissionMultiplier;
                    indirectCol+=Lo;
                    rayPayload.Radiance=indirectCol;
               
                    //
 
            }
            ENDHLSL
        
        }
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    SubShader
    {
       Tags { "LightMode" = "MotionVectors" }
       UsePass "Hidden/Universal Render Pipeline/ObjectMotionVectorFallback/MOTIONVECTORS"
       
    }
}
