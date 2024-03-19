Shader "Unlit/invertcolor"
{
    Properties
    {
      
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
    
           HLSLINCLUDE
           
           
       
           
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // The Blit.hlsl file provides the vertex shader (Vert),
        // the input structure (Attributes), and the output structure (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl "
             
           
        

        float4 frag(Varyings i) : SV_Target
        {
        
                // sample the texture
        float4 col = SampleSceneNormals(i.texcoord);//tex2D(_CameraGBufferTexture0,i.texcoord).rgba;
                // apply fog
                return col;
         
        }
            ENDHLSL
        Pass
        {
            HLSLINCLUDE
            
           #pragma vertex Vert
            #pragma fragment frag
            
            ENDHLSL
        }
    }
}
