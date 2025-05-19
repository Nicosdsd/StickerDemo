Shader "Custom/URPSimpleLit"
{
    Properties
    {
        [Header(Base)]
        [HideInInspector]_BaseColor ("BaseColor", Color)=(1,1,1,1)
        [HideInInspector]_MainTex ("MainTex", 2D) = "white" {}
        [Normal]_BumpMap("Normal Map",2D) = "Bump"{}
        _BumpScale("Normal Scale",Range(0, 1)) = 0
        _ColorMuti("ColorMuti", Float) = 1

        [Header(Specular 1)]
        [hdr]_SpecularColor("Specular Color 1", Color) = (1,1,1,1)
        _Glossiness("Glossiness 1", Range(8.0, 256)) = 20
        _SpecYaw1("Horizontal Angle", Range(-180, 180)) = 0
        _SpecPitch1("Vertical Angle", Range(-90, 90)) = 0
        
        [Header(Specular Mask 1)]
        _SpecularMask1("Specular Mask 1", 2D) = "white" {}
        _SpecularMaskStrength1("Specular Mask Strength 1", Range(0, 1)) = 1.0

        [Header(Specular 2)]
        [hdr]_SecondSpecularColor("Specular Color 2", Color) = (1,1,1,1)
        _SecondGlossiness("Glossiness 2", Range(8.0, 256)) = 30
        _SpecYaw2("Horizontal Angle", Range(-180, 180)) = 0
        _SpecPitch2("Vertical Angle", Range(-90, 90)) = 0

        [Header(Transparency)]
        _AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Toggle]_AlphaClip("Alpha Clip", Float) = 0

        [Header(Fake Shadow)]
        _ShadowColor ("Shadow Color", Color) = (0,0,0,0.5)
        _ShadowOffsetX ("Shadow Offset X", Range(-0.1, 0.1)) = 0.005
        _ShadowOffsetY ("Shadow Offset Y", Range(-0.1, 0.1)) = -0.005

        [Header(SphereMask)]
        _SphereCenter ("Sphere Center", Vector) = (0,0,0,0)
        _Radius ("Radius", Float) = 1.0
        _Hardness ("Hardness", Float) = 1.0
    }

    SubShader
    {
        Tags { 
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ALPHACLIP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct a2v
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float4 tangent : TANGENT;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 tangent : TEXCOORD3;
                float3 tangentB : TEXCOORD4;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_SpecularMask1);
            SAMPLER(sampler_SpecularMask1);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _BumpMap_ST;
            float4 _SpecularMask1_ST;
            float4 _BaseColor;
            float _ColorMuti;
            float4 _SpecularColor;
            float4 _SecondSpecularColor;
            float _BumpScale;
            float _Glossiness;
            float _SecondGlossiness;
            float _AlphaCutoff;

            // New Yaw and Pitch controls for Specular directions
            float _SpecYaw1;
            float _SpecPitch1;

            float _SpecYaw2;
            float _SpecPitch2;

            float4 _ShadowColor;
            float _ShadowOffsetX;
            float _ShadowOffsetY;

            float4 _SphereCenter;
            float _Radius;
            float _Hardness;

            float _SpecularMaskStrength1;
            CBUFFER_END

            v2f vert (a2v v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.tangent = normalize(TransformObjectToWorld(v.tangent.xyz));
                o.tangentB = cross(o.worldNormal.xyz,o.tangent.xyz) * v.tangent.w;
                o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*_BaseColor;

                 //SphereMask
                float3 sphereMask = pow(saturate( dot((i.worldPos - _SphereCenter.xyz) / _Radius, (i.worldPos - _SphereCenter.xyz) / _Radius)), _Hardness);

                

                mainTex *= i.color; // Apply vertex color

                float normalScale = _BumpScale * sphereMask;
                half3 normalTex = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,TRANSFORM_TEX(i.uv, _BumpMap)), normalScale);

                Light mainLight = GetMainLight();
                half3 ambient = _GlossyEnvironmentColor.rgb;

                float3x3 TBN = float3x3(i.tangent,i.tangentB,i.worldNormal);
                float3 nDirWS = normalize(mul(normalTex,TBN));

                float3 worldLightDir = normalize(_MainLightPosition.xyz);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

                // Convert angles to direction vector for Specular 1
                float3 customSpecDir1 = float3(
                    cos(radians(_SpecYaw1)) * cos(radians(_SpecPitch1)),
                    sin(radians(_SpecPitch1)),
                    sin(radians(_SpecYaw1)) * cos(radians(_SpecPitch1))
                );
                customSpecDir1 = normalize(customSpecDir1);

                // First Specular Calculation
                half3 halfDir1 = normalize(customSpecDir1 + viewDir);
                half NdotH1 = max(0, dot(nDirWS, halfDir1));
                half specularIntensity1 = pow(NdotH1, _Glossiness);

                // Sample Specular Mask 1
                half specularMask1 = SAMPLE_TEXTURE2D(_SpecularMask1, sampler_SpecularMask1, i.uv).a;
                specularIntensity1 *= lerp(1.0, specularMask1, _SpecularMaskStrength1);

                half3 specular1 = _SpecularColor.rgb * specularIntensity1 * mainLight.color;

                // Convert angles to direction vector for Specular 2
                float3 customSpecDir2 = float3(
                    cos(radians(_SpecYaw2)) * cos(radians(_SpecPitch2)),
                    sin(radians(_SpecPitch2)),
                    sin(radians(_SpecYaw2)) * cos(radians(_SpecPitch2))
                );
                customSpecDir2 = normalize(customSpecDir2);

                // Second Specular Calculation
                half3 halfDir2 = normalize(customSpecDir2 + viewDir);
                half NdotH2 = max(0, dot(nDirWS, halfDir2));
                half specularIntensity2 = pow(NdotH2, _SecondGlossiness);
                half3 specular2 = _SecondSpecularColor.rgb * specularIntensity2 * mainLight.color;

                // Diffuse
                half3 diffuse = LightingLambert(mainLight.color.rgb, worldLightDir, nDirWS) * 0.5 + 0.5;
         
                // Combine all components
                half3 finalCol = mainTex.rgb * (diffuse + specular1 + specular2) * _ColorMuti;
                


                
                // 阴影相关
                half3 sprite_lit_rgb = finalCol;
                half sprite_alpha = mainTex.a;

                // Pre-multiply sprite component
                half3 pRGB_sprite = sprite_lit_rgb * sprite_alpha;
                half  A_sprite = sprite_alpha;

                // Shadow component
                float2 shadowUV = i.uv + float2(_ShadowOffsetX, _ShadowOffsetY);
                half4 shadowTextureSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, shadowUV);
                
                // Calculate alpha for the shadow's shape, mirroring how mainTex.a is derived
                // mainTex.a was from (texture.a * _BaseColor.a * i.color.a)
                half shadow_shape_source_alpha = shadowTextureSample.a * _BaseColor.a * i.color.a;
                
                half3 shadow_straight_rgb = _ShadowColor.rgb;
                half  shadow_effect_alpha = shadow_shape_source_alpha * _ShadowColor.a; // Shadow's own alpha after shape and tint

                // Pre-multiply shadow component
                half3 pRGB_shadow = shadow_straight_rgb * shadow_effect_alpha;
                half  A_shadow = shadow_effect_alpha;

                // Composite sprite over shadow (using pre-multiplied alpha)
                // pRGB_final = pRGB_over + pRGB_under * (1 - A_over)
                // A_final    = A_over    + A_under    * (1 - A_over)
                half3 composite_pRGB = pRGB_sprite + pRGB_shadow * (1.0h - A_sprite);
                half composite_A    = A_sprite + A_shadow * (1.0h - A_sprite);
                
                return half4(composite_pRGB, composite_A);
            }
            ENDHLSL
        }

         Pass {
            Tags { "LightMode"="ShadowCaster" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
            struct appdata
            {
              float4 vertex : POSITION;
            };
 
            struct v2f
            {
              float4 pos : SV_POSITION;
            };
 
             v2f vert(appdata v)
             {
                 v2f o;
                 o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
                 return o;
             }
             float4 frag(v2f i) : SV_Target
             {
                 return float4(0.0, 0.0, 0.0, 0.0); // shadow caster output
             }
             ENDHLSL
         }
    }

    FallBack "Packages/com.unity.render-pipelines.universal/FallbackError"
}