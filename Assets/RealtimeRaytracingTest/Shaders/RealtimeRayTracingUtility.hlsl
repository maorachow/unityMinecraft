#ifndef REALTIME_RAYTRACINGUTILITY_INCLUDED
#define REALTIME_RAYTRACINGUTILITY_INCLUDED
#include "UnityRaytracingMeshUtils.cginc"
 
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Sampling/Sampling.hlsl"
#ifndef PI
#define PI 3.141592653589
#endif
#define MAX_RAY_RECURSION_DEPTH 3
#define MAX_RAY_TRACING_DIST 2000
#define MIN_RAY_TRACING_DIST 0.0001
#define SAMPLING_TEXTURE_GAMMA_CORRECTION_ENABLED
#define OUTPUT_RESULT_GAMMA_CORRECTION_ENABLED
float4x4 CameraToWorld;
float4x4 CameraInverseProjection;
TextureCube SRV_SkyboxTex;
uniform float4 AmbientSHArray[7];
Texture2D _MotionVectorTexture;
Texture2D _MotionVectorDepthTexture;
Texture2D _CameraDepthTexture;
sampler sampler_point_clamp;
 
sampler sampler_linear_clamp;
float4 RayTracingPassWorldSpaceCameraPos;


bool isUsingOfflineAccum;

