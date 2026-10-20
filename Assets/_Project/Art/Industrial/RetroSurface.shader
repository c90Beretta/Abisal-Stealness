Shader "Abisal/Retro Surface"
{
    Properties
    {
        _BaseMap("Albedo",2D)="white"{}
        _BaseColor("Tint",Color)=(1,1,1,1)
        _SpecColor("Specular",Color)=(0,0,0,0)
        _BumpMap("Normal",2D)="bump"{}
        _BumpScale("Normal scale",Float)=1
        _EmissionMap("Emission",2D)="white"{}
        _EmissionColor("Emission tint",Color)=(0,0,0,0)
        _Cutoff("Alpha cutoff",Range(0,1))=.5
        _Surface("Surface",Float)=0
        _SrcBlend("Source",Float)=1
        _DstBlend("Destination",Float)=0
        _ZWrite("Depth write",Float)=1
        _Cull("Cull",Float)=2
        _Smoothness("Smoothness",Float)=0
        _Metallic("Metallic",Float)=0
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
        Pass
        {
            Name "RetroForward"
            Tags {"LightMode"="UniversalForwardOnly"}
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite] Cull [_Cull]
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex RetroVertex
            #pragma fragment LitPassFragmentSimple
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitForwardPass.hlsl"
            float _AbisalRetroStrength;
            Varyings RetroVertex(Attributes input)
            {
                Varyings output=LitPassVertexSimple(input);
                float2 grid=float2(480,270);
                float2 ndc=output.positionCS.xy/output.positionCS.w;
                float2 snapped=round(ndc*grid*.5)/(grid*.5);
                output.positionCS.xy=lerp(ndc,snapped,_AbisalRetroStrength)*output.positionCS.w;
                float tick=floor(_Time.y*8);
                output.uv+=sin(tick+output.positionWS.xz*2.1)*.0015*_AbisalRetroStrength;
                return output;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Simple Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Simple Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Simple Lit/DepthNormals"
    }
}
