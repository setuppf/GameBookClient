#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FX/Water GrabPass" {
	Properties {
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Base Texture", 2D) = "white" {}
		_HeightTex ("Bump Texture", 2D) = "bump" {}
		_FoamTex ("Foam Texture", 2D) = "white" {}
		_CubeTex ("_CubeTex", CUBE) = "white" {}
		
		_Refractivity ("_Refractivity", Range (0.1, 100.0)) = 1.0
		
		_Ambient ("_Ambient", Range (0.0, 1.0)) = 0.8
		
		_Shininess ("_Shininess", Range (0.1, 10.0)) = 20.0
		_SpecColor ("Spec Color", Color) = (0.5,0.5,0.5,0.5)
		
		_Displacement ("_Displacement", float) = 1.0
		_DisplacementTiling ("_DisplacementTiling", float) = 1.0
		
		_InvFade ("_InvFade", float) = 1.0
		_InvFadeFoam ("_InvFadeFoam", float) = 1.0
		
		_FresnelPower ("_FresnelPower", float) = 2.0
		
		_ColorTextureOverlay ("_ColorTextureOverlay", Range (0.0, 1.0)) = 0.75
		
		_WorldLightDir("_WorldLightDir", Vector) = (0,0,0,1)
		
		_Speed("_Speed", Range (0.0, 10.0)) = 0.8
		_CubeFar ("cubefar", float) = 50.0
		
	}

// Common water code that will be used in all CGPROGRAMS below
CGINCLUDE
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members worldPos)
#pragma exclude_renderers d3d11 xbox360
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct v2f members worldPos)
#pragma exclude_renderers xbox360
#include "UnityCG.cginc"

struct v2f {
	float4 vertex : POSITION;
	float4 texcoord : TEXCOORD0;
	float4 projPos : TEXCOORD2;
	float3 viewDirWorld : TEXCOORD3;
	float3 TtoW0 : TEXCOORD4;
	float3 TtoW1 : TEXCOORD5;
	float3 TtoW2 : TEXCOORD6;
	float2 texcoord2 : TEXCOORD7;
	float4 worldPos;
};

struct v2f_noProjPos {
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
	float2 texcoord1 : TEXCOORD1;
	float3 viewDirWorld : TEXCOORD2;
	float3 TtoW0 : TEXCOORD3;
	float3 TtoW1 : TEXCOORD4;
	float3 TtoW2 : TEXCOORD5;
	float2 texcoord2 : TEXCOORD6;
};

struct v2f_aniOnly {
	float4 vertex : POSITION;
};

sampler2D _MainTex;
sampler2D _HeightTex;
sampler2D _FoamTex;
samplerCUBE _CubeTex;
sampler2D _CameraDepthTexture;
sampler2D _GrabTexture;

float4 _MainTex_ST; float4 _HeightTex_ST; float4 _FoamTex_ST;
				
float4 _TintColor;
float4 _SpecColor;
				
float4 _GrabTexture_TexelSize;
			
float _Displacement;
float _DisplacementTiling;
							
float _InvFade;
float _InvFadeFoam;
				
float _FresnelPower;
				
float _Shininess;
float _Ambient;
				
float _Refractivity;
				
float _ColorTextureOverlay;
				
float4 _WorldLightDir;
				
float _Speed;

float4 _CenterPosition;
float _CubeFar;
float4x4 _WorldMatrix;
				
half3 vertexOffsetObjectSpace(appdata_full v) {
	return v.normal.xyz * sin((length(v.vertex.zy + v.color.rgb-0.5) + _Time.w * _Speed )*_DisplacementTiling) * _Displacement * 1.5 * v.color.a;				
}
				
v2f_aniOnly vert_onlyAnimation(appdata_full v)
{
	v2f_aniOnly o;
					
	v.vertex.xyz += vertexOffsetObjectSpace(v);		
	o.vertex = UnityObjectToClipPos(v.vertex);				

	return o;
}

