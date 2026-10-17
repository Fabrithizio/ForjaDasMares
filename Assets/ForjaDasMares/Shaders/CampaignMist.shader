Shader "Forja/CampaignMist"
{
    Properties{_BaseColor("Mist color",Color)=(.15,.22,.24,1)}
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            CBUFFER_END
            struct Input{float4 positionOS:POSITION;};
            struct Output{float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;};
            Output Vert(Input v){Output o;o.world=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);return o;}
            half4 Frag(Output i):SV_Target
            {
                float2 p=i.world.xz*.045;
                float n=sin(p.x+_Time.y*.045)*cos(p.y*.87-_Time.y*.03);
                n+=.35*sin(p.x*2.8+p.y*1.2+_Time.y*.06);
                return half4(_BaseColor.rgb*(.9+n*.16),.96);
            }
            ENDHLSL
        }
    }
}
