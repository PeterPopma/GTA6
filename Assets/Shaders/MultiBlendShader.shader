Shader "Custom/MultiBlendShader"
{
    Properties
    {
        _MainTex ("Background Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _OverlayPos ("Overlay Position", Vector) = (0, 0, 0, 0)
        _OverlayScale ("Overlay Scale", Vector) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode Off }
            Blend SrcAlpha OneMinusSrcAlpha

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
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _OverlayTex;
            float4 _OverlayPos;
            float4 _OverlayScale;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 bgColor = tex2D(_MainTex, i.texcoord);
                
                // Calculate overlay texture coordinates
                float2 overlayCoord = (i.texcoord - _OverlayPos.xy) / _OverlayScale.xy;

                // Check if within overlay bounds
                if (overlayCoord.x >= 0.0 && overlayCoord.x <= 1.0 && overlayCoord.y >= 0.0 && overlayCoord.y <= 1.0)
                {
                    half4 overlayColor = tex2D(_OverlayTex, overlayCoord);
                    return lerp(bgColor, overlayColor, overlayColor.a);
                }
                else
                {
                    return bgColor;
                }
            }
            ENDCG
        }
    }
}
