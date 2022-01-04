Shader "ConnectDots/FrameShader"
{
    Properties
    {
        _Color("_FrameColor",Color) = (1.,1.,1.,0.)
        _FillAmount("_FrameFillAmount", Range(0.0,1.0)) = 0.
        _Width("_FrameWidth", Range(0.0,1.0)) = 0.
        _EdgeSharpness("_EdgeSharpness", Range(0.0,1.0)) = 0.
        [Toggle]_FlipY ("Flip Y", Float) = 0
        [Toggle]_FlipX ("Flip X", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Cull Off
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

            float4 _Color;
            float _FillAmount;
            float _Width;
            float _EdgeSharpness;
            float _FlipX;
            float _FlipY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                if(_FlipY > 0.) uv.y = 1.-uv.y;
                if(_FlipX > 0.) uv.x = 1.-uv.x;
                
                float edgeOffset = _EdgeSharpness;
                
                //Rectangle Mask
                float mask = smoothstep( _Width, _Width+edgeOffset,max(uv.x, uv.y));;
    
                //Mask the edges
                float maskX = smoothstep(_Width,_Width+edgeOffset,uv.x);
                float maskY = smoothstep(_Width,_Width+edgeOffset,uv.y);
    
    
                float y = 1.-uv.y;
                float x = 1.-uv.x;
                //create a slanted edge at the corner
                float greaterCoordinate = max(x,y);
                float xVal = .5 - .5 * greaterCoordinate;
                float yVal = .5 + .5 * greaterCoordinate;

                //Combine the two mask values and offset by the x and y values so the corner isn't brigh
                float gradient = yVal*maskX + xVal*maskY - .5*(yVal+xVal) * maskX*maskY;

                float distance = smoothstep(_FillAmount+edgeOffset,_FillAmount,gradient);

                float4 col = float4(_Color.xyz,mask*distance);
                
                return col;
            }
            ENDCG
        }
    }
}
