Shader "Hidden/OIT/Depth Peeling/Depth Copy" {
    SubShader {
        Pass {
            ZTest Always Cull Off ZWrite On ColorMask 0

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"

			sampler2D _CameraDepthTexture;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i, out float oDepth : SV_Depth) : SV_Target
            {
                oDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.texcoord);
                return float4(oDepth, 0, 0, 1);
            }
            ENDCG

        }
    }
    Fallback Off
}
