Shader "CombatBackgroundHQ" 
{
	Properties 
	{
		_MainTex ("Main (RGB)", 2D) = "black" {}
		_DetailTex ("Detail (RGB)", 2D) = "black" {}
		_NoiseTex ("Noise (ARGB)", 2D) = "black" {}
		_StarColorTex ("Star colors (RGB)", 2D) = "black" {}
		_AsteroidTex ("Asteroid (RGB)", 2D) = "black" {}
		_StarBrightness("Star brighness", Float) = 0.5
		_WorldToUv("WorldToUv", Float) = 0.005
		_Size("Viewport size", Vector) = (10,10,0,0)
		_Offset("Offset", Vector) = (0,0,0,0)
		_StarSizeMultiplier("Star size multiplier", Float) = 0.1
		_MainFlarePower("Main flare power", Float) = 0.01
		_ExtraFlarePower("Extra flare power", Float) = 0.004
		_NebulaR("NebulaRed", Vector) = (1,0,0,0)
		_NebulaG("NebulaGreen", Vector) = (0,1,0,0)
		_NebulaB("NebulaBlue", Vector) = (0,0,1,0)	}
	
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

			uniform float4 _MainTex_ST;
			uniform float4 _DetailTex_ST;

			static const float NoiseMapSize = 256.0;
			static const float NoiseMapStep = 0.00390625;// 1 / noise_map_size;

			uniform float _StarSizeMultiplier;
			uniform float _MainFlarePower;
			uniform float _ExtraFlarePower;

			uniform float _WorldToUv;
			uniform float2 _Size;
			uniform float2 _Offset;

			uniform float _StarBrightness;

			uniform fixed3 _NebulaR;
			uniform fixed3 _NebulaG;
			uniform fixed3 _NebulaB;

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

			float2 rotate45(float2 v)
			{
				const float inv_sqrt2 = 0.70710678;
				return float2((v.y + v.x)*inv_sqrt2, (v.y - v.x)*inv_sqrt2);
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

			float3 asteroid_data(float2 uv, float count, float minsize, float extrasize)
			{
				float2 cell = floor(uv * count);
				float4 value = tex2D(_NoiseTex, cell*NoiseMapStep + count*1.5*NoiseMapStep);
				float2 pos = frac(uv*count);
				float2 center = value.xy;
				float radius = minsize + value.z * extrasize;
				center = clamp(center, float2(radius,radius), float2(1.0 - radius, 1.0 - radius));
	
				if (pos.x - center.x > radius || pos.x - center.x < -radius ||
					pos.y - center.y > radius || pos.y - center.y < -radius) return float3(1,1,1);
		
				return float3((pos.x - center.x)/radius, (pos.y - center.y)/radius, value.w);
			}

			float4 stardust(float2 uv, float star_count, float size, float brightness)
			{
				float3 data = asteroid_data(uv, star_count, size, size);
				if (data.x >= 1.0) return float4(0,0,0,0);

				float time = _UnscaledTime * (1.0 + data.z)*2.0;

				float2 xy = float2(clamp(1.0 + data.x, 0.0, 2.0), clamp(1.0 + data.y, 0.0, 2.0)) * 0.5 / 8.0;
				xy.x += floor(frac(time)*7.99)/8.0;
				xy.y += 0.5*step(data.z, 0.5) + floor((1.0 - frac(time/4.0))*3.99)/8.0;

				float4 color = tex2D(_AsteroidTex, xy);
				color.rgb *= (brightness + data.z)*0.5;
				return color;
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

			float2 calculate_uv(float2 uv, float scale)
			{
				float2 uv_new = (uv - 0.5) * _Size + _Offset;
				return uv_new * _WorldToUv * scale;
			}

			float2 calculate_uv2(float2 uv, float scale)
			{
				float factor = lerp(1.0/_Size.y, _WorldToUv, scale);
				float2 size = _Size * factor;
				float2 uv_new = (uv - 0.5) * size + _Offset * _WorldToUv;
				return uv_new * scale;
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
				fixed4 color;

				fixed2 uv = calculate_uv2(i.texcoord, 1.0);
				fixed2 uv2 = calculate_uv2(i.texcoord, 0.95);
				fixed2 uv3 = calculate_uv2(i.texcoord, 0.9);
				fixed2 uv4 = calculate_uv2(i.texcoord, 0.8);
				fixed2 uv5 = calculate_uv2(i.texcoord, 0.7);
				fixed2 uv6 = calculate_uv2(i.texcoord, 0.6);

				fixed4 nebula = tex2D(_MainTex, uv6 * _MainTex_ST.xy);
				color.r = dot(nebula, _NebulaR);
				color.g = dot(nebula, _NebulaG);
				color.b = dot(nebula, _NebulaB);

				color += tex2D(_DetailTex, uv6 * _DetailTex_ST.xy) * _StarBrightness;
				color += normal_star(uv5, 16, 0.15) * _StarBrightness;
				color += bright_star(uv4, 4, 0.1, 0.2) * _StarBrightness;

				fixed4 c;
				c = stardust(uv3, 32, 0.02, 0.5);
				color.rgb = lerp(color.rgb, c.rgb, c.a*0.75);
				c = stardust(uv2, 24, 0.02, 0.6);
				color.rgb = lerp(color.rgb, c.rgb, c.a*0.75);
				c = stardust(uv, 8, 0.01, 0.7);
				color.rgb = lerp(color.rgb, c.rgb, c.a*0.75);

				//if (uv.x < 0.001 && uv.x >= 0.0) color.r += 0.5;
				//if (uv.y < 0.001 && uv.y >= 0.0) color.g += 0.5;
				//if (uv.x > 0.999 && uv.x <= 1.0) color.r += 0.5;
				//if (uv.y > 0.999 && uv.y <= 1.0) color.g += 0.5;

				color.a = 1;
				return color;
			}

			ENDCG
		}
	}
}
