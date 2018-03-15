﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/OIT/Depth Peeling/Final Blended" {
	Properties {
		_MainTex ("Main Tex", 2D) = "white" {}
	}
	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass {			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			
			struct a2v {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;			
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			
			ENDCG
		}
	}
	FallBack "Diffuse/VertexLit"
}
