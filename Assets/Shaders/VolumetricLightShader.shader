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
            float4 frag (Varyings i) : SV_Target
            {
                
                
                  float3 worldPos=ReconstructViewPosMatrix(i.texcoord,SampleSceneDepth(i.texcoord));
                  
                  float3 startPos=_WorldSpaceCameraPos;
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
