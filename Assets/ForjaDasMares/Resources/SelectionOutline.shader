Shader "Forja/Selection Outline"
{
 Properties{_OutlineColor("Outline",Color)=(1,.85,.3,1) _Thickness("Pixels",Float)=2.5}
 SubShader
 {
  Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Geometry+20" "RenderType"="Opaque"}
  Pass
  {
   Tags{"LightMode"="SRPDefaultUnlit"}
   Cull Front ZWrite Off ZTest LEqual
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct Attributes{float4 position:POSITION;float3 normal:NORMAL;};
   struct Varyings{float4 position:SV_POSITION;};
   CBUFFER_START(UnityPerMaterial)
   half4 _OutlineColor;float _Thickness;
   CBUFFER_END
   Varyings Vert(Attributes input)
   {
    Varyings output;float3 world=TransformObjectToWorld(input.position.xyz);
    float3 normal=TransformObjectToWorldNormal(input.normal);
    output.position=TransformWorldToHClip(world);
    float4 projected=TransformWorldToHClip(world+normal);
    float2 direction=projected.xy/max(abs(projected.w),.0001)-output.position.xy/max(abs(output.position.w),.0001);
    direction/=max(length(direction),.0001);
    output.position.xy+=direction*(_Thickness*2/_ScreenParams.xy)*output.position.w;
    return output;
   }
   half4 Frag(Varyings input):SV_Target{return _OutlineColor;}
   ENDHLSL
  }
 }
}
