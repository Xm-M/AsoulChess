Shader "Unlit/GameUnitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorAppend ("Tint Multiply (Color1)", Color) = (1,1,1,1)
        _CustomColorAppend ("Tint Multiply 2 (Color2)", Color) = (1,1,1,1)
        _AngleX ("AngleX", Float) = 0.0
        _AngleY ("AngleY", Float) = 0.0
        _ScaleX ("ScaleX", Float) = 1.0
        _ScaleY ("ScaleY", Float) = 1.0
        _Alpha ("Alpha", Float) = 1
        _IsVisible ("IsVisible", Range(-1,0)) = 0
        _FlashAmount ("Flash Time Stamp (Time.time on hit, -1 off)", Float) = -1
        [HDR] _FlashColor ("Flash Color HDR (A scales add)", Color) = (2,2,2,1)
        _FlashPeak ("Flash Peak", Float) = 0.1
        _FlashSpeed ("Flash Speed", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ColorAppend;
            fixed4 _CustomColorAppend;
            float _AngleX;
            float _AngleY;
            float _ScaleX;
            float _ScaleY;
            float _Rotation;
            float _Alpha;
            float _IsVisible;
            float _FlashAmount;
            float4 _FlashColor;
            float _FlashPeak;
            float _FlashSpeed;

            float2 scale(float2 v, float s_x, float s_y)
            {
                return float2(v.x * s_x, v.y * s_y);
            }

            float2 rotate(float2 v, float sin_rotation, float cos_rotation)
            {
                return float2(v.x * cos_rotation - v.y * sin_rotation,
                               v.x * sin_rotation + v.y * cos_rotation);
            }

            float2 angle(float2 v, float sin_angle_x, float cos_angle_x)
            {
                return float2(v.x + v.y * sin_angle_x, v.y * cos_angle_x);
            }

            v2f vert(appdata v)
            {
                v2f o;

                float angle_x = _AngleX ;
                float angle_y = -_AngleY;
                
                angle_x += angle_y;
                const float rotation = angle_y;

                const float sin_angle_x = sin(angle_x);
                const float cos_angle_x = cos(angle_x);
                const float sin_rotation = sin(rotation);
                const float cos_rotation = cos(rotation);

                float2 v_in = float2(v.vertex.x, v.vertex.y);
                v_in = scale(v_in, _ScaleX, _ScaleY);
                v_in = angle(v_in, sin_angle_x, cos_angle_x);
                v_in = rotate(v_in, sin_rotation, cos_rotation);

                o.vertex = UnityObjectToClipPos(float4(v_in.x, v_in.y, 0, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _ColorAppend * _CustomColorAppend;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 tex_color = tex2D(_MainTex, i.uv) * i.color;
                if (_IsVisible == -1)
                {
                    tex_color.a = 0.0;
                    return tex_color;
                }
                // 与 Shader Graph 一致：base + HDR * max(0, Peak - (Time-FlashAmount)*Speed) * A
                if (_FlashAmount >= 0.0)
                {
                    float dt = _Time.y - _FlashAmount;
                    float flash = max(0.0, _FlashPeak - dt * max(_FlashSpeed, 0.0001));
                    tex_color.rgb += _FlashColor.rgb * flash * _FlashColor.a;
                }
                tex_color.a *= _Alpha;
                return tex_color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}