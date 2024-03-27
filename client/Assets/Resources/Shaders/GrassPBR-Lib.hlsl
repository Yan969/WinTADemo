#ifndef GRASS_PBR_LIB_INCLUDE
#define GRASS_PBR_LIB_INCLUDE
#include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
#include "Assets/Resources/Shaders/WindSystem.hlsl"

Varyings GrassPBRPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 dis = distance(_ActorPosition, positionWS);
    float3 circle = 1 - saturate(dis/_Range);
    float3 sphereCircle = positionWS - _ActorPosition;
    sphereCircle *= circle;
    input.positionOS.xz += sphereCircle.xz;
    
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);

    half fogFactor = 0;
    #if !defined(_FOG_FRAGMENT)
        fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    #endif

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    output.tangentWS = tangentWS;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    output.viewDirTS = viewDirTS;
#endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
#else
    output.fogFactor = fogFactor;
#endif

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    float3 windData;
    GetWindSimulationFunc(input.positionOS, output.normalWS, output.positionWS, windData);
    
    output.positionCS = TransformWorldToHClip(output.positionWS);// vertexInput.positionCS;

    return output;
}
// 针对植被的快速渲染
// Translucency(Translucidity):https://www.alanzucconi.com/2017/08/30/fast-subsurface-scattering-2/
half3 FastTranslucency(float3 gi_color, float3 viewDir, float3 lightDir, float3 normal, float distortion, float power, float scale, float thickness, float atten, float3 ambient)
{
    float3 H = normalize(lightDir + normal * distortion);
    float VdotH = pow(saturate(dot(viewDir, -H)), power) * scale;
    float3 back_intensity = atten * (VdotH + ambient) * thickness;
    float3 translucency_color = gi_color * back_intensity;
    return translucency_color;
}

half4 GrassUniversalFragmentPBR(InputData inputData, SurfaceData surfaceData, float thickness)
{
    #if defined(_SPECULARHIGHLIGHTS_OFF)
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);

    #if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
    {
        return debugColor;
    }
    #endif

    // Clear-coat calculation...
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLightLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    lightingData.giColor = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                              inputData.bakedGI, aoFactor.indirectAmbientOcclusion, inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS);

    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    {
        lightingData.mainLightColor = LightingPhysicallyBased(brdfData, brdfDataClearCoat,
                                                              mainLight,
                                                              inputData.normalWS, inputData.viewDirectionWS,
                                                              surfaceData.clearCoatMask, specularHighlightsOff);
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_CLUSTERED_LIGHTING
    for (uint lightIndex = 0; lightIndex < min(_AdditionalLightsDirectionalCount, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased(brdfData, brdfDataClearCoat, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        {
            lightingData.additionalLightsColor += LightingPhysicallyBased(brdfData, brdfDataClearCoat, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
    #endif
    half4 pbrColor = CalculateFinalColor(lightingData, surfaceData.alpha);
    //(float3 gi_color, float3 viewDir, float3 lightDir, float3 normal, float distortion, float power, float scale, float thickness)
    pbrColor.rgb += FastTranslucency(lightingData.giColor, inputData.viewDirectionWS, mainLight.direction, inputData.normalWS, _DiffuseDistortion, _Power, _Scale, thickness, mainLight.distanceAttenuation, _Ambient);
    return pbrColor;
}

half4 GrassPBRPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if defined(_PARALLAXMAP)
    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = input.viewDirTS;
    #else
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, viewDirWS);
    #endif
    ApplyPerPixelDisplacement(viewDirTS, input.uv);
    #endif

    SurfaceData surfaceData;
    GrassPBRInitializeStandardLitSurfaceData(input.uv, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

    #ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
    #endif
    float thickness = SAMPLE_TEXTURE2D(_ThicknessMap, sampler_ThicknessMap, input.uv).r;
    half4 color = GrassUniversalFragmentPBR(inputData, surfaceData, 1-saturate(thickness));
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, _Surface);

    return color;
}
#endif