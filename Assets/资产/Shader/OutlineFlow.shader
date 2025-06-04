Shader "Unlit/OutlineFlow"
{
    Properties
    {
        _BaseMap ("主纹理 (Texture)", 2D) = "white" {}
        [HDR]_OutlineColor ("描边颜色 (Outline Color)", Color) = (1,0,0,1) // 用户已修改为HDR
        _OutlineThickness ("描边厚度 (Outline Thickness - texels)", Float) = 1.0 // 描边宽度，单位：纹素
        _AlphaThreshold ("Alpha阈值 (Alpha Threshold)", Range(0.01, 1.0)) = 0.5 // Alpha值低于此阈值的像素被视为透明

        // 新增流动效果属性
        _FlowTex ("流动纹理 (Flow Texture)", 2D) = "gray" {} // 流动图案纹理
        [HDR]_FlowColor ("流动颜色 (Flow Color Tint)", Color) = (1,1,1,1) // 流动颜色，支持HDR
        _FlowOffset ("流动偏移 (Flow Offset)", Range(0.0, 1.0)) = 0.0 // 新增：流动偏移控制，并添加范围
    }
    SubShader
    {
        // 为了Alpha混合，使用Transparent队列
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        // Alpha混合模式
        Blend SrcAlpha OneMinusSrcAlpha
        // Cull Off // 如果需要双面显示描边，可以取消注释这一行，默认为 Cull Back
        ZWrite On // 通常开启，如果与其他透明物体有排序问题，可尝试关闭

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // 启用雾效
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION; //顶点位置
                float2 uv : TEXCOORD0;    //UV坐标
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;      //传递给片元着色器的UV坐标
                UNITY_FOG_COORDS(1)         //雾效坐标
                float4 vertex : SV_POSITION;//裁剪空间中的顶点位置
            };

            sampler2D _BaseMap;             //主纹理
            float4 _BaseMap_ST;             //主纹理的缩放和平移
            float4 _BaseMap_TexelSize;      //主纹理的纹素大小 (1/width, 1/height, width, height)

            fixed4 _OutlineColor;           //描边颜色 (用户已改为HDR)
            float _OutlineThickness;        //描边厚度
            float _AlphaThreshold;          //Alpha阈值

            // 新增uniform变量
            sampler2D _FlowTex;
            fixed4 _FlowColor;
            float _FlowOffset; //新增

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);    //转换顶点到裁剪空间
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);         //转换UV坐标
                UNITY_TRANSFER_FOG(o,o.vertex);               //传递雾效坐标
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //对主纹理进行采样
                fixed4 mainColor = tex2D(_BaseMap, i.uv);

                //如果当前像素的Alpha值低于阈值，则认为其完全透明，不进行后续处理
                if (mainColor.a < _AlphaThreshold)
                {
                    discard; //直接丢弃该片元
                }

                //当前像素不透明，检查其是否为边缘像素
                //获取纹素的实际步进大小
                float2 texelSize = _BaseMap_TexelSize.xy;
                float stepX = texelSize.x * _OutlineThickness;
                float stepY = texelSize.y * _OutlineThickness;

                //采样8个方向邻居像素的Alpha值
                //上、下、左、右
                float alphaN = tex2D(_BaseMap, i.uv + float2(0, stepY)).a;
                float alphaS = tex2D(_BaseMap, i.uv - float2(0, stepY)).a;
                float alphaW = tex2D(_BaseMap, i.uv - float2(stepX, 0)).a;
                float alphaE = tex2D(_BaseMap, i.uv + float2(stepX, 0)).a;
                
                //四个对角线方向
                float alphaNW = tex2D(_BaseMap, i.uv + float2(-stepX, stepY)).a;
                float alphaNE = tex2D(_BaseMap, i.uv + float2(stepX, stepY)).a;
                float alphaSW = tex2D(_BaseMap, i.uv + float2(-stepX, -stepY)).a;
                float alphaSE = tex2D(_BaseMap, i.uv + float2(stepX, -stepY)).a;

                bool isEdge = false;
                //如果任何一个邻居像素的Alpha值低于阈值，则当前像素是边缘像素
                if (alphaN < _AlphaThreshold || alphaS < _AlphaThreshold ||
                    alphaW < _AlphaThreshold || alphaE < _AlphaThreshold ||
                    alphaNW < _AlphaThreshold || alphaNE < _AlphaThreshold ||
                    alphaSW < _AlphaThreshold || alphaSE < _AlphaThreshold)
                {
                    isEdge = true;
                }
                
                fixed4 finalColor;
                if (isEdge)
                {
                    // finalColor = _OutlineColor; //边缘像素使用指定的描边颜色 (旧逻辑)

                    // ---- 极坐标流动效果 ----
                    float2 centerUV = float2(0.5, 0.5); // UV中心点
                    float2 dir = i.uv - centerUV;       // 从中心到当前像素的向量

                    // 计算角度 (atan2 返回值范围 -PI 到 PI)
                    float angle = atan2(dir.y, dir.x);
                    // 将角度标准化到 [0, 1] 范围，用于UV坐标
                    float normalizedAngle = (angle / (2.0f * 3.14159265359f)) + 0.5f;

                    // 计算流动纹理的UV坐标
                    // U坐标根据标准化角度和时间流动，V坐标取0.5 (纹理中间)
                    // _Time.y 是Unity内置的时间变量
                    // float2 flowUV = float2(normalizedAngle - _Time.y * _FlowSpeed, 0.5f); (旧逻辑)
                    float2 flowUV = float2(normalizedAngle - _FlowOffset, 0.5f); // 新逻辑：使用_FlowOffset
                    
                    fixed4 flowSample = tex2D(_FlowTex, flowUV); // 采样流动纹理
                    
                    // 最终颜色 = 流动纹理采样值 * 流动颜色
                    // 流动纹理的Alpha通道和流动颜色的Alpha通道共同决定最终Alpha
                    finalColor = flowSample * _FlowColor;
                   // finalColor.a = flowSample.r; // 保持主纹理的Alpha值
                }

                
                else
                {
                    discard;     //内部像素使用纹理自身的颜色
                }
                
                UNITY_APPLY_FOG(i.fogCoord, finalColor); //应用雾效
                return finalColor;
            }
            ENDCG
        }
    }
    Fallback "Transparent/VertexLit" //为不支持此Shader的硬件提供一个回退选项
}
