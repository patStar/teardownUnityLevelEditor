Shader "Custom/NewSurfaceShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 3D) = "" {}
        _TexSize ("Texture Size", Int) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass{
            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            sampler3D _MainTex;  
            int _TexSize;

            struct vertInput
            {
                float4 pos : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct vertOutput
            {                
                float3 camPos : TEXCOORD1;
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            vertOutput vert(vertInput i)
            {
                vertOutput o;
                o.pos = UnityObjectToClipPos(i.pos);
                o.texcoord = i.texcoord;
                o.camPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
                return o;
			}

            half4 frag(vertOutput o) : SV_Target
            {
                          
                half4 mainColor = tex3D(_MainTex,o.texcoord+normalize(o.texcoord - o.camPos)*0.1);
                return mainColor;
			}

            ENDCG
        }
    }
    FallBack "Diffuse"
}
