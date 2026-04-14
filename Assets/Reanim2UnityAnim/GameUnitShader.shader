Shader "Unlit/GameUnitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AngleX ("AngleX", Float) = 0.0
        _AngleY ("AngleY", Float) = 0.0
        _ScaleX ("ScaleX", Float) = 1.0
        _ScaleY ("ScaleY", Float) = 1.0
        _Alpha ("Alpha", Float) = 1
        _IsVisible ("IsVisible", Range(-1,0)) = 0
        _FlashAmount ("Flash Time Stamp (Time.time on hit, -1 off)", Float) = -1
        [HDR] _FlashColor ("Flash Color HDR", Color) = (2,2,2,1)
        _FlashDecay ("Flash Decay (higher = shorter flash)", Float) = 10
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _AngleX;
            float _AngleY;
            float _ScaleX;
            float _ScaleY;
            float _Rotation;
            float _Alpha;
            float _IsVisible;
            float _FlashAmount;
            float4 _FlashColor;
            float _FlashDecay;

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
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 tex_color = tex2D(_MainTex, i.uv);
                if (_IsVisible == -1)
                {
                    tex_color.a = 0.0;
                    return tex_color;
                }
                // 与 AnimatorController.OnGetDamage 等一致：SetFloat("_FlashAmount", Time.time)
                if (_FlashAmount >= 0.0)
                {
                    float dt = _Time.y - _FlashAmount;
                    float w = saturate(1.0 - dt * max(_FlashDecay, 0.001));
                    tex_color.rgb += _FlashColor.rgb * w;
                }
                tex_color.a *= _Alpha;
                return tex_color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}