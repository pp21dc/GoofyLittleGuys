Shader "Custom/WaterShader"
{
    Properties
    {
        _ColourMap ("Colour Map", 2D) = "white" {}
        _Tint ("Tint Colour", Color) = (.5,.5,.5,1)
        _Reflectivity ( "Reflectivity", Range(0,1)) = 0.5
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _DisplacementMap ("Displacement Map", 2D) = "black" {}
        _Speed ("Speed", Range(0,1)) = 0.005
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert alpha

        sampler2D _ColourMap;
        sampler2D _NormalMap;
        sampler2D _DisplacementMap;
        float4 _Tint;
        float _Reflectivity;
        float _Speed;

        struct Input
        {
            float2 uv_ColourMap;
            float2 uv_NormalMap;
            float2 uv_DisplacementMap;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            float2 animatedUVs = IN.uv_NormalMap + _Time.yy * _Speed;
            float3 normal = UnpackNormal(tex2D(_NormalMap, animatedUVs));
            float displacement = tex2D(_DisplacementMap, animatedUVs).r;
            float3 colourMap = tex2D(_ColourMap, IN.uv_ColourMap).rgb;
            float wave = sin(IN.uv_ColourMap.x * 10.0 + _Time.y * _Speed) * 0.05;
            float3 normalWave = float3(0, 0, wave);
            o.Normal = normalize(normal + normalWave);
            o.Albedo = colourMap * _Tint.rgb; // Multiply the texture colour by the tint colour
            o.Alpha = 0.7 - displacement * 0.2; // Base transparency of 70% adjusted by the displacement
        }
        ENDCG
    }
    FallBack "Diffuse"
}