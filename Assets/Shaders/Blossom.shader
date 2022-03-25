Shader "TNTC/Blossom"{
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _ColorEnd ("Color End", Color) = (1, 1, 1, 1)
        _ColorCenter ("Color Center", Color) = (1, 1, 1, 1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Amount("Amount", Range(-1.0, 1.0)) = 0
        _Radius("Radius", float) = 0
        [MaterialToggle] _Leaf("isLeaf", Float) = 0.1
        [MaterialToggle] _Horizontal("Horizontal", Float) = 0.1
    }

    SubShader{
        Tags { "RenderType"="Opaque"}
        LOD 100
        
        CGPROGRAM
        #pragma surface surf WrapLambert addshadow
        
        sampler2D _MainTex;
        sampler2D _NormalMap;
        float4 _Color;
        float4 _ColorEnd;
        float4 _ColorCenter;
        float _Amount;
        float _Radius;
        float _Leaf;
        float _Horizontal;

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
            float2 p = (2 * IN.uv_MainTex) - 1;
            o.Albedo =  lerp(lerp(_Color, _ColorEnd, c.g), _ColorCenter, c.r);

            if(_Leaf == 0){ 
                o.Alpha = 1 - (p.x * p.x + p.y * p.y) + _Amount;
            }else{
                if(_Horizontal == 1){
                    o.Alpha = 1 - saturate(IN.uv_MainTex.x - _Amount);
                }else{
                    o.Alpha = 1 - saturate(IN.uv_MainTex.y - _Amount);
                }
            }
            
            clip (o.Alpha - .5);
        }
        ENDCG
    }

}