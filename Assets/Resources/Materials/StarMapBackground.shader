Shader "StarMapBackground" 
{
	Properties 
	{
		_MainTex ("Main (RGB)", 2D) = "black" {}
		_DetailTex ("Detail (RGB)", 2D) = "black" {}

		_Size("Viewport size", Vector) = (10,10,0,0)
		_Position("Position", Vector) = (0,0,0,0)
		_CenterPosition("Center", Vector) = (0,0,0,0)

		_WorldToUv("WorldToUv", Float) = 0.01
		_NebulaBrightness("Nebula brighness", Float) = 0.5
		_StarBrightness("Star brighness", Float) = 0.5
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
			sampler2D _DetailTex;
			float4 _MainTex_ST;
			float4 _DetailTex_ST;

			uniform float _WorldToUv;
			
			uniform float2 _Size;
			uniform float2 _Position;
			uniform float2 _CenterPosition;

			uniform float _StarBrightness;
			uniform float _NebulaBrightness;

			uniform float _UnscaledTime = 0.0f;

			float2 calculate_uv(float2 uv, float scale)
			{
				float2 size = _Size * _WorldToUv;
				float2 position = ((_Position - _CenterPosition) * scale + _CenterPosition) * _WorldToUv;
				float2 uv_new = (uv - 0.5) * size - position;
				return uv_new * scale;
			}

			float2 calculate_uv2(float2 uv, float scale)
			{
				float factor = lerp(1.0/_Size.y, _WorldToUv, scale);
				float2 size = _Size * factor;
				float2 position = /*((_Position - _CenterPosition)*scale + _CenterPosition)*/ _Position * factor; //_WorldToUv;// * scale;
				float2 uv_new = (uv - 0.5) * size - position;
				return uv_new;
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
				fixed2 uv0 = calculate_uv2(i.texcoord, 0.4);
				fixed2 uv1 = calculate_uv2(i.texcoord, 0.5);
				fixed2 uv2 = calculate_uv2(i.texcoord, 0.6);
				fixed2 uv3 = calculate_uv2(i.texcoord, 0.9);
				fixed2 uv4 = calculate_uv2(i.texcoord, 0.95);

				fixed4 color = tex2D(_MainTex, uv0 * _MainTex_ST) * _NebulaBrightness;
				color.rgb += tex2D(_MainTex, uv1 * 3 * _MainTex_ST) * _NebulaBrightness;

				if (_Size.y < 200)
				{
					color.rgb += tex2D(_DetailTex, uv1 * 2 * _DetailTex_ST) * _StarBrightness;
					color.rgb += tex2D(_DetailTex, uv2 * _DetailTex_ST) * _StarBrightness;
					color.rgb += tex2D(_DetailTex, uv3 * 4 * _DetailTex_ST) * _StarBrightness;
					//color.rgb += bright_star(uv4, 16, 1.0, 1.0) * _StarBrightness;
				}
				else
				{
					color.rgb += tex2D(_DetailTex, uv1 * 2 * _DetailTex_ST) * _StarBrightness;
					color.rgb += tex2D(_DetailTex, uv2 * _DetailTex_ST) * _StarBrightness;
				}
					//color += normal_star(uv3, 16, 1.0) * _StarBrightness;
				//else
					//color += simple_star(uv3, 16, 1.0) * _StarBrightness;



				color.a = 1;

				//color.r = frac(uv3.x * 10)*0.5;
				//color.g = frac(uv3.y * 10)*0.5;
				//color.b = frac(uv3.x * uv3.y);
				//if (abs(fmod(uv3.x, 0.1)) < 0.0001) color.rgb = 0.3;
				//if (abs(fmod(uv3.y, 0.1)) < 0.0001) color.rgb = 0.3;

				////color.r = frac(uv_detail.x * 100)*0.5;
				////color.g = frac(uv_detail.y * 100)*0.5;
				////color.b = frac(uv_detail.x * uv3.y);

				//if (abs(fmod(uv_detail.x, 0.1)) < 0.0001) color.rgb = 0.5;
				//if (abs(fmod(uv_detail.y, 0.1)) < 0.0001) color.rgb = 0.5;

				//if (abs(fmod(uv_main.x, 0.1)) < 0.001) color.rgb = 1;
				//if (abs(fmod(uv_main.y, 0.1)) < 0.001) color.rgb = 1;

				return color;
			}

			ENDCG
		}
	}
}
