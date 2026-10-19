Shader "Abisal/Damage Vignette"
{
    Properties { _Color("Tint",Color)=(1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct inputData { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct outputData { float4 position:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            fixed4 _Color;
            outputData vert(inputData input) { outputData output;output.position=UnityObjectToClipPos(input.vertex);output.uv=input.uv;output.color=input.color*_Color;return output; }
            fixed4 frag(outputData input):SV_Target { fixed4 c=input.color;float edge=smoothstep(.14,.65,length(input.uv-.5)*1.3);c.a*=edge*edge;return c; }
            ENDCG
        }
    }
}
