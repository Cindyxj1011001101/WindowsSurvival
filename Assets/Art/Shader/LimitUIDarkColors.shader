Shader "Custom/LimitUIDarkColors"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DarkThreshold ("DarkThresholdColor", Color) = (0,0,0,1)
        _TintColor ("Tint Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
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
            float4 _DarkThreshold;
            fixed4 _TintColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                fixed4 col = texColor * _TintColor;
                col.a = texColor.a;

                if (col.r < _DarkThreshold.r && col.a > 0)
                {
                    col.rgb = _DarkThreshold.rgb;
                }


                return col;
            }
            ENDCG
        }
    }
}