v2f_noProjPos vert_noScreen(appdata_full v) 
{
	v2f_noProjPos o;
			
	o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
	o.texcoord1 = v.texcoord.xy;			
	o.texcoord2 = TRANSFORM_TEX(v.texcoord,_HeightTex);
										
	v.vertex.xyz += vertexOffsetObjectSpace(v);
					
	o.vertex = UnityObjectToClipPos(v.vertex);		
						
	o.viewDirWorld = -WorldSpaceViewDir(v.vertex);

					
	TANGENT_SPACE_ROTATION;
	o.TtoW0 = mul(rotation, unity_ObjectToWorld[0].xyz * 1.0);
	o.TtoW1 = mul(rotation, unity_ObjectToWorld[1].xyz * 1.0);
	o.TtoW2 = mul(rotation, unity_ObjectToWorld[2].xyz * 1.0);				
					
	return o;
}

v2f vert_full (appdata_full v)
{
	v2f o;
					
	o.texcoord.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
	o.texcoord.zw = TRANSFORM_TEX(v.texcoord1,_FoamTex);			
	o.texcoord2 = TRANSFORM_TEX(v.texcoord,_HeightTex);
					
	v.vertex.xyz += vertexOffsetObjectSpace(v);
					
	o.vertex = UnityObjectToClipPos(v.vertex);		
	o.projPos = ComputeScreenPos(o.vertex);
					
	o.viewDirWorld = -WorldSpaceViewDir(v.vertex);
	o.worldPos=mul(_WorldMatrix,v.vertex);
					
	TANGENT_SPACE_ROTATION;
	o.TtoW0 = mul(rotation, unity_ObjectToWorld[0].xyz * 1.0);
	o.TtoW1 = mul(rotation, unity_ObjectToWorld[1].xyz * 1.0);
	o.TtoW2 = mul(rotation, unity_ObjectToWorld[2].xyz * 1.0);							
	return o;
}

half4 frag_alphaMask(v2f_aniOnly i) : COLOR
{
	return half4(0,0,0, 1);
}

half4 frag_full (v2f i) : COLOR
{
	// normalize needed values
	i.viewDirWorld.xyz = normalize(i.viewDirWorld.xyz);
					
	// calc fade factor 
	float sceneZ = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r);
	float partZ = i.projPos.z;
	float zDiff = (sceneZ-partZ);
	float2 fade2 = saturate (half2(_InvFade,_InvFadeFoam) * (sceneZ-partZ)); // x is z fade, y is foam fade
					
	// calculate world normal
	half3 normal = ((UnpackNormal(tex2D(_HeightTex,i.texcoord2))));
	half3 worldNormal;
	worldNormal.x = dot(i.TtoW0, normal.xyz);
	worldNormal.y = dot(i.TtoW1, normal.xyz);
	worldNormal.z = dot(i.TtoW2, normal.xyz);	
	
	worldNormal = normalize(worldNormal);	
							
	// foam
	float4 foam = tex2D(_FoamTex, i.texcoord.zw);
	float4 color = tex2D(_MainTex, i.texcoord.xy);
	color = lerp(half4(1,1,1,1),color,_ColorTextureOverlay);					
					
	// REFRACTION
	float2 offset = normal * _GrabTexture_TexelSize.xy;
	float4 refractionUv = i.projPos;
	if(_ProjectionParams.x>0.0f)
	{
		refractionUv.y=i.projPos.z-i.projPos.y;
	}
	float2 refrOffset = offset * i.projPos.z * _Refractivity * fade2.y;	
		
	// 2 grab pass samples to correct false refraction
	float4 grabPass = tex2Dproj(_GrabTexture, refractionUv + half4(refrOffset,0,0));
	float3 refrColor  = tex2Dproj(_GrabTexture, refractionUv + half4(refrOffset,0,0) * (grabPass.a));
	//return half4(refrColor,1.0f);
	
	// REFLECTION
	float3 reflectVector = normalize(reflect(i.viewDirWorld,worldNormal));
	//计算正确的反射向量
	float3 E= i.worldPos-_CenterPosition;
    float B=dot(reflectVector,E);
    float t=sqrt(_CubeFar*_CubeFar-dot(E,E)+B*B)-B;
    float3 refVector2=reflectVector*t+E;
	
	half4 reflColor = texCUBE(_CubeTex, refVector2);
	//reflColor = lerp(0.75,reflColor, saturate(fade2.y));
	
	//reflectVector = normalize(reflectVector);
					
	// FRESNEL CALCS
	float fcbias = 0.20373;
	float facing = saturate(0.8 - max(dot(-i.viewDirWorld,worldNormal), 0.0));
	float refl2Refr = max(fcbias + (1.0-fcbias) * pow(facing, _FresnelPower), 0);				
					
	color.rgb *= (lerp(refrColor,reflColor,refl2Refr)); 
	
	color = lerp(foam, color, fade2.y);
	
	// light
	color.rgb = color.rgb * max(_Ambient, dot(normalize(-_WorldLightDir.xyz),worldNormal));
	//color.rgb += _SpecColor.rgb * pow(saturate(dot(worldNormal,normalize(-_WorldLightDir-i.viewDirWorld))), _Shininess);											
	color.a *= fade2.x;

	return  _TintColor * color;
}

