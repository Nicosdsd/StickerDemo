Shader "Unlit/ShineLight"
{
    Properties
    {
        _MaskTex ("Mask Texture", 2D) = "white" {}
        [hdr]_ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShineLocation ("Shine Location", Range(-1, 2)) = 0
        _ShineWidth ("Shine Width", Range(0, 1)) = 0.1
        _ShineAngle ("Shine Angle", Range(0, 360)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha // Enable Alpha Blending
        ZWrite Off // Don't write to depth buffer for transparent objects

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MaskTex;
            float4 _MaskTex_ST;
            float4 _ShineColor;
            float _ShineLocation;
            float _ShineWidth;
            float _ShineAngle;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MaskTex); // Use _MaskTex for UV transform
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 maskCol = tex2D(_MaskTex, i.uv);

                // Invert mask (using alpha channel instead of red channel)
                float invertedMask =  maskCol.a;

                // Calculate shine direction and normalized progress
                float angle_rad = _ShineAngle * UNITY_PI / 180.0;
                float cos_a = cos(angle_rad);
                float sin_a = sin(angle_rad);
                float2 dir = float2(cos_a, sin_a);

                float p_offset = (dir.x < 0 ? dir.x : 0) + (dir.y < 0 ? dir.y : 0);
                float p_range = abs(dir.x) + abs(dir.y);
                p_range = max(p_range, 0.00001);
                float p = dot(i.uv, dir);
                float normalized_p = (p - p_offset) / p_range;

                float shineFactor = smoothstep(_ShineLocation - _ShineWidth / 2.0, _ShineLocation, normalized_p) - smoothstep(_ShineLocation, _ShineLocation + _ShineWidth / 2.0, normalized_p);
                
                // Base color is the HDR shine color
                fixed4 outputCol = _ShineColor;
                // Modulate the alpha by the inverted mask and shineFactor
                outputCol.a *= shineFactor * invertedMask; // Multiply with existing alpha of _ShineColor

                // apply fog (optional for transparent effects, might look odd)
                // UNITY_APPLY_FOG(i.fogCoord, outputCol);
                return outputCol;
            }
            ENDCG
        }
    }
}
