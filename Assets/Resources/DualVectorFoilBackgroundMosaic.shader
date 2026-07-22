Shader "ThreeBody/DualVectorFoilBackgroundMosaic"
{
    Properties
    {
        _PixelSize ("Pixel Size", Range(4, 64)) = 24
    }
    SubShader
    {
        // Render after the opaque starfield but before all SpriteRenderer ships
        // and projectiles. The GrabPass therefore contains background only, and
        // gameplay units are drawn normally on top of the mosaic afterwards.
        Tags { "Queue"="Transparent-100" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        ZTest Always

        GrabPass { "_DualVectorFoilBackground" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _DualVectorFoilBackground;
            float4 _DualVectorFoilBackground_TexelSize;
            float _PixelSize;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD0;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.grabPos = ComputeGrabScreenPos(output.vertex);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 uv = input.grabPos.xy / input.grabPos.w;
                float2 pixels = floor(uv * _ScreenParams.xy / _PixelSize) * _PixelSize + _PixelSize * 0.5;
                float2 pixelUv = pixels / _ScreenParams.xy;
                fixed4 background = tex2D(_DualVectorFoilBackground, pixelUv);
                float luminance = dot(background.rgb, fixed3(0.299, 0.587, 0.114));
                background.rgb = lerp(background.rgb, fixed3(luminance, luminance * 0.82, luminance * 1.12), 0.38);
                background.a = 1;
                return background;
            }
            ENDCG
        }
    }
}
