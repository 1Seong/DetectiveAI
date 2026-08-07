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
        _BorderTex ("Border Pattern", 2D) = "white" {}

        _BorderStrength (
            "Border Effect Strength",
            Range(0, 1)
        ) = 1

        _BorderFade (
            "Border Fade",
            Range(0.1, 30)
        ) = 6

        _BorderIrregularity (
            "Border Irregularity",
            Range(0, 30)
        ) = 7

        _BorderPatternScale (
            "Border Pattern Scale",
            Float
        ) = 8

        _BorderCrackStrength (
            "Border Crack Strength",
            Range(0, 1)
        ) = 0.45

        _BorderCrackDepth (
            "Border Crack Depth",
            Range(0, 50)
        ) = 18
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
                float2 rawUV : TEXCOORD2;
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
            
            sampler2D _BorderTex;

            float _BorderStrength;
            float _BorderFade;
            float _BorderIrregularity;
            float _BorderPatternScale;
            float _BorderCrackStrength;
            float _BorderCrackDepth;

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.worldPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(input.vertex);

                output.rawUV = input.uv;
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
                
                // 화면에 표시되는 사진의 실제 비율을 반영한 테두리 좌표
                float2 borderUV = saturate(input.rawUV);

                float2 borderEffectUV =
                    borderUV * aspectScale;

                // 각 픽셀이 사진의 네 변에서 얼마나 떨어져 있는지 계산합니다.
                // UV가 아니라 RectSize를 사용하므로 가로세로 비율이 달라도
                // 테두리 두께가 한쪽으로 늘어나지 않습니다.
                float2 distanceToEdge =
                    min(borderUV, 1.0 - borderUV) *
                    rectSize;

                float edgeDistance0 =
                    min(distanceToEdge.x, distanceToEdge.y);

                // 검은 균열과 흰 셀로 구성된 Voronoi 텍스처를 가정합니다.
                float borderPattern =
                    tex2D(
                        _BorderTex,
                        borderEffectUV * _BorderPatternScale
                    ).r;

                // 검은 부분일수록 테두리를 더 깊게 깎습니다.
                float erosionDistance =
                    (1.0 - borderPattern) *
                    _BorderIrregularity;

                // 사진 바깥쪽으로 갈수록 부드럽게 투명해지는 기본 마스크
                float softBorderMask =
                    smoothstep(
                        erosionDistance,
                        erosionDistance + max(_BorderFade, 0.001),
                        edgeDistance0
                    );

                // 균열이 사진 안쪽으로 적용되는 범위
                float crackArea =
                    1.0 - smoothstep(
                        _BorderCrackDepth,
                        _BorderCrackDepth + max(_BorderFade, 0.001),
                        edgeDistance0
                    );

                // Voronoi의 검은 선을 실제 투명 균열로 변환
                float crackMask =
                    lerp(
                        1.0,
                        borderPattern,
                        crackArea * _BorderCrackStrength
                    );

                float organicBorderMask =
                    softBorderMask * crackMask;

                // 0이면 테두리 효과를 완전히 비활성화
                float borderMask =
                    lerp(
                        1.0,
                        organicBorderMask,
                        _BorderStrength
                    );
                

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
                    saturate(bodyAlpha + dustAlpha) *
                borderMask;

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
