Shader "Unlit/CameraImages"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Colour", Color) = (1, 0, 0, 1)
        _threshold ("Threshold", range(0,1)) = 0.5
        _HatchWidth ("Hatch Width", Range(0,0.1)) = 0.02
        _HatchScrollSpeed ("Scroll Speed", Range(0,0.1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile LEAP_FORMAT_IR LEAP_FORMAT_RGB

            #include "Assets/Plugins/LeapMotion/Core/Resources/LeapCG.cginc"
            #include "UnityCG.cginc"

            uniform float _LeapGlobalColorSpaceGamma;

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _threshold;
            float _HatchWidth;
            float _HatchScrollSpeed;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float timeOffset = _Time * _HatchScrollSpeed;
                float uvSumTime = i.uv.x + (i.uv.y * 2) + timeOffset;
                float4 color = _Color * step(0.5, fmod(uvSumTime * (1/_HatchWidth), 1) );

                float4 images = float4(LeapGetUVColor(i.uv), 1);
                float4 threshold = step(images, (_threshold));
                float4 highlight = color * (1 - threshold);
                
                return (threshold * images) + highlight;
            }
            ENDCG
        }
    }
}
