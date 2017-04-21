// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/Memory Dashes"
{
	Properties
	{
		_SpriteTex("Base (RGB)", 2D) = "white" {
		}

		_DepthAxis("Depth Axis", Vector) = (0,0,1,1)
		_ElevationAxis("Elevation Axis", Vector) = (0,1,0,1)
		_WidthAxis("Width Axis", Vector) = (1,0,0,1)
	}

		SubShader
		{
			Pass
			{
				Tags{
				"RenderType" = "Opaque"	}
				LOD 200
				Cull Off

				CGPROGRAM
					//#pragma target 3.0
					#pragma vertex VS_Main
					#pragma fragment FS_Main
					#pragma geometry GS_Main
					#include "UnityCG.cginc" 


			// **************************************************************
			// Data structures												*
			// **************************************************************
			struct GS_INPUT
			{
				float4	pos		: POSITION;
				float3  size	: NORMAL;  // expressed as (width, elevation, depth)
				float2  tex0	: TEXCOORD0;
				float4	color	: COLOR;
			}
			;
			struct FS_INPUT
			{
				float4	pos		: POSITION;
				float2  tex0	: TEXCOORD0;
				float4	color	: COLOR;
			}
			;

			struct fragmentOutput {
				float4 color : COLOR;
				float depth : DEPTH;
			};

			// **************************************************************
			// Vars															*
			// **************************************************************
			float4 _DepthAxis;
			float4 _ElevationAxis;
			float4 _WidthAxis;

			Texture2D _SpriteTex;
			SamplerState sampler_SpriteTex;

			// **************************************************************
			// Shader Programs												*
			// **************************************************************

			// Vertex Shader ------------------------------------------------
			GS_INPUT VS_Main(appdata_full v)
			{
				GS_INPUT output = (GS_INPUT)0;
				output.pos = mul(unity_ObjectToWorld, v.vertex);

				//output.nextPoint = float4(length(v.normal.xyz)>.001?v.vertex+v.normal:v.vertex+float4(0,1,0,1),1);
				//output.size = mul(_Object2World, float4(v.vertex + v.normal, 1));
				//output.size = mul(_Object2World, float4(v.vertex));

				output.size = v.normal;

				//output.lastPoint = length(v.tangent.xyz)>.001?v.vertex + v.tangent:v.vertex+float4(0,1,0,1);
				//output.lastPoint = mul(_Object2World, v.vertex + v.tangent);


				output.tex0 = float2(0, 0);
				output.color = v.color;


				return output;
			}



			// Geometry Shader -----------------------------------------------------
			[maxvertexcount(4)]
			void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
			{
				GS_INPUT singlePoint = p[0];

				float4 centerPoint = singlePoint.pos;

				float4x4 vp = mul(UNITY_MATRIX_MVP, unity_WorldToObject);
				FS_INPUT pIn;

				float minAxis = min(_ScreenParams.x, _ScreenParams.y);

				float4 screenSpaceCenter = mul(vp, centerPoint);

				float sizeMin = 4*screenSpaceCenter.w/minAxis;

					//1/((.5*screenSpaceCenter.z) * minAxis);//.01 / (centerPoint.z*minAxis);


				////////


				singlePoint.size.x = max(sizeMin, singlePoint.size.x);
				singlePoint.size.z = max(sizeMin, singlePoint.size.z);

				float4 widthOffset = _WidthAxis * singlePoint.size.x / 2.0f;
				float4 elevationOffset = _ElevationAxis * singlePoint.size.y;
				float4 depthOffset = _DepthAxis * singlePoint.size.z / 2.0f;
				float4 lateralSizingOffset = normalize(_WidthAxis) * singlePoint.size.z / 2.0f;

				float4 pointColor = singlePoint.color;

				////////

				//p0
				pIn.pos = mul(vp, centerPoint - lateralSizingOffset + depthOffset + elevationOffset);
				pIn.tex0 = float2(0.0f, 0.0f);
				pIn.color = pointColor;
				triStream.Append(pIn);
				//p3
				pIn.pos = mul(vp, centerPoint + lateralSizingOffset + widthOffset + depthOffset + elevationOffset);
				pIn.tex0 = float2(1.0f, 0.0f);
				pIn.color = pointColor;
				triStream.Append(pIn);

				//p1
				pIn.pos = mul(vp, centerPoint - lateralSizingOffset - depthOffset + elevationOffset);
				pIn.tex0 = float2(0.0f, 1.0f);
				pIn.color = pointColor;
				triStream.Append(pIn);

				//p2
				pIn.pos = mul(vp, centerPoint + lateralSizingOffset + widthOffset - depthOffset + elevationOffset);
				pIn.tex0 = float2(1.0f, 1.0f);
				pIn.color = pointColor;
				triStream.Append(pIn);


				triStream.RestartStrip();

			}


			// Fragment Shader -----------------------------------------------
			fragmentOutput FS_Main(FS_INPUT input)
			{
				float4 outColor = _SpriteTex.Sample(sampler_SpriteTex, input.tex0)
					* input.color;

				float currentDepth = input.pos.z; // + ((19991 * input.pos.x + 49081 * input.pos.y) % 18741)*.00001;

				fragmentOutput outDat;

				outDat.color = outColor;
				outDat.depth = currentDepth;

			//	* (fmod(input.tex0.x + _PointEdge,1.0)<2.0*_PointEdge ? input.color : _Color);
			//float4 kDat = ComputeScreenPos(input.pos);
			//if (fmod(input.tex0.y + _Edge,1.0)<2.0*_Edge) {
			//	outDat = _EdgeColor;
			//}

			//outDat.x *= fmod(kDat.x,10.0)/10.0;
			//outDat.y *= fmod(kDat.y,10.0)/10.0;

			return outDat;
		}
		ENDCG
	}
		}
}