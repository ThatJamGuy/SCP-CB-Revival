Shader "Custom/Wireframe" {
    Properties {
        _WireColor ("Wire Color", Color) = (1,1,1,1)
        _WireThickness ("Wire Thickness", Range(0.1, 10)) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" "RenderPipeline"="HDRenderPipeline" }
        Pass {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2g { float4 vertex : SV_POSITION; };
            struct g2f {
                float4 vertex : SV_POSITION;
                float3 barycentric : TEXCOORD0;
            };

            float4 _WireColor;
            float _WireThickness;

            v2g vert(appdata v) {
                v2g o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> stream) {
                g2f o0, o1, o2;
                o0.vertex = input[0].vertex; o0.barycentric = float3(1,0,0);
                o1.vertex = input[1].vertex; o1.barycentric = float3(0,1,0);
                o2.vertex = input[2].vertex; o2.barycentric = float3(0,0,1);
                stream.Append(o0);
                stream.Append(o1);
                stream.Append(o2);
            }

            float edgeFactor(float3 bary) {
                float3 d = fwidth(bary);
                float3 a3 = smoothstep(float3(0,0,0), d * _WireThickness, bary);
                return min(min(a3.x, a3.y), a3.z);
            }

            fixed4 frag(g2f i) : SV_Target {
                float e = edgeFactor(i.barycentric);
                clip(0.999 - e);
                return _WireColor;
            }
            ENDCG
        }
    }
}