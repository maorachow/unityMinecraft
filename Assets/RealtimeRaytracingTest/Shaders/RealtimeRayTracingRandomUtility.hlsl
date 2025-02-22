    #ifndef REALTIME_RAYTRACING_RANDOM_UTILITY
    #define REALTIME_RAYTRACING_RANDOM_UTILITY

sampler sampler_point_repeat;
Texture2D RealtimeRayTracingWhiteNoiseTexture;
float Random2DTo1D(float2 value, float a, float2 b)
{
	//avaoid artifacts
    float2 smallValue = sin(value);
	//get scalar value from 2d vector	
    float random = dot(smallValue, b);
    random = frac(sin(random) * a);
    return random;
}
float Random1DTo1D(float value,float a,float b){
	//make value more random by making it bigger
    float smallValue=sin(value);
	float random = frac(sin(smallValue+b)*a);
        return random;
}
float2 Random2DTo2D(float2 value)
{
    return float2(
		Random2DTo1D(value,14395.5964, float2(15.637, 76.243)),
		Random2DTo1D(value,13684.6034,float2(45.366, 23.168))
	);
}

float2 Random2DTo2D(float2 seed,float2 x)
{
    return float2(
		Random1DTo1D(x.x,43758.5453,seed.x),
		Random1DTo1D(x.y,43758.5453,seed.y)
	);
}
void GenerateRandomAndNextSeed(inout float2 seed,out float2 random){
random=Random2DTo2D(seed);
seed.x+=Random2DTo2D(seed).x*_Time.w;
seed.y-=Random2DTo2D(seed).y*_Time.w;
}

void GenerateRandomAndNextSeed(inout float2 seed,out float2 random1,out float2 random2){

seed.x+=1;
seed.y+=1;
random1=Random2DTo2D(seed,1);
seed.x+=1;
seed.y+=1;
random2=Random2DTo2D(seed,1);
 
}

void GenerateRandomAndNextSeed(inout float2 seed,out float2 random1,out float2 random2,out float2 random3,out float2 random4){

    float4 rand1= RealtimeRayTracingWhiteNoiseTexture.SampleLevel(sampler_point_repeat,seed,0).xyzw;
    seed=rand1.xy;

    float4 rand2= RealtimeRayTracingWhiteNoiseTexture.SampleLevel(sampler_point_repeat,seed,0).xyzw;
    random1=rand2.xy;
    seed=rand2.xy;
 
    random2=rand2.zw;
    random3=rand1.xy;
    random4=rand1.zw;
}
    #endif