Shader "StarMapBackgroundHQ" 
{
	Properties
	{
		_MainTex ("Main (RGB)", 2D) = "black" {}
		_DetailTex ("Detail (RGB)", 2D) = "black" {}
		_NoiseTex ("Noise (ARGB)", 2D) = "black" {}
		_StarColorTex ("Star colors (RGB)", 2D) = "black" {}
		_TextureWorldSize("Texture world size", Float) = 100
		_ReferenceSize("Reference size", Float) = 20
		_NebulaBrightness("Nebula brighness", Float) = 0.5
		_StarBrightness("Star brighness", Float) = 0.5
		_NebulaColorMode("Nebula Color Mode", Int) = 0
		_Zoom("Zoom", Float) = 1
		_Size("Viewport size", Vector) = (10,10,0,0)
		_Offset("Offset", Vector) = (0,0,0,0)
		_CenterPoint("Center Point", Vector) = (0.5,0.5,0,0)
		_StarSizeMultiplier("Star size multiplier", Float) = 0.1
		_MainFlarePower("Main flare power", Float) = 0.01
		_ExtraFlarePower("Extra flare power", Float) = 0.004
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
			sampler2D _NoiseTex;
			sampler2D _StarColorTex;
			sampler2D _AsteroidTex;
			float4 _MainTex_ST;
			float4 _DetailTex_ST;

			static const float NoiseMapSize = 256.0;
			static const float NoiseMapStep = 0.00390625;// 1 / noise_map_size;

			uniform float _StarSizeMultiplier;
			uniform float _MainFlarePower;
			uniform float _ExtraFlarePower;

			uniform float _TextureWorldSize;
			uniform float _Zoom;
			uniform float2 _Size;
			uniform float2 _Offset;
			uniform float _DetailOffsetFactor;
			uniform float _OtherAlpha;

			uniform float2 _CenterPoint;
			uniform float _ReferenceSize;

			uniform float _StarBrightness;
			uniform float _NebulaBrightness;
			uniform int _NebulaColorMode;

			uniform float _UnscaledTime = 0.0f;

			float4 star_data(float2 uv, float star_count, float minsize, float extrasize)
			{
				float2 cell = floor(uv * star_count);
				float4 value = tex2D(_NoiseTex, cell*NoiseMapStep + star_count*1.5*NoiseMapStep);
				float2 pos = frac(uv*star_count);
				float2 center = value.xy;
				float radius = minsize + value.z * extrasize;
				center = clamp(center, float2(radius,radius), float2(1.0 - radius, 1.0 - radius));
				float d = distance(pos, center);
	
				if (d > radius) return float4(1,1,1,1);
	
				return float4((pos.x - center.x)/radius, (pos.y - center.y)/radius, d/radius, value.w);
			}

			float4 star_color(float x)
			{
				return tex2D(_StarColorTex, float2(x, 0));
			}

			float4 simple_star(float2 uv, float star_count, float brightness)
			{
				float4 data = star_data(uv, star_count, 0.02, _StarSizeMultiplier*0.1);
				float p = brightness * max(0.0, 1.0 - data.z);
				return star_color(data.w)*p;
			}

			float4 normal_star(float2 uv, float star_count, float brightness)
			{
				float4 data = star_data(uv, star_count, 0.05, _StarSizeMultiplier);
				float p = 0.1 * brightness * (1.0/abs(data.z) - 1.0);
				return star_color(data.w)*p;
			}

			float2 rotate45(float2 v)
			{
				const float inv_sqrt2 = 0.70710678;
				return float2((v.y + v.x)*inv_sqrt2, (v.y - v.x)*inv_sqrt2);
			}

			float4 bright_star(float2 uv, float star_count, float brightness, float flare)
			{
				float4 data = star_data(uv, star_count, 0.05, _StarSizeMultiplier);
	
				if (data.z >= 1.0) return float4(0,0,0,0);
	
				float p = 0.1 * brightness * (1.0/abs(data.z) - 1.0);
				float f = 0;
				f += _MainFlarePower / (0.0125 + abs(data.x));
				f += _MainFlarePower / (0.0125 + abs(data.y));
				float2 r = rotate45(data.xy);
				f += _ExtraFlarePower / (0.0125 + abs(r.x));
				f += _ExtraFlarePower / (0.0125 + abs(r.y));
				f *= flare * (1.0 - data.z);
				p += f;
	
				p *= 1.0 + 0.2 * sin(_UnscaledTime*(data.w*2.0 + 1.0) + data.w*4.0);
				return star_color(data.w)*clamp(p, 0.0, 8.0);
			}

			float2 calculate_uv(float2 uv, float2 scale)
			{
				//float2 aspect_ratio = _Size / _Size.y;
				//float zoom = _Size.y / _TextureWorldSize;

				//zoom = (zoom - 1.0)*scale*0.5 + 1.0;

				float y = (_Size.y - _ReferenceSize)*scale + _ReferenceSize;
				float x = y * _Size.x / _Size.y;

				float2 uv_new = ((uv - 0.5) * float2(x,y) + _Offset*scale) / _TextureWorldSize;

				//float2 uv_new = ((uv - 0.5 - _CenterPoint*(1.0 - scale)) * _Size + _Offset*scale) /* _Zoom*/ / _TextureWorldSize;
				return uv_new;//frac(uv_new*0.0625)*16.0;
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
				fixed4 color = fixed4(0,0,0,0);
				fixed2 uv = calculate_uv(i.texcoord, 1.0);
				fixed2 uv2 = calculate_uv(i.texcoord, 0.9);
				fixed2 uv3 = calculate_uv(i.texcoord, 0.8);
				fixed2 uv4 = calculate_uv(i.texcoord, 0.7);

				fixed4 c = tex2D(_MainTex, uv4 * _MainTex_ST.xy) * _NebulaBrightness;
				switch (_NebulaColorMode) {
					case 1: color.rgb += c.brg; break;
					case 2: color.rgb += c.bgr; break;
					case 3: color.rgb += c.gbr; break;
					case 4: color.rgb += c.grb; break;
					case 5: color.rgb += c.rbg; break;
					default: color.rgb += c.rgb; break;
				}

				color += tex2D(_DetailTex, uv4 * _DetailTex_ST.xy) * _StarBrightness;
				color += normal_star(uv3, 64, 1.0) * _StarBrightness;
				
				//if (tileX < 0.3)
				//	color += simple_star(uv3, 128, 0.5);
				//if (tileX < 0.2)
				//	color += simple_star(uv3, 256, 0.5);

				color += normal_star(uv2, 32, 0.5) * _StarBrightness;
				color += bright_star(uv2, 16, 0.2, 0.2);
				//color += bright_star(calculate_uv(i.texcoord, _Size, _Offset*0.4, 0.4), 2, 0.2, 0.2);

				color.a = 1;
				return color;
			}

			ENDCG
		}
	}
}
