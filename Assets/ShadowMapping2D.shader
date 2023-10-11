Shader "Hidden/ShadowMapping2D"
{
    Properties
    {
        _Screen("Screen", vector) = (1920.0, 1080.0, 0.0, 0.0)
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

            struct Spotlight{
                float3 m_position;
                float  m_radius;
                float3 m_direction;
                float  m_angle;
            };

            struct BoxObject{
                float4 m_lines[4];
            };

            struct CircleObject{
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
                float2     uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            StructuredBuffer<Spotlight>    Spotlights;
            StructuredBuffer<CircleObject> CircleObjects;
            StructuredBuffer<BoxObject>    BoxObjects;
            float4 _Screen;
            int CircleObjectCount;
            int BoxObjectCount;
            sampler2D _MainTex;

            // Line to circle intersection detection taken from https://stackoverflow.com/a/1084899
            int LineCircleIntersection(CircleObject circle, float2 startPosition, float2 endPosition){                
                // If this line intersects 2 points on the circle, the pixel is behind the object
                // and is a shadow
                int intersections = 0;
            
                float2 d = endPosition   - startPosition;
                float2 f = startPosition - circle.m_position.xy;
            
                float aspectRatio = _Screen.x / _Screen.y;
                d.x *= aspectRatio;
                f.x *= aspectRatio;
            
                float a = dot(d, d);
                float b = 2*dot(f, d);
                float c = dot(f, f) - circle.m_radius * circle.m_radius;
            
                float discriminant = b*b-4*a*c;
                if(discriminant < 0.0)
                    return intersections;
                else{
                   discriminant = sqrt(discriminant);
            
                   float t1 = (-b - discriminant)/(2*a);
                   float t2 = (-b + discriminant)/(2*a);
            
                   if(t1 >= 0.0 && t1 <= 1.0)
                      intersections++;
                   
                   if(t2 >= 0.0 && t2 <= 1.0)
                      intersections++;
                }
                return intersections;
            }

            // Forgot to copy paste the link for this solution here :(
            int LineToLineIntersection(float2 aLineStart, float2 aLineEnd, float2 bLineStart, float2 bLineEnd){
                float2 s1, s2;
                s1 = aLineEnd - aLineStart;
                s2 = bLineEnd - bLineStart;
            
                float s, t;
                s = (-s1.y * (aLineStart.x - bLineStart.x) + s1.x * (aLineStart.y - bLineStart.y)) / (-s2.x * s1.y + s1.x * s2.y);
                t = ( s2.x * (aLineStart.y - bLineStart.y) - s2.y * (aLineStart.x - bLineStart.x)) / (-s2.x * s1.y + s1.x * s2.y);
            
                if(s >= 0.0 && s <= 1.0 && t>= 0.0 && t <= 1.0){
                    return 1;
                }
                return 0;
            }
           
            fixed4 frag (v2f v) : SV_Target
            {                 
                // Checking if current pixel is located in Spotlight, if not then we return red color
                float aspectRatio = _Screen.x / _Screen.y;
                float2 lightDir = v.uv.xy - Spotlights[0].m_position.xy;
                
                float2 lightRadius = float2(Spotlights[0].m_radius / aspectRatio, Spotlights[0].m_radius);
                
                // Check if fragmentPosition is in the cone
                float ligthDot = dot(normalize(lightDir), normalize(Spotlights[0].m_direction.xy));
                if(ligthDot < cos(radians(Spotlights[0].m_angle)/2.0) || length(lightDir / lightRadius) >= 1.0)
                    return fixed4(1.0, 0.0, 0.0, 1.0);
                
                for(int i = 0; i < CircleObjectCount; i++){
                    if(LineCircleIntersection(CircleObjects[i], Spotlights[0].m_position.xy, v.uv.xy) == 2)
                        return fixed4(0.0, 0.0, 0.0, 1.0);
                }

                for(int i = 0; i < BoxObjectCount; i++){
                    int intersections = 0;
                    for(int j = 0; j < 4; j++ ) {
                       // if pixel has intersected with object 2 times it's behind it
                       intersections += LineToLineIntersection(Spotlights[0].m_position.xy, v.uv.xy, BoxObjects[i].m_lines[j].xy, BoxObjects[i].m_lines[j].zw);
                       if(intersections == 2)
                          return fixed4(0.0, 0.0, 0.0, 1.0); 
                    } 
                }

                return fixed4(0.0, 1.0, 0.0, 1.0);
            }
            ENDCG
        }
    }
   
}