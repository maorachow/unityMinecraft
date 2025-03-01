Shader "Unlit/VolumetricLightShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
         LOD 100
      
        Pass
        {
            HLSLPROGRAM

        
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"  
   
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
              #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 
            #pragma vertex Vert
            #pragma fragment frag
             #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
 
             uniform float Intensity;
             uniform int StepCount;
             uniform float4 FogColor;
             
             float4 GetWorldPos(float2 ScreenUV , float Depth)
			{
				 
				float3 ScreenPos = float3(ScreenUV , Depth);
				float4 normalScreenPos = float4(ScreenPos * 2.0 - 1.0 , 1.0);
			 
				float4 ndcPos = mul( unity_CameraInvProjection , normalScreenPos );
				ndcPos = float4(ndcPos.xyz / ndcPos.w , 1.0);
				 
				float4 sencePos = mul( unity_CameraToWorld , ndcPos * float4(1,1,-1,1));
				sencePos = float4(sencePos.xyz , 1.0);
				return sencePos;
			} 

            half3 ReconstructViewPosMatrix(float2 uv, float rawDepth) {  
                // Screen is y-inverted  
           //     uv.y = 1.0 - uv.y;  
                #if UNITY_REVERSED_Z
                    rawDepth = SampleSceneDepth(uv);
                #else
                    //  调整 Z 以匹配 OpenGL 的 NDC ([-1, 1])
                    rawDepth= lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(uv));
                #endif
                float3 worldPos = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
                return worldPos;
            }
          

            float GetShadow(float3 posWorld)
            {
                float4 shadowCoord = TransformWorldToShadowCoord(posWorld);
                float shadow = MainLightRealtimeShadow(shadowCoord);
                return shadow;
            }  
 

 
 
            float ShadowAtten(float3 worldPosition)
            {
                    return MainLightRealtimeShadow(TransformWorldToShadowCoord(worldPosition));
            }

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
            float4 frag (Varyings input) : SV_Target
            {
                
                
                  float3 worldPos=ReconstructViewPos(input.texcoord,LinearEyeDepth(SampleSceneDepth(input.texcoord),_ZBufferParams))+_WorldSpaceCameraPos;
                //  float3 viewPos=mul(UNITY_MATRIX_V,float4(worldPos-_WorldSpaceCameraPos,0));
                   
                  if(LinearEyeDepth(SampleSceneDepth(input.texcoord),_ZBufferParams)/_ProjectionParams.z>0.99){
                      return  Intensity*FogColor*StepCount;
                      }
               //   return float4(worldPos.xyz,1);
               float3 viewPos1 = CameraViewTopLeftCorner.xyz + CameraViewXExtent.xyz * input.texcoord.x + CameraViewYExtent.xyz * input.texcoord.y;  
             //  return float4(viewPos1.xyz,1);
                  float3 startPos=_WorldSpaceCameraPos+viewPos1;
                  float3 dir=normalize(worldPos-startPos);
                   float len = length(worldPos - startPos);
                float3 stepLen = dir * len / StepCount;
                  float4 color=float4(0,0,0,1);
                   for (int i = 0; i < StepCount; i++)
                {
                    startPos +=stepLen;
                    float4 shadowPos = TransformWorldToShadowCoord(startPos);
                    float intensity = MainLightRealtimeShadow(shadowPos)*Intensity;
                    color += intensity*FogColor;
                }
                 return color;
                 
            }
            ENDHLSL
        }
        Pass
        {
         Name "Blending"

         ZTest NotEqual
        ZWrite Off
        Cull Off
        Blend One One, One Zero
     // Blend Off
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
