Shader "GalaxyMapStar"
{
	Properties
	{
		_MainTex ("Halo", 2D) = "white" {}
		_CoreColor ("CoreColor", Color) = (1,1,1,1)
		_CoreScale ("CoreScale", Float) = 5.0
		_TimeScale ("TimeScale", Float) = 0.1
		_MinBrightness ("MinBrightness", Float) = 0.5
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
		Blend One One

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
				fixed4 coreColor : COLOR0;
				fixed4 haloColor : COLOR1;
				half2 texcoord : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
			};
			
			fixed4 _CoreColor;
			fixed _CoreScale;
			fixed _TimeScale;
			fixed _MinBrightness;
            uniform float _UnscaledTime = 0.0f;

			v2f vert(appdata_t IN)
			{
                fixed t = sin(_TimeScale * _UnscaledTime * (IN.color.r + IN.color.g + IN.color.b + IN.color.a));
                t *= t;
                t *= t;
                t = lerp(_MinBrightness, 1, t);

				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.texcoord1 = (IN.texcoord - 0.5)*_CoreScale + 0.5;
				OUT.haloColor = IN.color;
				OUT.coreColor.rgb = _CoreColor;
                OUT.coreColor.a = t;

				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed halo = tex2D(_MainTex, IN.texcoord).a * IN.coreColor.a;
				fixed core = tex2D(_MainTex, IN.texcoord1).a;
				return IN.coreColor*core + IN.haloColor*halo;
			}
		ENDCG
		}
	}
}
