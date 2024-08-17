Shader "Custom/RadialProgressBar"
{
    Properties
    {
        _MainTex ("Mask", 2D) = "white" {}
        _PositiveColor ("Positive Color", Color) = (1, 1, 1, 1)
        _NegativeColor ("Negative Color", Color) = (1, 1, 1, 1)
        _Value ("Value", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100

        Pass
        {
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
                float v : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _PositiveColor;
            float4 _NegativeColor;
            float _Value;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // o.v = asin(v.vertex.y / sqrt(v.vertex.x * v.vertex.x + v.vertex.y * v.vertex.y));
                o.v = atan2(v.vertex.y, v.vertex.x) / (2 * 3.14) + 0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mask = tex2D(_MainTex, i.uv);
                fixed d = mask.b > _Value;
                fixed4 col = _PositiveColor * d + _NegativeColor * (1 - d);
                col *= mask.a;
                return col;
            }
            ENDCG
        }
    }
}
