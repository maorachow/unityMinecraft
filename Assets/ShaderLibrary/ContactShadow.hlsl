
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
              #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
               #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
             
            uniform int SampleCount;
            uniform float EdgeWidth;
            uniform float StepLength;
            uniform float ShadowWeight;
            uniform float ContactShadowBias;
            uniform float FadeDistance;
            uniform float4 _BlitScreenParams;

           
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

                #if UNITY_REVERSED_Z
                 return lerp(_ProjectionParams.y,_ProjectionParams.z,1-rawDepth);
                #else
                return lerp(_ProjectionParams.y,_ProjectionParams.z,rawDepth);
                #endif
               

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
                    float zScale = (linearDepth-_ProjectionParams.y) /(_ProjectionParams.z-_ProjectionParams.y); // divide by far plane
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

 
              SamplerState sampler_point_clamp;
            float4 frag (Varyings input) : SV_Target
            {
                
         
              
         //       if(SampleS      ceneDepth(i.texcoord)<=0.0001){
         //       return float4(0,0,0,0);
          //      }
               
                float rawDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_point_clamp,input.texcoord).r;  
                  
                float linearDepth = GetLinearEyeDepth(rawDepth); 
                
                float3 wpos = ReconstructViewPos(input.texcoord,linearDepth);  
              //  float3 worldPos=vpos+_WorldSpaceCameraPos;
               //       return float4(wpos,1);
                float3 normal=SampleSceneNormals(input.texcoord).xyz;
                #ifndef _ORTHOGRAPHIC
                 wpos=wpos+ normal * 0.01 ;
                #else
                 wpos=wpos+ normal * 0.01 ;
                #endif
               
          //       return float4((worldPos-_WorldSpaceCameraPos),1);
                float3 lightDir=GetMainLight().direction;
                float3 lightDirView=mul(_CameraViews[unity_eyeIndex],float4(lightDir,0));
                  
                
                float magnitude =1;  

             
                float3 startView=mul(_CameraViews[unity_eyeIndex],float4(wpos,0));
                if(length(startView.z)>FadeDistance){
                          return float4(1,1,1,1);
                }
             
                    float end = startView.z + lightDirView.z * magnitude;
                    if (end > - _ProjectionParams.y)
                    {
                        magnitude = (-_ProjectionParams.y - startView.z) / lightDirView.z;
                    }

                    if (end < - _ProjectionParams.z)
                    {
                        magnitude = (-_ProjectionParams.z - startView.z) / lightDirView.z;
                    }
                float3 endView =startView+lightDirView*magnitude;
               
 

                    float4 startScreenNDCSpace=mul(_CameraProjections[unity_eyeIndex],float4(startView.xyz,1)).xyzw;
                    startScreenNDCSpace.xyz/=startScreenNDCSpace.w;
            
                    startScreenNDCSpace.xyz=startScreenNDCSpace.xyz*0.5+0.5;
                    #if  UNITY_REVERSED_Z
                     startScreenNDCSpace.z=1-startScreenNDCSpace.z;
                     #endif


                    float4 endScreenNDCSpace=mul(_CameraProjections[unity_eyeIndex],float4(endView.xyz,1)).xyzw;
                    endScreenNDCSpace.xyz/=endScreenNDCSpace.w;

               
                    endScreenNDCSpace.xyz=endScreenNDCSpace.xyz*0.5+0.5;

                      #if  UNITY_REVERSED_Z
                     endScreenNDCSpace.z=1-endScreenNDCSpace.z;
                     #endif
                    float3 startScreenTextureSpace = float3(startScreenNDCSpace.xyz);
                
                    float3 endScreenTextureSpace = float3(endScreenNDCSpace.xyz);
              
                     float3 rayDirTextureSpace = normalize(endScreenTextureSpace - startScreenTextureSpace);
    
                        float outMaxDistance = rayDirTextureSpace.x >= 0 ? (1 - startScreenTextureSpace.x) / rayDirTextureSpace.x : -startScreenTextureSpace.x / rayDirTextureSpace.x;
                        outMaxDistance = min(outMaxDistance, rayDirTextureSpace.y < 0 ? (-startScreenTextureSpace.y / rayDirTextureSpace.y) : ((1 - startScreenTextureSpace.y) / rayDirTextureSpace.y));
                        outMaxDistance = min(outMaxDistance, rayDirTextureSpace.z < 0 ? (-startScreenTextureSpace.z / rayDirTextureSpace.z) : ((1 - startScreenTextureSpace.z) / rayDirTextureSpace.z));

                        float3 rayEndTextureSpace=startScreenTextureSpace+rayDirTextureSpace*outMaxDistance;
                       
                        float3 dp=rayEndTextureSpace-startScreenTextureSpace;
                        float2 sampleScreenPos=startScreenTextureSpace.xy*_BlitScreenParams.xy;
                         float2 sampleScreenPosEnd=rayEndTextureSpace.xy*_BlitScreenParams.xy;
                         float2 dPix=sampleScreenPosEnd-sampleScreenPos;

                            float pixDelta= max(abs(dPix.x), abs(dPix.y));
                            dp /= pixDelta;
                            dp*=StepLength;
                            float4 rayPosTextureSpace = float4(startScreenTextureSpace.xyz, 0);
                            float4 deltaRayDirTextureSpace = float4(dp.xyz, 0);

                            float contactShadow=1;
                            UNITY_LOOP
                            for(int i=0;i<SampleCount;i++){
                               rayPosTextureSpace += deltaRayDirTextureSpace;
                            float sampleDepth = GetLinearEyeDepth(SampleSceneDepth(rayPosTextureSpace.xy)) ;
                            float testDepth= GetLinearEyeDepth(rayPosTextureSpace.z);
	                        float thickness = abs(testDepth- sampleDepth);
                            if(rayPosTextureSpace.x<=0||rayPosTextureSpace.x>=1||rayPosTextureSpace.y<=0||rayPosTextureSpace.y>=1){
                            break;
                            }

                           
	                        if( testDepth>sampleDepth && thickness< EdgeWidth)
	                        {
                             if(sampleDepth<_ProjectionParams.y||sampleDepth>_ProjectionParams.z*0.99||testDepth<_ProjectionParams.y||testDepth>_ProjectionParams.z){
                                    contactShadow=0;
                                    break;
                                }
	                           contactShadow=0;
		                        break;
	                        }

                           
                            }
                return float4(contactShadow.xxx,1);
                

        //       return float4(shadow.xxxx);
            }