Shader "Forja/Hero Energy"
{
 Properties { _BaseColor("Tint",Color)=(1,1,1,1) _Billboard("Billboard",Float)=0 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Pass
  {
   Blend SrcAlpha One
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A {float4 p:POSITION;half4 color:COLOR;float2 uv:TEXCOORD0;};
   struct V {float4 p:SV_POSITION;half4 color:COLOR;float2 uv:TEXCOORD0;};
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;float _Billboard;
   CBUFFER_END
   V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.color=i.color*_BaseColor;o.uv=i.uv;return o;}
   half4 Frag(V i):SV_Target
   {
    float d=lerp(abs(i.uv.y-.5)*2,length(i.uv-.5)*2,_Billboard);
    float edge=saturate(1-d);float core=pow(edge,6);
    return half4(i.color.rgb+half3(.6,.7,.8)*core*.65,(edge*edge+core*.5)*i.color.a);
   }
   ENDHLSL
  }
 }
}
