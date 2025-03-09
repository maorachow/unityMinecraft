Shader "Hidden/ContactShadowShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
             ZTest Always Cull Off ZWrite Off
             HLSLPROGRAM
              
            #pragma multi_compile _ _ORTHOGRAPHIC
            #include "Assets/ShaderLibrary/ContactShadow.hlsl"
            
            #pragma vertex Vert
            #pragma fragment frag
            ENDHLSL
        }

         
    }
}
