Shader "Hidden/CubemapToEquirectangular"
{
    Properties
    {
        _MainTex ("Cubemap", CUBE) = "" {}
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
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            samplerCUBE _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // Convert UV to spherical coordinates
                float pi = 3.14159265359;
                float phi = uv.x * 2.0 * pi;
                float theta = uv.y * pi;

                // Convert spherical to cartesian coordinates
                float3 dir;
                dir.x = sin(phi) * sin(theta);
                dir.y = cos(theta);
                dir.z = cos(phi) * sin(theta);

                // Sample the cubemap
                return texCUBE(_MainTex, dir);
            }
            ENDCG
        }
    }
}