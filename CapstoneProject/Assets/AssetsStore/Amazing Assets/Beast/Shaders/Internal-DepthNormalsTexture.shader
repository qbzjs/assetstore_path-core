Shader "Hidden/Beast/Internal-DepthNormalsTexture" {
Properties 
{
//[HideInInspector][CurvedWorldBendSettings] _CurvedWorldBendSettings("0|1|1", Vector) = (0, 0, 0, 0)


	_MainTex ("", 2D) = "white" {}
	_Cutoff ("", Float) = 0.5
	_Color ("", Color) = (1,1,1,1)


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



SubShader 
{
	Tags { "RenderType"="Beast_Opaque" }
	
	Pass 
	{
		CGPROGRAM
		#pragma vertex tessvert_surf       
		#pragma hull hs_surf
		#pragma domain ds_surf
		#pragma fragment frag
		#include "UnityCG.cginc"

		#pragma target 4.6

		struct VertexInput
		{
			float4 vertex	: POSITION;
			half3 normal	: NORMAL;
			float2 uv0		: TEXCOORD0;
			float2 uv1		: TEXCOORD1;
		
			float4 smoothNormal : TEXCOORD3;			

			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2fInternal 
		{
			float4 pos : SV_POSITION;
			float4 nz : TEXCOORD0;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		v2fInternal vert(VertexInput v )
		{ 
			v2fInternal o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.nz.xyz = COMPUTE_VIEW_NORMAL;
			o.nz.w = COMPUTE_DEPTH_01;

			return o;
		}



//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"


		#define _BEAST_TESSELLATION_PASS_INTERNAL
		#pragma shader_feature_local _BEAST_TESSELLATION_TYPE_FIXED _BEAST_TESSELLATION_TYPE_DISTANCE_BASED _BEAST_TESSELLATION_TYPE_EDGE_LENGTH _BEAST_TESSELLATION_TYPE_PHONG

		#include "Beast.cginc"


		fixed4 frag(v2fInternal i) : SV_Target
		{
			return EncodeDepthNormal (i.nz.w, i.nz.xyz);
		}

		ENDCG
	}
}



Fallback Off
}
