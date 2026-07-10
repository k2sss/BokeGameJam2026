// URP 特效 Shader：遮罩(Mask) + 溶解(Dissolve) + 扭曲(Distort)
// 三个功能均可独立开关（shader_feature_local），关闭时对应采样/分支不会参与编译，不额外增加开销。
// 适用管线：本项目内定制版 URP14（Packages/com.unity.render-pipelines.universal@14.0.12）
Shader "Effect/URP_MaskDissolveDistort_V260709"
{
    Properties
    {
        [Header(RenderState)][Space(8)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("源混合 Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("目标混合 Dst Blend", Float) = 10
        [Enum(Off,0,On,1)] _ZWrite("深度写入 ZWrite", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("深度测试 ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("剔除模式 Cull", Float) = 0

        [Header(MainTex)][Space(8)]
        [PerRendererData] _MainTex("主贴图 RGB:颜色 A:透明", 2D) = "white" {}
        [HDR] _MainColor("颜色叠加 Main Color", Color) = (1,1,1,1)
        _ColorIntensity("颜色强度(过曝倍数)", Range(0,5)) = 1
        _Opacity("整体透明度 Opacity", Range(0,1)) = 1
        [Toggle] _TreatZeroAlphaAsOne("主贴图 Alpha 为 0 时按 1 处理", Float) = 1
        [Toggle] _UseVertexColor("使用顶点色 Use VertexColor", Float) = 1
        _MainPanner("主贴图UV滚动 Main UV Panner(xy:速度)", Vector) = (0,0,0,0)

        [Header(Distort)][Space(8)]
        [Toggle(_DISTORT_ON)] _DistortOn("开启扭曲 Enable Distort", Float) = 0
        _DistortTex("扭曲噪波图(建议Linear/RG存偏移方向) Distort Noise", 2D) = "white" {}
        _DistortPanner("扭曲图UV滚动 Distort UV Panner(xy:速度)", Vector) = (0,0,0,0)
        _DistortIntensity("扭曲强度 Distort Intensity", Range(0,0.5)) = 0.05
        [Toggle] _DistortAffectMaskDissolve("扭曲同时影响遮罩/溶解采样", Float) = 0

        [Header(Mask)][Space(8)]
        [Toggle(_MASK_ON)] _MaskOn("开启遮罩 Enable Mask", Float) = 0
        _MaskTex("遮罩图 Mask Tex", 2D) = "white" {}
        [Enum(R,0,G,1,B,2,A,3)] _MaskChannel("遮罩取样通道 Mask Channel", Float) = 3
        [Toggle] _MaskInvert("反转遮罩 Invert Mask", Float) = 0
        _MaskPanner("遮罩UV滚动 Mask UV Panner(xy:速度)", Vector) = (0,0,0,0)

        [Header(Dissolve)][Space(8)]
        [Toggle(_DISSOLVE_ON)] _DissolveOn("开启溶解 Enable Dissolve", Float) = 0
        _DissolveTex("溶解噪波图 Dissolve Noise", 2D) = "white" {}
        [Enum(R,0,G,1,B,2,A,3)] _DissolveChannel("溶解取样通道 Dissolve Channel", Float) = 0
        _DissolveProgress("溶解进度 Dissolve Progress(0完整~1消失)", Range(0,1)) = 0
        _DissolveEdgeWidth("溶解边缘宽度 Edge Width", Range(0.001,1)) = 0.1
        [HDR] _DissolveEdgeColor("溶解边缘颜色 Edge Color", Color) = (1,0.6,0.1,1)
        _DissolveEdgeIntensity("溶解边缘强度 Edge Intensity", Range(0,10)) = 2
        [Toggle] _DissolveInvert("反向溶解(由外向内) Invert Dissolve", Float) = 0
        _DissolvePanner("溶解图UV滚动 Dissolve UV Panner(xy:速度)", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }
        LOD 100

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local _ _DISTORT_ON
            #pragma shader_feature_local _ _MASK_ON
            #pragma shader_feature_local _ _DISSOLVE_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            #if defined(_DISTORT_ON)
            TEXTURE2D(_DistortTex); SAMPLER(sampler_DistortTex);
            #endif
            #if defined(_MASK_ON)
            TEXTURE2D(_MaskTex); SAMPLER(sampler_MaskTex);
            #endif
            #if defined(_DISSOLVE_ON)
            TEXTURE2D(_DissolveTex); SAMPLER(sampler_DissolveTex);
            #endif

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _MainColor;
                half _ColorIntensity;
                half _Opacity;
                half _TreatZeroAlphaAsOne;
                half _UseVertexColor;
                float4 _MainPanner;

                float4 _DistortTex_ST;
                float4 _DistortPanner;
                half _DistortIntensity;
                half _DistortAffectMaskDissolve;

                float4 _MaskTex_ST;
                half _MaskChannel;
                half _MaskInvert;
                float4 _MaskPanner;

                float4 _DissolveTex_ST;
                half _DissolveChannel;
                half _DissolveProgress;
                half _DissolveEdgeWidth;
                half4 _DissolveEdgeColor;
                half _DissolveEdgeIntensity;
                half _DissolveInvert;
                float4 _DissolvePanner;
            CBUFFER_END

            // channel: 0=R 1=G 2=B 3=A，用于在关闭分支宏时也能安全编译的通道选择
            half SelectChannel(half4 tex, half channel)
            {
                half4 channelMask = half4(
                    channel < 0.5,
                    channel >= 0.5 && channel < 1.5,
                    channel >= 1.5 && channel < 2.5,
                    channel >= 2.5);
                return dot(tex, channelMask);
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 distortOffset = 0;
                #if defined(_DISTORT_ON)
                {
                    float2 distortUV = input.uv * _DistortTex_ST.xy + _DistortTex_ST.zw + _DistortPanner.xy * _Time.y;
                    half4 distortSample = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, distortUV);
                    distortOffset = (distortSample.rg * 2.0 - 1.0) * _DistortIntensity;
                }
                #endif

                float2 mainUV = input.uv * _MainTex_ST.xy + _MainTex_ST.zw + _MainPanner.xy * _Time.y + distortOffset;
                half4 mainSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);

                half4 vertexColor = _UseVertexColor > 0.5 ? input.color : half4(1, 1, 1, 1);
                half sampledAlpha = mainSample.a;
                if (_TreatZeroAlphaAsOne > 0.5 && sampledAlpha <= 0.001h)
                {
                    sampledAlpha = 1.0h;
                }
                half3 albedo = mainSample.rgb * _MainColor.rgb * vertexColor.rgb * _ColorIntensity;
                half alpha = sampledAlpha * _MainColor.a * vertexColor.a * _Opacity;

                // 遮罩/溶解是否复用扭曲的偏移量，可让整体扭曲感更统一
                float2 extraOffset = (_DistortAffectMaskDissolve > 0.5) ? distortOffset : float2(0, 0);

                #if defined(_MASK_ON)
                {
                    float2 maskUV = input.uv * _MaskTex_ST.xy + _MaskTex_ST.zw + _MaskPanner.xy * _Time.y + extraOffset;
                    half4 maskSample = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, maskUV);
                    half maskValue = SelectChannel(maskSample, _MaskChannel);
                    maskValue = _MaskInvert > 0.5 ? 1.0 - maskValue : maskValue;
                    alpha *= maskValue;
                }
                #endif

                #if defined(_DISSOLVE_ON)
                {
                    float2 dissolveUV = input.uv * _DissolveTex_ST.xy + _DissolveTex_ST.zw + _DissolvePanner.xy * _Time.y + extraOffset;
                    half4 dissolveSample = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, dissolveUV);
                    half noise = SelectChannel(dissolveSample, _DissolveChannel);
                    noise = _DissolveInvert > 0.5 ? 1.0 - noise : noise;

                    half edge = max(_DissolveEdgeWidth, 0.0001);
                    half dissolveFactor = saturate((noise - _DissolveProgress) / edge);
                    // 仅在“尚未被溶解”一侧显示边缘发光，避免叠加/One One 等混合模式下透出多余亮边
                    half edgeGlow = (1.0 - smoothstep(0.0, edge, abs(noise - _DissolveProgress))) * step(_DissolveProgress, noise);

                    alpha *= dissolveFactor;
                    albedo += _DissolveEdgeColor.rgb * edgeGlow * _DissolveEdgeIntensity;
                }
                #endif

                return half4(albedo, saturate(alpha));
            }
            ENDHLSL
        }
    }
}
