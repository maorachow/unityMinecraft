Shader "CustomEffects/SSREffect"
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
            Name "SSRQuad"

            HLSLPROGRAM
            
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"  
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"  
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"  
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl" 
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"  
   
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
              #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 
           
              #pragma vertex Vert
              #pragma fragment SSRPassFragment
               #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
      
           
         
                
            uniform float StrideSize;
            uniform float SSRThickness;
            uniform int StepCount;
            uniform float FadeDistance;
            uniform Texture2D HiZBufferTexture;
            uniform float MaxHiZufferTextureMipLevel;
             SamplerState sampler_point_clamp;
            // SamplerState sampler2D_float;
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
            uniform sampler2D _GBuffer2;
            float4 SSRPassFragment(Varyings input) : SV_Target {  

         
                float rawDepth = SampleSceneDepth(input.texcoord).r;  
                
                float rawDepth1=SAMPLE_TEXTURE2D_X_LOD(HiZBufferTexture,sampler_point_clamp,input.texcoord,1);
              //  return float4(rawDepth1.xxx,1);
               // float4 gbuff = tex2D(_GBuffer2, input.texcoord);
                float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);  
                float3 vpos = ReconstructViewPos(input.texcoord,linearDepth);  
            //    return float4(vpos.xyz,1);
                float3 vnormal = (SampleSceneNormals(input.texcoord).xyz);
          //     return float4(vnormal.xyz,1);
                vpos=vpos+ vnormal * (length(vpos) / _ProjectionParams.z * 4.2) ;
               

                float3 vDir = normalize(vpos);  
                //return float4(vDir.xyz,1);
        //         float diff = max(dot(vnormal, vDir), -1.0);
         //       if (diff >= -0.3)
         //       {
         //       return float4(0,0,0,1);
         //       }

               

                float3 rDir = normalize(reflect(vDir, vnormal));  
                float strideLen=StrideSize;
                
               
                 
                 float3 curPos=vpos;
               
                
 
                
                float mipLevel = 0.0;
                UNITY_LOOP
                for(int i = 0; i < StepCount; i++){
                    curPos += rDir*strideLen;// 步近
                         float2 uv1=0;
                     float resultDepth=0;
                    ReconstructUVAndDepthMatrix(curPos,uv1,resultDepth);
                    float sampleDepth=0.0;
                    
                    
                    
                     
                    
                     sampleDepth = LinearEyeDepth(SAMPLE_TEXTURE2D_X_LOD(HiZBufferTexture,sampler_point_clamp,uv1,mipLevel), _ZBufferParams);
                    
                    
                    
                    if(resultDepth>sampleDepth){
                    
                        if(mipLevel <= 0){
                         if (resultDepth>sampleDepth &&abs(resultDepth-sampleDepth) <  SSRThickness )
                           {
                              if(sampleDepth/_ProjectionParams.z>0.9||resultDepth/_ProjectionParams.z>0.9){
                               return float4(0,0,0,0);
                             }
                              if(uv1.x<0||uv1.x>1||uv1.y<0||uv1.y>1){
                                 return float4(0,0,0,0);
                             }
                               return GetSource(uv1);
                            }
                        }
                      //  return float4(mipLevel.xxx/MaxHiZufferTextureMipLevel,1);
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
                return float4(0,0,0,0);
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
        }
        
        Pass
        {
         Name "Blending"

         ZTest NotEqual
        ZWrite Off
        Cull Off
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
        
    }
}