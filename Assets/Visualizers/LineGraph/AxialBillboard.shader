// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/GS Billboard" 
{
    Properties 
	{
        _SpriteTex ("Base (RGB)", 2D) = "white" {
            }
		_Size ("Size", Range(0, 3)) = 0.5
		_Edge ("Edge Width", Range(0, .5)) = 0.1
		_Color ("Main Color", Color) = (1,1,1,1) 
		_EdgeColor ("Edge Color", Color) = (0,0,0,1) 
		_PointEdge ("Width of Point Highlight", Range(0,.5)) = .1
		_MaxMiter ("Maximum Miter Width", Range(1.0,10.0)) = 4.0
		_Pulse ("Pulse Line?", Range(0, 1)) = 0.0
		_PulseWidth ("Pulse Maximum Width", Range(0,3))=0.5
		_ZDepthOverride ("Output Z Override", Range(-100,100))=0
		_Wipe("Wipe Line along X?", Range(0, 1)) = 0.0
		_StartTime("Start time for Wipe", Range(-100.0,100.0))=0.0
		_WipeDuration("Wipe Duration", Range(0,100.0))=10.0
	}

	SubShader 
	{
        Pass
		{
			//Offset 0, -100

            //Cull Off
			//ZTest Always
			Tags {
                "RenderType"="Opaque" }
			LOD 200
		
			CGPROGRAM

			#pragma target 3.0
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
				float4	lastPoint : TANGENT;
				float4	nextPoint : NORMAL;
				float2  tex0	: TEXCOORD0;
				float4	color	: COLOR;
			}
;
			struct FS_INPUT
			{
				float4	pos		: POSITION;
				float2  tex0	: TEXCOORD0;
				float4	color	: COLOR;
				float4	worldPos: TANGENT;
			}
;
			// **************************************************************
			// Vars															*
			// **************************************************************

			float _Size;
			float _Edge;
			float4x4 _VP;
			Texture2D _SpriteTex;
			float4 _Color;
			float4 _EdgeColor;
			float _PointEdge;
			float _MaxMiter;
			bool _Pulse;
			float _PulseWidth;
			float _ZDepthOverride;
			bool _Wipe;
			float _StartTime;
			float _WipeDuration;

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
				output.nextPoint = mul(unity_ObjectToWorld, float4(v.vertex + v.normal, 1));
				//output.lastPoint = length(v.tangent.xyz)>.001?v.vertex + v.tangent:v.vertex+float4(0,1,0,1);
				output.lastPoint = mul(unity_ObjectToWorld, v.vertex + v.tangent);

				output.tex0 = float2(0, 0);
				output.color = v.color;

				//if (length(v.normal)<.0001){
				//	output.pos += float4(0,.001,0,0);
				//	output.color = float4(1,0,1,1);
				//}

				return output;
			}


			// Geometry Shader -----------------------------------------------------
			[maxvertexcount(10)]
			void GS_Main(line GS_INPUT p[2], inout TriangleStream<FS_INPUT> triStream)
			{
				float3 up = float3(0, 1, 0);
				float4 lastPoint = p[0].lastPoint;
				float4 nextPoint = p[1].nextPoint;

				float3 lastLook = _WorldSpaceCameraPos - lastPoint;
				float3 look = _WorldSpaceCameraPos - p[0].pos;
				float3 look2 = _WorldSpaceCameraPos - p[1].pos;

				lastLook = normalize(lastLook);
				look = normalize(look);
				look2 = normalize(look2);

				float3 lastDiff = p[0].pos-lastPoint;
				float3 lineDiff = p[1].pos-p[0].pos;
				float3 nextDiff = nextPoint-p[1].pos;

				float3 lastNormal = normalize(cross(lastLook,(lastDiff)));
				float3 lineNormal = normalize(cross(look, (lineDiff)));
				float3 nextNormal = normalize(cross(look2, (nextDiff)));

				// line pulse formulas:
				//  use localSize = _Size; for no pulse
				//float localSize = _Size * (4*_SinTime.w*_SinTime.w*_CosTime.w*_CosTime.w+.1);
				float localSize = _Size + _Pulse * (_PulseWidth - _Size) * (4 * _SinTime.w*_SinTime.w*_CosTime.w*_CosTime.w + .1); //forms a 0-1 pulse at high frequency

				float3 lastMiter = normalize(lastNormal + lineNormal);
				float3 nextMiter = normalize(lineNormal + nextNormal);

				float lastLength = localSize / dot(lastMiter, lineNormal);
				float nextLength = localSize / dot(nextMiter, lineNormal);


				// matrix for world-space;
				float4x4 vp = mul(UNITY_MATRIX_MVP, unity_WorldToObject);
				FS_INPUT pIn;
				float4 worldPos;
				// compute the facing of the next section of the line so we can draw the corners
				
				float4 multNormal = float4(lineNormal * localSize,0);
				float4 nextMultNormal = float4(nextNormal * localSize,0);

				bool capLast = abs(lastLength)>localSize*_MaxMiter;
				bool capNext = abs(nextLength)>localSize*_MaxMiter;
										
				float4 lastMiter4 = float4(lastMiter * lastLength,0);
				float4 nextMiter4 = float4(nextMiter * nextLength,0);

				// draw normal quad starts

				worldPos = p[0].pos + (capLast ? multNormal : lastMiter4);
				pIn.pos = mul(vp, worldPos);
				pIn.tex0 = float2(0.0f, 0.0f);
				pIn.color = p[0].color;
				pIn.worldPos = worldPos;
				triStream.Append(pIn);
				
				worldPos = p[0].pos - (capLast ? multNormal : lastMiter4);
				pIn.pos = mul(vp, worldPos);
				pIn.tex0 = float2(0.0f, 1.0f);
				pIn.color = p[0].color;
				pIn.worldPos = worldPos;
				triStream.Append(pIn);

				// draw normal quad ends, then corners	
				
				worldPos = p[1].pos + (capNext ? multNormal : nextMiter4);
				pIn.pos = mul(vp, worldPos);
				pIn.tex0 = float2(1.0f, 0.0f);
				pIn.color = p[1].color;
				pIn.worldPos = worldPos;
				triStream.Append(pIn); 

				worldPos = p[1].pos - (capNext ? multNormal : nextMiter4);
				pIn.pos = mul(vp, worldPos);
				pIn.tex0 = float2(1.0f, 1.0f);
				pIn.color = p[1].color;
				pIn.worldPos = worldPos;
				triStream.Append(pIn);

				triStream.RestartStrip();
						

				if (!capNext)
					return;

				//////////// draw corner tris ///////////////
				worldPos = p[1].pos;
				pIn.pos = mul(vp, worldPos);
				pIn.tex0 = float2(1,.5);
				pIn.color = p[1].color;
				pIn.worldPos = worldPos;
				triStream.Append(pIn);
				
				worldPos = p[1].pos + nextMultNormal;
				pIn.pos = mul(vp, worldPos);
				pIn.tex0 = float2(1,0);
				pIn.color = p[1].color;
				pIn.worldPos = worldPos;
				triStream.Append(pIn);

				worldPos = p[1].pos + multNormal;
				pIn.pos = mul(vp, worldPos);
				pIn.tex0 = float2(1,0);
				pIn.color = p[1].color;
				pIn.worldPos = worldPos;
				triStream.Append(pIn);


				triStream.RestartStrip();
				
					
				worldPos = p[1].pos;
				pIn.pos = mul(vp, worldPos);
				pIn.tex0 = float2(0,.5);
				pIn.color = p[1].color;
				pIn.worldPos = worldPos;
				triStream.Append(pIn);
				
				worldPos = p[1].pos - multNormal;
				pIn.pos = mul(vp, worldPos);
				pIn.tex0 = float2(0,0);
				pIn.color = p[1].color;
				pIn.worldPos = worldPos;
				triStream.Append(pIn);
				
				worldPos = p[1].pos - nextMultNormal;
				pIn.pos = mul(vp, worldPos);
				pIn.tex0 = float2(0,0);
				pIn.color = p[1].color;
				pIn.worldPos = worldPos;
				triStream.Append(pIn);
				
				triStream.RestartStrip();

			}


			struct fragmentOutput {
				float4 color : COLOR;
				float depth : DEPTH;
			};


			// Fragment Shader -----------------------------------------------
			fragmentOutput FS_Main(FS_INPUT input)
			{
				float4 outColor = _SpriteTex.Sample(sampler_SpriteTex, input.tex0)
						* (fmod(input.tex0.x+_PointEdge,1.0)<2.0*_PointEdge?input.color: _Color);
				float4 kDat = ComputeScreenPos(input.pos);
				if (fmod(input.tex0.y+_Edge,1.0)<2.0*_Edge){
					outColor = _EdgeColor;

				}

				float currentDepth = input.pos.z + _ZDepthOverride;

				if (_Wipe) {
					if ((input.worldPos.x - 25 * (((_Time.y-_StartTime)/_WipeDuration ))) > 0)
						//if ((_Wipe*(_Time % 1) - input.worldPos.x>0))
						currentDepth += 2;
				}

				fragmentOutput outDat;
				outDat.depth = currentDepth;

				// x offset depth render
				//outDat.depth = input.pos.x/10+_ZDepthOverride;
				outDat.color = outColor;

				return outDat;
			}


			ENDCG
		}
	} 
}
