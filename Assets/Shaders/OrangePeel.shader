Shader "SprunkiMiniGames/OrangePeel"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Erase Mask", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BoundsMin ("Bounds Min (object space)", Vector) = (0,0,0,0)
        _BoundsSize ("Bounds Size (object space)", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;
            fixed4 _Color;
            float4 _BoundsMin;
            float4 _BoundsSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 maskUv : TEXCOORD1;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.maskUv = (v.vertex.xy - _BoundsMin.xy) / _BoundsSize.xy;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed maskValue = tex2D(_MaskTex, i.maskUv).r;
                clip(maskValue - 0.5);

                fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
                return col;
            }
            ENDCG
        }
    }
}
