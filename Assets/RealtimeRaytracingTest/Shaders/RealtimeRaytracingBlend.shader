Shader "Hidden/RealtimeRaytracingBlend"
{
    HLSLINCLUDE
    
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
       
        float4 SourceSize;
        Texture2D UAV_PrevResultTex;
        Texture2D UAV_ResultTex;
        Texture2D _MotionVectorTexture;
       
        float4 GetSource(float2 uv) {
       
        return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv , 0);
        }

       
        float4 RayTracingTextureBlend (Varyings input) : SV_Target
        {
        float4 curColor=UAV_ResultTex.Sample(sampler_PointClamp,input.texcoord);
          float2 motionVector=_MotionVectorTexture.Sample(sampler_PointClamp,input.texcoord);
          float4 prevColor=UAV_PrevResultTex.Sample(sampler_PointClamp,input.texcoord-motionVector);
          float3 finalCol=lerp(curColor.xyz,prevColor.xyz,0.99);
          return float4(curColor.xyz,1);
        }
 
      
    ENDHLSL
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "RealtimeRaytracingBlend"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment RayTracingTextureBlend
            
            ENDHLSL
        }
    }
}
