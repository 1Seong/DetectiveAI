Shader "UI/DiagonalDustDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}

        _Progress ("Progress", Range(0, 1)) = 0
        _EdgeWidth ("Dust Width", Range(0.01, 0.4)) = 0.15
        _NoiseScale ("Noise Scale", Float) = 8

        _ScatterDistance ("Scatter Distance", Range(0, 0.15)) = 0.04
        _ScatterNoiseScale ("Scatter Noise Scale", Float) = 35
        _DustDensity ("Dust Density", Range(0, 1)) = 0.55
        _DustSize ("Dust Size", Range(1, 20)) = 7

        _FadeColor ("Dust Tint", Color) = (0.75, 0.7, 0.65, 1)
        
        _RectSize ("Rect Size", Vector) = (100, 100, 0, 0)

        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;

            float4 _MainTex_ST;
            float4 _Color;
            float4 _FadeColor;
            float4 _ClipRect;
            float4 _RectSize;

            float _Progress;
            float _EdgeWidth;
            float _NoiseScale;
            float _ScatterDistance;
            float _ScatterNoiseScale;
            float _DustDensity;
            float _DustSize;

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.worldPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;

                return output;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                
                float2 rectSize = max(_RectSize.xy, float2(1.0, 1.0));

                // 짧은 축을 1로 맞춘 비율 보정 좌표
                float minSide = min(rectSize.x, rectSize.y);
                float2 aspectScale = rectSize / minSide;

                float2 effectUV = uv * aspectScale;

                // 좌측 상단은 0, 우측 하단은 1에 가까워집니다.
                float diagonal =
                    (
                        effectUV.x +
                        (aspectScale.y - effectUV.y)
                    )
                    /
                    (aspectScale.x + aspectScale.y);

                float noise =
                     tex2D(
                        _NoiseTex,
                        effectUV * _NoiseScale
                     ).r;

                float dissolveValue =
                    diagonal +
                    (noise - 0.5) * _EdgeWidth;

                float front =
                    _Progress * (1.0 + _EdgeWidth);

                // 소멸 경계 주변 영역
                float edgeDistance =
                    dissolveValue - front;
                
                float progressMask =
                    step(0.0001, _Progress);

                float intactMask =
                    step(0.0, edgeDistance);

                float dustBand =
                    1.0 - smoothstep(
                        0.0,
                        _EdgeWidth,
                        abs(edgeDistance)
                    );
                
                float2 scatterCoord =
                    effectUV * _ScatterNoiseScale;
                
                float2 scatterCell =
                    floor(scatterCoord);

                // 경계 주변 픽셀을 우측 위 방향으로 흩뿌립니다.
                float scatterNoise =
                    Hash21(scatterCell);

                float2 scatterDirection = normalize(
                    float2(
                        0.8 + scatterNoise,
                        0.3 + scatterNoise * 0.8
                    )
                );

                float scatterStrength =
                    dustBand *
                    scatterNoise *
                    _ScatterDistance *
                    _Progress;

                float2 uvScatterDirection =
                    scatterDirection / aspectScale;

                float2 displacedUV =
                    uv - uvScatterDirection * scatterStrength;

                fixed4 mainColor =
                    tex2D(_MainTex, displacedUV) *
                    input.color;

                // 작은 사각형 형태의 가루 조각
                float2 dustCellUV =
                    frac(scatterCoord);

                float2 dustCenter =
                    float2(
                        Hash21(scatterCell),
                        Hash21(scatterCell + 17.3)
                    );

                float dustDistance =
                    length(dustCellUV - dustCenter);

                float dustRadius =
                    0.5 / max(_DustSize, 1.0);

                float dustParticle =
                    1.0 - step(
                        dustRadius,
                        dustDistance
                    );

                float dustRandom =
                    step(
                        scatterNoise,
                        _DustDensity
                    );

                float dustMask =
                    dustBand *
                    dustParticle *
                    dustRandom * progressMask;
                
                // Progress가 0이면 노이즈 계산과 무관하게 전체 이미지 표시
                float bodyMask =
                    lerp(
                        1.0,
                        intactMask,
                        progressMask
                    );

                // 이미 지나간 영역은 본체가 사라지고,
                // 경계 부근에는 가루 조각만 남습니다.
                float bodyAlpha =
                    mainColor.a *
                    bodyMask;

                float dustAlpha =
                    mainColor.a *
                    dustMask;

                float3 finalRgb =
                    lerp(
                        mainColor.rgb,
                        _FadeColor.rgb,
                        dustMask * _FadeColor.a
                    );

                float finalAlpha =
                    saturate(bodyAlpha + dustAlpha);

                #ifdef UNITY_UI_CLIP_RECT
                finalAlpha *= UnityGet2DClipping(
                    input.worldPosition.xy,
                    _ClipRect
                );
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(finalAlpha - 0.001);
                #endif

                return fixed4(finalRgb, finalAlpha);
            }

            ENDHLSL
        }
    }
}
