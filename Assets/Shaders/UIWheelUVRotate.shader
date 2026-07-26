Shader "VertigoDemo/UI/WheelUVRotate"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Angle ("Angle Radians", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _Angle;

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = input.texcoord;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float2 uv = input.texcoord - center;

                float s = sin(_Angle);
                float c = cos(_Angle);

                float2 rotatedUv;
                rotatedUv.x = uv.x * c - uv.y * s;
                rotatedUv.y = uv.x * s + uv.y * c;
                rotatedUv += center;

                if (rotatedUv.x < 0 || rotatedUv.x > 1 || rotatedUv.y < 0 || rotatedUv.y > 1)
                    return fixed4(0, 0, 0, 0);

                fixed4 color = tex2D(_MainTex, rotatedUv) * input.color;
                return color;
            }
            ENDCG
        }
    }
}