// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'PositionFog()' with multiply of UNITY_MATRIX_MVP by position
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'

Shader "Nature/Transparent/MyTree" { 
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,0)
		_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		_WindSpeed ("WaveSpeed", range (0, 10)) =2			
		_WaveScale ("WaveScale", range (0, 40)) = 15
		_Scale ("Scale", Vector) = (1,1,1,1)
}
	SubShader {
		Tags {"IgnoreProjector"="True" "RenderType"="TreeTransparentCutout"}
		Cull Off
		Alphatest Greater [_Cutoff]
		Pass {
CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma vertex Vertex
#pragma fragment frag
#include "UnityCG.cginc" // Standard Unity properties
#include "TerrainEngine.cginc" // Helper wave functions

struct v2f {
   float4 pos : POSITION;
   float4 color : COLOR0;
   float2 uv : TEXCOORD0;
};

uniform float4  _Color;	
uniform int _WindSpeed, _WaveScale;
uniform sampler2D _MainTex;
uniform float _Cutoff;


v2f Vertex(appdata_tree v)
{
   v2f o;
	
   const float4 _waveXSize = float4(0.012, 0.02, -0.06, 0.048) * 2;
   const float4 _waveZSize = float4 (0.006, .02, -0.02, 0.1) * 2;
   const float4 waveSpeed = float4 (0.3, .3, .08, .07) * 4;

   float4 _waveXmove = _waveXSize * waveSpeed * _WaveScale;
   float4 _waveZmove = _waveZSize * waveSpeed * _WaveScale;

   // We model the wind as basic waves...

   // Calculate the wind input to leaves from their vertex positions...
   // for now, we transform them into world-space x/z positions...
   // Later on, we should actually be able to do the whole calc's in post-projective space
   float3 worldPos = mul ((float3x4)unity_ObjectToWorld, v.vertex);
    
   // This is the input to the sinusiodal warp
   float4 waves;
   waves = worldPos.x * _waveXSize;
   waves += worldPos.z * _waveZSize;

   // Add in time to model them over time
   waves += _Time.x * waveSpeed *_WindSpeed;

   float4 s, c;
   waves = frac (waves);
   FastSinCos (waves, s,c);

   float waveAmount = v.texcoord.y;
   s *= waveAmount;

   // Faster winds move the grass more than slow winds 
   s *= normalize (waveSpeed);

   s = s * s;
   float fade = dot (s, 1.3);
   s = s * s;
   float3 waveMove = float3 (0,0,0);
   waveMove.x = dot (s, _waveXmove);
   waveMove.z = dot (s, _waveZmove);

   v.vertex.xz -= mul ((float3x3)unity_WorldToObject, waveMove).xz;
   o.pos = UnityObjectToClipPos (v.vertex);
	
   o.uv = v.texcoord;  
   o.color=v.color;
   return o;
} 

float4 frag (v2f i) : COLOR
{		
	float4 texcol = tex2D( _MainTex, i.uv);
	return half4(i.color.rgb*texcol.rgb*_Color.rgb*2.0f,texcol.a);
}
ENDCG
		}
		
		
		
	}

}

	