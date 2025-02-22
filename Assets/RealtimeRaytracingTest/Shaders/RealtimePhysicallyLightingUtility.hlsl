#ifndef PHYSICALLYLIGHTING_UTILITY
#define PHYSICALLYLIGHTING_UTILITY
#ifndef PI
#define PI 3.141592653589
#endif

 #define NON_METALLIC_F0 float3(0.04,0.04,0.04)
float DistributionGGX(float3 N, float3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;

    float num = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return num / denom;
}
float DistributionGGX(float NdotH, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH2 = NdotH * NdotH;

    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
    return a2 / denom;
}
float GetGGXImportanceSamplingPDF(float3 wo, float3 normal, float3 halfvector, float roughness) {
    float NoH = max(dot(normal, halfvector),0);
    
    float NoH2 = NoH * NoH;
    float sin = sqrt((1.0f - NoH2));
       float a= clamp(roughness * roughness,0.0001,0.9999);
    float alpha2 = a * a;
     
    float a1 = (NoH2 * (alpha2 - 1.0f) + 1.0f);

    float pdf_h = (2.0f * alpha2 * NoH * sin) / (a1 * a1);

    return pdf_h / (4.0f * dot(wo, halfvector));
}
float GetGGXImportanceSamplingPDFUE4(float cosTheta, float roughness) {
    float a= clamp(roughness * roughness,0.000001,0.999999);
    float a2=a*a;
    float d=(cosTheta*a2-cosTheta)*cosTheta+1;
    float D=a2/(PI*d*d);
    float PDF=D*cosTheta;
  
    return PDF;
}

float GetGGXImportanceSamplingPDFUE4(float cosTheta, float roughness,float3 wi,float3 wh) {
    float a= roughness * roughness;
    float a2=a*a;
    float d1=(a2-1)*cosTheta*cosTheta+1;
    float d=(cosTheta*a2-cosTheta)*cosTheta+1;
    float D=a2/(PI*d*d);
    float PDF=max(D*max((cosTheta/ (4.0f * dot(wi, wh))),0.0001),1e-7);
  
    return PDF;
}

float GeometrySchlickGGX(float NdotV, float roughness,bool isIBL=false)
{
    float r = (roughness + 1.0);
   
    float k = (r * r) / 8.0;
    if (isIBL)
    {

       // float a = 0.5 + 0.5 * roughness;
     //    a = a * a;
     //   k =( roughness) / 1.0;
    }
    float num = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return num / denom;
}
float GeometrySmith(float3 N, float3 V, float3 L, float roughness, bool isIBL = false)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness,isIBL);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness, isIBL);

    return ggx1 * ggx2;
}

 

float3 fresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}
float3 fresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
{
    return F0 + (max(float3(1.0 - roughness, 1.0 - roughness, 1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}
float GeometrySchlickGGX1(float NdotDir, float HdotDir, float roughness)
{
    if(NdotDir * HdotDir <= 0.0)
        return 0.0;
    float a = 0.5 + 0.5 * roughness;
    a = a * a;

    float temp = 1 - NdotDir * NdotDir;
	if (temp <= 0.0)
		return 0.0;
	float tanTheta = sqrt(temp) / NdotDir;

    float denom = 1.0 + sqrt(1.0 + a * a * tanTheta * tanTheta);
    return 2.0 / denom;
}


float GeometrySmith1(float3 N, float3 H, float3 V, float3 L, float roughness)
{
    float NdotV = dot(N, V);
    float NdotL = dot(N, L);
    float HdotV = dot(H, V);
    float HdotL = dot(H, L);

    float ggx2 = GeometrySchlickGGX1(NdotV, HdotV, roughness);
    float ggx1 = GeometrySchlickGGX1(NdotL, HdotL, roughness);

    return ggx1 * ggx2;
}
float FresnelDielectric(float cosi, float etai, float etat)
{
    float sint = etai / etat * sqrt(max(0.0, 1.0 - cosi * cosi));
    if(sint >= 1.0) //全反射
    {
        return 1.0;
    }
    else
    {
        float cost = sqrt(max(0.0, 1.0 - sint * sint));
        cosi = abs(cosi);

        float para = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost));
        float perp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));

        return 0.5 * (para * para + perp * perp);
    }
}
float FresnelConductor(float cosi,  float eta, float k)
		{
			float tmp = (eta*eta + k*k) * cosi * cosi;
			float Rparl2 = (tmp - (2.f * eta * cosi) + 1) /
				(tmp + (2.f * eta * cosi) + 1);
			float tmp_f = eta*eta + k*k;
			float Rperp2 =
				(tmp_f - (2.f * eta * cosi) + cosi * cosi) /
				(tmp_f + (2.f * eta * cosi) + cosi * cosi);

			return 0.5f * (Rparl2 + Rperp2);
		}

