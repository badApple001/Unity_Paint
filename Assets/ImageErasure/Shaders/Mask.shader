Shader "ScratchCard/Mask"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "black" {}
        _Offset ("Offset", Vector) = (0, 0, 1, 1)
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"}
        ZWrite Off
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _MaskTex;
            uniform float4 _MainTex_ST;
            uniform float4 _MaskTex_ST;
            uniform float4 _Offset;

            struct app2vert
            {
                float4 position: POSITION;
                half4 color: COLOR;
                float2 texcoord: TEXCOORD0;
            };

            struct vert2frag
            {
                float4 position: SV_POSITION;
                half4 color: COLOR;
                float2 texcoord: TEXCOORD0;
            };

            vert2frag vert(app2vert input)
            {
                vert2frag output;
                output.position = UnityObjectToClipPos(input.position);
				output.color = input.color;
                output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
                return output;
            }

            float4 frag(vert2frag input) : COLOR
            {
                float4 main_color = tex2D(_MainTex, float2(_Offset.x + input.texcoord.x * _Offset.z, _Offset.y + input.texcoord.y * _Offset.w));
                float4 mask_color = tex2D(_MaskTex, input.texcoord);
                float4 value = float4(input.color.r * main_color.r, input.color.g * main_color.g, input.color.b * main_color.b, input.color.a * main_color.a * (1.0f - mask_color.r));
                return value;
            }
            ENDCG
        }
    }
}