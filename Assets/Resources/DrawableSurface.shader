Shader "DrawableSurface"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Start ("Start", Vector) = (0, 0, 0, 0)
        _End ("End", Vector) = (0, 0, 0, 0)
        _Color ("Color", Color) = (0, 0, 0, 1)
        _BrushSize ("BrushSize", float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Start;
            float4 _End;
            float4 _Color;
            float  _BrushSize;

            v2f vert(float4 vertex:POSITION, float2 uv:TEXCOORD0)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertex);
                o.uv = uv;

                return o;
            }

            float4 frag(v2f IN) : SV_TARGET
            {
                const float4 texel = tex2D(_MainTex, IN.uv);

                float dist = 0;

                if (dot(_Start - _End, IN.uv - _Start) > 0) {
                    dist = distance(_Start, IN.uv);
                } else if (dot(_End - _Start, IN.uv - _End) > 0) {
                    dist = distance(_End, IN.uv);
                } else {
                    float2 dir = _End - _Start;
                    float2 perp = float2(dir.y, -dir.x);
                    perp = perp / length(perp);

                    dist = abs(dot(perp, _Start - IN.uv));
                }

                if (dist < _BrushSize) {
                    return lerp(_Color, texel, smoothstep(0.8, 1.0, dist / _BrushSize));
                }

                return texel;
            }
            ENDHLSL
        }
    }

    FallBack "Diffuse"
}
