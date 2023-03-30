// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "GabroMedia/VPaint_Puddles"
{
	Properties
	{
		[NoScaleOffset]_Wet_Albedo("Wet_Albedo", 2D) = "white" {}
		[NoScaleOffset]_Dry_Albedo("Dry_Albedo", 2D) = "white" {}
		[NoScaleOffset]_Dry_MetallicSmoothness("Dry_MetallicSmoothness", 2D) = "white" {}
		[NoScaleOffset]_Dry_AO("Dry_AO", 2D) = "white" {}
		[NoScaleOffset]_Wet_MetallicSmoothness("Wet_MetallicSmoothness", 2D) = "white" {}
		[NoScaleOffset]_Wet_AO("Wet_AO", 2D) = "white" {}
		[NoScaleOffset][Normal]_Wet_Normal("Wet_Normal", 2D) = "bump" {}
		[NoScaleOffset][Normal]_Dry_Normal("Dry_Normal", 2D) = "bump" {}
		[NoScaleOffset]_Height_BlendDry("Height_Blend(Dry)", 2D) = "white" {}
		_Blend_Contrast("Blend_Contrast", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _Wet_Normal;
		uniform sampler2D _Dry_Normal;
		uniform sampler2D _Height_BlendDry;
		uniform float _Blend_Contrast;
		uniform sampler2D _Wet_Albedo;
		uniform sampler2D _Dry_Albedo;
		uniform sampler2D _Wet_MetallicSmoothness;
		uniform sampler2D _Dry_MetallicSmoothness;
		uniform sampler2D _Wet_AO;
		uniform sampler2D _Dry_AO;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Wet_Normal44 = i.uv_texcoord;
			float2 uv_Dry_Normal45 = i.uv_texcoord;
			float2 uv_Height_BlendDry47 = i.uv_texcoord;
			float HeightMask63 = saturate(pow(((( tex2D( _Height_BlendDry, uv_Height_BlendDry47 ).r * 0.0 )*i.vertexColor.r)*4)+(i.vertexColor.r*2),_Blend_Contrast));
			float3 lerpResult46 = lerp( UnpackNormal( tex2D( _Wet_Normal, uv_Wet_Normal44 ) ) , UnpackNormal( tex2D( _Dry_Normal, uv_Dry_Normal45 ) ) , HeightMask63);
			o.Normal = lerpResult46;
			float2 uv_Wet_Albedo39 = i.uv_texcoord;
			float2 uv_Dry_Albedo40 = i.uv_texcoord;
			float4 lerpResult4 = lerp( tex2D( _Wet_Albedo, uv_Wet_Albedo39 ) , tex2D( _Dry_Albedo, uv_Dry_Albedo40 ) , HeightMask63);
			o.Albedo = lerpResult4.rgb;
			float2 uv_Wet_MetallicSmoothness41 = i.uv_texcoord;
			float4 tex2DNode41 = tex2D( _Wet_MetallicSmoothness, uv_Wet_MetallicSmoothness41 );
			float2 uv_Dry_MetallicSmoothness42 = i.uv_texcoord;
			float4 tex2DNode42 = tex2D( _Dry_MetallicSmoothness, uv_Dry_MetallicSmoothness42 );
			float lerpResult48 = lerp( tex2DNode41.r , tex2DNode42.r , HeightMask63);
			o.Metallic = lerpResult48;
			float lerpResult43 = lerp( tex2DNode41.a , tex2DNode42.a , HeightMask63);
			o.Smoothness = lerpResult43;
			float2 uv_Wet_AO49 = i.uv_texcoord;
			float2 uv_Dry_AO50 = i.uv_texcoord;
			float lerpResult51 = lerp( tex2D( _Wet_AO, uv_Wet_AO49 ).r , tex2D( _Dry_AO, uv_Dry_AO50 ).r , HeightMask63);
			o.Occlusion = lerpResult51;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16100
1960;47;1906;1004;2262.869;-778.2317;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;56;-1844.423,1250.877;Float;False;Constant;_Wet_Height_Power;Wet_Height_Power;10;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;47;-1926.194,1032.288;Float;True;Property;_Height_BlendDry;Height_Blend(Dry);9;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-1519.423,1186.877;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;5;-1472.736,931.4946;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;60;-1327.423,1394.877;Float;False;Property;_Blend_Contrast;Blend_Contrast;11;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;44;-1451.835,19.82715;Float;True;Property;_Wet_Normal;Wet_Normal;6;2;[NoScaleOffset];[Normal];Create;True;0;0;False;0;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;45;-1447.455,231.3605;Float;True;Property;_Dry_Normal;Dry_Normal;7;2;[NoScaleOffset];[Normal];Create;True;0;0;False;0;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;49;-1528.749,502.3655;Float;True;Property;_Wet_AO;Wet_AO;5;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;41;-1304.44,-460.2685;Float;True;Property;_Wet_MetallicSmoothness;Wet_MetallicSmoothness;4;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;50;-1513.749,703.3652;Float;True;Property;_Dry_AO;Dry_AO;3;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;39;-1006.655,-906.0647;Float;True;Property;_Wet_Albedo;Wet_Albedo;0;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;40;-1002.655,-702.0646;Float;True;Property;_Dry_Albedo;Dry_Albedo;1;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.HeightMapBlendNode;63;-1089.2,1107.514;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;42;-1289.44,-259.2689;Float;True;Property;_Dry_MetallicSmoothness;Dry_MetallicSmoothness;2;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;46;-780.4557,166.3604;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-1841.423,1593.377;Float;False;Property;_Dry_Height_Power;Dry_Height_Power;10;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;61;-802.9736,1115.686;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;43;-661.3716,-376.6017;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-1612.423,1490.377;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;51;-699.4592,651.2424;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;54;-1926.423,1365.877;Float;True;Property;_Height_Wet;Height_Wet;8;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;48;-295.9781,-60.59964;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;4;-383.0136,-649.6754;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;441.4423,-22.68069;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;GabroMedia/VPaint_Puddles;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;55;0;47;1
WireConnection;55;1;56;0
WireConnection;63;0;55;0
WireConnection;63;1;5;1
WireConnection;63;2;60;0
WireConnection;46;0;44;0
WireConnection;46;1;45;0
WireConnection;46;2;63;0
WireConnection;61;0;63;0
WireConnection;43;0;41;4
WireConnection;43;1;42;4
WireConnection;43;2;63;0
WireConnection;57;0;54;1
WireConnection;57;1;58;0
WireConnection;51;0;49;1
WireConnection;51;1;50;1
WireConnection;51;2;63;0
WireConnection;48;0;41;1
WireConnection;48;1;42;1
WireConnection;48;2;63;0
WireConnection;4;0;39;0
WireConnection;4;1;40;0
WireConnection;4;2;63;0
WireConnection;0;0;4;0
WireConnection;0;1;46;0
WireConnection;0;3;48;0
WireConnection;0;4;43;0
WireConnection;0;5;51;0
ASEEND*/
//CHKSM=CC557CBA233732BD9C79DC9F85249AC2D6B16A07