Shader "Unlit / OutLine"
{
    Properties
    {
        _Color("outline color",color) = (1,1,1,1)
        _Width("outline width",range(0,1)) = 0.2
    }
    Subshader
    {
        Pass
        {
        Tags {"LightMode" = "LightweightForward" "RenderType" = "Opaque" "Queue" = "Geometry + 10"}
                //Tags可不添加，只是为了演示
 
        colormask 0 //不输出颜色
        ZWrite Off
        ZTest Off
 
        Stencil
        {
            Ref 1
            Comp Always
            Pass replace
        }
 
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
            struct appdata
            {
                float4 vertex: POSITION;
            };
 
            struct v2f
            {
                float4 vertex: SV_POSITION;
            };
 
            v2f vert(appdata v)
            {
                v2f o;
                                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }
 
            half4 frag(v2f i) : SV_Target
            {
                return half4 (0.5h, 0.0h, 0.0h, 1.0h);
            }
            ENDHLSL
        }
 
        Pass
        {
                Tags {"RenderType" = "Opaque" "Queue" = "Geometry + 20"}
 
            ZTest off
 
            Stencil {
                Ref 1
                Comp notEqual
                Pass keep
            }
 
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
            struct appdata
            {
                float4 vertex: POSITION;
                float3 normal:NORMAL;
            };
 
            struct v2f
            {
                float4 vertex: SV_POSITION;
            };
 
            half4 _Color;
            half _Width;
 
            v2f vert(appdata v)
            {
                v2f o;
                //v.vertex.xyz += _Width * normalize(v.vertex.xyz);
 
                v.vertex.xyz += _Width * normalize(v.normal);
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }
 
            half4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}