float GetRefractedGGXImportanceSamplingPDFUE4(float cosTheta, float roughness,float3 H,float3 wo,float3 wi,float3 normal,float etai,float etat) {
    float a= clamp(roughness * roughness,0.0001,0.9999);
    float a2=a*a;
    float d=(cosTheta*a2-cosTheta)*cosTheta+1;
    float D=a2/(PI*d*d);
    float PDF=D*cosTheta;


    float HDotV = dot(H, wo), HDotL =dot(H, wi);
    float sqrtDenom = etai * HDotV + etat * HDotL;
    
    float G = GeometrySmith1(normal, H, wo,wi, roughness + 0.01);
   
    
    float dwh_dwl = (etat * etat * HDotL) / (sqrtDenom * sqrtDenom + 1e-6);
    PDF = abs(PDF * dwh_dwl);
    return PDF;
}
/*float3 CalculateLightPhysically(float3 worldPos, float3 lightPos, float3 N, float3 viewDir, float3 albedo,float3 lightColor, float roughness, float3 F0, bool isDirectionalLight = false,bool isFDistributionEnabled=true)
{
    
    if (abs(lightPos.x) < 0.001 && abs(lightPos.y) < 0.001 && abs(lightPos.z) < 0.001)
    {
        return float3(0, 0, 0);
    }
    float3 Lo = float3(0.0, 0.0, 0.0);
    
    
    
    
    float3 L = normalize(lightPos - worldPos);
    float3 H = normalize(viewDir + L);
   
    
    float distance = length(lightPos - worldPos);
    float attenuation = 1.0 / (distance * distance);
    if (isDirectionalLight)
    {
        attenuation = 1;
    }
    float3 radiance = lightColor * attenuation;
    
    
    float D =isFDistributionEnabled==true? DistributionGGX(N, H, roughness+0.01):1;
    float G = GeometrySmith(N, viewDir, L, roughness + 0.01);
    float3 F = fresnelSchlickRoughness(max(dot(H, viewDir), 0.0), F0, roughness + 0.01);
    float3 kS = F;
    float3 kD = float3(1.0, 1.0, 1.0) - kS;
    
    float3 nominator = D * G * F;
    float denominator = 4.0 * max(dot(N, viewDir), 0.0) * max(dot(N, L), 0.0) + 0.001 + 0.01;
    float3 specular = nominator / denominator;
    
    float NdotL = max(dot(N, L), 0.0);
    Lo = (kD * albedo / PI+ specular) * radiance * NdotL;
    return Lo;
}*/


/*float3 FrBRDF(float3 p,float3 wi,float3 wo,float3 albedo,float3 normal,float roughness,float3 F0,bool useDisbFunc=true){

    float3 fLambert=albedo/PI;
    float3 L = normalize(wi);
    float3 H = normalize(wo + L);
     float3 V=wo;
    float D =useDisbFunc? DistributionGGX(normal, H, roughness):1;
  //     D=clamp(D,0,100000000);
    float G = GeometrySmith(normal, V, L, roughness,true );
    float3 F = fresnelSchlickRoughness(max(dot(H, V), 0.0), F0, roughness);
    float3 kS = F;
    float3 kD = float3(1.0, 1.0, 1.0) - kS;
    float3 nominator = D * G * F;
    float denominator = 4.0 * max(dot(normal, wo), 0.0) * max(dot(normal, L), 0.0) + 0.0001;
    float3 fCookTorrance     = nominator / denominator;

    return fLambert*kD+fCookTorrance;
}*/

