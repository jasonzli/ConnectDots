Shader "ConnectDots/DotShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DotColor ("Dot Color", Color) = (1.,1.,1.,1.)
        _DotRadius ("Radius", Range(0.0,0.5)) = .25
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _DotRadius;
            float4 _DotColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                //draw a circle distance field from the center
                float d = distance(i.uv,float2(.5,.5)) - _DotRadius;
                //1.5/R.y is Fabrice Neyret's trick for smoothstep AA
                float circleEdge = smoothstep(1.5/_ScreenParams.y, -1.5/_ScreenParams.y, d);
                float4 finalColor = _DotColor * circleEdge;
                
                return finalColor;
            }
            ENDCG
        }
    }
}
