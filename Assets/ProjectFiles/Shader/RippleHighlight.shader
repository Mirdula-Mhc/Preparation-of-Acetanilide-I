Shader "Custom/RippleHighlight"
{
    // Flat-plane ripple highlight for URP.
    // Draws concentric rings expanding outward from the plane's center (UV 0.5,0.5),
    // fading out with distance and looping over time. Meant to sit just under a
    // selectable object on the table surface.

    Properties
    {
        _RingColor      ("Ring Color", Color) = (0.3, 0.8, 1.0, 1.0)
        _RingSpeed      ("Ring Speed", Float) = 1.0
        _RingWidth      ("Ring Width", Range(0.01, 0.5)) = 0.08
        _RingSpacing    ("Ring Spacing", Range(0.05, 1.0)) = 0.25
        _MaxRadius      ("Max Radius", Range(0.05, 1.0)) = 0.5
        _FadeSharpness  ("Edge Fade Sharpness", Range(0.5, 8.0)) = 2.0
        _EmissionIntensity ("Emission Intensity", Range(0.0, 10.0)) = 2.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "RippleUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _RingColor;
                float _RingSpeed;
                float _RingWidth;
                float _RingSpacing;
                float _MaxRadius;
                float _FadeSharpness;
                float _EmissionIntensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Distance from plane center (UV space, 0.5,0.5 is the middle)
                float2 centered = IN.uv - float2(0.5, 0.5);
                float dist = length(centered);

                // Discard outside the visible radius
                if (dist > _MaxRadius)
                {
                    discard;
                }

                // Animate rings expanding outward: subtract time*speed from distance,
                // then wrap by spacing so multiple rings repeat.
                float t = frac((dist - _Time.y * _RingSpeed) / _RingSpacing) * _RingSpacing;

                // Distance to nearest ring line within one spacing period
                float ringDist = abs(t - _RingSpacing * 0.5) - (_RingSpacing * 0.5 - _RingWidth * 0.5);
                float ringMask = saturate(1.0 - ringDist / (_RingWidth * 0.5));

                // Fade rings out as they approach the outer edge
                float edgeFade = saturate(1.0 - pow(dist / _MaxRadius, _FadeSharpness));

                float alpha = ringMask * edgeFade * _RingColor.a;

                // Boost RGB beyond 1.0 so URP Bloom (if enabled on the URP asset/volume)
                // picks this up as an emissive glow. With no Bloom in the scene this
                // still simply renders as a brighter ring color.
                half3 emissiveColor = _RingColor.rgb * _EmissionIntensity;

                return half4(emissiveColor, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
