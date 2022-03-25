Shader "TNTC/WrapLambert"{
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
    }

    SubShader{
        Tags { "RenderType"="Opaque"}
        LOD 100
        
        CGPROGRAM
        #pragma surface surf WrapLambert addshadow
        
        sampler2D _MainTex;
        sampler2D _NormalMap;
        float4 _Color;

        struct Input {
            float2 uv_MainTex;
            float2 uv_NormalMap;
        };

        half4 LightingWrapLambert (SurfaceOutput s, half3 lightDir, half atten) {
            half NdotL = dot (s.Normal, lightDir);
            half diff = NdotL * 0.5 + 0.5;
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * clamp(diff * atten, 0.4, 1);
            c.a = s.Alpha;
            return c;
        }

        void surf (Input IN, inout SurfaceOutput o) {
            half4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_NormalMap));
            o.Albedo = c.rgb * _Color;
            o.Alpha =_Color.a;
        }
        ENDCG
        
    }

    // Fallback "VertexLit"
}