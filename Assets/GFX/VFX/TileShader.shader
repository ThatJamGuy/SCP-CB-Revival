Shader "Custom/TileShader"
{   
    Properties {
        _MainTex ("Texture Atlas", 2D) = "white" {}
        _TileIndex ("Current Tile Index", Float) = 0
        _TileCount ("Total Tiles", Float) = 4
        _TileSize ("Tile Size in Atlas", Vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        ENDHLSL

        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "TileShader"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TileIndex;
            float _TileCount;
            float4 _TileSize;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float offsetX = _TileIndex * _TileSize.x;
                float offsetY = _TileIndex * _TileSize.y;
                o.uv = v.uv * _TileSize.xy + float2(offsetX, offsetY);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}