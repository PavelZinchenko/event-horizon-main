Shader "Sprites/EnergyShield"
{
Properties{
	[PerRendererData] _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	_Scale("Scale", Float) = 1.0
	_Rect("Rect", Vector) = (0,0,0,0)
}

SubShader{
	Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
	}

	Cull Off
	Lighting Off
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha

	Pass{
		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag			
		#include "UnityCG.cginc"

		struct appdata_t {
			float4 vertex : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f {
			float4 vertex  : SV_POSITION;
			float4 color   : COLOR;
			half2 texcoord : TEXCOORD0;
		};

		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		half _Scale;
		half4 _Rect;

        half getPixel(half2 uv, half lod)
        {
            if (uv.x < _Rect.x || uv.x > _Rect.y || uv.y < _Rect.z || uv.y > _Rect.w)
                return 0;

            return tex2Dlod(_MainTex, half4(uv, 0, lod)).a;
        }

		v2f vert(appdata_t v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.texcoord = v.texcoord;
			o.color = v.color;
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
            float2 center = float2((_Rect.x + _Rect.y) * 0.5, (_Rect.z + _Rect.w) * 0.5);
            float2 offset = (i.texcoord - center) * _Scale.x;
            float2 size = float2((_Rect.y - _Rect.x) * 0.5, (_Rect.w - _Rect.z) * 0.5);

            half2 uv = center + offset;
			half2 d = (_Scale.x - 1.0)*size;

			half a0 = getPixel(uv,0);
			half a1 = getPixel(half2(uv.x - d.x, uv.y), 3);
			half a2 = getPixel(half2(uv.x + d.x, uv.y), 3);
			half a3 = getPixel(half2(uv.x, uv.y - d.y), 3);
			half a4 = getPixel(half2(uv.x, uv.y + d.y), 3);

            d *= 0.71; // 1 / sqrt(2)

            half a5 = getPixel(half2(uv.x - d.x, uv.y - d.y), 3);
			half a6 = getPixel(half2(uv.x - d.x, uv.y + d.y), 3);
			half a7 = getPixel(half2(uv.x + d.x, uv.y - d.y), 3);
			half a8 = getPixel(half2(uv.x + d.x, uv.y + d.y), 3);

            half a = a1+a2+a3+a4+a5+a6+a7+a8;
            a = clamp(a*0.25 - 0.1 - a0*4, 0, 1) + a0*0.1;
			half4 c = i.color;
			c.a *= a;
			return c;
		}

		ENDCG
	}
}
}
