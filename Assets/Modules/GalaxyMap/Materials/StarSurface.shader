Shader "StarSurface"
{
	Properties
	{
		_MainTex ("SurfaceMap", 2D) = "white" {}
		_Detalization ("Detalization", Float) = 15
		_Contrast ("Contrast", Float) = 0.1
		_Brightness ("Brightness", Float) = 1.5
		_AnimationSpeed ("AnimationSpeed", Float) = 1.0
		_Min ("Min", Float) = 0.5
		_Max ("Max", Float) = 0.9
    }

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
    	Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 colorGlow : COLOR0;
				fixed4 colorLight : COLOR1;
				fixed4 colorDark : COLOR2;
				half2 texcoord : TEXCOORD0;
			};
			
            fixed _Contrast;
            fixed _Brightness;
            fixed _AnimationSpeed;
            fixed _Detalization;
            fixed _Min;
            fixed _Max;

            uniform float _UnscaledTime = 0.0f;

			//sampler2D _MainTex;

            inline float2 randomVector(float2 UV, float offset)
            {
                float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
                UV = frac(sin(mul(UV, m)) * 46839.32);
                return float2(sin(UV.y*offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
            }

            float voronoi(float2 uv, float density, float angle) {
                float2 g = floor(uv * density);
                float2 f = frac(uv * density);
                float2 md = 8;

                for (int y=-1; y<=1; y++) {
                    for (int x=-1; x<=1; x++) {
                        float2 lattice = float2(x, y);
                        float2 offset = randomVector(lattice + g, angle);
                        float d = distance(lattice + offset, f);
                        md = min(d,md);
                    }
                }
                
                return md;
            }

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.colorGlow = saturate(IN.color*_Brightness + _Contrast*_Brightness);
				OUT.colorLight = IN.color + _Contrast*_Brightness;
				OUT.colorDark = IN.color - _Contrast;

				return OUT;
			}

            fixed4 frag (v2f IN) : SV_Target
            {
                half2 uv = (IN.texcoord - 0.5)*2;
                fixed r2 = uv.x*uv.x + uv.y*uv.y;
                float d = voronoi(uv, _Detalization, _UnscaledTime*_AnimationSpeed);
                float n = smoothstep(_Min, _Max, d);
                float4 c = lerp(lerp(IN.colorDark, IN.colorLight, n), IN.colorGlow, r2);
                c.a = smoothstep(1,0.95,r2);
                return c;
            }
		ENDCG
		}
	}
}
