Shader "Custom/DepthGrayscale" {
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
	 SubShader {
		 Tags { "RenderType"="Opaque" }
		 
		 Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma fragment frag_overlay
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			sampler2D _CameraMotionVectorsTexture;
			float4 _MainTex_TexelSize;
			float4 _MainTex_ST;
			half _Amplitude;
			half _Blend;
			
			struct v2f_common
			{
				float4 pos: SV_POSITION;
				half2 uv: TEXCOORD0;    // Screen space UV (supports stereo rendering)
				half2 uvAlt: TEXCOORD2; // Alternative UV (supports v-flip case)
			};

			half4 VectorToColor(float2 mv)
			{
				half phi = atan2(mv.x, mv.y);
				half hue = (phi / UNITY_PI + 1) * 0.5;

				half r = abs(hue * 6 - 3) - 1;
				half g = 2 - abs(hue * 6 - 2);
				half b = 2 - abs(hue * 6 - 4);
				half a = length(mv);

				return saturate(half4(r, g, b, a));
			}

			half3 HueToRGB(half h)
			{
				half r = abs(h * 6 - 3) - 1;
				half g = 2 - abs(h * 6 - 2);
				half b = 2 - abs(h * 6 - 4);
				half3 rgb = saturate(half3(r, g, b));
			#if UNITY_COLORSPACE_GAMMA
				return rgb;
			#else
				return GammaToLinearSpace(rgb);
			#endif
			}
			
			struct v2f {
				float4 pos: SV_POSITION;
				float4 scrPos: TEXCOORD1;
			 };
			
			//Vertex Shader
			v2f_common vert(appdata_base v)
			{
				half2 uvAlt = v.texcoord;
			#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0) uvAlt.y = 1 - uvAlt.y;
			#endif

				v2f_common o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
				o.uvAlt = UnityStereoScreenSpaceUVAdjust(uvAlt, _MainTex_ST);

				return o;
			}
			
			//Fragment Shader
			//half4 frag (v2f i) : COLOR{
			//	half4 depth;
			//	depth.r = saturate(abs(300*tex2Dproj(_CameraMotionVectorsTexture, UNITY_PROJ_COORD(i.scrPos)).r));
			//	depth.g = saturate(abs(300*tex2Dproj(_CameraMotionVectorsTexture, UNITY_PROJ_COORD(i.scrPos)).g));
			//	depth.b = saturate(abs(300*tex2Dproj(_CameraMotionVectorsTexture, UNITY_PROJ_COORD(i.scrPos)).b));
			//	depth.a = 1;
			// 
			//	float combined = sqrt( depth.r*depth.r + depth.g*depth.g + depth.b*depth.b);
			//	depth.r = combined;
			//	depth.g = combined;
			//	depth.b = combined;
			// 
			//	return depth;
			//}

			// Motion vectors overlay shader (fragment only)
			half4 frag(v2f_common i) : SV_Target
			{
				half _Amplitude = 1.0;
				half _Blend = 1.0;
				half4 src = tex2D(_MainTex, i.uv);

				half2 mv = tex2D(_CameraMotionVectorsTexture, i.uvAlt).rg * _Amplitude;
				half4 mc = VectorToColor(mv);

				half3 rgb = mc.rgb;
				#if !UNITY_COLORSPACE_GAMMA
					rgb = GammaToLinearSpace(rgb);
				#endif

				//half src_ratio = saturate(2 - _Blend * 2);
				//half mc_ratio = saturate(_Blend * 2);
				//rgb = lerp(src.rgb * src_ratio, rgb, mc.a * mc_ratio);

				return half4(rgb, 1.0);
			}
			 ENDCG
		 }
	 }
	 FallBack "Diffuse"
 }