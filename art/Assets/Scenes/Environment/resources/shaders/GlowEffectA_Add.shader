// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'

Shader "XY/Transparent/GlowEffectA_Add" 
{
	Properties 
	{
	
		_MainTex ("Base (RGB)", 2D) = "white" {}
		color1 ("Main Color", Color) = (1,1,1,1) 
		_Add("add",float)=1.0
		_Filter("filter",float)=1.0	
	}
	SubShader 
	{
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		pass
		{ 
		Cull Off
		ZWrite Off
		Blend SrcAlpha One
		Fog {Mode Off}
		CGPROGRAM
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct v2f members uv,cor)
#pragma exclude_renderers xbox360
#pragma fragment frag
#pragma vertex vert
#include "UnityCG.cginc"
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Add;
		uniform float _Filter;
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
			half3 c1=color.rgb*_Add;
			half3 c2=(color-half3(0.5,0.5,0.5))*_Filter;		
			half3 c3=c1+c2;
		    return half4(c3.r*i.cor.r*color1.r,c3.g*i.cor.g*color1.g,c3.b*i.cor.b*color1.b,color.a*i.cor.a*color1.a);


		}
ENDCG
		

		}
		
	} 
	FallBack "Diffuse"
}