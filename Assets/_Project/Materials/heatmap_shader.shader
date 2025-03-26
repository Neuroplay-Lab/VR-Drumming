// This shader visuzlizes the normal vector values on the mesh.
Shader "Custom/Heatmap"
{    
    Properties
    {
        _HeatTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" "Queue" = "Transparent" }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {            
            HLSLPROGRAM            
            #pragma vertex vert            
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //#include "UnityCG.cginc"

            struct Attributes
            {
                float4 pos : POSITION;
            };

            struct Varyings
            {
                float4 pos  : POSITION;
                float3 worldPos : TEXCOORD1;
            };                                   

            Varyings vert(Attributes IN)
            {                
                Varyings OUT;                
                OUT.pos = mul(UNITY_MATRIX_MVP, IN.pos);
                OUT.worldPos = mul(unity_ObjectToWorld, IN.pos).xyz;                
                return OUT;
            }

            int _Points_Length;
            float4 _Points[100];        // (x, y, z) = position
            float4 _Properties[100];    // x = radius, y = intensity

            Texture2D _HeatTex;
            sampler _HeatTexSampler;


            half4 frag(Varyings IN) : SV_Target
            {                
                half h = 0;
                for (int i = 0; i < _Points_Length; i++)
                {
                    half di = distance(IN.worldPos, _Points[i].xyz);
                    half ri = _Properties[i].x;
                    half hi = 1 - saturate(di / ri);
                    h += hi * _Properties[i].y;
                }

                h = saturate(h);
                half4 color = _HeatTex.Sample(_HeatTexSampler, float2(h, 0.5));
                return color;

            }
            ENDHLSL
        }
    }
    Fallback "Diffuse"
}