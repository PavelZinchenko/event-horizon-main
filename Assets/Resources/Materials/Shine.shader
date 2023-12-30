Shader "Sprites/Shine"
{
Properties {
	/*[PerRendererData] */_MainTex("Sprite Texture", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
	_Threshold("Threshold", Float) = 0.999
	_TimeScale("TimeScale", Float) = 1.0

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
				float2 screenpos : TEXCOORD1;
			};

			sampler2D _MainTex;
			fixed4 _Color;
			float _Threshold;
			float _TimeScale;
			uniform float _UnscaledTime = 0.0f;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenpos = ComputeScreenPos(o.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, i.texcoord)*i.color;

				float p = 0.5 + 0.5*sin(-_UnscaledTime*_TimeScale + i.screenpos.x/2 + i.screenpos.y/5);


				c.rgb = lerp(c.rgb, _Color.rgb, /*pow(p,1024)*/step(_Threshold, p) * _Color.a);
				return c;
			}
		ENDCG
	}
}

}
