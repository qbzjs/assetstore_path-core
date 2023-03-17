Shader "Amazing Assets/Beast/Standard (Specular setup)"
{    
	Properties
	{
//[HideInInspector][CurvedWorldBendSettings] _CurvedWorldBendSettings("0|1|1", Vector) = (0, 0, 0, 0)


		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo", 2D) = "white" {}
		 
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
			     
		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		_GlossMapScale("Smoothness Factor", Range(0.0, 1.0)) = 1.0
		[Enum(Specular Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

		_SpecColor("Specular", Color) = (0.2,0.2,0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}

		[Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


		// Blending state
		[HideInInspector] _Mode ("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0


		//Tessellation
		[KeywordEnum(Fixed, Distance Based, Edge Length, Phong)] _Beast_Tessellation_Type ("", Float) = 0
		_Beast_TessellationFactor("", Range(1, 64)) = 4
		_Beast_TessellationMinDistance("", float) = 10
		_Beast_TessellationMaxDistance("", float) = 35
		_Beast_TessellationEdgeLength("", Range(2, 64)) = 16
		_Beast_TessellationPhong("", Range(0, 1)) = 0.5
		_Beast_TessellationDisplaceMap("", 2D) = "black" {}
		[Enum(UV0,0,UV1,1)] _Beast_TessellationDisplaceMapUVSet("", Float) = 0
		[Enum(Red,0, Green,1, Blue,2, Alpha,3)] _Beast_TessellationDisplaceMapChannel("", Float) = 0
	    _Beast_TessellationDisplaceStrength("", float) = 0
		_Beast_TessellationShadowPassLOD("", Range(0, 1)) = 0.5
		_Beast_TessellationDepthPassLOD("", Range(0, 1)) = 0.5
		_Beast_TessellationUseSmoothNormals("", float) = 0
        [KeywordEnum(None, Normals, Normals And Tangent)] _Beast_Generate ("", Float) = 0
		_Beast_TessellationNormalCoef("", Float) = 1
		_Beast_TessellationTangentCoef("", Float) = 1
	}

	CGINCLUDE
		#define UNITY_SETUP_BRDF_INPUT SpecularSetup
	ENDCG

	SubShader
	{
		Tags { "RenderType"="Beast_Opaque" "PerformanceChecks"="False" }
		LOD 300
	

		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
		{
			Name "FORWARD" 
			Tags { "LightMode" = "ForwardBase" }

			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			CGPROGRAM
			#pragma target 4.6 
			#pragma exclude_renderers vulkan
			// -------------------------------------

			#pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature_local _PARALLAXMAP

			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile_instancing

			#pragma vertex tessvert_surf       
			#pragma hull hs_surf
			#pragma domain ds_surf
			#pragma fragment fragBase
			#include "BeastUnityStandardCoreForward.cginc"


//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"


			#define _BEAST_TESSELLATION_PASS_FORWARDBASE
			#pragma shader_feature_local _BEAST_TESSELLATION_TYPE_FIXED _BEAST_TESSELLATION_TYPE_DISTANCE_BASED _BEAST_TESSELLATION_TYPE_EDGE_LENGTH _BEAST_TESSELLATION_TYPE_PHONG
			#pragma shader_feature_local _ _BEAST_GENERATE_NORMALS _BEAST_GENERATE_NORMALS_AND_TANGENT
			#include "Beast.cginc"

			ENDCG
		}
		// ------------------------------------------------------------------
		//  Additive forward pass (one light per pass)
		Pass
		{
			Name "FORWARD_DELTA"
			Tags { "LightMode" = "ForwardAdd" }
			Blend [_SrcBlend] One
			Fog { Color (0,0,0,0) } // in additive pass fog should be black
			ZWrite Off
			ZTest LEqual

			CGPROGRAM
			#pragma target 4.6  
			#pragma exclude_renderers vulkan
			// -------------------------------------

			#pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP

			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog

			#pragma vertex tessvert_surf       
			#pragma hull hs_surf
			#pragma domain ds_surf
			#pragma fragment fragAdd
			#include "BeastUnityStandardCoreForward.cginc"


//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"


			#define _BEAST_TESSELLATION_PASS_FORWARDADD
			#pragma shader_feature_local _BEAST_TESSELLATION_TYPE_FIXED _BEAST_TESSELLATION_TYPE_DISTANCE_BASED _BEAST_TESSELLATION_TYPE_EDGE_LENGTH _BEAST_TESSELLATION_TYPE_PHONG
			#pragma shader_feature_local _ _BEAST_GENERATE_NORMALS _BEAST_GENERATE_NORMALS_AND_TANGENT
			#include "Beast.cginc"

			ENDCG
		}
		// ------------------------------------------------------------------
		//  Shadow rendering pass
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 4.6 
			#pragma exclude_renderers vulkan
			// -------------------------------------


            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
           // #pragma shader_feature_local _SPECGLOSSMAP
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _PARALLAXMAP
			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing

			#pragma vertex tessvert_surf       
			#pragma hull hs_surf
			#pragma domain ds_surf
			#pragma fragment fragShadowCaster
			#include "BeastUnityStandardShadow.cginc"


//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"


			#define _BEAST_TESSELLATION_PASS_SHADOWCASTER
			#pragma shader_feature_local _BEAST_TESSELLATION_TYPE_FIXED _BEAST_TESSELLATION_TYPE_DISTANCE_BASED _BEAST_TESSELLATION_TYPE_EDGE_LENGTH _BEAST_TESSELLATION_TYPE_PHONG
			#include "Beast.cginc"

			ENDCG
		}

		// ------------------------------------------------------------------
		//  Deferred pass
		Pass
		{
			Name "DEFERRED"
			Tags { "LightMode" = "Deferred" }

			CGPROGRAM
			#pragma target 4.6 
			#pragma exclude_renderers nomrt vulkan


			// -------------------------------------

			#pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP

			#pragma multi_compile_prepassfinal
			#pragma multi_compile_instancing

			#pragma vertex tessvert_surf       
			#pragma hull hs_surf
			#pragma domain ds_surf
			#pragma fragment fragDeferred
			#include "BeastUnityStandardCore.cginc"


//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"


			#define _BEAST_TESSELLATION_PASS_DEFERRED
			#pragma shader_feature_local _BEAST_TESSELLATION_TYPE_FIXED _BEAST_TESSELLATION_TYPE_DISTANCE_BASED _BEAST_TESSELLATION_TYPE_EDGE_LENGTH _BEAST_TESSELLATION_TYPE_PHONG
			#pragma shader_feature_local _ _BEAST_GENERATE_NORMALS _BEAST_GENERATE_NORMALS_AND_TANGENT
			#include "Beast.cginc"

			ENDCG
		}

		// ------------------------------------------------------------------
		// Extracts information for lightmapping, GI (emission, albedo, ...)
		// This pass it not used during regular rendering.
		Pass
		{
			Name "META" 
			Tags { "LightMode"="Meta" }

			Cull Off

			CGPROGRAM
			#pragma target 4.6 
			#pragma exclude_renderers vulkan

			#pragma vertex tessvert_surf       
			#pragma hull hs_surf
			#pragma domain ds_surf
			#pragma fragment frag_meta

			#pragma shader_feature_fragment _EMISSION
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _DETAIL_MULX2
			#pragma shader_feature EDITOR_VISUALIZATION

			#include "BeastUnityStandardMeta.cginc"


//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"


			#define _BEAST_TESSELLATION_PASS_META
			#pragma shader_feature_local _BEAST_TESSELLATION_TYPE_FIXED _BEAST_TESSELLATION_TYPE_DISTANCE_BASED _BEAST_TESSELLATION_TYPE_EDGE_LENGTH _BEAST_TESSELLATION_TYPE_PHONG
			#include "Beast.cginc"

			ENDCG
		}
	}


	FallBack "Standard (Specular setup)"
	CustomEditor "AmazingAssets.BeastEditor.StandardShaderGUI"
}