half4 frag_noScreen(v2f_noProjPos i) : COLOR
{					   
	// calculate world normal
	half3 normal = UnpackNormal(tex2D(_HeightTex,i.texcoord2));
	half3 worldNormal;

	worldNormal.x = dot(i.TtoW0, normal.xyz);
	worldNormal.y = dot(i.TtoW1, normal.xyz);
	worldNormal.z = dot(i.TtoW2, normal.xyz);		
					
	// normalize
	worldNormal = normalize(worldNormal);
	i.viewDirWorld = normalize(i.viewDirWorld);
					
	// color
	float4 color = tex2D(_MainTex, i.texcoord);
	color = lerp(half4(0.6,0.6,0.6,0.6),color,_ColorTextureOverlay);		
					
	// REFLECTION
	float3 reflectVector = normalize(reflect(i.viewDirWorld,worldNormal));
	half4 reflColor = 0.75;
					
	// FRESNEL CALCS
	float fcbias = 0.20373;
	float facing = saturate(1.0 - max(dot(-i.viewDirWorld,worldNormal), 0.0));
	float refl2Refr = max(fcbias + (1.0-fcbias) * pow(facing, _FresnelPower), 0);				
					
	color.rgba *= (lerp(half4(0.6,0.6,0.6, 0.6), half4(reflColor.rgb,1.0), refl2Refr)); 
					 
	// light
	color.rgb = color.rgb * max(_Ambient, saturate(dot(_WorldLightDir.xyz,worldNormal)));
	// a little more spec in low quality to have at least something going on 
	color.rgb += _SpecColor.rgb * 2.0 *  pow(saturate(dot(_WorldLightDir.xyz, reflectVector)),_Shininess); 
												
	return _TintColor * color;
}
ENDCG
	
	Category 
	{
		Tags { "Queue"="Transparent-100" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ColorMask RGB
		Lighting Off ZWrite Off
	
		// Everything; shader model 3.0
		SubShader 
		{
			Lod 500
			
			// flag pass
			
			Pass 
			{
				ColorMask A
				Blend One Zero

				CGPROGRAM

				#pragma fragmentoption ARB_precision_hint_fastest
				
				#pragma vertex vert_onlyAnimation
				#pragma fragment frag_alphaMask
				
				ENDCG
			}
			
			// grab pass
			
			GrabPass 
			{ }

			// color/render pass
			
			Pass 
			{
				ColorMask RGB
				Blend SrcAlpha OneMinusSrcAlpha

				CGPROGRAM

				#pragma target 3.0 
				#pragma fragmentoption ARB_precision_hint_fastest
								
				#pragma vertex vert_full
				#pragma fragment frag_full

				ENDCG 
			} // pass
		} // subshader

		// No GrabPass based refraction; shader model 2.0
		SubShader
		{			
			Lod 300
			
			Pass 
			{
				CGPROGRAM

				#pragma fragmentoption ARB_precision_hint_fastest
	
				#pragma vertex vert_noScreen
				#pragma fragment frag_noScreen

				ENDCG 
			} // pass
		} // subshader
		
		// No shaders; just a simple scrolling texture
		SubShader
		{			
			Lod 200
			
			Pass 
			{
				SetTexture [_MainTex] { constantColor (0.6, 0.6, 0.6, 0.3) combine texture, constant }
			} // pass
		} // subshader


	} // category
	
} // shader
