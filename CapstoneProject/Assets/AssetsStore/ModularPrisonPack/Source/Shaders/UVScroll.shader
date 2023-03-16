// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "GabroMedia/UVScroll"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_NumberAtlas("NumberAtlas", 2D) = "white" {}
		_Text_Color("Text_Color", Color) = (0,0,0,0)
		_RowNumber("RowNumber", Float) = 0
		_ColumnNumber("ColumnNumber", Float) = 0
		_TotalRowNumber("TotalRowNumber", Float) = 0
		_TotalColumnNumber("TotalColumnNumber", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha novertexlights 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Text_Color;
		uniform sampler2D _NumberAtlas;
		uniform float _TotalRowNumber;
		uniform float _RowNumber;
		uniform float _TotalColumnNumber;
		uniform float _ColumnNumber;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _Text_Color.rgb;
			float2 appendResult18 = (float2(( ( 1.0 / _TotalRowNumber ) * _RowNumber ) , ( ( 1.0 / -_TotalColumnNumber ) * _ColumnNumber )));
			float2 panner10 = ( 1.0 * appendResult18 + i.uv_texcoord);
			float4 tex2DNode4 = tex2D( _NumberAtlas, panner10 );
			o.Alpha = tex2DNode4.a;
			clip( tex2DNode4.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Standard"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16100
1927;29;1906;1004;979.1583;435.925;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;25;-3673.583,410.6644;Float;False;Property;_TotalColumnNumber;TotalColumnNumber;6;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-3743.441,-441.3749;Float;False;Property;_TotalRowNumber;TotalRowNumber;5;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;31;-3382.045,414.0236;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-3700.183,-1.635669;Float;False;Constant;_Float2;Float 2;4;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;27;-3179.239,-503.7755;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-2845.28,419.2642;Float;False;Property;_ColumnNumber;ColumnNumber;4;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-2851.641,-112.4756;Float;False;Property;_RowNumber;RowNumber;3;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;22;-3238.681,189.4644;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-2587.88,-247.2357;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-2609.481,235.0643;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;18;-2081.482,0.8643301;Float;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;12;-1441.481,-210.9357;Float;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;13;-1294.481,286.0643;Float;True;Constant;_Float0;Float 0;3;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;10;-1062.481,46.06433;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1975.481,368.0643;Float;False;Constant;_Float1;Float 1;3;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;29;-1662.14,220.324;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;4;-708,24;Float;True;Property;_NumberAtlas;NumberAtlas;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-566,-241;Float;False;Property;_Text_Color;Text_Color;2;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;2;113,-12;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;GabroMedia/UVScroll;False;False;False;False;False;True;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;Standard;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;31;0;25;0
WireConnection;27;0;23;0
WireConnection;27;1;28;0
WireConnection;22;0;23;0
WireConnection;22;1;31;0
WireConnection;19;0;27;0
WireConnection;19;1;26;0
WireConnection;20;0;22;0
WireConnection;20;1;21;0
WireConnection;18;0;19;0
WireConnection;18;1;20;0
WireConnection;10;0;12;0
WireConnection;10;2;18;0
WireConnection;10;1;13;0
WireConnection;29;0;18;0
WireConnection;29;1;16;0
WireConnection;4;1;10;0
WireConnection;2;0;8;0
WireConnection;2;9;4;4
WireConnection;2;10;4;4
ASEEND*/
//CHKSM=C812F771C22C9FA44676DB59ECD41404DA5DB4BC