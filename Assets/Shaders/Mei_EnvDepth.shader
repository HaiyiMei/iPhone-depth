Shader "Unlit/Mei_EnvDepth"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "black" {}
        _CameraTex ("Camera Texture", 2D) = "black" {}
        _HumanTex ("Human Depth Texture", 2D) = "black" {}
        _MinDistance ("Min Distance", Float) = 0.0
        _MaxDistance ("Max Distance", Float) = 8.0
        _DisplayDistance ("Display Distance", Float) = 1.5
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Geometry"
            "RenderType" = "Opaque"
            "ForceNoShadowCasting" = "True"
        }

        Pass
        {
            Cull Off
            ZTest Always
            ZWrite Off
            Lighting Off
            LOD 100
            Tags
            {
                "LightMode" = "Always"
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(DisplayRotationPerFrame)
            float4x4 _DisplayRotationPerFrame;
            float4x4 _InverseMatrix;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = mul(float3(v.uv, 1.0f), _DisplayRotationPerFrame).xy;
                return o;
            }

            float3 HSVtoRGB(float3 arg1)
            {
                float4 K = float4(1.0h, 2.0h / 3.0h, 1.0h / 3.0h, 3.0h);
                float3 P = abs(frac(arg1.xxx + K.xyz) * 6.0h - K.www);
                return arg1.z * lerp(K.xxx, saturate(P - K.xxx), arg1.y);
            }

            UNITY_DECLARE_TEX2D_FLOAT(_MainTex);
            UNITY_DECLARE_TEX2D_FLOAT(_CameraTex);
            UNITY_DECLARE_TEX2D_FLOAT(_HumanTex);

            float _MinDistance;
            float _MaxDistance;
            float _DisplayDistance;

            float4 distance2color(v2f i, float distance)
            {
                float lerpFactor = (distance - _MinDistance) / (_MaxDistance - _MinDistance);
                float hue = lerp(0.70h, -0.15h, saturate(lerpFactor));
                if (hue < 0.0h)
                {
                    hue += 1.0h;
                }
                float3 color = float3(hue, 0.9h, 0.6h);

                return float4(HSVtoRGB(color), 1.0h);
			}

            float4 frag (v2f i): SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float envDistance = UNITY_SAMPLE_TEX2D(_MainTex, i.uv).r;
                float humanDistance = UNITY_SAMPLE_TEX2D(_HumanTex, i.uv).r;

                if (humanDistance > 0.0f)
                {
                    return distance2color(i, humanDistance);
				}
                else
                {
					if (envDistance > _DisplayDistance)
					{
						fixed2 center = fixed2(0.5, 0.5);
						float2x2 fMatrix = float2x2( -1.0f, 0.0f, 0.0f, -1.0f);

						v2f tmp;
						tmp.uv = mul(float3(i.uv, 1.0f), _InverseMatrix).xy;
						fixed2 uv_OverTex = mul(fMatrix, tmp.uv - center) + center;
						return UNITY_SAMPLE_TEX2D(_CameraTex, uv_OverTex);
					}
					else
					{
						return distance2color(i, envDistance);
					}
				}
            }

            // float4 frag (v2f i): SV_Target
            // {
            //     UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

            //     float4 depthColor = distance2color(i);

            //     float4 cameraColor = UNITY_SAMPLE_TEX2D(_CameraTex, i.uv);
            //     return depthColor;

            // }

            ENDHLSL
        }
    }
}
