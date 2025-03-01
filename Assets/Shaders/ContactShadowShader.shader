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
            #pragma vertex Vert
            #pragma fragment frag
        
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
            uniform float StepLength;
            uniform float ShadowWeight;
            uniform float ContactShadowBias;
            uniform float FadeDistance;
            uniform float4 _BlitScreenParams;

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


              float4 TransformViewToHScreen(float3 vpos,float2 screenSize) {  
                float4 cpos = mul(UNITY_MATRIX_P, vpos);  
                cpos.xy = float2(cpos.x, cpos.y * _ProjectionParams.x) * 0.5 + 0.5* cpos.w;  
                cpos.xy *= screenSize;  
                return cpos;  
            }  
            float Random2DTo1D(float2 value,float a ,float2 b)
            {			
	            //avaoid artifacts
	            float2 smallValue = sin(value);
	            //get scalar value from 2d vector	
	            float  random = dot(smallValue,b);
	            random = frac(sin(random) * a);
	            return random;
            }
            float Random2DTo1D(float2 value){
	            return (
		            Random2DTo1D(value,14375.5964, float2(15.637, 76.243))
		          
	            );
            }
              SamplerState sampler_point_clamp;
            float4 frag (Varyings input) : SV_Target
            {
                
         
              
         //       if(SampleS      ceneDepth(i.texcoord)<=0.0001){
         //       return float4(0,0,0,0);
          //      }
               
                float rawDepth = SampleSceneDepth(input.texcoord).r;  
                float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams); 
             //    return float4(1,1,1,1);
                float3 vpos = ReconstructViewPos(input.texcoord,linearDepth);  
                float3 worldPos=vpos+_WorldSpaceCameraPos;
                      
                float3 normal=SampleSceneNormals(input.texcoord).xyz;
                worldPos=worldPos+ normal * 0.01 ;
          //       return float4((worldPos-_WorldSpaceCameraPos),1);
                float3 lightDir=GetMainLight().direction;
                float3 lightDirView=mul(UNITY_MATRIX_V,float4(lightDir,0));
                  
                
                float magnitude = 1000;  
                float3 startView=mul(UNITY_MATRIX_V,float4(worldPos,1));
                if(length(startView.z)>FadeDistance){
                          return float4(1,1,1,1);
                }
                float end = startView.z + lightDirView.z * magnitude;  
                if (end > -_ProjectionParams.y)  
                magnitude = abs(-_ProjectionParams.y - startView.z) / lightDirView.z;  
                
                 
                    float3 endView = startView + lightDirView * magnitude;  
              
                    float4 startHScreen = TransformViewToHScreen(startView, _BlitScreenParams.xy);  
                    float4 endHScreen = TransformViewToHScreen(endView, _BlitScreenParams.xy);  
                 

                    float startK = 1.0 / startHScreen.w;  
                    float endK = 1.0 / endHScreen.w;  

                    
                    float2 startScreen = startHScreen.xy * startK;  
                    float2 endScreen = endHScreen.xy * endK;  
                 //   return float4(input.texcoord.xy,1,1);
                  
                    // 经过齐次除法的视角坐标  
                    float3 startQ = startView * startK;  
                    float3 endQ = endView * endK;  
                    float2 diff = endScreen - startScreen;  
                  
                    bool permute = false;  
                    if (abs(diff.x) < abs(diff.y)) {  
                        permute = true;  

                        diff = diff.yx;  
                        startScreen = startScreen.yx;  
                        endScreen = endScreen.yx;  
                    }  

                    float dir = sign(diff.x);  
                    float invdx = dir / diff.x;  
                    float2 dp = float2(dir, invdx * diff.y);  
                    float3 dq = (endQ - startQ) * invdx;  
                    float dk = (endK - startK) * invdx;  
 
                    float2 hitUV = 0.0;


                    float2 P = startScreen;  
                    float3 Q = startQ;  
                    float K = startK;  
                    float testDepth=0;

                       float jitter = 1;  
                     dp*=jitter;  
                     dq*=jitter;  
                     dk*=jitter;

                     float contactShadow=1;
                     float endX=endScreen.x ;
                      float2 finalHitUV=0;
                    UNITY_LOOP
                    for(int i=0;i<SampleCount;i++){
                       
                        P += dp*StepLength;  
                        Q.z += dq.z*StepLength;  
                        K += dk*StepLength;  
                        testDepth = ( Q.z) / ( K);  

                          hitUV = permute ? P.yx : P;  
                        hitUV /= _BlitScreenParams.xy;  

                          if (any(hitUV < 0.0) || any(hitUV > 1.0)){ 
                          
                            break;
                            }
                            float surfaceDepth = -LinearEyeDepth(SAMPLE_TEXTURE2D_X(_CameraDepthTexture,sampler_point_clamp,hitUV), _ZBufferParams); 
                          
                            if(-surfaceDepth>_ProjectionParams.z*0.99||-surfaceDepth<_ProjectionParams.y||SAMPLE_TEXTURE2D_X(_CameraDepthTexture,sampler_point_clamp,hitUV).x==0){
                             break;
                            }
                            if(testDepth<surfaceDepth){
                                if(abs(testDepth-surfaceDepth)<EdgeWidth){
                                    contactShadow=0;
                              //      finalHitUV=hitUV;
                                    break;
                                  
                                    }
                               
                             }
                        }
                      
          /*   float shadow=MainLightRealtimeShadow(TransformWorldToShadowCoord(worldPos));
                if(shadow<0.001){
               return float4(0,0,0,0);
              }
           
                            if((nDotL)<0.1){
                                return float4(0,0,0,0);
                                    }
              float finalVal=0;*/

                return float4(contactShadow,contactShadow,contactShadow,1);
                

        //       return float4(shadow.xxxx);
            }
            ENDHLSL
        }

         
    }
}
