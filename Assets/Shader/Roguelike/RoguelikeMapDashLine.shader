Shader "AVZ/RoguelikeMapDashLine"
{
    // 给 LineRenderer 用：沿线条 UV.x 平铺虚线，并用 _ScrollSpeed 做流动动画。
    // 材质挂到 linePrefab 的 LineRenderer；LineRenderer.textureMode 建议 Tile。
    Properties
    {
        _Color ("Color", Color) = (0.75, 0.78, 0.85, 0.9)
        [Header(Dash Pattern)]
        _DashRepeat ("Dash Repeat (per UV unit)", Float) = 8
        _DashRatio ("Dash Fill Ratio", Range(0.05, 0.95)) = 0.55
        _ScrollSpeed ("Scroll Speed", Float) = 1.5
        [Header(Optional Texture Tile)]
        _MainTex ("Texture (optional)", 2D) = "white" {}
        _UseTexture ("Use Texture Alpha", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float _DashRepeat;
            float _DashRatio;
            float _ScrollSpeed;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _UseTexture;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // LineRenderer：uv.x 沿线条方向，uv.y 横跨线宽
                float along = i.uv.x * _DashRepeat - _Time.y * _ScrollSpeed;
                float dash = step(frac(along), _DashRatio);

                fixed4 c = i.color;
                if (_UseTexture > 0.5)
                {
                    fixed4 tex = tex2D(_MainTex, i.uv);
                    c.rgb *= tex.rgb;
                    c.a *= tex.a;
                }

                c.a *= dash;
                return c;
            }
            ENDCG
        }
    }
    FallBack Off
}
