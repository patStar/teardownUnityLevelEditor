Shader "Custom/Vertex Colored"
{
 Properties {
 }
     SubShader {                            
             Pass {
                     ColorMaterial AmbientAndDiffuse                 
             }

              Tags { "RenderType" = "Opaque" }
                  CGPROGRAM
                  #pragma surface surf Lambert
                  struct Input {
                      float4 color : COLOR;
                  };
                  void surf (Input IN, inout SurfaceOutput o) {
                      o.Albedo = IN.color.rgb;
                  }
                  ENDCG
     } 
 }