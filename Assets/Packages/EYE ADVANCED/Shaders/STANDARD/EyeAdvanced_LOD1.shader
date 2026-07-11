Shader "EyeAdvanced/EyeAdvanced_LOD1" {
Properties {

	_scleraColor ("Sclera Color", Color) = (0.95,0.95,0.95,1)
	_irisColor ("Iris Color", Color) = (1,1,1,1)
	_illumColor ("Glow Color", Color) = (0,0,0,0)

	_pupilSize("Pupil Dilation", Range(0.0,1.0)) = 0.27
	_irisSize("Eye Iris Size", Range(1.5,5.0)) = 1.88
	_parallax("Parallax Effect", Range(0.0,0.05)) = 0.05
	_scleraSize("Eye Sclera Size", Range(0.85,2.2)) = 0.968
	_limbus("Limbal Ring Amount", Range(0.0,1.0)) = 0.5
	_specsize("Specular", Range(0.0,1.0)) = 0.9
	_smoothness("Smoothness", Range(0.0,1.0)) = 0.75
	_reflectTerm("Reflection Term", Range(0.0,1.0)) = 0.025
	_brightShift("Overall Brightness", float) = 1.0

	_scleraShadowAmt("Shadow Sclera", Range(0.0,1.0)) = 0.0
	_irisShadowAmt("Shadow Iris", Range(0.0,1.0)) = 0.0
	//_reflectMaskAmt("Reflection Mask Amount", Range(0.0,1.0)) = 1.0
	
	_IrisColorTex ("Iris Color", 2D) = "white" {}
	_IrisTex ("Iris Mask", 2D) = "white" {}
	_CorneaBump ("Cornea Normal Map", 2D) = "bump" {}
	_EyeBump ("Eye Normal Map", 2D) = "bump" {}
	_IrisBump ("Iris Normal Map", 2D) = "bump" {}
	_MainTex ("Sclera Texture", 2D) = "white" {}
	_ShadeScleraTex ("Sclera Shade Texture", 2D) = "white" {}
	_ShadeIrisTex ("Shade Iris Texture", 2D) = "white" {}

}

SubShader { 
	Tags {"RenderType"="Opaque" "Queue"= "Geometry"}
	Cull Back
Lighting On
	
CGPROGRAM
#pragma target 3.0
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"
#pragma surface surf StandardSpecular vertex:vert addshadow
#pragma glsl

sampler2D _IrisColorTex;
sampler2D _IrisTex;
sampler2D _MainTex;
sampler2D _ShadeScleraTex;
sampler2D _ShadeIrisTex;
float _scleraShadowAmt;
float _irisShadowAmt;
//float _reflectMaskAmt;
float4 _albedoColor;
float4 reflectionMatte;
float4 irradianceTex;
float3 albedoColor;
float _roughness;
float _reflective;
float _metalMap;
float _ambientMap;
float _irisSize;
float _scleraSize;
float _pupilSize;
float _limbus;
sampler2D _CorneaBump;
sampler2D _EyeBump;
sampler2D _IrisBump;
float4 _scleraColor;
float4 _irisColor;
float4 _irisColorB;
float4 _pupilColor;
float4 _illumColor;
float _parallax;
float _brightShift;
float irismasktex;
float irisoffsettex;
float _smoothness;
float _specsize;
float _reflectTerm;


struct Input {
	float2 uv_MainTex;
	float3 viewDir;
	float3 worldRefl;
    float3 lightDir;
    INTERNAL_DATA
};



void vert (inout appdata_full v, out Input o) {
	UNITY_INITIALIZE_OUTPUT(Input, o);
    o.lightDir = WorldSpaceLightDir(v.vertex); 
}



void surf (Input IN, inout SurfaceOutputStandardSpecular o) {

	//CALCULATE NORMAL MAPS
	half3 cBump = UnpackNormal(tex2D(_CorneaBump, float2((IN.uv_MainTex.x*_irisSize)-((_irisSize-1.0)/2.0),(IN.uv_MainTex.y*_irisSize)-((_irisSize-1.0)/2.0))));

	half iSize2 = _irisSize*0.5;
	half3 iBump = UnpackNormal(tex2D(_IrisBump, float2((IN.uv_MainTex.x*iSize2)-((iSize2-1.0)/2.0),(IN.uv_MainTex.y*iSize2)-((iSize2-1.0)/2.0))));

	//CALCULATE ALBEDO MAP (SCLERA)
	half4 scleratex = tex2D(_MainTex, float2((IN.uv_MainTex.x*_scleraSize)-((_scleraSize-1.0)/2.0),(IN.uv_MainTex.y*_scleraSize)-((_scleraSize-1.0)/2.0)));
	scleratex.rgb = lerp(scleratex.rgb,scleratex.rgb*_scleraColor.rgb,_scleraColor.a);
	half3 eBump = UnpackNormal(tex2D(_EyeBump, float2((IN.uv_MainTex.x*_scleraSize)-((_scleraSize-1.0)/2.0),(IN.uv_MainTex.y*_scleraSize)-((_scleraSize-1.0)/2.0))));

	//SCLERA SHADING
	//half scleraShade = tex2D(_ShadeScleraTex, float2((IN.uv_MainTex.x*_scleraSize)-((_scleraSize-1.0)/2.0),(IN.uv_MainTex.y*_scleraSize)-((_scleraSize-1.0)/2.0))).r;
	//half scleraShade = tex2D(_ShadeScleraTex, float2(IN.uv_MainTex.x, IN.uv_MainTex.y)).r;
	half scleraShade = tex2D(_ShadeScleraTex, float2((IN.uv_MainTex.x*0.9)-((0.9-1.0)/2.0),(IN.uv_MainTex.y*0.9)-((0.9-1.0)/2.0))).r;
	scleratex.rgb *= 1 - (scleraShade * _scleraShadowAmt);

	//CALCULATE ALBEDO MAP (IRIS)
	irismasktex = tex2D(_MainTex, float2((IN.uv_MainTex.x*_irisSize)-((_irisSize-1.0)/2.0),(IN.uv_MainTex.y*_irisSize)-((_irisSize-1.0)/2.0))).a;

	//FINAL NORMAL COMBINATION
	o.Normal = lerp(eBump,cBump,irismasktex);

	// get mask texture
	half uvMask = 1.0-tex2D(_IrisTex,IN.uv_MainTex).b;

	//CALCULATE IRIS TEXTURE
	half iSize = _irisSize * 0.6;
	float2 irUVc = IN.uv_MainTex;
	irUVc = float2((IN.uv_MainTex.x*iSize)-((iSize-1.0)/2.0),((IN.uv_MainTex.y)*iSize)-((iSize-1.0)/2.0));
	_pupilSize = lerp(lerp(0.5,0.2,iSize/5),lerp(1.2,0.75,iSize/5),_pupilSize);
	irUVc = (irUVc*((-1.0+(uvMask*_pupilSize)))-(0.5*(uvMask*_pupilSize)));

	//CALCULATE IRIS/PUPIL MASK TEXTURES
	float2 irUV;
	irUV.x = lerp((IN.uv_MainTex.x*0.75)-((0.75-1.0)/2.0),(IN.uv_MainTex.x*_pupilSize)-((_pupilSize-1.0)/2.0),IN.uv_MainTex.x);
	irUV.y = lerp((IN.uv_MainTex.y*0.75)-((0.75-1.0)/2.0),(IN.uv_MainTex.y*_pupilSize)-((_pupilSize-1.0)/2.0),IN.uv_MainTex.y);

	//get iris and pupil texture
	half4 irisColTex = tex2D(_IrisColorTex,irUVc);

	//IRIS SHADING
	half irisShade = tex2D(_ShadeIrisTex, irUVc).r;
	irisColTex.rgb *= 1 - (irisShade * _irisShadowAmt);

	//combine sclera and iris colors
	irisColTex.rgb = lerp(irisColTex.rgb,irisColTex.rgb*_irisColor.rgb,_irisColor.a);
	o.Albedo = lerp(scleratex.rgb,irisColTex.rgb,irismasktex);

	//backscatter effects
	o.Emission = o.Albedo*(2.0*_illumColor.a)*_illumColor.rgb * irismasktex * (1-irisColTex.a);
	

	//----------------------------
	//##  CALCULATE SUBDERMAL  ##
	//----------------------------
	half lightFac = max(0,dot(IN.lightDir,o.Normal));
	half h3 = max(0,dot(iBump,IN.lightDir));
	half edge2 = saturate(lerp(0.5,-0.5,max(0,dot(o.Normal,IN.viewDir))));


	//------------------------------
	//##  CALCULATE ALPHA / CLIP  ##
	//------------------------------
	o.Alpha = 1.0;
	

	//-------------------------------
	//##  LIGHT TERM CALCULATION  ##
	//-------------------------------
	half3 useNormal = cBump;
    half NdotV2 = max(0,dot(cBump,IN.viewDir));
    half cNdotV2 = saturate(lerp(-0.2,0.25,max(0,dot(useNormal,IN.viewDir))));
    half h = max(0,dot(useNormal,normalize(normalize(IN.lightDir)+IN.viewDir)));


	//---------------------------
	//##  INDEX OF REFRACTION  ##
	//---------------------------
	half3 f0 = half3(0,0,0);

	//------------------------------------
	//##  FRESNEL CALULATION (Schlick)  ##
	//------------------------------------
	half3 fresnel;
	fresnel = f0+(1.0-f0)*pow((dot(cBump,normalize(IN.lightDir+IN.viewDir))),5);
	fresnel = fresnel * (f0+(1.0-f0)*pow((1.0-NdotV2),5));
	fresnel = saturate(max(fresnel,f0+(1.0-f0)*pow((1.0-NdotV2),5)));

	//add edge specular
	o.Albedo = (o.Albedo + (fresnel * 0.7 * h));

	//Limbus Darkening
	o.Albedo = lerp(o.Albedo, o.Albedo * (0 - (_limbus * 20.0)), saturate(irisoffsettex * (2.0-irismasktex) * (irismasktex) * (cNdotV2*1)));

	//-------------------------
	//##  UNITY 5 Features  ##
	//-------------------------
	float useReflTerm = _reflectTerm;// * lerp(1.0, saturate(lerp(4.0,-0.5,scleraShade)), _reflectMaskAmt );
	//o.Specular = lerp(0.05, saturate(lerp(useReflTerm*1.1,useReflTerm*1.2,irismasktex)), irismasktex);
	//o.Specular = lerp(o.Specular * 0.025, o.Specular * 16, irismasktex);
	//o.Specular = useReflTerm;//saturate(lerp(useReflTerm*1.1,useReflTerm*1.2,irismasktex));
	o.Specular = lerp(useReflTerm * 8, useReflTerm * 8, irismasktex);
	
	//o.Smoothness = lerp(0.8, 0.6, saturate(lerp(-2,5,irismasktex)) );
	o.Smoothness = lerp(_smoothness, _specsize, saturate(lerp(-2,5,irismasktex)) );


	//final brightness shift
	o.Albedo = lerp(o.Albedo, o.Albedo * 2, irismasktex * 2);
	o.Albedo = o.Albedo * _brightShift;
}

ENDCG



}

Fallback Off

}
