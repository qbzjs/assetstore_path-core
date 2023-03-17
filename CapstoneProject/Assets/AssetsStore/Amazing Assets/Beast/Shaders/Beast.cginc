#ifndef AMAZING_ASSETS_BEAST_CGINC
#define AMAZING_ASSETS_BEAST_CGINC

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "Tessellation.cginc"



float _Beast_TessellationFactor;
float _Beast_TessellationMinDistance;
float _Beast_TessellationMaxDistance;
float _Beast_TessellationEdgeLength;
float _Beast_TessellationPhong;
sampler2D _Beast_TessellationDisplaceMap;
float4 _Beast_TessellationDisplaceMap_TexelSize;
half4 _Beast_TessellationDisplaceMap_ST;
half _Beast_TessellationDisplaceMapUVSet;
int _Beast_TessellationDisplaceMapChannel;
float _Beast_TessellationDisplaceStrength;
float _Beast_TessellationNormalCoef;
float _Beast_TessellationTangentCoef;
float _Beast_TessellationShadowPassLOD;
float _Beast_TessellationDepthPassLOD;
float _Beast_TessellationUseSmoothNormals;


#ifdef UNITY_CAN_COMPILE_TESSELLATION

// tessellation vertex shader
struct InternalTessInterp_appdata 
{
	float4 vertex : INTERNALTESSPOS;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 texcoord : TEXCOORD0;
	float2 texcoord1 : TEXCOORD1;

	#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
		float2 texcoord2 : TEXCOORD2;
	#endif

	float4 smoothNormal : TEXCOORD3;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};
InternalTessInterp_appdata tessvert_surf (appdata_full v)
{
	InternalTessInterp_appdata o;
	o.vertex = v.vertex;
	o.normal = v.normal;
	o.tangent = v.tangent;
	o.texcoord = v.texcoord;
	o.texcoord1 = v.texcoord1;
   
	#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
		o.texcoord2 = v.texcoord2;
	#endif

	o.smoothNormal = v.texcoord3;

	#if  (defined(STEREO_INSTANCING_ON) && defined(SHADER_API_D3D11)) ||  (defined(UNITY_SUPPORT_INSTANCING) && defined(INSTANCING_ON))
		o.instanceID = v.instanceID;
	#endif

	return o;  
}

// tessellation hull constant shader
UnityTessellationFactors hsconst_surf (InputPatch<InternalTessInterp_appdata,3> v) 
{
	UnityTessellationFactors o;
	float4 tf;


	float tessFactor = _Beast_TessellationFactor;
	#if defined(_BEAST_TESSELLATION_PASS_SHADOWCASTER) || defined(_BEAST_TESSELLATION_PASS_INTERNAL)

		#if defined(_BEAST_TESSELLATION_PASS_SHADOWCASTER)
			tessFactor *= _Beast_TessellationShadowPassLOD;

			#if defined(_BEAST_TESSELLATION_TYPE_EDGE_LENGTH) || defined(_BEAST_TESSELLATION_TYPE_PHONG)
				_Beast_TessellationEdgeLength = clamp(_Beast_TessellationEdgeLength * _Beast_TessellationShadowPassLOD, 2, 64);
			#endif

		#else
			tessFactor *= _Beast_TessellationDepthPassLOD;

			#if defined(_BEAST_TESSELLATION_TYPE_EDGE_LENGTH) || defined(_BEAST_TESSELLATION_TYPE_PHONG)
			  _Beast_TessellationEdgeLength = clamp(_Beast_TessellationEdgeLength * _Beast_TessellationDepthPassLOD, 2, 64);
			#endif
		#endif

	#endif

	
	tessFactor = clamp(tessFactor, 1, 64);

	#if defined(_BEAST_TESSELLATION_TYPE_DISTANCE_BASED) || defined(_BEAST_TESSELLATION_TYPE_EDGE_LENGTH) || defined(_BEAST_TESSELLATION_TYPE_PHONG)
		VertexInput vi[3];
		vi[0].vertex = v[0].vertex;
		vi[0].normal = v[0].normal;
	#ifdef _TANGENT_TO_WORLD
		vi[0].tangent = v[0].tangent;
	#endif
		vi[0].uv0 = v[0].texcoord;
		vi[0].uv1 = v[0].texcoord1;
	#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
		vi[0].uv2 = v[0].texcoord2;
	#endif
		vi[0].smoothNormal = v[0].smoothNormal;

		vi[1].vertex = v[1].vertex;
		vi[1].normal = v[1].normal;
	#ifdef _TANGENT_TO_WORLD
		vi[1].tangent = v[1].tangent;
	#endif
		vi[1].uv0 = v[1].texcoord;
		vi[1].uv1 = v[1].texcoord1;
	#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
		vi[1].uv2 = v[1].texcoord2;
	#endif
		vi[1].smoothNormal = v[1].smoothNormal;


		vi[2].vertex = v[2].vertex;
		vi[2].normal = v[2].normal;
	#ifdef _TANGENT_TO_WORLD
		vi[2].tangent = v[2].tangent;
	#endif
		vi[2].uv0 = v[2].texcoord;
		vi[2].uv1 = v[2].texcoord1;
	#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
		vi[2].uv2 = v[2].texcoord2;
	#endif
		vi[2].smoothNormal = v[2].smoothNormal;


		#if  (defined(STEREO_INSTANCING_ON) && defined(SHADER_API_D3D11)) ||  (defined(UNITY_SUPPORT_INSTANCING) && defined(INSTANCING_ON))

			vi[0].instanceID = v[0].instanceID;
			vi[1].instanceID = v[1].instanceID;
			vi[2].instanceID = v[2].instanceID;

		#endif

		#if defined(_BEAST_TESSELLATION_TYPE_DISTANCE_BASED)
			tf = UnityDistanceBasedTess(vi[0].vertex, vi[1].vertex, vi[2].vertex, _Beast_TessellationMinDistance, _Beast_TessellationMaxDistance, tessFactor);
		#else
			tf = UnityEdgeLengthBasedTess(vi[0].vertex, vi[1].vertex, vi[2].vertex, _Beast_TessellationEdgeLength);
		#endif


		o.edge[0] = tf.x; 
		o.edge[1] = tf.y; 
		o.edge[2] = tf.z; o.inside = tf.w;

	#else

		tf = tessFactor;

	#endif



	o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
	return o;
}

