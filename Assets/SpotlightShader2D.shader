Shader "Hidden/SpotlightShader2D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Screen("Screen", vector) = (1920.0, 1080.0, 0.0, 0.0) // not Vector2??
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct SpotlightData{
              float4 m_color;
              float3 m_position;  
              float  m_radius;
              float3 m_direction;
              float  m_angle;
            };

            struct PointlightData{
                float4 m_color;
                float3 m_position;
                float  m_radius;
            };
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

    
            float4 _Screen;

            // StructuredBuffer for spotlights is definetly not the best since probably gonna have only one
            // but unity's constant buffers makes no sense so it'll be done like this. 
            StructuredBuffer<SpotlightData> Spotlights;
            int SpotlightCount;

            StructuredBuffer<PointlightData> Pointlights;
            int PointlightCount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                return o;
            }

            sampler2D _MainTex;
            Texture2D ShadowTexture;
            SamplerState samplerShadowTexture;

             // Spotlight/Pointlight calculation taken from https://stackoverflow.com/a/69575213
            float3 PixelInsideLightsource(float aspectRatio, float2 position, float2 lightPosition, float radius, float4 lightColor, bool isSpotlight, float2 direction = float2(0.0, 0.0), float angle = 0.0){
                
                // Spotlights and pointlights are almost treated the same so can reuse the code
                float2 lightRadius = float2(radius, radius);
                lightRadius.x /= aspectRatio;
                
                float2 lightDir = position - lightPosition;
                if(length(lightDir / lightRadius) >= 1.0f)
                    return float3(0.0, 0.0, 0.0);
                    
                // Spotlight check
                if(isSpotlight){
                    float ligthDot = dot(normalize(lightDir), normalize(direction.xy));
                    if(ligthDot < cos(radians(angle)/2.0))
                        return float3(0.0, 0.0, 0.0);
                }

                float3 col = lightColor.w * (1 - length(lightDir / lightRadius)) * lightColor.xyz;
                return col;                       
            }

            fixed4 frag (v2f v) : SV_Target
            {             
                float aspectRatio = _Screen.x / _Screen.y;
                fixed4 col = tex2D(_MainTex, v.uv);

                float4 lightColor = float4(0.0, 0.0, 0.0, 1.0);

                for(int i = 0; i < PointlightCount; i++){
                    lightColor.xyz += PixelInsideLightsource(aspectRatio, v.uv.xy, Pointlights[i].m_position.xy, Pointlights[i].m_radius, Pointlights[i].m_color, false, 0.0);                       
                }

                // Shadows are only for spotlights so exit after pointlights are rendered
                if(ShadowTexture.Sample(samplerShadowTexture, v.uv).g == 0.0)
                   return fixed4(lightColor * col);
                
                for(int i = 0; i < SpotlightCount; i++){
                    lightColor.xyz += PixelInsideLightsource(aspectRatio, v.uv.xy, Spotlights[i].m_position.xy, Spotlights[i].m_radius, Spotlights[i].m_color, true, Spotlights[i].m_direction.xy, Spotlights[i].m_angle);                                                           
                 }
                       
                return col * lightColor;
            }
            ENDCG
        }
    }
}