// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "GabroMedia/Decals_Albedo"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[NoScaleOffset]_Albedo_OpacityA("Albedo_Opacity(A)", 2D) = "white" {}
		_Decal_Color("Decal_Color", Color) = (0.6002505,0.6029412,0.407872,0)
		_Smoothness("Smoothness", Range( 0 , 0.8)) = 0
		[Toggle(_USETEXTUREALBEDO_ON)] _UseTextureAlbedo("UseTextureAlbedo", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Background+0" "ForceNoShadowCasting" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma shader_feature _USETEXTUREALBEDO_ON
		#pragma surface surf Standard keepalpha nofog 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Decal_Color;
		uniform sampler2D _Albedo_OpacityA;
		uniform float _Smoothness;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Albedo_OpacityA1 = i.uv_texcoord;
			float4 tex2DNode1 = tex2D( _Albedo_OpacityA, uv_Albedo_OpacityA1 );
			#ifdef _USETEXTUREALBEDO_ON
				float4 staticSwitch7 = tex2DNode1;
			#else
				float4 staticSwitch7 = _Decal_Color;
			#endif
			o.Albedo = staticSwitch7.rgb;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16100
1927;29;1906;1004;1086;361;1;True;True
Node;AmplifyShaderEditor.ColorNode;5;-618,-210;Float;False;Property;_Decal_Color;Decal_Color;2;0;Create;True;0;0;False;0;0.6002505,0.6029412,0.407872,0;0.8235294,0.6352703,0.2301037,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-656,266;Float;True;Property;_Albedo_OpacityA;Albedo_Opacity(A);1;1;[NoScaleOffset];Create;True;0;0;False;0;None;fb0c3489b233e8f43b2ff6840ef68cec;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;7;-195,-21;Float;False;Property;_UseTextureAlbedo;UseTextureAlbedo;4;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-135,103;Float;False;Property;_Smoothness;Smoothness;3;0;Create;True;0;0;False;0;0;0.3;0;0.8;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;263,-41;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;GabroMedia/Decals_Albedo;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;True;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Background;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;1;5;0
WireConnection;7;0;1;0
WireConnection;0;0;7;0
WireConnection;0;4;6;0
WireConnection;0;10;1;4
ASEEND*/
//CHKSM=8B1A05BDC17763D64CE41129864F16D9BBFAEE04