float3 FrBRDF(float3 p,float3 wi,float3 wo,float3 albedo,float3 normal,float roughness,float3 F0,float metallic,bool useDisbFunc=true){

    float3 fLambert=albedo/PI;
    float3 L = normalize(wi);
    float3 H = normalize(wo + wi);
    float3 V= normalize(wo);
    float NDotH=max(dot(normal,H),0);
    float NDotV=dot(normal,V);
    float LdotH = dot(wi, H);
    float D =DistributionGGX(NDotH, roughness);
 
    float G = GeometrySmith(normal, wo,wi, roughness);
    float3 F =fresnelSchlickRoughness(max(dot(H, V),0), F0, roughness);
    float3 kS = F;
    float3 kD = float3(1.0, 1.0, 1.0) - kS;
    kD*=1-metallic;
    float3 nominator = D *G* F;
    float denominator = 4.0 * max(dot(normal, V), 0.0) * max(dot(normal, L), 0.0)+0.0001;
    float3 fCookTorrance     = nominator / denominator;

    return fLambert*kD+fCookTorrance;
}

float3 FrBRDFDieletricSpecular(float3 p,float3 wi,float3 wo,float3 albedo,float3 normal,float roughness,float3 F,bool useDisbFunc=true){

    float3 fLambert=albedo/PI;
    float3 L = normalize(wi);
    float3 H = normalize(wo + L);

    float D =useDisbFunc? DistributionGGX(normal, H, roughness + 0.01):1;
    float G =clamp(GeometrySmith1(normal,H, wo,wi, roughness),0,10);
   
    float3 kS = F;
    float3 kD = float3(1.0, 1.0, 1.0) - kS;

    //nonSpecProb=kD.x;
    float3 nominator = D * G * F;
    float denominator = 4.0 * max(dot(normal, wo), 0.0) * max(dot(normal, wi), 0.0) + 0.001;
    float3 fCookTorrance     = nominator / denominator;

    return fCookTorrance;
}

float3 FrBTDF(float3 p,float3 wi,float3 wo,float3 albedo,float3 normal,float roughness,float3 F,float3 H,float etai,float etat,inout float pdf){

     
      
  
    
     float HDotV = dot(H, wo), HDotL =dot(H, wi);
     float LDotV=dot(wi,wo);
     if(LDotV>=0){
     pdf=1;
     return 0;
     }
     float sqrtDenom = etai * HDotV + etat * HDotL;
    float D = DistributionGGX(normal, H, roughness);
    float G = GeometrySmith1(normal,H, wo,wi, roughness);
   
    float3 kS = F;
    float dwh_dwl = (etat * etat * HDotL) / (sqrtDenom * sqrtDenom);
    pdf = abs(pdf * dwh_dwl*(1.0- F));
    if(sqrtDenom == 0.0)
      {
       
            return 0.0;
     }
    
    float3 nominator =(1.0- F)* D*G * etat * etat * HDotV * HDotL;
    float denominator =sqrtDenom*sqrtDenom * (dot(normal, wo)) * (dot(normal, wi));
    float3 fLambert=albedo;
    if(abs(denominator)<0.000001)
    {
  
    }
float3 fTrans     = nominator / denominator;
    
    return fTrans*albedo;//(D/(4.0 * max(dot(normal, wi), 0.00001) * max(dot(normal, L), 0.00001)));
}
float3 FrBRDFDiffuse(float3 p,float3 wi,float3 wo,float3 albedo,float3 normal,float roughness,float3 F0,float metallic,bool useDisbFunc=true){

    float3 fLambert=albedo/PI;
    float3 L = normalize(wi);
    float3 H = normalize(wo + L);

    float D =useDisbFunc? DistributionGGX(normal, H, roughness + 0.001):1;
    float G = GeometrySmith(normal, wi, L, roughness )+ 0.001;
    float3 F = fresnelSchlickRoughness(max(dot(H, wi), 0.0), F0, roughness) + 0.001;
    float3 kS = F;
    float3 kD = float3(1.0, 1.0, 1.0) - kS;
    kD*=1-metallic;
    float3 nominator = D * G * F;
    float denominator = 4.0 * max(dot(normal, wo), 0.0) * max(dot(normal, L), 0.0) + 0.001;
    float3 fCookTorrance     = nominator / denominator;

    return fLambert*kD;
}

