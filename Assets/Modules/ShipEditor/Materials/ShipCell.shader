Shader "ShipCells"
{
Properties {
	_MainTex ("Texture", 2D) = "white" {}
}

SubShader {
	Tags
	{ 
		"Queue"="Transparent" 
		"IgnoreProjector"="True" 
		"RenderType"="Transparent" 
		"PreviewType"="Plane"
		"CanUseSpriteAtlas"="True"
	}
	
	Lighting Off
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color    : COLOR0;
			};

			struct v2f {
				float4 vertex  : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				fixed4 color : COLOR0;
			};

			sampler2D _MainTex;
			sampler2D _Detail;
			fixed4 _Color;
			
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
				fixed4 c = tex2D(_MainTex, i.texcoord) * i.color;
				return c;
			}
		ENDCG
	}
}

}
