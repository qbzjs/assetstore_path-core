Shader "Hidden/Shader/UPGEN_Lighting"
{
	HLSLINCLUDE

	#define MAX_LIGHTS_COUNT 96

	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	// The default layout of the render targets (RT0 - RT4) in the geometry buffer (g-buffer)
	// RT0, ARGB32 format: Diffuse color (RGB), occlusion (A).
	// RT1, ARGB32 format: Specular color (RGB), roughness (A).
	// RT2, ARGB2101010 format: World space normal (RGB), unused (A).
	// RT3, ARGB2101010 (non-HDR) or ARGBHalf (HDR) format: Emission + lighting + lightmaps + reflection probes buffer.
	// Depth+Stencil buffer.

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
	TEXTURE2D_SAMPLER2D(_CameraGBufferTexture0, sampler_CameraGBufferTexture0);
	TEXTURE2D_SAMPLER2D(_CameraGBufferTexture2, sampler_CameraGBufferTexture2);

	uniform float _Intensity;
	uniform float4x4 _WorldFromView;
	uniform float4x4 _ViewFromScreen;

	uniform int _LightsCount = 0;
	uniform float4 _LightsPositions[MAX_LIGHTS_COUNT]; // XYZ - position, W - range
	uniform float4 _LightsColors[MAX_LIGHTS_COUNT]; // RGB - color, A - not used

    float4 CustomPostProcess(VaryingsDefault i) : SV_Target
    {
		float2 uv = i.texcoord;
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
		float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
		if (depth == 0) return float4(color.rgb, 1); // do not apply lighting to skybox

		float4 albedo = SAMPLE_TEXTURE2D(_CameraGBufferTexture0, sampler_CameraGBufferTexture0, uv);
		float4 normal = 2 * SAMPLE_TEXTURE2D(_CameraGBufferTexture2, sampler_CameraGBufferTexture2, uv) - 1;

		// transform screen to world space
		float4 viewPos = mul(_ViewFromScreen, float4(uv * 2 - 1, depth, 1)); // inverse projection by clip position
		viewPos /= viewPos.w; // perspective division
		float3 wpos = mul(_WorldFromView, viewPos).xyz;

		// DEBUG
		//return float4(cos(wpos * 100), 1);
		//return float4(depth, depth, depth, 1);
		//return normal;
		//return albedo;
		//return color;

		int c = _LightsCount;
		float3 light; float3 dir; float dist; float4 lp;

		while (c >= 11) // loop unrolling
		{
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }

			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }

			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }

			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
		}

		while (c >= 3) // loop unrolling
		{
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
			c--; lp = _LightsPositions[c]; dir = lp.xyz - wpos; dist = length(dir); if (dist < lp.w) { float i = 1 - dist / lp.w; i *= i; i *= max(0, dot(normal.xyz, dir / dist)); light += _LightsColors[c].rgb * i; }
		}

		while (c > 0) // real iteration
		{
			c--;
			lp = _LightsPositions[c];
			dir = lp.xyz - wpos;
			dist = length(dir);
			if (dist < lp.w)
			{
				float i = 1 - dist / lp.w;
				i *= i;
				i *= max(0, dot(normal.xyz, dir / dist));
				light += _LightsColors[c].rgb * i;
			}
		}

		return float4(color.rgb + albedo.rgb * light * albedo.a * _Intensity, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
				#pragma vertex VertDefault
				#pragma fragment CustomPostProcess
            ENDHLSL
        }
    }

    Fallback Off
}
