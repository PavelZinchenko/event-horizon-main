Shader "Sprites/Blink"
{
Properties {
	/*[PerRendererData] */_MainTex("Sprite Texture", 2D) = "white" {}
	_TimeScale("TimeScale", Float) = 1.0
	_Min("Min", Float) = 0.0
	_Max("Max", Float) = 1.0

	_StencilComp("Stencil Comparison", Float) = 8
	_Stencil("Stencil ID", Float) = 0
	_StencilOp("Stencil Operation", Float) = 0
	_StencilWriteMask("Stencil Write Mask", Float) = 255
	_StencilReadMask("Stencil Read Mask", Float) = 255
	_ColorMask("Color Mask", Float) = 15
}

SubShader {
	Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
	}
	
	Stencil
	{
		Ref[_Stencil]
		Comp[_StencilComp]
		Pass[_StencilOp]
		ReadMask[_StencilReadMask]
		WriteMask[_StencilWriteMask]
	}

	Cull Off
	Lighting Off
	ZWrite Off
	ZTest[unity_GUIZTestMode]
	Blend SrcAlpha OneMinusSrcAlpha
	ColorMask[_ColorMask]

	Pass {  
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
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float _TimeScale;
			float _Min;
			float _Max;
			uniform float _UnscaledTime = 0.0f;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, i.texcoord)*i.color;

				float p = 0.5 + 0.5*sin(_UnscaledTime*_TimeScale);
				c.rgb *= _Min + (_Max - _Min)*p;

				return c;
			}
		ENDCG
	}
}

}
