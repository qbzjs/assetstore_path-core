// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "GabroMedia/ColorChange"
{
	Properties
	{
		_Custom_Color("Custom_Color", Color) = (0.2151444,0.9191176,0.1216479,0)
		[NoScaleOffset][Normal]_Normal("Normal", 2D) = "bump" {}
		[NoScaleOffset]_Basecolor("Basecolor", 2D) = "white" {}
		[NoScaleOffset]_ColorMask("ColorMask", 2D) = "white" {}
		[NoScaleOffset]_MetallicSmoothness("MetallicSmoothness", 2D) = "white" {}
		[NoScaleOffset]_AO_Map("AO_Map", 2D) = "white" {}
		[Toggle(_HUESHIFTONLY_ON)] _HueShiftOnly("HueShiftOnly", Float) = 0
		_HueShift("HueShift", Range( 0 , 1)) = 0
		[Toggle(_COLORCHANGE_ON)] _ColorChange("ColorChange?", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma shader_feature _HUESHIFTONLY_ON
		#pragma shader_feature _COLORCHANGE_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform sampler2D _Basecolor;
		uniform float4 _Custom_Color;
		uniform sampler2D _ColorMask;
		uniform float _HueShift;
		uniform sampler2D _MetallicSmoothness;
		uniform sampler2D _AO_Map;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal6 = i.uv_texcoord;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal6 ) );
			float2 uv_Basecolor8 = i.uv_texcoord;
			float4 tex2DNode8 = tex2D( _Basecolor, uv_Basecolor8 );
			float2 uv_ColorMask10 = i.uv_texcoord;
			float4 tex2DNode10 = tex2D( _ColorMask, uv_ColorMask10 );
			float4 lerpResult9 = lerp( tex2DNode8 , _Custom_Color , tex2DNode10.r);
			#ifdef _COLORCHANGE_ON
				float4 staticSwitch13 = lerpResult9;
			#else
				float4 staticSwitch13 = tex2DNode8;
			#endif
			float3 hsvTorgb18 = HSVToRGB( float3(tex2DNode8.r,_HueShift,1.0) );
			float4 lerpResult21 = lerp( tex2DNode8 , float4( hsvTorgb18 , 0.0 ) , tex2DNode10.r);
			#ifdef _HUESHIFTONLY_ON
				float4 staticSwitch22 = lerpResult21;
			#else
				float4 staticSwitch22 = staticSwitch13;
			#endif
			o.Albedo = staticSwitch22.rgb;
			float2 uv_MetallicSmoothness11 = i.uv_texcoord;
			float4 tex2DNode11 = tex2D( _MetallicSmoothness, uv_MetallicSmoothness11 );
			o.Metallic = tex2DNode11.r;
			o.Smoothness = tex2DNode11.a;
			float2 uv_AO_Map12 = i.uv_texcoord;
			o.Occlusion = tex2D( _AO_Map, uv_AO_Map12 ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Standard"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16100
1927;29;1906;1004;2089.301;1380.169;1.996837;True;True
Node;AmplifyShaderEditor.SamplerNode;10;-1169.003,-478.6754;Float;True;Property;_ColorMask;ColorMask;3;1;[NoScaleOffset];Create;True;0;0;False;0;8e1275d618fdca84eb96fc70d0642f83;8e1275d618fdca84eb96fc70d0642f83;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;-763.2214,-98.91479;Float;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-807.2214,-204.9148;Float;False;Property;_HueShift;HueShift;7;0;Create;True;0;0;False;0;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-1028,-706;Float;False;Property;_Custom_Color;Custom_Color;0;0;Create;True;0;0;False;0;0.2151444,0.9191176,0.1216479,0;0.2151444,0.9191176,0.1216479,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;8;-1099,-924;Float;True;Property;_Basecolor;Basecolor;2;1;[NoScaleOffset];Create;True;0;0;False;0;bbf4a9ba581996f418c6a0dee387e059;bbf4a9ba581996f418c6a0dee387e059;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;9;-416,-784;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.HSVToRGBNode;18;-384.2214,-290.9148;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;21;59.77856,-393.9148;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;13;-107.5635,-932.696;Float;False;Property;_ColorChange;ColorChange?;8;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;11;-750.0031,240.3246;Float;True;Property;_MetallicSmoothness;MetallicSmoothness;4;1;[NoScaleOffset];Create;True;0;0;False;0;b1747b22dce9f8543a57242314e0245e;b1747b22dce9f8543a57242314e0245e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;22;328.9406,-441.8485;Float;False;Property;_HueShiftOnly;HueShiftOnly;6;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;12;-717.2022,472.0247;Float;True;Property;_AO_Map;AO_Map;5;1;[NoScaleOffset];Create;True;0;0;False;0;0d817dfe20e377c468fbd8e46b187b6e;0d817dfe20e377c468fbd8e46b187b6e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;6;-790.4005,-1.335846;Float;True;Property;_Normal;Normal;1;2;[NoScaleOffset];[Normal];Create;True;0;0;False;0;893bd8c2212125a4697da6b74f9cbbed;893bd8c2212125a4697da6b74f9cbbed;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;816.1692,-72.67062;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;GabroMedia/ColorChange;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;Standard;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;8;0
WireConnection;9;1;3;0
WireConnection;9;2;10;1
WireConnection;18;0;8;0
WireConnection;18;1;20;0
WireConnection;18;2;19;0
WireConnection;21;0;8;0
WireConnection;21;1;18;0
WireConnection;21;2;10;1
WireConnection;13;1;8;0
WireConnection;13;0;9;0
WireConnection;22;1;13;0
WireConnection;22;0;21;0
WireConnection;0;0;22;0
WireConnection;0;1;6;0
WireConnection;0;3;11;1
WireConnection;0;4;11;4
WireConnection;0;5;12;1
ASEEND*/
//CHKSM=653753EC43276CBF49C49B8683B4E715ABE93B84