RaytracingAccelerationStructure SceneRaytracingAccelerationStructure;
RayDesc CreateCameraRay(float2 uv)
{
    
    float3 origin = mul(CameraToWorld, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;
   
    float3 projectivePoint = mul(CameraInverseProjection, float4(uv * 2 - 1, 0.0f, 1.0f)).xyz;
   
    float3 raydirection = projectivePoint - float3(0, 0, 0).xyz;
    raydirection = mul(CameraToWorld, float4(raydirection, 0.0f)).xyz;
    raydirection = normalize(raydirection);
    RayDesc retValue;
    retValue.Origin = origin;
    retValue.Direction = raydirection;
    retValue.TMin = 0;
    retValue.TMax = 3000;
    return retValue;

}
 
 
    #define PRIMARY_RAY 0
    #define SHADOWRAY -1
    #define INDIRECT_DIFFUSE 2
    #define INDIRECT_SPECULAR 3
  
struct RealtimeRaytracingRayPayload
{
   
    float HitDistance;
    float3 Normal;
    float3 Albedo;
    float2 MetallicRoughness;
    uint TracingDepth;
    float3 Radiance;
    int HitMaterialType;//0 DieletricRoughness
    float3 PrevRayDirection;
    float3 ShadowRayDirection;
    float3 RayIntersectionPoint;
    int TracingRayType;
    float expectedShadowRayHitDistance;
    float2 randomSeed;
   
};
struct OcculusionTestRayPayload
{
    float HitDistance;
    uint TracingDepth;
};
struct AttributeData
{
	// Barycentric value of the intersection
    float2 barycentrics;
};

#include "./RealtimeRayTracingRandomUtility.hlsl"



float3 ImportanceSampleGGX(float2 Xi, float3 N, float roughness)
{
    float a = roughness * roughness;
	
    float phi = 2.0 * PI * Xi.x;
    float cosTheta = sqrt( (1.0 - Xi.y) / (1.0 + (a * a - 1.0) * Xi.y+0.00001));
    float sinTheta = sqrt( max(0.0,1.0 - cosTheta * cosTheta));
	
	// from spherical coordinates to cartesian coordinates - halfway vector
    float3 H;
    H.x = cos(phi) * sinTheta;
    H.y = sin(phi) * sinTheta;
    H.z = cosTheta;
	
	// from tangent-space H vector to world-space sample vector
    float3 up = abs(N.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
  //  float3 up = normalize(float3(Xi.xy*2-1, 0));// abs(N.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
   // float3 tangent = normalize(cross(up, N));
    float3 tangent =normalize(up - N * dot(up, N));
   
  float3  bitangent= cross(N,tangent);

 

    float3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
    return normalize(sampleVec);
}


float3 ImportanceSampleGGX(inout float2 Xi, float3 N, float roughness,out float outCosTheta,out float outSinTheta)
{
    Xi=clamp(Xi,0.0001,0.9999);
    float a =clamp(roughness * roughness,0.0001,0.9999);
	 
    float phi = 2.0 * PI * Xi.x;
   // Xi.y=clamp(Xi.y,0.001,0.999);
    float cosTheta = sqrt ((1.0 - Xi.y) / (1.0 + (a * a - 1.0) * Xi.y));
    outCosTheta=cosTheta;
    float sinTheta = sqrt((1.0 - cosTheta * cosTheta));
	outSinTheta=sinTheta;
	// from spherical coordinates to cartesian coordinates - halfway vector
    float3 H;
    H.x = cos(phi) * sinTheta;
    H.y = sin(phi) * sinTheta;
    H.z = cosTheta;
	
	// from tangent-space H vector to world-space sample vector
    float3 up = abs(N.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
  //  float3 up = normalize(float3(Xi.xy*2-1, 0));// abs(N.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
   // float3 tangent = normalize(cross(up, N));
    float3 tangent =normalize(up - N * dot(up, N));
   
  float3  bitangent= cross(N,tangent);

 

    float3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
    return normalize(sampleVec);
}


float3 ImportanceSampleGGX(inout float2 Xi, float3 N,float3 T, float roughness)
{
     Xi=clamp(Xi,0.0001,0.9999);
    float a = roughness * roughness;
	
    float phi = 2.0 * PI * Xi.x;
    float cosTheta = sqrt( (1.0 - Xi.y) / (1.0 + (a * a - 1.0) * Xi.y));
    float sinTheta = sqrt( max(0.0,1.0 - cosTheta * cosTheta));
	
	// from spherical coordinates to cartesian coordinates - halfway vector
    float3 H;
    H.x = cos(phi) * sinTheta;
    H.y = sin(phi) * sinTheta;
    H.z = cosTheta;
	
	// from tangent-space H vector to world-space sample vector
    //float3 up = abs(N.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
  
   // float3 tangent = normalize(cross(up, N));
    
    float3 bitangent =normalize(cross(N, T));
 

    float3 sampleVec = T * H.x + bitangent * H.y + N * H.z;
    return normalize(sampleVec);
}

float3 SampleHemisphere(in float3 normal,float2 randVal)
{

    float cosTheta = 1.0 -randVal.x;
    float sinTheta = sqrt(max(0.0f, 1.0f - cosTheta * cosTheta));
    float phi = 2 * PI * randVal.y;
    float3 tangentSpaceDir = normalize(float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta));
    float3 randDir=normalize(float3(randVal.xy*2-1,Random2DTo2D(randVal.yx).x));
    
       float3 randomVec = float3(randVal.xy*2-1, 0.0001);
    float3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
   float3 bitangent = cross(normal, tangent);
   
      return normalize(tangent*tangentSpaceDir.x+bitangent*tangentSpaceDir.y+normal*tangentSpaceDir.z );
}
 float3 SampleHemisphere( float3 normal, float3 tangent,inout float2 randVal)
{
     randVal=clamp(randVal,0.0001,0.9999);
    float cosTheta = 1.0 -randVal.x;
    float sinTheta = sqrt(max(0.0f, 1.0f - cosTheta * cosTheta));

    float phi = 2 * PI * randVal.y;
    float3 tangentSpaceDir = normalize(float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta));
    float3 randDir=normalize(float3(randVal.xy*2-1,Random2DTo2D(randVal.yx*100).x));
     
    
   float3 bitangent = cross(normal, tangent);
   
      return normalize(tangent*tangentSpaceDir.x+bitangent*tangentSpaceDir.y+normal*tangentSpaceDir.z );
}
float3 SampleCosineWeightedHemisphere(float3 normal,float3 tangent,inout float2 randVal)
{
     randVal=clamp(randVal,0.0001,0.9999);
    float cosTheta = sqrt(1.0 -randVal.x);
    float sinTheta = sqrt(max(0.0, 1.0 - cosTheta * cosTheta));
    float phi = 2.0 * PI * randVal.y;
    float3 tangentSpaceDir = float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
      float3 randomVec = float3(randVal.xy*2-1, 0.0001);

      float3 bitangent = cross(normal, tangent);
    return normalize(tangent*tangentSpaceDir.x+bitangent*tangentSpaceDir.y+normal*tangentSpaceDir.z );
   
}
float3 SampleCosineWeightedHemisphere(float3 normal,float3 tangent,inout float2 randVal,out float theta)
{
    randVal=clamp(randVal,0.0001,0.9999);
    float cosTheta = sqrt(max(0.000001,randVal.x));
    theta=acos(cosTheta);
    float sinTheta = sqrt(max(0.000001, 1.0 - cosTheta * cosTheta));
    float phi = 2.0 * PI * randVal.y;
    float3 tangentSpaceDir = float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
      float3 randomVec = float3(randVal.xy*2-1, 0.0001);

      float3 bitangent = cross(normal, tangent);
    return normalize(tangent*tangentSpaceDir.x+bitangent*tangentSpaceDir.y+normal*tangentSpaceDir.z );
   
}
 float3 SampleCosineWeightedHemisphere(float3 normal,inout float2 randVal,out float theta)
{
      randVal=clamp(randVal,0.0001,0.9999);
    float cosTheta = sqrt(1.0 -randVal.x);
     theta=acos(cosTheta);
    float sinTheta = sqrt(max(0.0, 1.0 - cosTheta * cosTheta));
    float phi = 2.0 * PI * randVal.y;
    float3 tangentSpaceDir = float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
       float3 up = abs(normal.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
    float3 tangent = normalize(up - normal * dot(up, normal));
      float3 bitangent = cross(normal, tangent);
    return normalize(tangent*tangentSpaceDir.x+bitangent*tangentSpaceDir.y+normal*tangentSpaceDir.z );
   
}
float3 SampleThetaClampedHemisphere(float3 normal,float3 tangent,inout float2 randVal,float maxTheta){

     randVal=clamp(randVal,0.0001,0.9999);
    float phi = 2.0 * PI * randVal.y;
    float theta=randVal.x*maxTheta;
    float3 tangentSample = float3(sin(theta) * cos(phi),  sin(theta) * sin(phi), cos(theta));
     float3 bitangent = cross(normal, tangent);
     return  normalize(tangent*tangentSample.x+bitangent*tangentSample.y+normal*tangentSample.z );
}

 float3 UnpackNormalmapRGorAGCustom(float4 packednormal)
{
    // This do the trick
   packednormal.x *= packednormal.w;
 
    float3 normal;
    normal.xy = packednormal.xy * 2 - 1;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
    return normal;
}

inline float3 UnpackNormalCustom(float4 packednormal)
{
#if defined(UNITY_NO_DXT5nm)
    return packednormal.xyz * 2 - 1;
#else
    return UnpackNormalmapRGorAGCustom(packednormal);
#endif
}


#include "./RealtimeRayTracingGeometryUtility.hlsl"
#endif