Shader "UI/AnimatedDashedBorder"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _BorderColor ("Border Color", Color) = (1, 1, 1, 1)
        _Thickness ("Thickness (Pixels)", Float) = 2
        _DashLength ("Dash Length (Pixels)", Float) = 10
        _GapLength ("Gap Length (Pixels)", Float) = 6
        _MoveSpeed ("Move Speed", Float) = 30

        [HideInInspector] _Color ("Tint", Color) = (1, 1, 1, 1)

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
            Name "AnimatedDashedBorder"

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _BorderColor;

            float4 _ClipRect;

            float _Thickness;
            float _DashLength;
            float _GapLength;
            float _MoveSpeed;

            v2f vert(appdata_t input)
            {
                v2f output;

                output.worldPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.texcoord;
                output.color = input.color * _Color;

                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 uv = input.uv;

                // UV가 화면상 한 픽셀 움직일 때 변하는 양
                float2 uvPerPixel = max(fwidth(uv), float2(0.000001, 0.000001));

                // 각 테두리까지의 거리를 픽셀 단위로 계산
                float leftDistance   = uv.x / uvPerPixel.x;
                float rightDistance  = (1.0 - uv.x) / uvPerPixel.x;
                float bottomDistance = uv.y / uvPerPixel.y;
                float topDistance    = (1.0 - uv.y) / uvPerPixel.y;

                float horizontalDistance =
                    min(leftDistance, rightDistance);

                float verticalDistance =
                    min(bottomDistance, topDistance);

                float edgeDistance =
                    min(horizontalDistance, verticalDistance);

                // 테두리 안쪽 영역
                float borderAA = 1.0;

                float borderMask =
                    1.0 - smoothstep(
                        _Thickness - borderAA,
                        _Thickness + borderAA,
                        edgeDistance
                    );

                // 현재 픽셀이 좌우 테두리 쪽인지, 상하 테두리 쪽인지 판정
                float isVerticalEdge =
                    step(horizontalDistance, verticalDistance);

                // 좌우 테두리에서는 Y축, 상하 테두리에서는 X축 기준
                float perimeterPosition =
                    lerp(
                        uv.x / uvPerPixel.x,
                        uv.y / uvPerPixel.y,
                        isVerticalEdge
                    );

                float patternLength =
                    max(_DashLength + _GapLength, 0.001);

                float animatedPosition =
                    perimeterPosition +
                    _Time.y * _MoveSpeed;

                float pattern =
                    frac(animatedPosition / patternLength);

                float dashRatio =
                    _DashLength / patternLength;

                float dashAA =
                    fwidth(animatedPosition / patternLength);

                float dashMask =
                    1.0 - smoothstep(
                        dashRatio - dashAA,
                        dashRatio + dashAA,
                        pattern
                    );

                float alpha =
                    borderMask *
                    dashMask *
                    _BorderColor.a *
                    input.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                alpha *= UnityGet2DClipping(
                    input.worldPosition.xy,
                    _ClipRect
                );
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha - 0.001);
                #endif

                return fixed4(
                    _BorderColor.rgb * input.color.rgb,
                    alpha
                );
            }

            ENDCG
        }
    }
}
