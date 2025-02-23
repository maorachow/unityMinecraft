#ifndef REALTIME_RAYTRACING_LIGHTSAMPLINGUTILITY_INCLUDED
#define REALTIME_RAYTRACING_LIGHTSAMPLINGUTILITY_INCLUDED
 #include "./RealtimeRayTracingUtility.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LightCookie/LightCookie.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Clustering.hlsl"
float3 SampleThetaClampedHemisphere(float3 normal,float2 randVal,float maxTheta){

    float phi = 2.0 * PI * randVal.y;
    float theta=randVal.x*maxTheta;
    float3 tangentSample = float3(sin(theta) * cos(phi),  sin(theta) * sin(phi), cos(theta));
    float3 up = abs(normal.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
 
    float3 tangent =normalize(up - normal * dot(up, normal));
    float3 bitangent = cross(normal, tangent);
    return  normalize(tangent*tangentSample.x+bitangent*tangentSample.y+normal*tangentSample.z );
}
float RealtimeRaytracingDirectionalLightConeTheta;
float3 GetDirectionalLightRandomDirection(float2 randVal,float3 normal,out float pdf){
    
    pdf=1.0/(2*PI*(1.0-cos(RealtimeRaytracingDirectionalLightConeTheta)));
    return SampleThetaClampedHemisphere(normal,randVal,RealtimeRaytracingDirectionalLightConeTheta);
}


bool IntersectsSphere(float3 rayOrigin,float3 rayDirection, float4 sphere, out float3 intersectingPosition,out float outT){
    float3 d = rayOrigin - sphere.xyz;
    if (length(d)<sphere.w){
     outT = -1;
     return false;
    }
    float p1 = -dot(rayDirection, d);
    float p2sqr = p1 * p1 - dot(d, d) + sphere.w * sphere.w;
    if (p2sqr < 0){
      outT = -1;
     return false;
    }
       
    float p2 = sqrt(p2sqr);
    float t = p1 - p2 > 0 ? p1 - p2 : p1 + p2;
    if (t > 0)
    {
        outT = t;
        intersectingPosition =rayOrigin + t * rayDirection;
      return true;
    }
         outT = -1;
        intersectingPosition =rayOrigin +(-1) * rayDirection;
        return false;
}


struct RealtimeRayTracingPointLight{

    float3 positionWS;
    float radius;
    float3 color;
};
float RealtimeRaytracingGlobalPointLightRadius;
RealtimeRayTracingPointLight RayTracingGetAdditionalPerObjectLight(int perObjectLightIndex)
{
    // Abstraction over Light input constants
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
    half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
    half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
    uint lightLayerMask = _AdditionalLightsBuffer[perObjectLightIndex].layerMask;
#else
    float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
    half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
    half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
    uint lightLayerMask = asuint(_AdditionalLightsLayerMasks[perObjectLightIndex]);
#endif

    // Directional lights store direction in lightPosition.xyz and have .w set to 0.0.
    // This way the following code will work for both directional and punctual lights.
  //  float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
 //   float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

  //  half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
    // full-float precision required on some platforms
   // float attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

    RealtimeRayTracingPointLight light;
    light.positionWS = lightPositionWS;
    light.radius =RealtimeRaytracingGlobalPointLightRadius;
 //   light.shadowAttenuation = 1.0; // This value can later be overridden in GetAdditionalLight(uint i, float3 positionWS, half4 shadowMask)
    light.color = color;
  //  light.layerMask = lightLayerMask;

    return light;
}

float3 SamplePointLightSphere(float3 origin,float3 lightCenter,float radius,float2 randVal,out float pdf){
    
    float3 normal=normalize(lightCenter - origin);
    float distSquared=length(lightCenter - origin)*length(lightCenter - origin);
    float sinThetaMax2 = radius * radius / distSquared;
    float cosThetaMax = sqrt(max((float)0, 1 - sinThetaMax2));
    float cosTheta = (1 - randVal.x) + randVal.x * cosThetaMax;
    float sinTheta = sqrt(max((float)0, 1 - cosTheta * cosTheta));
    float phi = randVal.y * 2 * PI;
    
     pdf=1.0/(2*PI*(1.0-cosTheta));
    float3 tangentSample = float3(sinTheta * cos(phi), sinTheta * sin(phi), cosTheta);
    float3 up = abs(normal.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
 
    float3 tangent =normalize(up - normal * dot(up, normal));
    float3 bitangent = cross(normal, tangent);
    return  normalize(tangent*tangentSample.x+bitangent*tangentSample.y+normal*tangentSample.z );
}
void SamplePointLightRandomDirection(float2 rand,float3 position,float lightRadius,float3 lightCenterPos,out float3 sampleDirection,out float sampleExpectedDistance,out float pdf,in float lightIntensity,out bool isCulled){
    
  /*  float3 lightNormal=normalize(position-lightCenterPos);
       float3 outgoingDir = lightCenterPos - position;
     float sqDist = Length2(outgoingDir);
    float3 sampleDir;
    float rcpPDF;
  
    SampleCone(rand.xy,sqrt(1.0 / (1.0 + (lightRadius) / sqDist)),sampleDir,rcpPDF);



    float3 up = normalize(float3(1, 0, 0.0));
    float3 tangent =normalize(up - normalize(outgoingDir) * dot(up, normalize(outgoingDir)));
    float3 bitangent = cross(normalize(outgoingDir), tangent);*/

    float3 sampleDirWS=0;
    sampleDirWS=  SamplePointLightSphere(position,lightCenterPos,lightRadius,rand,pdf);
    float3 intersectPos;
    float expectedDist=0;
    bool intersected=IntersectsSphere(position,sampleDirWS,float4(lightCenterPos,lightRadius),intersectPos,expectedDist);
    
    sampleDirection=sampleDirWS;
    sampleExpectedDistance=expectedDist;
    if(lightIntensity/pdf<0.01){
    isCulled=true;
    }else{
    isCulled=false;
    }
}


#endif