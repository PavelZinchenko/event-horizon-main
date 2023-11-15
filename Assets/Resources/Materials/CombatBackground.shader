Shader "CombatBackground" 
{
	Properties 
	{
		_MainTex("Main (RGB)", 2D) = "black" {}
		_SecondTex("Stars (RGB)", 2D) = "black" {}
		_DetailTex("Detail (RGB)", 2D) = "black" {}
		_StarAlpha("Stars brightness", Float) = 2.0
		_WorldToUv("WorldToUv", Float) = 0.005
		_Size("Viewport size", Vector) = (10,10,0,0)
		_Offset("Offset", Vector) = (0,0,0,0)
		_DetailOffsetFactor("Detail Offset Factor", Float) = 0.75
		_NebulaR("NebulaRed", Vector) = (1,0,0,0)
		_NebulaG("NebulaGreen", Vector) = (0,1,0,0)
		_NebulaB("NebulaBlue", Vector) = (0,0,1,0)
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		ZWrite Off

		Pass {  

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex  : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _SecondTex;
			sampler2D _DetailTex;
			float4 _MainTex_ST;
			float4 _DetailTex_ST;
			float4 _SecondTex_ST;

			uniform float _WorldToUv;
			uniform float2 _Size;
			uniform float2 _Offset;
			uniform float _DetailOffsetFactor;
			uniform float _StarAlpha;

			uniform fixed3 _NebulaR;
			uniform fixed3 _NebulaG;
			uniform fixed3 _NebulaB;

			float2 calculate_uv(float2 uv, float2 size, float2 offset)
			{
				return ((uv - 0.5) * size + offset) * _WorldToUv;
			}

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed2 uv_main = calculate_uv(i.texcoord, _Size, _Offset);
				fixed2 uv_detail = calculate_uv(i.texcoord, _Size, _Offset * _DetailOffsetFactor);

				fixed3 nebula = tex2D(_MainTex, uv_main * _MainTex_ST).rgb;
				fixed4 second = tex2D(_SecondTex, uv_main * _SecondTex_ST.xy) * _StarAlpha;
				fixed4 detail = tex2D(_DetailTex, uv_detail * _DetailTex_ST.xy) * _StarAlpha;

				fixed4 color;
				color.r = dot(nebula, _NebulaR);
				color.g = dot(nebula, _NebulaG);
				color.b = dot(nebula, _NebulaB);

				//color.rgb *= color.rgb;

				color.rgb += second.rgb;
				color.rgb += detail.rgb;
				color.a = 1;

				return color;
			}

			ENDCG
		}
	}
}
