Shader "Sprites/Color Swap Additive"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        [PerRendererData] _InsideColor ("Inside Color", Color) = (1,1,1,1)
        [PerRendererData] _OutsideColor ("Outside Color", Color) = (1,1,1,1)
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
        Blend One One

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            fixed4 _InsideColor;
            fixed4 _OutsideColor;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 color = tex2D (_MainTex, IN.texcoord) * IN.color;
                
                if(color.r > 0)
                {
                    color.rgb = color.r * _InsideColor.rgb;
				}
                else if(color.g > 0)
                {
                    color.rgb = color.g * _OutsideColor.rgb;
				}

                color.rgb *= color.a;
                return color;
            }
        ENDCG
        }
    }
}