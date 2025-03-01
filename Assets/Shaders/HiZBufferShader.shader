Shader "Hidden/HiZBufferShader"
{
    HLSLINCLUDE
    
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // The Blit.hlsl file provides the vertex shader (Vert),
        // the input structure (Attributes), and the output structure (Varyings)
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
       
        float4 SourceSize;
        float4 GetSource(half2 uv, float2 pixelOffset = 0.0) {
        pixelOffset *= SourceSize.zw;
        return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv + pixelOffset, 0);
        }
        float4 HiZTextureGenerate (Varyings input) : SV_Target
        {
          float4 depths=float4(

                    GetSource(input.texcoord, float2(-1, -1)).x,
                    GetSource(input.texcoord, float2(-1, 1)).x,
                    GetSource(input.texcoord, float2(1, -1)).x,
                    GetSource(input.texcoord, float2(1, 1)).x
              );
          #if UNITY_REVERSED_Z 
          return float4(max(max(depths.r, depths.g), max(depths.b, depths.a)),min(min(depths.r, depths.g), min(depths.b, depths.a)),0,1);
          #else
          return  float4(min(min(depths.r, depths.g), min(depths.b, depths.a)),max(max(depths.r, depths.g), max(depths.b, depths.a)),0,1);
          #endif
            
        }
        Texture2D _CameraDepthTexture;
        float4 HiZTextureCopy (Varyings input) : SV_Target
        {
       
               return float4(SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_LinearClamp, input.texcoord).xx,0,1);
        }
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "HiZBufferBlit"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment HiZTextureGenerate
            
            ENDHLSL
        }
         Pass
        {
            Name "HiZBufferTextureCopy"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment HiZTextureCopy
            
            ENDHLSL
        }
        
    }
}