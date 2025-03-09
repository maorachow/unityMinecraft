 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"  
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"  
         

          

            uniform Texture2D HiZBufferTexture;
          
            uniform int MaxHiZBufferTextureMipLevel;
            uniform float SSRBlendFactor;
            SamplerState sampler_point_clamp;
            SamplerState sampler_linear_clamp ;
            SamplerState sampler_linear_repeat ;
            uniform float4 SSRSourceSize;
            uniform int MaxIterations;
            uniform float SSRThickness;
            uniform float MaxTracingDistance;
               uniform float SSRMinSmoothness;
            uniform int UseColorPyramid;
            uniform int UseTemporalFilter;
            uniform int UseNormalImportanceSampling;



            float4 ProjectionParams2;
            float4 _CameraViewXExtent[2];
            float4 _CameraViewYExtent[2];
            float4 _CameraViewZExtent[2];
            float4x4 _CameraViewProjections[2];

            float4x4 _CameraViews[2];

            float4x4 _CameraProjections[2];
            float4 _CameraViewTopLeftCorner[2];

              #if defined(USING_STEREO_MATRICES)
                #define unity_eyeIndex unity_StereoEyeIndex
            #else
                #define unity_eyeIndex 0
            #endif
            float GetLinearEyeDepth(float rawDepth)
            {
            #if defined(_ORTHOGRAPHIC)
                return LinearDepthToEyeDepth(rawDepth);
            #else
                return LinearEyeDepth(rawDepth, _ZBufferParams);
            #endif
            }

            float SampleAndGetLinearEyeDepth(float2 uv)
            {
                const float rawDepth =  SampleSceneDepth(uv);
                return GetLinearEyeDepth(rawDepth);
            }

            // This returns a vector in world unit (not a position), from camera to the given point described by uv screen coordinate and depth (in absolute world unit).
            half3 ReconstructViewPos(float2 uv, float linearDepth)
            {
                #if defined(SUPPORTS_FOVEATED_RENDERING_NON_UNIFORM_RASTER)
                UNITY_BRANCH if (_FOVEATED_RENDERING_NON_UNIFORM_RASTER)
                {
                    uv = RemapFoveatedRenderingNonUniformToLinear(uv);
                }
                #endif

                // Screen is y-inverted.
                uv.y = 1.0 - uv.y;

                // view pos in world space
                #if defined(_ORTHOGRAPHIC)
                     float zScale = (linearDepth-_ProjectionParams.y) /(_ProjectionParams.z-_ProjectionParams.y);  // divide by far plane
                    float3 viewPos = _CameraViewTopLeftCorner[unity_eyeIndex].xyz
                                        + _CameraViewXExtent[unity_eyeIndex].xyz * uv.x
                                        + _CameraViewYExtent[unity_eyeIndex].xyz * uv.y
                                        + _CameraViewZExtent[unity_eyeIndex].xyz * zScale;
                #else
                    float zScale = linearDepth * ProjectionParams2.x; // divide by near plane
                    float3 viewPos = _CameraViewTopLeftCorner[unity_eyeIndex].xyz
                                        + _CameraViewXExtent[unity_eyeIndex].xyz * uv.x
                                        + _CameraViewYExtent[unity_eyeIndex].xyz * uv.y;
                    viewPos *= zScale;
                #endif

                return half3(viewPos);
            }

            float4 TransformViewToHScreen(float3 vpos,float2 screenSize) {  
                float4 cpos = mul(UNITY_MATRIX_P, float4(vpos,1));  
                cpos.xy = float2(cpos.x, cpos.y * _ProjectionParams.x) * 0.5 + 0.5 * cpos.w;  
                cpos.xy *= screenSize;  
                return cpos;  
            }  





           
            float ProjectionDepthToLinearDepth(float depth,bool isTestDepth)
            {
                return GetLinearEyeDepth(depth);
 
               
            }
            float3 IntersectDepthPlane(float3 RayOrigin, float3 RayDir, float t)
            {
                return RayOrigin + RayDir * t;
            }

            float2 GetCellCount(float2 Size, float Level)
            {
                return floor(Size / (Level > 0.0 ? exp2(Level) : 1.0));
            }

            float2 GetCell(float2 pos, float2 CellCount)
            {
                return floor(pos * CellCount);
            }
            float GetNearestDepthPlane(float2 p, int mipLevel)
            {
         //       int mipLevel1=clamp(mipLevel,0,MaxHiZBufferTextureMipLevel);
                #if UNITY_REVERSED_Z
                return SAMPLE_TEXTURE2D_X_LOD(HiZBufferTexture,sampler_point_clamp,p,mipLevel).x;
                #else
                  return SAMPLE_TEXTURE2D_X_LOD(HiZBufferTexture,sampler_point_clamp,p,mipLevel).x;
                #endif
            }
              float GetMaximumDepthPlane(float2 p, int mipLevel)
            {
         //       int mipLevel1=clamp(mipLevel,0,MaxHiZBufferTextureMipLevel);
                #if UNITY_REVERSED_Z
                return SAMPLE_TEXTURE2D_X_LOD(HiZBufferTexture,sampler_point_clamp,p,mipLevel).y;
                #else
                  return SAMPLE_TEXTURE2D_X_LOD(HiZBufferTexture,sampler_point_clamp,p,mipLevel).y;
                #endif
            }
            float3 IntersectCellBoundary(float3 o, float3 d, float2 cell, float2 cell_count, float2 crossStep, float2 crossOffset)
            {
                float3 intersection = 0;
	
                float2 index = cell + crossStep;
                float2 boundary = index / cell_count;
            //    boundary += crossOffset;
	
                float2 delta = boundary - o.xy;
                delta /= d.xy;
                float t = min(delta.x, delta.y);
   
                intersection = IntersectDepthPlane(o, d, t);
               intersection.xy += (delta.x < delta.y) ? float2(crossOffset.x, 0.0) : float2(0.0, crossOffset.y);
                return intersection;
            }
            inline bool FloatEqApprox(float a, float b) {
                const float eps = 0.00000001f;
                return abs(a - b) < eps;
            }
            bool CrossedCellBoundary(float2 CellIdxA, float2 CellIdxB)
            {
             //  return CellIdxA.x!=CellIdxB.x || CellIdxA.y!=CellIdxB.y;
                return !FloatEqApprox( CellIdxA.x,CellIdxB.x) || !FloatEqApprox( CellIdxA.y,CellIdxB.y);
            }
            
                 uniform  Texture2D CameraLumTex;
                 float3 ImportanceSampleGGX(float2 Xi, float3 N, float roughness)
                    {
                        float a = roughness * roughness;
	
                        float phi = 2.0 * PI * Xi.x;
                        float cosTheta = sqrt(max(((1.0 - Xi.y) / (1.0 + (a * a - 1.0) * Xi.y)),1e-8));
                        float sinTheta = sqrt(max((1.0 - cosTheta * cosTheta),1e-8));
	
	                    // from spherical coordinates to cartesian coordinates - halfway vector
                        float3 H;
                        H.x = cos(phi) * sinTheta;
                        H.y = sin(phi) * sinTheta;
                        H.z = cosTheta;
	
	                    // from tangent-space H vector to world-space sample vector
                        float3 up = abs(N.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
                        float3 tangent = normalize(cross(up, N));
                        float3 bitangent = cross(N, tangent);
	
                        float3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
                        return normalize(sampleVec);
                    }


                    float Random2DTo1D(float2 value,float a ,float2 b)
                    {			
	                    //avaoid artifacts
	                    float2 smallValue = sin(value);
	                    //get scalar value from 2d vector	
	                    float  random = dot(smallValue,b);
	                    random = frac(sin(random) * a);
	                    return random;
                    }

                    float2 Random2DTo2D(float2 value){
	                    return float2(
		                    Random2DTo1D(value,14375.5964, float2(15.637, 76.243)),
		                    Random2DTo1D(value,14684.6034,float2(45.366, 23.168))
	                    );
                    }

            float4 SSRTracing(Varyings input):SV_Target{
                  float rawDepth=SAMPLE_TEXTURE2D_X_LOD(HiZBufferTexture,sampler_point_clamp,input.texcoord,0);
                  float linearDepth = GetLinearEyeDepth(rawDepth);
                
                   float3 vnormal = (SampleSceneNormals(input.texcoord).xyz);
                
                 
                


                  float3 rayOriginWorld = ReconstructViewPos(input.texcoord,linearDepth); 
                   
                  #ifndef _ORTHOGRAPHIC
                  rayOriginWorld=rayOriginWorld+ vnormal * (0.01+5.1*(linearDepth/ _ProjectionParams.z));
                  #else
                  rayOriginWorld=rayOriginWorld+ vnormal * (0.04);
                  #endif
                  #if SSR_USE_FORWARD_RENDERING
                        float roughness=0.04;
                  #else 
                        float roughness=1-SAMPLE_TEXTURE2D_X(_CameraNormalsTexture,sampler_point_clamp,input.texcoord).a;
                  #endif
                   
                   UNITY_BRANCH
                   if(1.0f-roughness<SSRMinSmoothness){
                       return float4(0,0,0,0);
                       }
                        #if defined(_ORTHOGRAPHIC)
                        float3 invViewDirWS = -normalize(UNITY_MATRIX_V[2].xyz);

                         float3 vDir=invViewDirWS;
                        float3 rDir = reflect(vDir, normalize(vnormal));
                        #else
                         float3 vDir=normalize(rayOriginWorld);
                        float3 rDir = reflect(vDir, normalize(vnormal));
                        #endif
                 
                  float3 finalRDir=rDir;

                      
                      
                  UNITY_BRANCH
                   if(UseNormalImportanceSampling){
                     
                
                    float2 randomVal=Random2DTo2D(input.texcoord+float2(_Time.y,-_Time.y));
                    float3 importanceSampleRDir=normalize(ImportanceSampleGGX(randomVal,vnormal,roughness));

                        rDir = reflect(vDir, normalize(importanceSampleRDir));
                    finalRDir=rDir;
                         
                       }
 
                  float3 viewPosOrigin=mul(UNITY_MATRIX_V,float4(rayOriginWorld.xyz,0)).xyz;

                
                 float3 viewRDir = normalize(mul(UNITY_MATRIX_V,float4(finalRDir, 0)).xyz);

                   
                 float maxDist =1000;
               
                   float end = viewPosOrigin.z + viewRDir.z * maxDist;
                    if (end > - _ProjectionParams.y)
                    {
                        maxDist = (-_ProjectionParams.y - viewPosOrigin.z) / viewRDir.z;
                    }
                    float3 worldPosEnd = rayOriginWorld + rDir * maxDist;
                  
                    float4 startScreenViewSpace=mul(_CameraViews[unity_eyeIndex],float4(rayOriginWorld.xyz,0)).xyzw;

                    float4 startScreenNDCSpace=mul(_CameraProjections[unity_eyeIndex],float4(startScreenViewSpace.xyz,1)).xyzw;
                    startScreenNDCSpace.xyz/=startScreenNDCSpace.w;
            
                    startScreenNDCSpace.xyz=startScreenNDCSpace.xyz*0.5+0.5;
                    #if  UNITY_REVERSED_Z
                     startScreenNDCSpace.z=1-startScreenNDCSpace.z;
                     #endif
                    float4 endScreenViewSpace=mul(_CameraViews[unity_eyeIndex],float4(worldPosEnd.xyz,0)).xyzw;

                    float4 endScreenNDCSpace=mul(_CameraProjections[unity_eyeIndex],float4(endScreenViewSpace.xyz,1)).xyzw;
                    endScreenNDCSpace.xyz/=endScreenNDCSpace.w;

               
                    endScreenNDCSpace.xyz=endScreenNDCSpace.xyz*0.5+0.5;

                      #if  UNITY_REVERSED_Z
                     endScreenNDCSpace.z=1-endScreenNDCSpace.z;
                     #endif
                    float3 startScreenTextureSpace = float3(startScreenNDCSpace.xyz);
               
                    float3 endScreenTextureSpace = float3(endScreenNDCSpace.xyz);
               
                     float3 reflectDirTextureSpace = normalize(endScreenTextureSpace - startScreenTextureSpace);
    
    
    
                    float outMaxDistance = reflectDirTextureSpace.x >= 0 ? (1 - startScreenTextureSpace.x) / reflectDirTextureSpace.x : -startScreenTextureSpace.x / reflectDirTextureSpace.x;
                    outMaxDistance = min(outMaxDistance, reflectDirTextureSpace.y < 0 ? (-startScreenTextureSpace.y / reflectDirTextureSpace.y) : ((1 - startScreenTextureSpace.y) / reflectDirTextureSpace.y));
                    outMaxDistance = min(outMaxDistance, reflectDirTextureSpace.z < 0 ? (-startScreenTextureSpace.z / reflectDirTextureSpace.z) : ((1 - startScreenTextureSpace.z) / reflectDirTextureSpace.z));

                  //  outMaxDistance=1;


                    
                       bool isIntersecting = false;

                        int maxLevel = MaxHiZBufferTextureMipLevel;
                        float2 crossStep = float2(reflectDirTextureSpace.x >= 0 ? 1 : -1, reflectDirTextureSpace.y >= 0 ? 1 : -1);
                        float2 crossOffset = crossStep.xy / ( SSRSourceSize.xy) / 64.0;
                       
                        crossStep = saturate(crossStep);
        
                        float3 ray = startScreenTextureSpace.xyz;
                        float nearZ = ray.z;
                        float farZ = ray.z + reflectDirTextureSpace.z * outMaxDistance;
    
                        float deltaZ = (farZ - nearZ);

                        float3 o = ray;
                        float3 d = reflectDirTextureSpace * outMaxDistance;
    
    
                        int startLevel =0;
                        int stopLevel = 0;
    
    
                        float2 startCellCount = GetCellCount( SSRSourceSize.xy, startLevel);
	
                        float2 rayCell = GetCell(ray.xy, startCellCount);
                        ray = IntersectCellBoundary(o, d, rayCell, startCellCount, crossStep, crossOffset);
    
                        int level = startLevel;
                        uint iter = 0;
                     #if UNITY_REVERSED_Z
                        bool isBackwardRay = reflectDirTextureSpace.z> 0;
                        float rayDir = isBackwardRay ? 1 : -1;
                     #else
                      bool isBackwardRay = reflectDirTextureSpace.z< 0;
                        float rayDir = isBackwardRay ? -1 : 1;

                     #endif
                        float thickness = 0;
                        float rayZLinear=0;
                         float cellNearZLinear =0;
                        UNITY_LOOP
                        while (level >= stopLevel &&  ray.z*rayDir <= farZ*rayDir &&iter <MaxIterations)
                        {
        
                            float2 cellCount = GetCellCount( SSRSourceSize.xy, level);
                             float2 oldCellIdx = GetCell(ray.xy, cellCount);
                             
                                float cell_nearZ =GetNearestDepthPlane((oldCellIdx+0.5f) / cellCount, level);
                                float cell_maxZ=GetMaximumDepthPlane((oldCellIdx+0.5f) / cellCount, level);
                       
                #if UNITY_REVERSED_Z
                            float3 tmpRay = (((cell_nearZ)< ray.z) && !isBackwardRay) ? IntersectDepthPlane(o, d, ((cell_nearZ) - nearZ) / deltaZ) : ray;
                            //if ray intersects depth buffer on current mip level then keep it as last iteration and go down a mip level
                            //otherwise ray is not intersected, try move it further using IntersectCellBoundary
                         
                #else
                            float3 tmpRay = (((cell_nearZ)> ray.z) && !isBackwardRay) ? IntersectDepthPlane(o, d,((cell_nearZ) - nearZ) / deltaZ) : ray;

                           
                #endif
                             float2 newCellIdx = GetCell(tmpRay.xy, cellCount);
        
                           
                            float thicknessMaxZ=0;
                             rayZLinear = ProjectionDepthToLinearDepth(ray.z, true);
                             cellNearZLinear = ProjectionDepthToLinearDepth(cell_nearZ,false);
                             float cellMaxZLinear = ProjectionDepthToLinearDepth(cell_maxZ,false);
                            if (level <= stopLevel)
                            {
                                thickness = abs(rayZLinear
                                 - cellNearZLinear);
                                 thicknessMaxZ= abs(rayZLinear
                                 - cellMaxZLinear);
                            }
                            else
                            {
                                thicknessMaxZ=0;
                                thickness = 0;
          
                            }
                            bool crossed = false;
                            bool crossedBehind = false;
                           // (isBackwardRay && ) ||
                            if (isBackwardRay)
                            {
                             
                                if ((cellNearZLinear > rayZLinear ))
                                {
                                    crossed = true;
                                
                                }else if((cellNearZLinear+0.02  <rayZLinear && thickness >= SSRThickness*1.1)){
                                crossedBehind = true;
                               
                                    crossed = false;
                                }
                             }else{
                            if ((cellNearZLinear+0.02< rayZLinear && thickness >= SSRThickness*1.1))
                            {
                                crossedBehind = true;//tracing ray behind downgrades into linear search
      //
                            }

                            else if (CrossedCellBoundary(oldCellIdx, newCellIdx))
                            {
                                crossed = true;
                            }
                            else
                            {
                                crossed = false;
                            }
                             }
                           
        
       
      
                            if (crossed == true||crossedBehind==true)
                            {
                                    ray = IntersectCellBoundary(o, d, oldCellIdx, cellCount, crossStep, crossOffset);
                                    level = min(level + 1.0f,maxLevel);
                            /*   if( rayZLinear >  _ProjectionParams.z*0.9 || cellMinZLinear > _ProjectionParams.z*0.9){
                                        isIntersecting = false;
                                        break;
                                     }else{
                                    
                                     }*/
                                     
                            

                            }
                           
                            else
                            {
                                ray = tmpRay;
                                level = max(level - 1, 0);
                           
                               
                            }
                            [branch]
                            if (ray.x < 0 || ray.y < 0 || ray.x > 1 || ray.y > 1)
                            {
                                isIntersecting = false;
                                break;
                            }
                            if (level <= stopLevel)
                            {
                                thickness = abs(rayZLinear
                                 - cellNearZLinear);
                                 thicknessMaxZ= abs(rayZLinear
                                 - cellMaxZLinear);
                            }
                            else
                            {
                                thicknessMaxZ=0;
                                thickness = 0;
          
                            }
                            
                            if (level <= stopLevel)
                            {
                         
                           
                                if (thickness< SSRThickness&&rayZLinear< cellNearZLinear+0.1)
                                {
                                    if( rayZLinear >  _ProjectionParams.z*0.9 || cellNearZLinear > _ProjectionParams.z*0.9){
                                        isIntersecting = false;
                                        break;
                                     }
                                    
                                    isIntersecting = true;
                                    break;
                                } 
                                else{

                                    isIntersecting=false;
                                    
                                 }
          
            
          
                            }
      
                            ++iter;
                            
                        }
                        
                      
                    float2 uv =ray.xy;
                
                        if(isIntersecting==true){
            
                            
                  float rawDepth2=SAMPLE_TEXTURE2D_X_LOD(HiZBufferTexture,sampler_point_clamp,uv,0);
                  float linearDepth2 = GetLinearEyeDepth(rawDepth2);
                  float3 hitPosWorld = ReconstructViewPos(uv,linearDepth2)+ProjectionParams2.yzw; 
                    float rayLength=length(hitPosWorld-rayOriginWorld);
                            if(isBackwardRay==true){
                          //      return float4(1,1,1,1);
                            }
                             return float4(uv.xy,rayLength/100.0f,1);//SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_point_clamp,uv);
                            }else{
                          //    return float4(uv.xy,rayLength/100.0f,1);
                                return float4(0,0,0,0);
                          }
            }
            



            #define PI 3.1415926
        
                float3 fresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
                {
                    return F0 + (max(float3(1.0 - roughness, 1.0 - roughness, 1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
                }

                float fresnelSchlickRoughness(float cosTheta, float F0, float roughness)
                {
                    return F0 + (max(1.0 - roughness, F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
                }
                    uniform Texture2D _GBuffer0;  
                    uniform Texture2D _GBuffer1;  
                    uniform Texture2D _GBuffer2;  
                    SamplerState sampler_Point_Clamp;
         
                 uniform Texture2D SSRResultTex;
                 uniform Texture2D BRDFLutTex;


                   uniform Texture2D ColorPyramidTexture;
                   uniform float MaxColorPyramidMipLevel;

                   uniform Texture2D PrevSSRBlendResult;

                   uniform float4 AmbientSH[7];



                    TEXTURE2D_X(_MotionVectorTexture);
                    SAMPLER(sampler_MotionVectorTexture);
                    
                   

                    void AdjustColorBox(float2 uv, inout float3 boxMin, inout float3 boxMax,Texture2D tex) {
                         const float2 kOffssets3x3[9]={
                        float2(0,0),
                        float2(1,1),
                        float2(-1,-1),
                        float2(-1,1),
                        float2(1,-1),
                        float2(0,1),
                        float2(1,0),
                        float2(0,-1),
                        float2(-1,0)
                        };
                        boxMin = 1.0;
                        boxMax = 0.0;

                        UNITY_UNROLL
                        for (int k = 0; k < 9; k++) {
                            float3 C = RGBToYCoCg(SAMPLE_TEXTURE2D_X(tex,sampler_point_clamp,uv + kOffssets3x3[k] * SSRSourceSize.zw*2));
                            boxMin = min(boxMin, C);
                            boxMax = max(boxMax, C);
                        }
                    }
                           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"  
                               #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"  




 

half4 UniversalFragmentPBRCustom(InputData inputData, SurfaceData surfaceData,BRDFData brdfData)
{

   
    #if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
  //  BRDFData brdfData=(BRDFData)0;

    // NOTE: can modify "surfaceData"...
  //  InitializeBRDFData(surfaceData, brdfData);

    #if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
    {
        return debugColor;
    }
    #endif

    // Clear-coat calculation...
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLayer();
    Light mainLight = GetMainLight(inputData, float4(1,1,1,1), aoFactor);
    
  
    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    float3 diffuse=SampleSH9(AmbientSH,inputData.normalWS);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    lightingData.giColor = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                              inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS, inputData.normalizedScreenSpaceUV,diffuse,0);
 
    {
        lightingData.mainLightColor = LightingPhysicallyBased(brdfData, brdfDataClearCoat,
                                                              mainLight,
                                                              inputData.normalWS, inputData.viewDirectionWS,
                                                              surfaceData.clearCoatMask, specularHighlightsOff);
    }

   /* #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

 
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased(brdfData, brdfDataClearCoat, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

 
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased(brdfData, brdfDataClearCoat, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
    #endif*/

#if REAL_IS_HALF
    // Clamp any half.inf+ to HALF_MAX
    return min(CalculateFinalColor(lightingData, surfaceData.alpha), HALF_MAX);
#else
    return CalculateFinalColor(lightingData, surfaceData.alpha);
#endif
}




SurfaceData SurfaceDataFromGbufferCustom(half4 gbuffer0, half4 gbuffer1, half4 gbuffer2, int lightingMode)
{
    SurfaceData surfaceData=(SurfaceData)0;

    surfaceData.albedo = gbuffer0.rgb;
    uint materialFlags = UnpackMaterialFlags(gbuffer0.a);
    surfaceData.occlusion = 1.0; // Not used by SimpleLit material.
    surfaceData.specular = gbuffer1.rgb;
    half smoothness = gbuffer2.a;

    surfaceData.metallic = 0.0; // Not used by SimpleLit material.
    surfaceData.alpha = 1.0; // gbuffer only contains opaque materials
    surfaceData.smoothness = smoothness;

    surfaceData.emission = (half3)0; // Note: this is not made available at lighting pass in this renderer - emission contribution is included (with GI) in the value GBuffer3.rgb, that is used as a renderTarget during lighting
    surfaceData.normalTS = (half3)0; // Note: does this normalTS member need to be in SurfaceData? It looks like an intermediate value

    return surfaceData;
}

     
            float4 SSRGenerateReflectionFrag(Varyings input):SV_TARGET{
                
                if(SAMPLE_TEXTURE2D_X_LOD(SSRResultTex, sampler_point_clamp, input.texcoord, 0).a<0.1){
                return float4(0,0,0,0);
                }
                float2 screen_uv=SAMPLE_TEXTURE2D_X_LOD(SSRResultTex, sampler_point_clamp, input.texcoord, 0);
                float d        = SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, sampler_point_clamp, screen_uv, 0).x;
                half4 gbuffer0 = SAMPLE_TEXTURE2D_X_LOD(_GBuffer0, sampler_point_clamp, screen_uv, 0);
                half4 gbuffer1 = SAMPLE_TEXTURE2D_X_LOD(_GBuffer1, sampler_point_clamp, screen_uv, 0);
                half4 gbuffer2 = SAMPLE_TEXTURE2D_X_LOD(_GBuffer2, sampler_point_clamp, screen_uv, 0);


                float linearDepth = GetLinearEyeDepth(d);

                if(linearDepth>_ProjectionParams.z*0.9f){
                return float4(0,0,0,0);
                        }
                float3 worldPos = ReconstructViewPos(screen_uv,linearDepth)+ProjectionParams2.yzw; 
                SurfaceData surfaceData=(SurfaceData)0;
                surfaceData=SurfaceDataFromGbufferCustom(gbuffer0,gbuffer1,gbuffer2,0);
                InputData inputData=InputDataFromGbufferAndWorldPosition(gbuffer2,worldPos);

                BRDFData brdfData= BRDFDataFromGbuffer(gbuffer0,gbuffer1,gbuffer2);
                float4 shadowCoord = TransformWorldToShadowCoord(worldPos.xyz);
                inputData.shadowCoord=shadowCoord;
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                inputData.shadowCoord=float4(screen_uv.xy,0,1);
                #endif
                inputData.normalizedScreenSpaceUV=screen_uv;
                half4 reflectedCol= UniversalFragmentPBRCustom(inputData,surfaceData,brdfData);



                
                float dOrigin        = SAMPLE_TEXTURE2D_X_LOD(_CameraDepthTexture, sampler_point_clamp, input.texcoord, 0).x;
                half4 gbuffer0Origin = SAMPLE_TEXTURE2D_X_LOD(_GBuffer0, sampler_point_clamp, input.texcoord, 0);
                half4 gbuffer1Origin = SAMPLE_TEXTURE2D_X_LOD(_GBuffer1, sampler_point_clamp, input.texcoord, 0);
                half4 gbuffer2Origin = SAMPLE_TEXTURE2D_X_LOD(_GBuffer2, sampler_point_clamp, input.texcoord, 0);

                float linearDepthOrigin = GetLinearEyeDepth(dOrigin);
                float3 worldPosOrigin = ReconstructViewPos(input.texcoord,linearDepthOrigin)+ProjectionParams2.yzw; 

                SurfaceData surfaceDataOrigin=(SurfaceData)0;
                surfaceDataOrigin=SurfaceDataFromGbufferCustom(gbuffer0Origin,gbuffer1Origin,gbuffer2Origin,0);
                InputData inputDataOrigin=InputDataFromGbufferAndWorldPosition(gbuffer2Origin,worldPosOrigin);


                    float3 diffuse=SampleSH9(AmbientSH,inputDataOrigin.normalWS);
                   // BRDFData brdfDataOrigin;
                    BRDFData brdfDataOrigin= BRDFDataFromGbuffer(gbuffer0Origin,gbuffer1Origin,gbuffer2Origin);
                  //  InitializeBRDFData(surfaceDataOrigin.albedo, surfaceDataOrigin.metallic, surfaceDataOrigin.specular, surfaceDataOrigin.smoothness, surfaceDataOrigin.alpha, brdfDataOrigin);

                    BRDFData brdfDataNoClearcoat=(BRDFData)0;
                    half3 color = GlobalIllumination(brdfDataOrigin,brdfDataNoClearcoat,0, inputDataOrigin.bakedGI, surfaceDataOrigin.occlusion, inputDataOrigin.positionWS, inputDataOrigin.normalWS, inputDataOrigin.viewDirectionWS,float2(0,0),diffuse,reflectedCol.xyz);
                        
               
                return float4(color,1);
            }

             float4 SSRFinalBlend(Varyings input):SV_TARGET{
             return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_linear_clamp, input.texcoord, 0);
             }