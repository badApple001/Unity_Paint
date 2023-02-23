Shader "ScratchCard/Brush"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}

        [Header(Blending)]
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOpValue ("__blendOp", Int) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcColorBlend ("__srcC", Int) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstColorBlend ("__dstC", Int) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcAlphaBlend ("__srcA", Int) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstAlphaBlend ("__dstA", Int) = 1
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"}
        ZWrite Off
        ZTest Off
        Lighting Off
        BlendOp [_BlendOpValue]
        Blend [_SrcColorBlend] [_DstColorBlend], [_SrcAlphaBlend] [_DstAlphaBlend]
        Pass
        {
        	CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
		    #pragma fragmentoption ARB_precision_hint_fastest
				
			sampler2D _MainTex;
			half4 _MainTex_ST;
			float4 _Color;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = o.vertex;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}
						
			float4 frag (v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv) * _Color;
			}
			ENDCG
        }
    }
}
