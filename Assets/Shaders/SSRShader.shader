Shader "Hidden/SSRShader"
{
    
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       
        Pass
        {
            Name "SSRTracing"

            HLSLPROGRAM
             #include "Assets/ShaderLibrary/SSR.hlsl"
            #pragma multi_compile _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile _ SSR_USE_FORWARD_RENDERING

            #pragma multi_compile_local_fragment _ _ORTHOGRAPHIC
            #pragma vertex Vert
            #pragma fragment SSRTracing
            
            ENDHLSL
        }
        Pass
        {
            Name "SSRGenerateReflection"

           
            HLSLPROGRAM
              #include "Assets/ShaderLibrary/SSR.hlsl"
            #pragma multi_compile _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile _ SSR_USE_FORWARD_RENDERING

            #pragma multi_compile _ _CONTACT_SHADOW
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_local_fragment _ _ORTHOGRAPHIC

           
            #pragma vertex Vert
            #pragma fragment SSRGenerateReflectionFrag
            
            ENDHLSL
        }

        Pass
        {
            Name "SSRFinalBlend"

            ZTest Always
            ZWrite Off
            Cull Off
            Blend  One OneMinusSrcAlpha
            HLSLPROGRAM

              #include "Assets/ShaderLibrary/SSR.hlsl"
            #pragma multi_compile _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile _ SSR_USE_FORWARD_RENDERING
 
            #pragma vertex Vert
            #pragma fragment SSRFinalBlend
            
            ENDHLSL
        }
    }
}