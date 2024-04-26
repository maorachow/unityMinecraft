Shader "CustomEffects/SSIDEffect"
{
    
     
           
 
 
  Properties {
        _MainTex ("Base (RGB)", 2D) = "" {}
    }

    
    
   
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "SSIDQuad"

            HLSLPROGRAM
      //      #include "UnityStandardCore.cginc"
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"  
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"  
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl" 
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"  
   
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
              #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Deferred.hlsl"
        //    #include "com.unity.render-pipelines.universal/Shaders/Utils/UnityGBuffer.hlsl"
              #pragma vertex Vert
              #pragma fragment SSIDPassFragment
               #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
       
 
           
 
                
            uniform float SSIDIntensity;
             uniform float SSIDStepCount;
              uniform float SSIDRayCount;
               uniform float SSIDStepLength;
            uniform Texture2D HiZBufferTexture;
            uniform float MaxHiZufferTextureMipLevel;
             SamplerState sampler_point_clamp;
              SamplerState sampler_point_repeat;
          
             uniform Texture2D SSIDNoiseTex;
             uniform float SSIDRadius;
             uniform float SSIDFadeDistance;
                 uniform float4 ProjectionParams2;
            uniform float4 CameraViewTopLeftCorner;
            uniform float4 CameraViewXExtent;
            uniform float4 CameraViewYExtent;
            float3 ReconstructViewPos(float2 uv, float linearEyeDepth) {  
                // Screen is y-inverted  
                uv.y = 1.0 - uv.y;  

                float zScale = linearEyeDepth * ProjectionParams2.x; // divide by near plane  
                float3 viewPos = CameraViewTopLeftCorner.xyz + CameraViewXExtent.xyz * uv.x + CameraViewYExtent.xyz * uv.y;  
                viewPos *= zScale;  
                return viewPos;  
            }
            half3 ReconstructViewPosMatrix(float2 uv) {  
                // Screen is y-inverted  
           //     uv.y = 1.0 - uv.y;  
           float rawDepth=0;
                #if UNITY_REVERSED_Z
                    rawDepth = SampleSceneDepth(uv);
                #else
                    //  调整 Z 以匹配 OpenGL 的 NDC ([-1, 1])
                    rawDepth= lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(uv));
                #endif
                float3 worldPos = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
               worldPos-=_WorldSpaceCameraPos;
                return worldPos;
            }
             
            void ReconstructUVAndDepthMatrix(float3 wpos, out float2 uv, out float depth) {  
                float4 cpos = mul(UNITY_MATRIX_VP, float4(wpos,0));  
                uv = float2(cpos.x, cpos.y * _ProjectionParams.x) / cpos.w * 0.5 + 0.5;  
                depth = cpos.w;  
            }

            void ReconstructUVAndDepth(float3 wpos, out float2 uv, out float depth) {  
                float4 cpos = mul(UNITY_MATRIX_VP, wpos);  
                uv = float2(cpos.x, cpos.y * _ProjectionParams.x) / cpos.w * 0.5 + 0.5;  
                depth = cpos.w;  
            }
            half4 GetSource(half2 uv) {  
                return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);  
            }


 

            float3 GetWorldPosition(float2 vTexCoord, float depth)
            {
              #if UNITY_REVERSED_Z
                    depth = SampleSceneDepth(vTexCoord);
                #else
                    //  调整 Z 以匹配 OpenGL 的 NDC ([-1, 1])
                    depth= lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(vTexCoord));
                #endif
                float3 worldPos = ComputeWorldSpacePosition(vTexCoord, depth, UNITY_MATRIX_I_VP);
                return worldPos;
            }


            float GetShadow(float3 posWorld)
            {
                float4 shadowCoord = TransformWorldToShadowCoord(posWorld);
                float shadow = MainLightRealtimeShadow(shadowCoord);
                return shadow;
            }  

            float3 UnpackNormal(float3 normal)
            {
            
                float2 remappedOctNormalWS = Unpack888ToFloat2(normal); // values between [ 0,  1]
                float2 octNormalWS = remappedOctNormalWS.xy * 2.0 - 1.0;    // values between [-1, +1]
                normal = UnpackNormalOctQuadEncode(octNormalWS);
             
    
                return normal;
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
            float Random2DTo1D(float2 value){
	            return (
		            Random2DTo1D(value,14375.5964, float2(15.637, 76.243))
		          
	            );
            }
          
            float Random1DTo1D(float value,float a,float b){
	            //make value more random by making it bigger
	            float random = frac(sin(value+b)*a);
                    return random;
            }
              float3 Random1DTo3D(float value){
                return float3(
                    Random1DTo1D(value,14375.5964,0.546),
                    Random1DTo1D(value,18694.2233,0.153),
                    Random1DTo1D(value,19663.6565,0.327)
                );
            }
            TEXTURE2D_X(_GBuffer3);
            TEXTURE2D_X(_GBuffer2);
             TEXTURE2D_X(_GBuffer1);
                TEXTURE2D_X(_GBuffer0);
            float4 SSIDPassFragment(Varyings input) : SV_Target {  

         
                float rawDepth = SampleSceneDepth(input.texcoord).r;  
                
             
              //  return float4(rawDepth1.xxx,1);
               // float4 gbuff = tex2D(_GBuffer2, input.texcoord);
                float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);  
                float3 vpos = ReconstructViewPos(input.texcoord,linearDepth);  
               // return float4(_CameraViewTopLeftCorner.xyz*10000,1); 
             
                float3 vnormal = (SampleSceneNormals(input.texcoord).xyz);  
                 
          //     return float4(vnormal.xyz,1);
                vpos=vpos+ vnormal * (length(vpos-_WorldSpaceCameraPos) / _ProjectionParams.z * 0.2) ;
                float3 wPos=GetWorldPosition(input.texcoord,rawDepth).xyz;

                float3 vDir = normalize(vpos);  
        //         float diff = max(dot(vnormal, vDir), -1.0);
         //       if (diff >= -0.3)
         //       {
         //       return float4(0,0,0,1);
         //       }
         UNITY_BRANCH
          if(length(vpos)>SSIDFadeDistance){
                return float4(0,0,0,0);
                }
               
               float3 randomVec = float3(Random2DTo1D((input.texcoord.xy)*200).r*2-1,
               Random2DTo1D((input.texcoord.xy)*100).r*2-1,0);
                
                float strideLen=SSIDStepLength;
             
                 
               //  float3 curPos=vpos;
                
                float3 tangent =normalize(randomVec - vnormal * dot(randomVec,vnormal));
                float3 bitangent = cross(vnormal,tangent);
                float3x3 TBN = float3x3(tangent,bitangent,vnormal);
                
              
              //  return float4(randomVec.xyz,1);
                float4 finalColor=float4(0,0,0,0);
 
                for(int j=0;j<SSIDRayCount;j++){
                 float3 curPos=vpos;
                 float3 randomDir=Random1DTo3D(j+curPos.x*curPos.y+curPos.z+_Time.x);
                 randomDir=normalize(float3(randomDir.x*2-1,randomDir.y*2-1,randomDir.z));
                float3 rDir = (normalize(mul(randomDir,TBN)));  
            //  return float4(rDir.xyz,1);
                float mipLevel = 0.0;
                bool hit=false;
                float2 finalUV=0.0; 
            
                UNITY_LOOP
                for(int i = 0; i < SSIDStepCount; i++){
                    curPos += rDir*strideLen;// 步近
                         float2 uv1=0;
                     float resultDepth=0;
                    ReconstructUVAndDepthMatrix(curPos,uv1,resultDepth);
                    float sampleDepth=0.0;
                    
                    
                    
                     
                    
                     sampleDepth = LinearEyeDepth(SAMPLE_TEXTURE2D_X_LOD(HiZBufferTexture,sampler_point_clamp,uv1,mipLevel), _ZBufferParams);
               //      return float4(SAMPLE_TEXTURE2D_X_LOD(HiZBufferTexture,sampler_point_clamp,uv1,2).xxx,1);
                    
                    
                    if(resultDepth>sampleDepth){
                    
                        if(mipLevel <= 0){
                         if (resultDepth>sampleDepth &&abs(resultDepth-sampleDepth) <  0.1 )
                           {
                              if(sampleDepth/_ProjectionParams.z>0.9||resultDepth/_ProjectionParams.z>0.9){
                               hit=false;
                               break;
                             }
                              if(uv1.x<0||uv1.x>1||uv1.y<0||uv1.y>1){
                              hit=false;
                               break;
                             }
                               finalUV=uv1;
                               hit=true;
                            //   return i/16.0;
                               break;
                            }
                        }
                     //   return float4(mipLevel.xxx/MaxHiZufferTextureMipLevel,1);
                              mipLevel --; 
                        
                       curPos -= rDir*strideLen; 
                       
                        strideLen /= 2.0;

                       
                    }
                    else{
                    if(mipLevel<MaxHiZufferTextureMipLevel){
                        mipLevel ++;
                        
                    strideLen *=2.0;
                    }
                         
                    }

                }
                if(hit==true){
                float lightWeight=(SSIDRadius-length(curPos-vpos))/SSIDRadius;
              //   float brightness=(dot(SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_point_clamp,input.texcoord).rgb,float3(0.2126, 0.7152, 0.0722)));
                   
             //   float brightness=length(SAMPLE_TEXTURE2D_X_LOD(_GBuffer3,sampler_point_clamp,finalUV,mipLevel).rgb);
             
                finalColor +=float4(GetSource(finalUV).rgb*lightWeight,1*lightWeight);
              // finalColor+=float4(lightWeight.xxx/20,1);
                }  
                
                }
                finalColor/=SSIDRayCount;
                finalColor*=SSIDIntensity;
                return (finalColor);
           //     UNITY_LOOP
          //       for (int i = 0;i <StepCount; i++)
           //             {
          //          // Has it hit anything yet
          //              if (hit == false)
          //              {
                        // Update the Current Position of the Ray
           //             curPos +=rDir *curLength;
          //              if(length(curPos)>FadeDistance){
           //             return float4(0,0,0,1);
           //             }
                        // Get the UV Coordinates of the current Ray
           //             ReconstructUVAndDepth(curPos,uv1,resultDepth);
                        // The Depth of the Current Pixel
          //              float sampleDepth = LinearEyeDepth(SampleSceneDepth(uv1), _ZBufferParams);
          //   
          //                  if (resultDepth>sampleDepth &&resultDepth < sampleDepth +SSRThickness )
           //                 {
                                // If it's hit something, then return the UV position
           //                   if(uv1.x<0||uv1.x>1||uv1.y<0||uv1.y>1){
            //                     return float4(0,0,0,1);
            //                  }
            //                    return GetSource(uv1);
              //              }
               //         if (resultDepth < 0.00001||sampleDepth<0.00001)
             //           {
                                // If it's hit something, then return the UV position
                             
            //                return float4(0,0,0,1);
            //            }
                           // curDepth = GetDepth(curUV.xy + (float2(0.01, 0.01) * 2));
           

                        // Get the New Position and Vector
                        
           //         }
          //          }
              //  ReconstructUVAndDepth(vpos,uv,depth);
              //  ReconstructUVAndDepthMatrix(ReconstructViewPosMatrix(input.texcoord,linearDepth),uv,depth);
              //  return float4(uv.xy,1,1);
             //   UNITY_LOOP
              //  for (int i = 0; i < 16; i++) {  
              //  float3 vpos2 = vpos + rDir * 1 * i;  
              //  float2 uv2;  
             //   float stepDepth;  
             //   ReconstructUVAndDepth(vpos2, uv2, stepDepth);  
             //   float stepRawDepth = SampleSceneDepth(uv2);  
             //   float stepSurfaceDepth = LinearEyeDepth(stepRawDepth, _ZBufferParams);  
             //   if (stepSurfaceDepth < stepDepth && stepDepth < stepSurfaceDepth + 0.1 )  {
             //    if(uv2.x<0||uv2.x>1||uv2.y<0||uv2.y>1){
            //    return float4(0,0,0,1);
             //   }
            //    return GetSource(uv2);  
            //    }
               
                       
            //    }    
             //   return half4(0.0, 0.0, 0.0, 1.0);  
            }






            ENDHLSL

              HLSLINCLUDE
    
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // The Blit.hlsl file provides the vertex shader (Vert),
        // the input structure (Attributes), and the output structure (Varyings)
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

       uniform float BlurStrength;
        
    
        float4 _BlitTexture_TexelSize;
    
        float4 BlurVertical (Varyings input) : SV_Target
        {
            const float BLUR_SAMPLES = 8;
            const float BLUR_SAMPLES_RANGE = BLUR_SAMPLES / 2;
            
            float4 color = 0;
            float blurPixels = BlurStrength * _ScreenParams.y;
            
            for(float i = -BLUR_SAMPLES_RANGE; i <= BLUR_SAMPLES_RANGE; i++)
            {
                float2 sampleOffset =
                    float2 (0, (blurPixels / _BlitTexture_TexelSize.w) *
                        (i / BLUR_SAMPLES_RANGE));
                color +=
                    SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,
                        input.texcoord + sampleOffset).rgba;
            }
            
            return float4(color.rgba / (BLUR_SAMPLES + 1));
        }

        float4 BlurHorizontal (Varyings input) : SV_Target
        {
            const float BLUR_SAMPLES = 8;
            const float BLUR_SAMPLES_RANGE = BLUR_SAMPLES / 2;
            
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float4 color = 0;
            float blurPixels = BlurStrength * _ScreenParams.x;
            for(float i = -BLUR_SAMPLES_RANGE; i <= BLUR_SAMPLES_RANGE; i++)
            {
                float2 sampleOffset =
                    float2 ((blurPixels / _BlitTexture_TexelSize.z) *
                        (i / BLUR_SAMPLES_RANGE), 0);
                color +=
                    SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,
                        input.texcoord + sampleOffset).rgba;
            }
            return float4(color / (BLUR_SAMPLES + 1));
        }
    
    ENDHLSL
        }
        
        Pass
        {
         Name "Blending"

         ZTest NotEqual
        ZWrite Off
        Cull Off
     //  Blend Off
        Blend SrcAlpha OneMinusSrcAlpha
        HLSLPROGRAM
            
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"  
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"  
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl" 
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

          
              #pragma vertex Vert
              #pragma fragment Blending
              half4 GetSource(half2 uv) {  
                return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);  
                }
              
              float4 Blending(Varyings input):SV_Target{
             
              return GetSource(input.texcoord);
              }

              ENDHLSL
        
        }

        Pass
        {
         Name "BlurVertical"

         ZTest NotEqual
        ZWrite Off
        Cull Off
       Blend Off
      //  Blend SrcAlpha OneMinusSrcAlpha
        HLSLPROGRAM
            
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"  
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"  
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl" 
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

          
              #pragma vertex Vert
              #pragma fragment BlurVertical
              

              ENDHLSL
        
        }
        
        Pass
        {
         Name "BlurHorizontal"

         ZTest NotEqual
        ZWrite Off
        Cull Off
       Blend Off
      //  Blend SrcAlpha OneMinusSrcAlpha
        HLSLPROGRAM
            
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"  
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"  
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl" 
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

          
              #pragma vertex Vert
              #pragma fragment BlurHorizontal
               

              ENDHLSL
        
        }
    }
}