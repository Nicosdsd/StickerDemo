Shader "Unlit/OutlineFlowURP" // Renamed to indicate URP compatibility
{
    Properties
    {
        _BaseMap ("主纹理 (Texture)", 2D) = "white" {}
        [HDR]_OutlineColor ("描边颜色 (Outline Color)", Color) = (1,0,0,1) // 用户已修改为HDR
        _OutlineIntensity ("描边强度 (Outline Intensity)", Range(0.0, 1.0)) = 1.0 // 新增：描边强度控制
        _OutlineThickness ("描边厚度 (Outline Thickness - texels)", Float) = 1.0 // 描边宽度，单位：纹素
        _AlphaThreshold ("Alpha阈值 (Alpha Threshold)", Range(0.01, 1.0)) = 0.5 // Alpha值低于此阈值的像素被视为透明

        // 新增流动效果属性
        _FlowTex ("流动纹理 (Flow Texture)", 2D) = "gray" {} // 流动图案纹理
        [HDR]_FlowColor ("流动颜色 (Flow Color Tint)", Color) = (1,1,1,1) // 流动颜色，支持HDR
        _FlowOffset ("流动偏移 (Flow Offset)", Range(0.0, 1.0)) = 0.0 // 新增：流动偏移控制，并添加范围
        _FlowIntensity ("流光强度 (Flow Intensity)", Range(0.0, 1.0)) = 1.0 // 新增：流光强度控制
    }
    SubShader
    {
        Tags 
        {
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline" // URP Tag
            "IgnoreProjector"="True" 
        }
        // LOD 100 // LOD is less relevant or handled differently in URP specific contexts

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" } // URP specific LightMode

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Back // Or Off if double-sided outline is needed
            ZWrite On

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            
            // Required for URP shaders
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            // #pragma multi_compile_fog // URP handles fog differently, often automatically or via keywords
            #pragma multi_compile_instancing // Enable GPU Instancing

            #pragma vertex vert
            #pragma fragment frag

            // HLSL code from OutlineFlowURP.hlslinc starts here
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" // Include for lighting functions like LerpWhiteTo

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _OutlineColor;
                float _OutlineThickness;
                float _AlphaThreshold;
                half4 _FlowColor;
                float _FlowOffset;
                float _OutlineIntensity;
                float _FlowIntensity;
                // _BaseMap_TexelSize is usually provided by Unity automatically for URP
                // If not, it can be declared here: float4 _BaseMap_TexelSize;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_FlowTex);
            SAMPLER(sampler_FlowTex);

            // If _BaseMap_TexelSize is not automatically available, declare it.
            #ifndef _BaseMap_TexelSize
                float4 _BaseMap_TexelSize;
            #endif

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                half4 mainColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                if (mainColor.a < _AlphaThreshold)
                {
                    clip(-1); // discard
                }

                float2 texelSize = _BaseMap_TexelSize.xy;
                float stepX = texelSize.x * _OutlineThickness;
                float stepY = texelSize.y * _OutlineThickness;

                half alphaN = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv + float2(0, stepY)).a;
                half alphaS = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv - float2(0, stepY)).a;
                half alphaW = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv - float2(stepX, 0)).a;
                half alphaE = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv + float2(stepX, 0)).a;
                half alphaNW = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv + float2(-stepX, stepY)).a;
                half alphaNE = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv + float2(stepX, stepY)).a;
                half alphaSW = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv + float2(-stepX, -stepY)).a;
                half alphaSE = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv + float2(stepX, -stepY)).a;

                bool isEdge = false;
                if (alphaN < _AlphaThreshold || alphaS < _AlphaThreshold ||
                    alphaW < _AlphaThreshold || alphaE < _AlphaThreshold ||
                    alphaNW < _AlphaThreshold || alphaNE < _AlphaThreshold ||
                    alphaSW < _AlphaThreshold || alphaSE < _AlphaThreshold)
                {
                    isEdge = true;
                }

                half4 finalColor;
                if (isEdge)
                {
                    float2 centerUV = float2(0.5, 0.5);
                    float2 dir = IN.uv - centerUV;
                    float angle = atan2(dir.y, dir.x);
                    float normalizedAngle = (angle / (2.0f * 3.14159265359f)) + 0.5f;
                    float2 flowUV = float2(normalizedAngle - _FlowOffset, 0.5f);
                    
                    half4 flowSample = SAMPLE_TEXTURE2D(_FlowTex, sampler_FlowTex, flowUV);
                    half4 flowEffectColor = flowSample * _FlowColor;
                    
                    half4 finalOutlineComponent = _OutlineColor * _OutlineIntensity;
                    half4 finalFlowComponent = flowEffectColor * _FlowIntensity;

                    finalColor.rgb = finalOutlineComponent.rgb + finalFlowComponent.rgb;
                    finalColor.a = saturate(finalOutlineComponent.a + finalFlowComponent.a);
                }
                else
                {
                    finalColor = mainColor;
                }
                
                return finalColor;
            }
            // HLSL code from OutlineFlowURP.hlslinc ends here

            ENDHLSL
        }

        Pass 
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _BaseMap;

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _Cutoff;         
            float _CurvedToggle;
            float _LightIntensity;

            half _Root;
            half4 _WindDir;
            half _WindStrength;
            half _WindSpeed;
            half _VegetationScale;
            CBUFFER_END

            struct a2v {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            v2f vert(a2v v)
            {
                v2f o = (v2f)0;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }
            real4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_BaseMap, i.uv);
                clip(col.a - 0.5f);
                return 0;
            }
            ENDHLSL
        }
        
    }
    // Fallback "Transparent/VertexLit" // Legacy fallback, less used in URP
}
