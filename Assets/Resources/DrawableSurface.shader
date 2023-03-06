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
            CGPROGRAM
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
                float distance = 100.0f * length(_End - _Start);

                for (int i = 0; i < ceil(distance); ++i) {
                    const float2 pos = lerp(_Start, _End, i / distance);

                    if (length(IN.uv - pos) < _BrushSize) {
                        return _Color;
                    }
                }

                // float2 dir = _End.xy - _Start.xy;
                // float t = dot(IN.uv - _Start.xy, dir) / length(dir);
                // float2 p = (IN.uv - _Start.xy) - t * dir;
                //
                // if (length(p) < _BrushSize) {
                //     return _Color;
                // }

                return tex2D(_MainTex, IN.uv);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
