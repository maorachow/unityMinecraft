Shader "Unlit/ContactShadowShader"
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
             HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #define MAIN_LIGHT_CALCULATE_SHADOWS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
              #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
               #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
             
        uniform int SampleCount;
        uniform float EdgeWidth;
        uniform float Length;
        uniform float ShadowWeight;
        uniform float ContactShadowBias;
        uniform float FadeDistance;
            float4 frag (Varyings i) : SV_Target
            {
                
         //       if(SampleSceneDepth(i.texcoord)<=0.0001){
         //       return float4(0,0,0,0);
          //      }
             
                float3 worldPos=ComputeWorldSpacePosition(i.texcoord,SampleSceneDepth(i.texcoord),UNITY_MATRIX_I_VP);
                float3 worldPosO=worldPos;
                float3 normal=SampleSceneNormals(i.texcoord).xyz;
                worldPos=worldPos+ normal * (length(worldPos-_WorldSpaceCameraPos) / _ProjectionParams.z *0.5) ;
          //       return float4((worldPos-_WorldSpaceCameraPos),1);
                float3 lightDir=GetMainLight().direction;
                if(lightDir.y<0.1){
                return float4(0,0,0,0);
                }
                float shadow=1;
                if(length(worldPos-_WorldSpaceCameraPos)>FadeDistance){
              return float4(0,0,0,0);
              }
                UNITY_LOOP
                for(int i=0;i<SampleCount;i++){
                float3 marchPos=worldPos+(normalize(lightDir)*Length/SampleCount*i);
                float4 clipPos1= (TransformWorldToHClip(marchPos));
                clipPos1.xyz/=clipPos1.w;
                clipPos1.xy=clipPos1.xy*0.5+0.5;
                clipPos1.y=1-clipPos1.y;
                float testDepth=LinearEyeDepth(clipPos1.z,_ZBufferParams);
             // return testDepth/100;
                float sampleDepth=LinearEyeDepth(SampleSceneDepth(clipPos1.xy),_ZBufferParams);
             // return float4(SampleSceneDepth(clipPos1.xy).xxx,1);
                if(testDepth>sampleDepth+ContactShadowBias){
                   if(abs(testDepth-sampleDepth)<EdgeWidth){
                 shadow-=(1.0/SampleCount)*ShadowWeight;
                }
                } 
              
                }
                 float4 shadowPos = TransformWorldToShadowCoord(worldPosO);
           //   return float4(MainLightRealtimeShadow(shadowPos).xxx,1);
                 if(MainLightRealtimeShadow(shadowPos)<0.001){
               return float4(0,0,0,0);
              }

                return float4( shadow.xxx,shadow.x>0.1?0:0.7);
                

        //       return float4(shadow.xxxx);
            }
            ENDHLSL
        }

             Pass
        {
         Name "Blending"

         ZTest NotEqual
        ZWrite Off
        Cull Off
    //    Blend Off
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
