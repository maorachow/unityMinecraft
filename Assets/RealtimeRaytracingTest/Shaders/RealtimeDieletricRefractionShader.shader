Shader "Custom/RealtimeDeletricRefractionShader"
{
    Properties
    {
            _MainTex ("Texture", 2D) = "white" {}
            _BumpMap("Normal Map", 2D) = "bump" {}
            _MetallicRoughness ("Metallic Sommthness Map", 2D) = "grey" {}

            _EmissionMultiplier("Emission", Range(0.0, 100000.0)) = 0
            _RoughnessMultiplier("Smoothness Multiplier", Range(0.0, 1.0)) = 0
            _IoR("Index Of Refraction",Range(0.001,2.0))=1.5
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
            #include "./RealtimeRayTracingLightSamplingUtility.hlsl"
         
            #include "./RealtimeRayTracingCommonShaders.hlsl"
       
            #pragma raytracing test
        //    #pragma max_recursion_depth 6
          
         
            float _EmissionMultiplier;
            float _RoughnessMultiplier;
            float _IoR;
          
          COMMON_ALPHATEST_ANYHIT_SHADER 
                  [shader("closesthit")]
            void ClosestHit(inout RealtimeRaytracingRayPayload rayPayload : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
            {
                 
                    float2 randVal1;
                    float2 randVal2;
                    float2 randVal3;
                    float2 randVal4;
                   PROCESS_RAY_RANDOM_SEED(rayPayload,randVal1,randVal2,randVal3,randVal4);
                IntersectionVertex curVertex;
                GetCurrentIntersectionVertex(attributeData, curVertex);
                float2 uv = curVertex.texCoord0;
              
                float rayDist=RayTCurrent();
             //   rayPayload.HitDistance = rayDist;


                    float3 worldPosOrigin=WorldRayOrigin();
                    float3 rayDir = WorldRayDirection();
                    float3 worldPos= worldPosOrigin +rayDist* rayDir;
                     
                          rayPayload.HitMaterialType=0;
             
              
              
                float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
				float3 worldNormalVertex = normalize(mul(objectToWorld, curVertex.normalOS));
                float3 worldTangentVertex= normalize(mul(objectToWorld, curVertex.tangentOS));

                float3 bitangent=cross(worldNormalVertex, worldTangentVertex);
               
                float3 worldNormalInTexture= UnpackNormalCustom(_BumpMap.SampleLevel(sampler_BumpMap, uv, 0).xyzw);
                float3 worldNormalInTextureOS= normalize(worldTangentVertex*worldNormalInTexture.x+bitangent*worldNormalInTexture.y+worldNormalVertex*worldNormalInTexture.z );
             
                float3 worldNormal=worldNormalInTextureOS;// normalize(mul(objectToWorld, worldNormalInTextureOS));
                 bool isBehindSurface=false;
                   float currentIoR=_IoR<1?_IoR:1.0/_IoR;
                   float etai=1;
                   float etat=1.0/currentIoR;
                if(dot(-rayDir,worldNormalVertex)<0){
              //      worldNormal=-worldNormal;
              isBehindSurface=true;
             //   currentIoR=1.0/_IoR;
                etai=1.0/currentIoR;
                etat=1;
                }
                rayPayload.Normal=worldNormal;
              
              
                float2 metallicRoughness=_MetallicRoughness.SampleLevel(sampler_MetallicRoughness, uv, 0).xw;
                metallicRoughness.y*=_RoughnessMultiplier;
                metallicRoughness.y=1- metallicRoughness.y;
                
                metallicRoughness.xy=clamp(metallicRoughness.xy,0.03,0.97);
                rayPayload.MetallicRoughness=metallicRoughness;
               

                #ifdef SAMPLING_TEXTURE_GAMMA_CORRECTION_ENABLED
                float3 albedo=pow(_MainTex.SampleLevel(sampler_MainTex, uv, 0).xyz,2.2);
                #else
                float3 albedo=_MainTex.SampleLevel(sampler_MainTex, uv, 0).xyz;
                #endif

                   float3 viewDir=-rayDir;
               
                  //      float2 randVal1=saturate(Random2DTo2D(float2(abs(worldPos.x)*100+worldPos.y*100+ _Time.x*43,abs(worldPos.z)*100-_Time.x*100)));
                        float cosTheta=0;
                        float sinTheta=0;
                        float3 h = ImportanceSampleGGX(randVal1, worldNormal,metallicRoughness.y,cosTheta,sinTheta);

                       
                        float F =saturate(FresnelDielectric((dot(h,viewDir)),etai,etat));
                //    float randVal2=saturate(Random2DTo2D(float2(abs(worldPos.x)*140+worldPos.y*120+ _Time.x*16,abs(worldPos.z)*150-_Time.x*130))).x;
                   bool isReflecting=randVal1.x<F.x;
                   if(isnan(F)==true){
                   F=1;
                   }
                    float reflChance=clamp(F,0.001,0.999);
                      
                 if(rayPayload.TracingRayType==SHADOWRAY){

                    if(rayPayload.expectedShadowRayHitDistance>0){
                       rayPayload.Radiance = 0;
                       rayPayload.HitDistance = rayDist;
                      return;
                    }


                    if(rayPayload.TracingDepth+1>=MAX_RAY_RECURSION_DEPTH){
                        rayPayload.HitDistance = rayDist;
                        rayPayload.Radiance = 0;
                        return;
                    }else{
                      float3 newShadowRayDir =normalize(refract(normalize(rayDir),!isBehindSurface?normalize(h):normalize(-h),(currentIoR)/(1.0/currentIoR)));
                      if(isnan(newShadowRayDir.x)||isnan(newShadowRayDir.y)||isnan(newShadowRayDir.z)){
                               rayPayload.HitDistance = rayDist;
                                rayPayload.Radiance = 0;
                        return;
                        }
                    RayDesc rayDescMainNewShadow;
                    RealtimeRaytracingRayPayload testRayResultNewShadow ;
				 
                    
                     INITIALIZE_DIRECTIONAL_SHADOWRAY(rayDescMainNewShadow,
                    testRayResultNewShadow,
                    worldPos+normalize(newShadowRayDir)* MIN_RAY_TRACING_DIST,
                    newShadowRayDir,
                    MIN_RAY_TRACING_DIST*2,
                    MAX_RAY_TRACING_DIST,
                    rayPayload.TracingDepth,
                    worldNormal,
                     rayPayload.randomSeed
                    )
                      /* rayDescMainNewShadow.Origin = worldPos+normalize(newShadowRayDir)* MIN_RAY_TRACING_DIST*2;
				    rayDescMainNewShadow.Direction = newShadowRayDir;
				    rayDescMainNewShadow.TMin = MIN_RAY_TRACING_DIST*2;
				    rayDescMainNewShadow.TMax = MAX_RAY_TRACING_DIST;
                    testRayResultNewShadow.TracingDepth =rayPayload.TracingDepth+1;	
                    testRayResultNewShadow.HitDistance=0;
                    testRayResultNewShadow.MetallicRoughness=0;
                    testRayResultNewShadow.Normal=worldNormal;
                    testRayResultNewShadow.Albedo=0;
                    testRayResultNewShadow.ShadowRayDirection=newShadowRayDir;
                    testRayResultNewShadow.TracingRayType=SHADOWRAY;
                    testRayResultNewShadow.HitMaterialType=-1;  
                    testRayResultNewShadow.expectedShadowRayHitDistance=-1;*/
                    TraceRay(SceneRaytracingAccelerationStructure, RAY_FLAG_FORCE_NON_OPAQUE, 0xFF, 0, 1, 0, rayDescMainNewShadow, testRayResultNewShadow);


                     rayPayload.HitDistance = testRayResultNewShadow.HitDistance; 
                    float3 wi2=normalize(newShadowRayDir);
                    float3 wo2=normalize(-rayDir);
                     float pdf2 =GetGGXImportanceSamplingPDFUE4(cosTheta,metallicRoughness.y);

                    float FClamped=clamp(F,0.0001,0.9999);
                   float3 btdf2=FrBTDF(worldPos,wi2,wo2,albedo,worldNormal,metallicRoughness.y,FClamped, h,etai,etat,pdf2);
                  
                    float3 indirectColTransShadowRay=btdf2*testRayResultNewShadow.Radiance*max(dot(!isBehindSurface?-worldNormal:worldNormal,wi2),0)/(pdf2); 
                     rayPayload.Radiance = indirectColTransShadowRay;
                     rayPayload.ShadowRayDirection = testRayResultNewShadow.ShadowRayDirection;
                     return;
                    }
             //   return;
                 
                }
                 
          
              
                worldPos+=normalize(!isBehindSurface?worldNormal:-worldNormal)*0.001;
                float3 F0=NON_METALLIC_F0;
               
                rayPayload.Albedo=albedo;

                Light mainLight=GetMainLight();
                float3 lightDir=normalize(mainLight.direction);
             
                float3 lightPos=worldPos+lightDir;

                
                
               // float2 randVal=saturate(Random2DTo2D(float2(abs(worldPos.x)*100+worldPos.y*100+ _Time.x*10,abs(worldPos.z)*100-_Time.x*100)));


                float mainLightPDF=0;
                float3 shadowRayDir=GetDirectionalLightRandomDirection(randVal2,lightDir,mainLightPDF);//phi=0.01
                
                
                 
                    RayDesc rayDescMainShadow;
                    RealtimeRaytracingRayPayload testRayResultShadow ;
                    INITIALIZE_DIRECTIONAL_SHADOWRAY(rayDescMainShadow,
                    testRayResultShadow,
                    worldPos+normalize(!isBehindSurface?worldNormalVertex:-worldNormalVertex)*MIN_RAY_TRACING_DIST,
                    shadowRayDir,
                    MIN_RAY_TRACING_DIST*2,
                    MAX_RAY_TRACING_DIST,
                    rayPayload.TracingDepth,
                    worldNormal,
                     rayPayload.randomSeed
                    )
				  /*  rayDescMainShadow.Origin = worldPos+normalize(!isBehindSurface?worldNormalVertex:-worldNormalVertex)*0.0001;
				    rayDescMainShadow.Direction = shadowRayDir;
				    rayDescMainShadow.TMin =  MIN_RAY_TRACING_DIST*2;
				    rayDescMainShadow.TMax = MAX_RAY_TRACING_DIST;
                    testRayResultShadow.TracingDepth =rayPayload.TracingDepth+1;	
                    testRayResultShadow.HitDistance=0;
                    testRayResultShadow.MetallicRoughness=0;
                    testRayResultShadow.Normal=worldNormal;
                    testRayResultShadow.Albedo=0;
                    testRayResultShadow.TracingRayType=-1;
                    testRayResultShadow.HitMaterialType=-1;  
                    testRayResultShadow.ShadowRayDirection=shadowRayDir;
                    testRayResultShadow.expectedShadowRayHitDistance=-1;*/
                    TraceRay(SceneRaytracingAccelerationStructure, RAY_FLAG_FORCE_NON_OPAQUE, 0xFF, 0, 1, 0, rayDescMainShadow, testRayResultShadow);
                    bool isInShadow=testRayResultShadow.HitDistance>0&&testRayResultShadow.HitDistance<998;

                     float3 directRad=0;
                     
                    float3 Lo=0;
              
               
                    //直接光照
               //       rayPayload.Radiance+=albedo*_EmissionMultiplier;
                  

              
                    RayDesc rayDescIndirect;
				   
                    
                   
             //       h = ImportanceSampleGGX(randVal1, worldNormal,metallicRoughness.y,cosTheta,sinTheta);
                 
                    float3 indirectDiffuseDir=0;
                  
                      float3 lightDirGlossy= reflect(rayDir,normalize(h));
                      if(isBehindSurface){
                      lightDirGlossy= reflect(rayDir,normalize(-h));
                      }
                      float3 lightDirRefraction =normalize(refract(normalize(rayDir),!isBehindSurface?normalize(h):normalize(-h),(currentIoR)/(1.0/currentIoR)));

                  
                    
                    
                    float cosWeightedtheta=0;
                   
               //     indirectDiffuseDir =normalize(SampleCosineWeightedHemisphere(worldNormalVertex,worldTangentVertex,randVal1,cosWeightedtheta));

                   if(isReflecting){
                    indirectDiffuseDir=lightDirGlossy;
                    }else{
                     indirectDiffuseDir =lightDirRefraction;
              
                     }
                 
                       indirectDiffuseDir =isnan(indirectDiffuseDir)?reflect(rayDir,!isBehindSurface?normalize(h):normalize(-h)):indirectDiffuseDir;
                      if((isnan(indirectDiffuseDir).x==true||isnan(indirectDiffuseDir).y==true||isnan(indirectDiffuseDir).z==true)){
                       
                      isReflecting=true;
                      }
                   
                   
                  if(rayPayload.TracingDepth + 1>=MAX_RAY_RECURSION_DEPTH){
                     rayPayload.Radiance=0;
                  return;
                  }
                    // indirectDiffuseDir = indirectDiffuseDirDiffuse;//normalize(reflect(h,normalize(worldNormal)));
                   //   indirectDiffuseDir=normalize(lerp(indirectDiffuseDir,indirectDiffuseDirDiffuse,metallicRoughness.y));
                    rayDescIndirect.Origin = worldPos+normalize(indirectDiffuseDir)*0.003;
				    rayDescIndirect.Direction = indirectDiffuseDir;
				    rayDescIndirect.TMin = 0.001;
				    rayDescIndirect.TMax = 1000;
                    RealtimeRaytracingRayPayload testRayResultIndirect ;
                    
                    testRayResultIndirect.TracingDepth =rayPayload.TracingDepth + 1;	
                    testRayResultIndirect.HitDistance=0;
                    testRayResultIndirect.MetallicRoughness=metallicRoughness;
                    testRayResultIndirect.Normal=worldNormal;
                    testRayResultIndirect.Albedo=albedo;
                  
                    testRayResultIndirect.HitMaterialType=0;
                    testRayResultIndirect.TracingRayType=INDIRECT_DIFFUSE;
                    testRayResultIndirect.HitMaterialType=-1;  
                    testRayResultIndirect.randomSeed=rayPayload.randomSeed;
                   TraceRay(SceneRaytracingAccelerationStructure, RAY_FLAG_FORCE_NON_OPAQUE, 0xFF, 0, 1, 0, rayDescIndirect, testRayResultIndirect);

                    
                
                 

                    if(isReflecting==true){
                    
                    float3 wi=normalize(indirectDiffuseDir);
                    float3 wo=normalize(-rayDir);
                    float pdf=GetGGXImportanceSamplingPDFUE4(cosTheta,metallicRoughness.y);
                    
                    float3 brdf=FrBRDFDieletricSpecular(worldPos,wi,wo,albedo,!isBehindSurface?worldNormal:-worldNormal,metallicRoughness.y,F,true);
                  
                    float3 indirectColSpec= brdf*testRayResultIndirect.Radiance*max(dot(!isBehindSurface?worldNormal:-worldNormal,wi),0)/(pdf*(reflChance)); 


                  // rayPayload.Radiance=testRayResultIndirect.Radiance;

                    if(!isInShadow){
                     
                       
                        float3 wi1=normalize(testRayResultShadow.ShadowRayDirection);
                        float3 wo1=normalize(-rayDir);
                    
                        float3 brdf1=FrBRDFDieletricSpecular(worldPos,wi1,wo1,albedo,!isBehindSurface?worldNormal:-worldNormal,metallicRoughness.y,F,true);
                        Lo =brdf1*mainLight.color*max(dot(!isBehindSurface?worldNormal:-worldNormal,wi1),0)/(mainLightPDF*(reflChance<0.02?1:reflChance));
                        if(reflChance<0.0001){
                        Lo=0;
                        }
                     //Lo +=CalculateLightPhysically(worldPos,lightPos,worldNormal,wo,albedo,mainLight.color,metallicRoughness.y,F0,true);
                    
                
                    }


                    
                    for (uint lightIndex = 0; lightIndex < 8; lightIndex++)
                    {
                    RealtimeRayTracingPointLight light = RayTracingGetAdditionalPerObjectLight(lightIndex);
                    if(length(light.color)<=0.000001){
                    
                    continue;
                    }
               //     diffuse += LightingLambert(light.color, light.direction, IN.normalWS);
              //      specular += LightingSpecular(light.color, light.direction, normalize(IN.normalWS), normalize(IN.viewDirWS), _SpecularColor, _Smoothness);
                      float2 randVal=saturate(Random2DTo2D(float2(randVal1.x+(float)lightIndex/8.0,randVal1.y-(float)lightIndex/8.0)));
                      float3 sampleDir=0;
                      float additionalLightPDF;
                      float expectedDist;
                      float lightIntensity=max(light.color.x,max(light.color.y,light.color.z));
                      bool isLightCulled=false;
                            SamplePointLightRandomDirection(randVal,worldPos+normalize(worldNormal)*MIN_RAY_TRACING_DIST ,light.radius,light.positionWS,sampleDir,expectedDist,additionalLightPDF,lightIntensity,isLightCulled);
                            if(isLightCulled==true){
                            continue;
                            }
                            RayDesc rayDescAdditionalShadow;
                            RealtimeRaytracingRayPayload testRayResultAdditionalShadow ;


                            INITIALIZE_ADDITIONAL_SHADOWRAY(rayDescAdditionalShadow,testRayResultAdditionalShadow,worldPos+normalize(sampleDir)*MIN_RAY_TRACING_DIST,sampleDir, MIN_RAY_TRACING_DIST*2,MAX_RAY_TRACING_DIST,expectedDist,0,worldNormal,randVal)
				   
                            TraceRay(SceneRaytracingAccelerationStructure, RAY_FLAG_FORCE_NON_OPAQUE, 0xFF, 0, 1, 0, rayDescAdditionalShadow, testRayResultAdditionalShadow);
                            bool additionalLightIsInShadow=testRayResultAdditionalShadow.HitDistance>MIN_RAY_TRACING_DIST&&testRayResultAdditionalShadow.HitDistance<expectedDist+MIN_RAY_TRACING_DIST*100;
                            if(!additionalLightIsInShadow){
                            float3 wi=normalize(testRayResultAdditionalShadow.ShadowRayDirection);
                            float3 wo=normalize(-rayDir);
                    
                            float3 brdf2=(FrBRDFDieletricSpecular(worldPos,wi,wo,albedo,!isBehindSurface?worldNormal:-worldNormal,metallicRoughness.y,F,true));
                            Lo +=brdf2*light.color*max(dot(worldNormal,wi),0)/(additionalLightPDF*(reflChance<0.02?1:reflChance));
                            }
                           
                       
                    }

                     indirectColSpec+=Lo;
                //      rayPayload.Radiance+=testRayResultIndirect.Radiance;
                     rayPayload.Radiance=indirectColSpec;

                    }else{
                 
                    float3 wi=normalize(indirectDiffuseDir);
                    float3 wo=normalize(-rayDir);
                     

                       float D = DistributionGGX(worldNormal,h, metallicRoughness.y);
                       float NDotH=dot(worldNormal,h);
                        float pdf1 =GetGGXImportanceSamplingPDFUE4(cosTheta,metallicRoughness.y);

                    
                    float3 btdf=FrBTDF(worldPos,wi,wo,albedo,worldNormal,metallicRoughness.y,F, h,etai,etat,pdf1);
                  
                    float3 indirectColTrans=btdf*testRayResultIndirect.Radiance*max(dot(!isBehindSurface?-worldNormal:worldNormal,wi),0)/(pdf1*(1-reflChance)); 
                    rayPayload.Radiance=indirectColTrans;
                    }
           //     rayPayload.Radiance=F.x;
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
