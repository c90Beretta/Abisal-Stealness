Shader "Abisal/PS1 Screen"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        half4 RetroColour(Varyings input):SV_Target
        {
            float2 uv=input.texcoord;
            half3 c=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_PointClamp,uv).rgb;
            c=LinearToSRGB(saturate(c));
            float2 p=floor(input.positionCS.xy);
            const float pattern[16]={0,8,2,10,12,4,14,6,3,11,1,9,15,7,13,5};
            int index=((int)p.y&3)*4+((int)p.x&3);
            c=saturate(c+(pattern[index]/16.0-.5)/31.0);
            c=floor(c*31.0+.5)/31.0;return half4(SRGBToLinear(c),1);
        }
        half4 PointUpscale(Varyings input):SV_Target {return SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_PointClamp,input.texcoord);}
        ENDHLSL
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment RetroColour
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment PointUpscale
            ENDHLSL
        }
    }
}
