Shader "Hidden/Custom/Glitch/Digital"
{
	HLSLINCLUDE

		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

		sampler2D _NoiseTex;
		sampler2D _TrashTex;
		float _Intensity;

		float4 Frag(VaryingsDefault i) : SV_Target
		{
			float4 glitch = tex2D(_NoiseTex, i.texcoord);

			float thresh = 1.001 - _Intensity * 1.001;
			float w_d = step(thresh, pow(abs(glitch.z), 2.5)); // displacement glitch
			float w_f = step(thresh, pow(abs(glitch.w), 2.5)); // frame glitch
			float w_c = step(thresh, pow(abs(glitch.z), 3.5)); // color glitch

			// Displacement.
			float2 uv = frac(i.texcoord + glitch.xy * w_d);
			float4 source = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

			// Mix with trash frame.
			float3 color = lerp(source, tex2D(_TrashTex, uv), w_f).rgb;

			// Shuffle color components.
			float3 neg = saturate(color.grb + (1 - dot(color, 1)) * 0.5);
			color = lerp(color, neg, w_c);

			return float4(color, source.a);
		}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment Frag

			ENDHLSL
		}
	}
}