// tessellation hull shader
[UNITY_domain("tri")]
[UNITY_partitioning("fractional_odd")]
[UNITY_outputtopology("triangle_cw")]
[UNITY_patchconstantfunc("hsconst_surf")]
[UNITY_outputcontrolpoints(3)]
InternalTessInterp_appdata hs_surf (InputPatch<InternalTessInterp_appdata,3> v, uint id : SV_OutputControlPointID) 
{
	return v[id];
}

inline float4 CalcTangent(float3 v1, float3 v2, float3 v3, float2 w1, float2 w2, float2 w3, float3 _n)
{
	float x1 = v2.x - v1.x;
	float x2 = v3.x - v1.x;
	float y1 = v2.y - v1.y;
	float y2 = v3.y - v1.y;
	float z1 = v2.z - v1.z;
	float z2 = v3.z - v1.z;

	float s1 = w2.x - w1.x;
	float s2 = w3.x - w1.x;
	float t1 = w2.y - w1.y;
	float t2 = w3.y - w1.y;

	float r = 0.0001f;
	if (s1 * t2 - s2 * t1 != 0)
		r = 1.0f / (s1 * t2 - s2 * t1);

	float3 tan1 = normalize(float3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r));
	float3 tan2 = normalize(float3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r));


	float w = (dot(cross(_n, tan1), tan2) < 0.0f) ? -1.0f : 1.0f;

	return float4(tan1, w);
}

inline float UnpackDepth(float2 uv)
{
	float4 c = tex2Dlod(_Beast_TessellationDisplaceMap, float4(uv, 0, 0));
	
	return c[_Beast_TessellationDisplaceMapChannel] * _Beast_TessellationDisplaceStrength;
}

inline void Displace(inout float4 vertex, inout float3 normal, inout float4 tangent, float2 texcoord, float3 smoothNormal)
{


	float3 displaceVector = _Beast_TessellationUseSmoothNormals > 0.5 ? smoothNormal : normal;


	#if defined(_BEAST_GENERATE_NORMALS) || defined(_BEAST_GENERATE_NORMALS_AND_TANGENT)
		float3 v0 = vertex.xyz;

		#ifdef _BEAST_GENERATE_NORMALS_AND_TANGENT
				float3 v1 = v0 + tangent * _Beast_TessellationTangentCoef;
				float3 v2 = v0 + cross(normal, tangent * _Beast_TessellationTangentCoef) * -1;
		#else
				float3 v1 = v0 + tangent;
				float3 v2 = v0 + cross(normal, tangent) * -1;
		#endif


		float2 uv0 = texcoord * _Beast_TessellationDisplaceMap_ST.xy + _Beast_TessellationDisplaceMap_ST.zw;
		v0 += displaceVector * UnpackDepth(uv0);

		float2 offset = _Beast_TessellationDisplaceMap_TexelSize.xy * _Beast_TessellationNormalCoef * 10;

		float2 uv1 = uv0 + float2(offset.x, 0);
		v1 += displaceVector * UnpackDepth(uv1);

		float2 uv2 = uv0 + float2(0, offset.y);
		v2 += displaceVector * UnpackDepth(uv2);

		vertex.xyz = v0;
		normal = cross(normalize(v2 - v0), normalize(v1 - v0));


		#if defined(_BEAST_TESSELLATION_PASS_FORWARDBASE) || defined(_BEAST_TESSELLATION_PASS_FORWARDADD) || defined(_BEAST_TESSELLATION_PASS_DEFERRED)
			#ifdef _BEAST_GENERATE_NORMALS_AND_TANGENT
				tangent = CalcTangent(v0, v1, v2, uv0, uv1, uv2, normal);
			#endif
		#endif
	#else

		float2 uv = texcoord * _Beast_TessellationDisplaceMap_ST.xy + _Beast_TessellationDisplaceMap_ST.zw;

		vertex.xyz += displaceVector * UnpackDepth(uv);
	
	#endif
}

