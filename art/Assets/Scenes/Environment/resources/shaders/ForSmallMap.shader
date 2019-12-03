
Shader "Texture_SmallMap"
{
	Properties
    {
       _Color ("Main Color", Color) = (1,1,1,1)
       _MainTex ("Base (RGB)", 2D) = "white" {}
       _AlphaTex ("Base (RGB)", 2D) = "white" {}
	}
	
    SubShader
    {
		Blend SrcAlpha OneMinusSrcAlpha
       
		Pass
		{
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			BindChannels
			{
				Bind "Color", color
				Bind "Vertex", vertex
				Bind "texcoord", texcoord
			} 
			
			Lighting Off
			
			SetTexture [_AlphaTex]
			{
                combine texture, texture
            }

			SetTexture [_MainTex]
			{
				combine texture * primary, previous
            }            
		}
	}
}