float3 FrBRDFSpecular(float3 p,float3 wi,float3 wo,float3 albedo,float3 normal,float roughness,float3 F0,float metallic,bool useDisbFunc=true){

    float3 fLambert=albedo/PI;
    float3 L = normalize(wi);
    float3 H = normalize(wo + L);
    float3 V=wo;
    float D =useDisbFunc? DistributionGGX(normal, H, roughness + 0.001):1;
    float G = GeometrySmith(normal, V, L, roughness );
    float3 F = fresnelSchlickRoughness(max(dot(H, V), 0.0), F0, roughness) + 0.001;
    float3 kS = F;
    float3 kD = float3(1.0, 1.0, 1.0) - kS;
    kD*=1-metallic;
    float3 nominator = D * G * F;
    float denominator = 4.0 * max(dot(normal, V), 0.0) * max(dot(normal, L), 0.0) + 0.001;
    float3 fCookTorrance     = nominator / denominator;

    return fCookTorrance;
}
float RoughConductorPdf(float3 normalIn,float roughnessIn, float3 viewDir, float3 lightDir)
{
    float3 normal =normalIn;
    float roughness =roughnessIn; 

    float NdotL = dot(normal, lightDir);
    float NdotV = dot(normal, viewDir);
    if((NdotV < 0.0) || (NdotL < 0.0))
        return 0.0;
    //计算pdf
    float3 h = normalize(lightDir + viewDir);
    float NdotH = dot(normal, h);
    float LdotH = dot(lightDir, h);

    //GGX重要性采样的pdf
    float D = DistributionGGX(NdotH, roughness);
    float pdf = D * NdotH / (4.0 * LdotH);

    return pdf;
}

float3 CalculateLightPhysicallyDiffuse(float3 worldPos, float3 lightPos, float3 N, float3 viewDir, float3 albedo, float3 lightColor, float roughness, float3 F0, bool isDirectionalLight = false)
{
    
    if (abs(lightPos.x) < 0.001 && abs(lightPos.y) < 0.001 && abs(lightPos.z) < 0.001)
    {
        return float3(0, 0, 0);
    }
    float3 Lo = float3(0.0, 0.0, 0.0);
    
    
    
    
    float3 L = normalize(lightPos - worldPos);
    float3 H = normalize(viewDir + L);
   
    
    float distance = length(lightPos - worldPos);
    float attenuation = 1.0 / (distance * distance);
    if (isDirectionalLight)
    {
        attenuation = 1;
    }
    float3 radiance = lightColor * attenuation;
    
    
    float D = DistributionGGX(N, H, roughness + 0.01);
    float G = GeometrySmith(N, viewDir, L, roughness + 0.01);
    float3 F = fresnelSchlickRoughness(max(dot(H, viewDir), 0.0), F0, roughness + 0.01);
    float3 kS = F;
    float3 kD = float3(1.0, 1.0, 1.0) - kS;
    
    float3 nominator = D * G * F;
    float denominator = 4.0 * max(dot(N, viewDir), 0.0) * max(dot(N, L), 0.0) + 0.001 + 0.01;
    float3 specular = nominator / denominator;
    
    float NdotL = max(dot(N, L), 0.0);
    Lo = (kD * albedo / PI) * radiance * NdotL;
    return Lo;
}


float3 CalculateLightPhysicallySpecular(float3 worldPos, float3 lightPos, float3 N, float3 viewDir, float3 albedo, float3 lightColor, float roughness, float3 F0, bool isDirectionalLight = false)
{
    
    if (abs(lightPos.x) < 0.001 && abs(lightPos.y) < 0.001 && abs(lightPos.z) < 0.001)
    {
        return float3(0, 0, 0);
    }
    float3 Lo = float3(0.0, 0.0, 0.0);
    
    
    
    
    float3 L = normalize(lightPos - worldPos);
    float3 H = normalize(viewDir + L);
   
    
    float distance = length(lightPos - worldPos);
    float attenuation = 1.0 / (distance * distance);
    if (isDirectionalLight)
    {
        attenuation = 1;
    }
    float3 radiance = lightColor * attenuation;
    
    
    float D = DistributionGGX(N, H, roughness + 0.01);
    float G = GeometrySmith(N, viewDir, L, roughness + 0.01);
    float3 F = fresnelSchlickRoughness(max(dot(H, viewDir), 0.0), F0, roughness + 0.01);
    float3 kS = F;
    float3 kD = float3(1.0, 1.0, 1.0) - kS;
    
    float3 nominator = D * G * F;
    float denominator = 4.0 * max(dot(N, viewDir), 0.0) * max(dot(N, L), 0.0) + 0.001 + 0.01;
    float3 specular = nominator / denominator;
    
    float NdotL = max(dot(N, L), 0.0);
    Lo = ( specular) * radiance * NdotL;
    return Lo;
}
#endif