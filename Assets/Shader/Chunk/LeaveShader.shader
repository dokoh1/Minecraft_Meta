Shader "Minecraft/Leaves"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // 텍스처 프로퍼티
        _AlphaClipThreshold ("Alpha Clip Threshold", Range(0,1)) = 0.5 // 알파 클리핑 값
    }
    SubShader
    {
        Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
        LOD 100
        Lighting Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            sampler2D _MainTex;
            float _AlphaClipThreshold;
            float GlobalLight;
            float MinGlobalLight;
            float MaxGlobalLight;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 월드 좌표 변환
                o.uv = v.uv; // UV 전달
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float shade = (MaxGlobalLight - MinGlobalLight) * GlobalLight + MinGlobalLight;
                shade *= i.color.a;
                shade = clamp(1 - shade, MinGlobalLight, MaxGlobalLight);
                clip(col.a - _AlphaClipThreshold);
                col = lerp(col, float4(0, 0, 0, 1), shade);// 알파 클리핑 (0.5 기준)
                return col; // 최종 색상 출력
            }
            ENDCG
        }
    }
}