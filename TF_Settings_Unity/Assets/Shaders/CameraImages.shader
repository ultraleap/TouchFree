Shader "Unlit/CameraImages"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Colour", Color) = (1, 0, 0, 1)
        _HatchWidth ("Hatch Width", Range(0,0.1)) = 0.02
        _HatchScrollSpeed ("Scroll Speed", Range(0,0.1)) = 0.1

        // required for UI.Mask
         _StencilComp ("Stencil Comparison", Float) = 8
         _Stencil ("Stencil ID", Float) = 0
         _StencilOp ("Stencil Operation", Float) = 0
         _StencilWriteMask ("Stencil Write Mask", Float) = 255
         _StencilReadMask ("Stencil Read Mask", Float) = 255
         _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
		Tags
		{
		    "Queue" = "Transparent"
		}
        LOD 100

        // required for UI.Mask
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            // make fog work
            #pragma multi_compile LEAP_FORMAT_IR LEAP_FORMAT_RGB

            #include "Assets/ThirdParty/Ultraleap/Tracking/Core/Runtime/Resources/LeapCG.cginc"
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
            uniform float _threshold;
            float _HatchWidth;
            float _HatchScrollSpeed;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
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