// tessellation domain shader 
[UNITY_domain("tri")]
#if defined(_BEAST_TESSELLATION_PASS_SHADOWCASTER)
	#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
		VertexOutputStereoShadowCaster
	#else
		VertexOutputShadowCaster 
	#endif
#elif defined(_BEAST_TESSELLATION_PASS_FORWARDADD)
	VertexOutputForwardAdd 
#elif defined(_BEAST_TESSELLATION_PASS_DEFERRED)
	VertexOutputDeferred 
#elif defined(_BEAST_TESSELLATION_PASS_META)
	v2f_meta
#elif defined(_BEAST_TESSELLATION_PASS_INTERNAL)
	v2fInternal
#else
	VertexOutputForwardBase
#endif
ds_surf(UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata, 3> vi, float3 bary : SV_DomainLocation) 
{

	VertexInput v = (VertexInput)0;

	v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;	
	#ifdef _BEAST_TESSELLATION_TYPE_PHONG
		float3 pp[3];
		for (int i = 0; i < 3; ++i)
			pp[i] = v.vertex.xyz - vi[i].normal * (dot(v.vertex.xyz, vi[i].normal) - dot(vi[i].vertex.xyz, vi[i].normal));
			v.vertex.xyz = _Beast_TessellationPhong * (pp[0] * bary.x + pp[1] * bary.y + pp[2] * bary.z) + (1.0f - _Beast_TessellationPhong) * v.vertex.xyz;
	#endif


	v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;

	float4 tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
#ifdef _TANGENT_TO_WORLD
	v.tangent = tangent;
#endif

	v.uv0 = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;
	v.uv1 = vi[0].texcoord1*bary.x + vi[1].texcoord1*bary.y + vi[2].texcoord1*bary.z;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
	v.uv2 = vi[0].texcoord2*bary.x + vi[1].texcoord2*bary.y + vi[2].texcoord2*bary.z;
#endif
	v.smoothNormal = vi[0].smoothNormal*bary.x + vi[1].smoothNormal*bary.y + vi[2].smoothNormal*bary.z;

	
	#ifndef _BEAST_TESSELLATION_TYPE_PHONG 
		Displace(v.vertex, v.normal, tangent, _Beast_TessellationDisplaceMapUVSet > 0.5 ? v.uv1.xy : v.uv0.xy, v.smoothNormal.xyz);
	#endif


	//Curved World
	#if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
		#ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
			CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v.vertex, v.normal, tangent)
		#else
			CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)
		#endif
	#endif


	#if  (defined(STEREO_INSTANCING_ON) && defined(SHADER_API_D3D11)) ||  (defined(UNITY_SUPPORT_INSTANCING) && defined(INSTANCING_ON))
		v.instanceID = vi[0].instanceID*bary.x + vi[1].instanceID*bary.y + vi[2].instanceID*bary.z;
	#endif

	#ifdef _TANGENT_TO_WORLD
		v.tangent = tangent;
	#endif


#if defined(_BEAST_TESSELLATION_PASS_SHADOWCASTER)
	#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
		VertexOutputStereoShadowCaster o = vertShadowCaster(v)
	#else
		VertexOutputShadowCaster o = vertShadowCaster(v);
	#endif
#elif defined(_BEAST_TESSELLATION_PASS_FORWARDADD)
	VertexOutputForwardAdd o = vertForwardAdd(v);
#elif defined(_BEAST_TESSELLATION_PASS_DEFERRED)
	VertexOutputDeferred o = vertDeferred(v);
#elif defined(_BEAST_TESSELLATION_PASS_META)
	v2f_meta o = vert_meta(v);
#elif defined(_BEAST_TESSELLATION_PASS_INTERNAL)
	v2fInternal o = vert(v);
#else
	VertexOutputForwardBase o = vertForwardBase(v);
#endif

	return o;
}

#endif // UNITY_CAN_COMPILE_TESSELLATION


#endif
