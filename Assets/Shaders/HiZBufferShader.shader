Shader "CustomEffects/HiZBufferShader"
{
    
     
           
 
 
  Properties {
        _MainTex ("Base (RGB)", 2D) = "" {}
    }

    
    
   
    
    SubShader
    {
    
        
         
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

            uniform float HiZBufferFromMipLevel;
            uniform float HiZBufferToMipLevel;
            uniform float4 SourceSize;    
              #pragma vertex Vert
              #pragma fragment Blending
             float4 GetSource(float2 uv, float2 offset = 0.0, float mipLevel = 0.0) {
                offset *= SourceSize.zw;
                return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv + offset, 0);
                }
              float4 Blending(Varyings input):SV_Target{
              float4 minDepth= float4(
                GetSource(input.texcoord, float2(-1, -1), HiZBufferFromMipLevel).r,
                GetSource(input.texcoord, float2(-1, 1), HiZBufferFromMipLevel).r,
                GetSource(input.texcoord, float2(1, -1), HiZBufferFromMipLevel).r,
                GetSource(input.texcoord, float2(1, 1), HiZBufferFromMipLevel).r
                );
              return max(max(minDepth.r,minDepth.g),max(minDepth.b,minDepth.a));
              }

              ENDHLSL
        
        }
        
    }
}