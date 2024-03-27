#ifndef GRASS_PBR_INPUT_INCLUDE
#define GRASS_PBR_INPUT_INCLUDE
#include "TADemoLitInput.hlsl"

TEXTURE2D(_BottomBlendMap);     SAMPLER(sampler_BottomBlendMap);
TEXTURE2D(_ThicknessMap);       SAMPLER(sampler_ThicknessMap);

CBUFFER_START(UnityPerMaterial)
BASE_CBUFFER_PROPERTIES
float4 _BottomBlendMap_ST;
half4 _BottomColor;
float4 _ThicknessMap_ST;
half _DiffuseDistortion;
half _Power;
half _Scale;
//half _Attention;
half3 _Ambient;
CBUFFER_END

float4 _ActorPosition;
float _Range;

void GrassPBRInitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    float albedoColorBlend = SAMPLE_TEXTURE2D(_BottomBlendMap, sampler_BottomBlendMap, uv).r;
    float4 albedoColor = saturate(lerp(_BottomColor, _BaseColor, albedoColorBlend));
    
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff) * albedoColor.a;

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * albedoColor.rgb;

    #if _SPECULAR_SETUP
    outSurfaceData.metallic = half(1.0);
    outSurfaceData.specular = specGloss.rgb;
    #else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
    #endif

    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = SampleOcclusion(uv);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoat = SampleClearCoat(uv);
    outSurfaceData.clearCoatMask       = clearCoat.r;
    outSurfaceData.clearCoatSmoothness = clearCoat.g;
    #else
    outSurfaceData.clearCoatMask       = half(0.0);
    outSurfaceData.clearCoatSmoothness = half(0.0);
    #endif

    #if defined(_DETAIL)
    half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
    float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
    outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);
    #endif
}
#endif