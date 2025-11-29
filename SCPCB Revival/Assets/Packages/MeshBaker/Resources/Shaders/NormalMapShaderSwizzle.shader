Shader "MeshBaker/NormalMapShaderSwizzle"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

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

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 ConvertNormalFormatFromUnity_ToStandard(fixed4 c) {
				fixed4 n;
				n.a = 0.0;
				n.x = c.a * 2.0 - 1.0;
				n.y = c.g * 2.0 - 1.0;
				n.z = sqrt(1 - n.x * n.x - n.y * n.y);

				//now repack in the regular format
				fixed4 cc;
				cc.a = 1.0;
				cc.r = ((n.x + 1.0) * 0.5);
				cc.g = ((n.y + 1.0) * 0.5);
				cc.b = ((n.z + 1.0) * 0.5);
				return cc;
			}

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// Sample the texture
				fixed4 texColor = tex2D(_MainTex, i.uv);

				// Swizzle the color channels
				fixed4 swizzledColor = ConvertNormalFormatFromUnity_ToStandard(texColor);

				return swizzledColor;
			}

		ENDCG
	}
	}
}