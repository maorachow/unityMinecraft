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

           
              #pragma vertex Vert
              #pragma fragment SSRPassFragment
            uniform matrix matView;
            uniform matrix matProjection;
            uniform matrix matInverseView;
            uniform matrix matInverseProjection;
            uniform float3 CameraPos;
             uniform float4 _ProjectionParams2;
            uniform float4 _CameraViewTopLeftCorner;
            uniform float4 _CameraViewXExtent;
            uniform float4 _CameraViewYExtent;
                
            uniform float StrideSize;
            uniform float SSRThickness;
            uniform int StepCount;
            uniform float FadeDistance;
            uniform Texture2D HiZBufferTexture;
            uniform float MaxHiZufferTextureMipLevel;
             SamplerState sampler_point_clamp;

            half3 ReconstructViewPos(float2 uv, float linearEyeDepth) {  
                // Screen is y-inverted  
                uv.y = 1.0 - uv.y;  

                float zScale = linearEyeDepth * _ProjectionParams2.x; // divide by near plane  
                float3 viewPos = _CameraViewTopLeftCorner.xyz + _CameraViewXExtent.xyz * uv.x + _CameraViewYExtent.xyz * uv.y;  
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




 
            half4 SSRPassFragment(Varyings input) : SV_Target {  


                float rawDepth = SampleSceneDepth(input.texcoord);  
                 return float4(rawDepth.xxx,1);
                float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);  
                float3 vpos = ReconstructViewPosMatrix(input.texcoord);  
                
                float3 vnormal = SampleSceneNormals(input.texcoord);
                vpos=vpos+ vnormal * (-vpos.z / _ProjectionParams.z * 0.2 + 0.05) ;
                float3 wPos=GetWorldPosition(input.texcoord,rawDepth).xyz;

                float3 vDir = normalize(wPos-_WorldSpaceCameraPos);  
        //         float diff = max(dot(vnormal, vDir), -1.0);
         //       if (diff >= -0.3)
         //       {
         //       return float4(0,0,0,1);
         //       }
                float3 rDir = normalize(reflect(vDir, vnormal));  
                float strideLen=0.05;
                
                 bool hit=false;
                 float curLength=StrideSize;
                 float3 curPos=vpos;
               
                float2 uv1;
                float resultDepth=0;
                 ReconstructUVAndDepthMatrix(curPos,uv1,resultDepth);

               resultDepth=rawDepth;
              

                

                float mipLevel = 0.0;
                UNITY_LOOP
                for(int i = 0; i < 40; i++){
                    curPos += rDir*strideLen;// 步近
                    ReconstructUVAndDepthMatrix(curPos,uv1,resultDepth);
                    float sampleDepth=0.0;
                    if(mipLevel>0.0){ 
                    
                     sampleDepth = LinearEyeDepth(SAMPLE_TEXTURE2D_X_LOD(HiZBufferTexture,sampler_point_clamp,uv1,mipLevel), _ZBufferParams);
                    }else{
                      sampleDepth = LinearEyeDepth(SampleSceneDepth(uv1), _ZBufferParams);
                    }
                    
                    if(resultDepth>sampleDepth){
                        if(mipLevel == 0){
                         if (resultDepth>sampleDepth &&abs(resultDepth-sampleDepth) <  SSRThickness )
                           {
                                // If it's hit something, then return the UV position
                              if(uv1.x<0||uv1.x>1||uv1.y<0||uv1.y>1){
                                 return float4(0,0,0,1);
                             }
                               return GetSource(uv1);
                            }
                        }
                        mipLevel --;
                        curPos -= rDir*strideLen;// 回溯一步
                        strideLen /= 2;// 步长减半 
                    }
                    else{
                    if(mipLevel<MaxHiZufferTextureMipLevel){
                    mipLevel ++;
                        strideLen *= 2;
                    }
                         
                    }
                }
                return float4(0,0,0,1);
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
        Blend Off
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