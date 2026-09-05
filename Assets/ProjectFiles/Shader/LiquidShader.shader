Shader "Custom/LiquidShader"
{
    Properties
    {
        _LiquidColor      ("Liquid Color", Color) = (0.2, 0.6, 1.0, 0.85)
        _FoamColor        ("Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _FillAmount       ("Fill Amount", Range(0,1)) = 0.5

        _WobbleX          ("Wobble X", Float) = 0
        _WobbleZ          ("Wobble Z", Float) = 0
        _WobbleStrength   ("Wobble Strength", Range(0,2)) = 1
        _WobbleSpeed      ("Wobble Speed", Float) = 3.0
        _WobblePhase      ("Wobble Phase", Float) = 0.0
        _WobbleFrequency  ("Wobble Frequency", Float) = 15.0

        _MinFillY         ("Min Fill Y (Offset)", Float) = -0.15
        _MaxFillY         ("Max Fill Y (Offset)", Float) = 0.25

        _FoamWidth        ("Foam Band Width", Range(0,0.1)) = 0.02
        _Transparency     ("Transparency", Range(0,1)) = 0.85

        _RimPower         ("Rim Power", Range(0.5,8)) = 3
        _RimColor         ("Rim Color", Color) = (1,1,1,0.4)
    }

    SubShader
    {
        Tags { "Queue"="Transparent-1" "RenderType"="Transparent" }

        //==========================
        // Liquid Pass
        //==========================
        Pass
        {
            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _LiquidColor;
            fixed4 _FoamColor;

            float _FillAmount;
            float _MinFillY;
            float _MaxFillY;
            float _WobbleX;
            float _WobbleZ;
            float _WobbleStrength;
            float _WobbleSpeed;
            float _WobblePhase;
            float _WobbleFrequency;
            float _FoamWidth;
            float _Transparency;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float wave(float3 worldPos, float3 objectWorldPos)
            {
                float localX = worldPos.x - objectWorldPos.x;
                float localZ = worldPos.z - objectWorldPos.z;

                // Slosh
                float slosh =
                    (localX * _WobbleX + localZ * _WobbleZ)
                    * 0.08
                    * _WobbleStrength;

                // Ripple with per-material randomized speed, frequency, and phase
                float speed = _WobbleSpeed > 0 ? _WobbleSpeed : 3.0;
                float freq = _WobbleFrequency > 0 ? _WobbleFrequency : 15.0;
                float ripple =
                    (sin(localX * freq + _Time.y * speed + _WobblePhase) * 0.015 +
                     sin(localZ * freq + _Time.y * speed + _WobblePhase * 1.3) * 0.015)
                    * _WobbleStrength;

                return slosh + ripple;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 objectWorldPos = float3(
                    unity_ObjectToWorld[0][3],
                    unity_ObjectToWorld[1][3],
                    unity_ObjectToWorld[2][3]);

                float fillLevel = lerp(_MinFillY, _MaxFillY, _FillAmount);

                float fillLine =
                    objectWorldPos.y +
                    fillLevel +
                    wave(i.worldPos, objectWorldPos);

                clip(fillLine - i.worldPos.y);

                float foamMask = step(fillLine - _FoamWidth, i.worldPos.y);

                fixed4 col = lerp(_LiquidColor, _FoamColor, foamMask);
                col.a = _Transparency;

                return col;
            }

            ENDCG
        }

        //==========================
        // Rim Pass
        //==========================
        Pass
        {
            Cull Back
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _RimPower;
            fixed4 _RimColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float rim = 1.0 - saturate(dot(i.normal, i.viewDir));
                rim = pow(rim, _RimPower);

                return fixed4(_RimColor.rgb, rim * _RimColor.a);
            }

            ENDCG
        }
    }
}