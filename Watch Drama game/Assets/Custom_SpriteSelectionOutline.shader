Shader "Custom/SpriteSelectionOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineThickness ("Outline Thickness", Float) = 2.0
        _Glow ("Glow Intensity", Float) = 1.0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Sprite"
            "CanUseSpriteAtlas"="True"
        }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            Lighting Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _Glow;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                float4 c = tex2D(_MainTex, IN.texcoord);
                float alpha = c.a;

                // Offset'i sabit bir değere bağla (ör: 0.01)
                float2 offset = float2(_OutlineThickness * 0.01, _OutlineThickness * 0.01);

                float outline = 0.0;
                outline += tex2D(_MainTex, IN.texcoord + float2(offset.x, 0)).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(-offset.x, 0)).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(0, offset.y)).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(0, -offset.y)).a;
                outline += tex2D(_MainTex, IN.texcoord + offset).a;
                outline += tex2D(_MainTex, IN.texcoord - offset).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(offset.x, -offset.y)).a;
                outline += tex2D(_MainTex, IN.texcoord + float2(-offset.x, offset.y)).a;

                outline = step(0.01, outline) * (1 - alpha);

                float4 outlineColor = _OutlineColor * outline * _Glow;

                return c + outlineColor;
            }
            ENDCG
        }
    }
} 