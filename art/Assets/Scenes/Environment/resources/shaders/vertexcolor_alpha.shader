// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'

Shader "XY/Transparent/vertexcolor_alpha" 
{
	Properties 
	{
	
		_MainTex ("Base (RGB)", 2D) = "white" {}
		color1 ("Main Color", Color) = (1,1,1,1) 
	}
	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		pass
		{ 
		Cull Off
		ZWrite Off 
		Blend SrcAlpha OneMinusSrcAlpha  
		CGPROGRAM
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct v2f members uv,cor)
#pragma exclude_renderers xbox360
#pragma fragment frag
#pragma vertex vert
#include "UnityCG.cginc"
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float4 color1;
		struct appdate
		{	
		float4 vertex:POSITION;
		float2 texcoord:TEXCOORD0;
		float4 color:COLOR0;
		};
		struct v2f 
		{		
			float4 pos:POSITION;
			float2 uv:TEXCOORD0;
			float4 cor:TEXCOORD1;
			
		};
		v2f vert (appdate v)
		{
			v2f o;
			o.pos=UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
			o.cor=v.color;			
			return o;
		}
		float4 frag (v2f i) : COLOR
		{
			half4 color=tex2D(_MainTex,i.uv);
			return color*i.cor*color1;


		}
ENDCG

		}
		
	} 
	FallBack "Diffuse"
}