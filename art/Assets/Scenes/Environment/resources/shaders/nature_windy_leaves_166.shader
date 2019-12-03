// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'glstate.lightmodel.ambient' with 'UNITY_LIGHTMODEL_AMBIENT'
// Upgrade NOTE: replaced 'glstate.matrix.modelview[0]' with 'UNITY_MATRIX_MV'
// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'

Shader "Nature/Transparent/Soft Occlusion Windy Leaves" { 
	Properties { 
		_Color ("Main Color", Color) = (.5, .5, .5, 0)
		_Color2 ("Fade Color", Color) = (1, .9, .8, 0)
		_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
		_Cutoff ("Base Alpha cutoff", Range (0,1)) = 0.5
		_BaseLight ("BaseLight", range (0, 1)) = 0.35
		_AO ("Amb. Occlusion", range (0, 10)) = 2.4
		_Occlusion ("Dir Occlusion", range (0, 20)) = 7.5
		_WindSpeed ("WaveSpeed", range (1, 10)) =2			
		_WaveScale ("WaveScale", range (1, 40)) = 15
		_Scale ("Scale", Vector) = (1,1,1,1)
	}

	// ---- no vertex programs
	SubShader {		
			Tags {"Queue" = "Transparent-99" "RenderType" = "TreeTransparentCutout"}
			Pass {			
			Colormask RGB
			AlphaTest Greater [_Cutoff]
			Cull Off
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11 xbox360
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
// Upgrade NOTE: excluded shader from Xbox360 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers xbox360
#pragma exclude_renderers flash
#pragma vertex Vertex
#include "UnityCG.cginc" // Standard Unity properties
#include "TerrainEngine.cginc" // Helper wave functions

struct v2f {
   float4 pos : POSITION;
   float4 color : COLOR0;
   float4 uv : TEXCOORD0;
   float fog : FOGC;
};

uniform float _Occlusion, _AO, _BaseLight;
uniform float3[4] _TerrainTreeLightDirections;

uniform float4  _Color,_Color2;	
int _WindSpeed, _WaveScale;

// uniform float4x4 _CameraToWorld;

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

// Soft Occlusion	   
 //~ TerrainAnimateTree(v.vertex, v.color.w); // conflicted with above v.vertex wave function, not sure how to make it to work.
 
	float3 viewpos = mul(UNITY_MATRIX_MV, v.vertex).xyz;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.fog = o.pos.w;
	o.fog = o.pos.z;
	o.uv = v.texcoord;  
   
	
	o.color = lerp (_Color, _Color2, fade.xxxx) ;
	#ifdef WRITE_ALPHA_1
	o.color.a = 1;
	#endif

   return o;
} 
ENDCG 
			SetTexture [_MainTex] { combine texture * primary double, texture }	
		}
		
	// Pass to render object as a shadow caster
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			
			Fog {Mode Off}
			ZWrite On ZTest Less Cull Off
	
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members uv)
#pragma exclude_renderers d3d11 xbox360
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct v2f members uv)
#pragma exclude_renderers xbox360
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile SHADOWS_NATIVE SHADOWS_CUBE
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			#include "TerrainEngine.cginc"
			
			struct v2f { 
				V2F_SHADOW_CASTER;
				float2  uv;
			};
			
			struct appdata {
			    float4 vertex : POSITION;
			    float4 color : COLOR;
			    float4 texcoord : TEXCOORD0;
			};
			v2f vert( appdata v )
			{
				v2f o;
				TerrainAnimateTree(v.vertex, v.color.w);
				TRANSFER_SHADOW_CASTER(o)
				o.uv = v.texcoord;
				return o;
			}
			
			uniform sampler2D _MainTex;
			uniform float _Cutoff;
					
			float4 frag( v2f i ) : COLOR
			{
				half4 texcol = tex2D( _MainTex, i.uv );
				clip( texcol.a - _Cutoff );
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG	
		}
	}
 
	//~ // ---- no vertex programs 	  
	SubShader {
		Tags { "Queue" = "Transparent-99"
			"IgnoreProjector"="True"
			"BillboardShader" = "Hidden/TerrainEngine/Soft Occlusion Leaves rendertex"
			"RenderType" = "TreeTransparentCutout"}
		Pass {
			Colormask RGB
			AlphaTest Greater [_Cutoff]
			Cull Off
			Color [_Color]
			SetTexture [_MainTex] { combine texture * primary double, texture }		
		}
	}
}