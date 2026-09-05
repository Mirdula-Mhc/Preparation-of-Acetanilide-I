// =====================================================================================
//  Boiling Turquoise Liquid Shader v4 — Unity 6 (6000.x) URP
//  ----------------------------------------------------------------------------------
//  DROP-IN: create a Material from this shader, assign it to ANY liquid-volume mesh,
//  press Play. Boiling motion, color, transparency and Fresnel all come from the
//  shader alone — no script, Animator, Rigidbody or particle system required.
//
//  v4 CHANGE (no bubbles — up/down heaving motion instead):
//    - Removed the rising-bubble field entirely per request.
//    - Added a "Boil Pulse": a uniform, whole-surface up/down heave (independent
//      of XZ position) layered UNDER the existing high-frequency Boil Turbulence
//      bumps. The turbulence gives the small local rippling texture; the pulse
//      gives the sense that the entire liquid level is rising and falling, which
//      together reads as a rolling boil rather than an ocean swell (the pulse is
//      fast + small-amplitude, not a slow, long-wavelength swell).
// =====================================================================================

Shader "Custom/URP/TurquoiseLiquid"
{
    Properties
    {
        [Header(Water Color)]
        _WaterColor      ("Water Color (Shallow)", Color) = (0.25, 0.85, 0.80, 0.65)
        _DeepWaterColor  ("Deep Water Color",       Color) = (0.02, 0.20, 0.30, 0.9)

        [Header(Deep Water Absorption)]
        _DepthColorStrength ("Depth Color Strength", Range(0,3)) = 1.2

        [Header(Liquid Bounds)]
        _LiquidBottom     ("Liquid Bottom (Local Y)", Float) = -1.0
        _LiquidTop        ("Liquid Top (Local Y)", Float) = 1.0
        _SurfaceBand      ("Surface Band Thickness", Range(0.01,1)) = 0.35
        _NormalUpThreshold("Surface Normal Threshold", Range(0,1)) = 0.3

        [Header(Transparency)]
        _Transparency ("Transparency", Range(0,1)) = 0.5
        _Opacity      ("Base Opacity", Range(0,1)) = 0.8
        _EdgeSoftness ("Edge Softness", Range(0.001,1)) = 0.2

        [Header(Surface Sway)]
        _WaveDirection    ("Background Sway Direction (XY)", Vector) = (1,0.4,0,0)
        _PrimaryWaveSpeed ("Sway Speed", Range(0,5)) = 0.3
        _PrimaryWaveScale ("Sway Scale", Range(0.1,10)) = 1.2
        _SurfaceDisplacement ("Sway Strength", Range(0,0.3)) = 0.008
        _SecondaryWaveSpeed ("Sway Speed 2", Range(0,5)) = 0.2
        _SecondaryWaveScale ("Sway Scale 2", Range(0.1,10)) = 2.0

        [Header(Boil Turbulence)]
        _BoilFrequency ("Boil Frequency", Range(0.5,12)) = 3.0
        _BoilSpeed     ("Boil Speed", Range(0,6)) = 1.4
        _BoilStrength  ("Boil Strength", Range(0,0.2)) = 0.05

        [Header(Boil Pulse)]
        _PulseSpeed    ("Pulse Speed", Range(0,10)) = 3.0
        _PulseStrength ("Pulse Strength", Range(0,0.3)) = 0.05
        _PulseSecondarySpeed ("Pulse Secondary Speed", Range(0,10)) = 4.7
        _PulseSecondaryStrength ("Pulse Secondary Strength", Range(0,0.3)) = 0.02

        [Header(Micro Ripples)]
        _RippleIntensity ("Ripple Intensity", Range(0,0.5)) = 0.14
        _NoiseStrength    ("Noise Strength", Range(0,0.2)) = 0.03
        _NoiseScale       ("Noise Scale", Range(0.1,10)) = 3.0
        _NoiseSpeed       ("Noise Speed", Range(0,5)) = 0.6
        _NormalStrength   ("Normal Distortion Strength", Range(0,2)) = 1.1

        [Header(Fresnel)]
        _FresnelStrength ("Fresnel Strength", Range(0,2)) = 0.6
        _FresnelPower    ("Fresnel Power", Range(0.1,8)) = 2.5

        [Header(Specular Smoothness)]
        _Smoothness       ("Smoothness", Range(0,1)) = 0.85
        _SpecularStrength ("Specular Strength", Range(0,2)) = 0.5

        [Header(Animation Performance)]
        _AnimationSpeed  ("Animation Speed", Range(0,5)) = 1.0
        _FreezeAnimation ("Freeze Animation", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        // FALLBACK-SAFETY: never samples _CameraOpacityTexture / _CameraDepthTexture.
        // Transparency and depth cues are approximated with Fresnel and object-space
        // height, so there is nothing to fall back FROM — it renders correctly even
        // with opaque/depth textures disabled in the URP Renderer asset.

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back   // switch to "Cull Off" if your liquid mesh is single-sided/open

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _WaterColor;
                float4 _DeepWaterColor;
                float  _DepthColorStrength;

                float  _LiquidBottom;
                float  _LiquidTop;
                float  _SurfaceBand;
                float  _NormalUpThreshold;

                float  _Transparency;
                float  _Opacity;
                float  _EdgeSoftness;

                float4 _WaveDirection;
                float  _PrimaryWaveSpeed;
                float  _PrimaryWaveScale;
                float  _SurfaceDisplacement;
                float  _SecondaryWaveSpeed;
                float  _SecondaryWaveScale;

                float  _BoilFrequency;
                float  _BoilSpeed;
                float  _BoilStrength;

                float  _PulseSpeed;
                float  _PulseStrength;
                float  _PulseSecondarySpeed;
                float  _PulseSecondaryStrength;

                float  _RippleIntensity;
                float  _NoiseStrength;
                float  _NoiseScale;
                float  _NoiseSpeed;
                float  _NormalStrength;

                float  _FresnelStrength;
                float  _FresnelPower;

                float  _Smoothness;
                float  _SpecularStrength;

                float  _AnimationSpeed;
                float  _FreezeAnimation;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 normalOS   : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ---------------------------------------------------------------
            // Cheap hash / value-noise / fbm — no textures, mobile-safe.
            // ---------------------------------------------------------------
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float Fbm(float2 p)
            {
                float n = ValueNoise(p) * 0.6;
                n += ValueNoise(p * 2.03 + 17.1) * 0.4;
                return n;
            }

            float AnimTime()
            {
                return _Time.y * _AnimationSpeed * (1.0 - _FreezeAnimation);
            }

            // ---------------------------------------------------------------
            // Surface height field:
            //   sway  = very subtle whole-surface tilt (barely visible)
            //   boil  = local, high-frequency, multi-octave turbulence bumps
            //           (the fine rippling texture of a rolling boil)
            //   pulse = UNIFORM up/down heave applied equally to every point
            //           on the surface — this is what reads as "the water
            //           level going up and down" like a boiling pot. It's
            //           fast and small-amplitude on purpose, so it never
            //           reads as a slow ocean swell.
            // ---------------------------------------------------------------
            float WaveHeight(float2 pos, float t)
            {
                float2 dir1 = normalize(_WaveDirection.xy + 1e-5);
                float2 dir2 = normalize(float2(-_WaveDirection.y, _WaveDirection.x) + 1e-5);

                float w1 = sin(dot(pos, dir1) * _PrimaryWaveScale + t * _PrimaryWaveSpeed);
                float w2 = sin(dot(pos, dir2) * _SecondaryWaveScale + t * _SecondaryWaveSpeed * 1.15 + 2.3) * 0.6;
                float sway = (w1 + w2) * 0.5 * _SurfaceDisplacement;

                float boil = 0.0;
                boil += (ValueNoise(pos * _BoilFrequency + t * _BoilSpeed) - 0.5) * 1.0;
                boil += (ValueNoise(pos * _BoilFrequency * 2.3 - t * _BoilSpeed * 1.6 + 11.3) - 0.5) * 0.5;
                boil += (ValueNoise(pos * _BoilFrequency * 4.7 + t * _BoilSpeed * 2.1 + 5.7) - 0.5) * 0.25;
                boil *= _BoilStrength;

                float pulse = sin(t * _PulseSpeed) * _PulseStrength
                            + sin(t * _PulseSecondarySpeed + 1.7) * _PulseSecondaryStrength;

                return sway + boil + pulse;
            }

            // ---------------------------------------------------------------
            // Robust "is this the liquid's top surface" mask.
            // ---------------------------------------------------------------
            float TopFactor(float3 objPos, float3 normalOS, float normalLow, float normalHigh)
            {
                float normalGate = smoothstep(normalLow, normalHigh, saturate(normalOS.y));
                float topBand = smoothstep(_LiquidTop - _SurfaceBand, _LiquidTop, objPos.y);
                return normalGate * max(0.5, topBand);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float t = AnimTime();

                float vtxTop = TopFactor(IN.positionOS.xyz, IN.normalOS, _NormalUpThreshold, 1.0);

                float3 objPos = IN.positionOS.xyz;
                float2 flatPos = objPos.xz;

                float h = WaveHeight(flatPos, t);
                objPos.y += h * vtxTop;

                float eps = 0.06;
                float hL = WaveHeight(flatPos - float2(eps, 0), t);
                float hR = WaveHeight(flatPos + float2(eps, 0), t);
                float hD = WaveHeight(flatPos - float2(0, eps), t);
                float hU = WaveHeight(flatPos + float2(0, eps), t);
                float3 waveNormalOS = normalize(float3((hL - hR) / (2 * eps), 1.0, (hD - hU) / (2 * eps)));
                float3 finalNormalOS = normalize(lerp(IN.normalOS, waveNormalOS, vtxTop));

                float3 positionWS = TransformObjectToWorld(objPos);
                OUT.positionWS = positionWS;
                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.normalWS   = TransformObjectToWorldNormal(finalNormalOS);
                OUT.normalOS   = IN.normalOS;
                OUT.positionOS = objPos;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float t = AnimTime();

                float fragTop = TopFactor(IN.positionOS, IN.normalOS, 0.05, 0.6);

                float3 N = normalize(IN.normalWS);
                float3 V = normalize(_WorldSpaceCameraPos.xyz - IN.positionWS);

                // ----- Per-pixel micro ripple (fine detail layered on the boil)
                float2 p = IN.positionOS.xz;
                float eps = 0.08;
                float nL = Fbm(p * _NoiseScale + float2(-eps, 0) + t * _NoiseSpeed);
                float nR = Fbm(p * _NoiseScale + float2( eps, 0) + t * _NoiseSpeed);
                float nD = Fbm(p * _NoiseScale + float2(0, -eps) + t * _NoiseSpeed);
                float nU = Fbm(p * _NoiseScale + float2(0,  eps) + t * _NoiseSpeed);
                float3 bumpOS = float3((nL - nR) * _NormalStrength, 0, (nD - nU) * _NormalStrength);
                float3 bumpWS = TransformObjectToWorldDir(bumpOS) * _RippleIntensity;
                float3 rippledN = normalize(N + bumpWS * fragTop);

                // ----- Depth-based absorption color (shallow -> deep)
                float heightT = saturate((IN.positionOS.y - _LiquidBottom) / max(0.0001, (_LiquidTop - _LiquidBottom)));
                float depthFactor = saturate(pow(1.0 - heightT, 1.0) * _DepthColorStrength);
                float3 baseColor = lerp(_WaterColor.rgb, _DeepWaterColor.rgb, depthFactor);

                float shimmer = Fbm(p * _NoiseScale * 0.4 + t * _NoiseSpeed * 0.4);
                baseColor += (shimmer - 0.5) * 0.03;

                // ----- Lighting
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(rippledN, mainLight.direction));
                float3 ambient = SampleSH(rippledN) * baseColor;
                float3 diffuse = baseColor * mainLight.color * NdotL * mainLight.shadowAttenuation;

                float3 halfDir = normalize(mainLight.direction + V);
                float specPower = exp2(_Smoothness * 10.0 + 1.0);
                float specMask = lerp(0.35, 1.0, fragTop);
                float spec = pow(saturate(dot(rippledN, halfDir)), specPower) * _SpecularStrength * specMask;
                float3 specular = mainLight.color * spec;

                float fresnel = pow(1.0 - saturate(dot(rippledN, V)), _FresnelPower) * _FresnelStrength;
                fresnel *= lerp(0.5, 1.0, fragTop);
                float3 fresnelColor = lerp(baseColor, float3(1, 1, 1), 0.5) * fresnel;

                float3 finalColor = ambient * 0.5 + diffuse + specular + fresnelColor;

                // ----- Alpha
                float baseAlpha = lerp(_Opacity, _Opacity * (1.0 - _Transparency), 0.5);
                float sideSoftening = lerp(1.0 - _EdgeSoftness * 0.4, 1.0, fragTop);
                float alpha = saturate((baseAlpha + fresnel * 0.3) * sideSoftening);

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
