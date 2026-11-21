#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/BSDF.hlsl"
// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"

// void SampleDirectionalLightmap(TEXTURE2D_LIGHTMAP_PARAM(lightmapTex, lightmapSampler), TEXTURE2D_LIGHTMAP_PARAM(lightmapDirTex, lightmapDirSampler), LIGHTMAP_EXTRA_ARGS, float4 transform, float3 normalWS, float3 backNormalWS, bool isStaticLightmap, inout real3 bakeDiffuseLighting, inout real3 backBakeDiffuseLighting)

void LightmapSample_float(in float2 staticLightmapUV, in float3 normalWS, in float3 viewDirWS, in float roughness, in UnityTexture2D lightmap, in UnityTexture2D dirLightmap, out float3 color)
{
    float4 transformCoords = float4(1, 1, 0, 0);

    float3 backNormalWSUnused = 0.0;
    real3 bakeDiffuseLighting = 0.0;
    real3 backBakeDiffuseLightingUnused = 0.0;
    SampleDirectionalLightmap(TEXTURE2D_LIGHTMAP_ARGS(lightmap.tex, lightmap.samplerstate), TEXTURE2D_LIGHTMAP_ARGS(dirLightmap.tex, dirLightmap.samplerstate), staticLightmapUV, transformCoords, normalWS, backNormalWSUnused, true, bakeDiffuseLighting, backBakeDiffuseLightingUnused);
    real4 direction = SAMPLE_TEXTURE2D(dirLightmap.tex, dirLightmap.samplerstate, staticLightmapUV);
    // float3 reflectDir = reflect(-(direction.xyz - 0.5), normalWS);
    float3 lightDir = (direction.xyz - 0.5);
    float NdotH = max(dot(normalize(normalWS), normalize(normalize(viewDirWS) + normalize(lightDir))), 0.0);
    // color = D_GGX(NdotH, roughness);
    // color = pow(NdotH, 256);
    color = bakeDiffuseLighting;
}