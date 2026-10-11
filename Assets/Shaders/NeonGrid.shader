// Synthwave floor: glowing grid lines on a dark base, scrolling towards the camera and
// fading into a horizon colour. Works in world space (an endless floor) or object space
// (a grid that turns with the object, like the menu turntable).
Shader "Peggy/Neon Grid"
{
  Properties
  {
    [HDR] _LineColorA ("Line Colour A", Color) = (4, 0.35, 2.4, 1)
    [HDR] _LineColorB ("Line Colour B", Color) = (0.25, 2.2, 4, 1)
    _BaseColor ("Base Colour", Color) = (0.03, 0.005, 0.07, 1)
    _HorizonColor ("Horizon Colour", Color) = (0.22, 0.02, 0.28, 1)
    _CellSize ("Cell Size", Float) = 1.5
    _LineWidth ("Line Width (pixels)", Range(0.5, 4)) = 1.5
    _Glow ("Glow", Range(0, 2)) = 0.5
    _ScrollSpeed ("Scroll Speed", Float) = 0.8
    _FadeDistance ("Fade Distance", Float) = 60
    _ColorCycle ("Colour Cycle Speed", Float) = 0.15
    [Toggle] _WorldSpace ("World Space", Float) = 1
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

    Pass
    {
      Name "NeonGrid"
      Tags { "LightMode" = "UniversalForward" }

      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma multi_compile_fog
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

      CBUFFER_START(UnityPerMaterial)
        half4 _LineColorA;
        half4 _LineColorB;
        half4 _BaseColor;
        half4 _HorizonColor;
        float _CellSize;
        float _LineWidth;
        float _Glow;
        float _ScrollSpeed;
        float _FadeDistance;
        float _ColorCycle;
        float _WorldSpace;
      CBUFFER_END

      struct Attributes
      {
        float4 positionOS : POSITION;
      };

      struct Varyings
      {
        float4 positionCS : SV_POSITION;
        float3 positionWS : TEXCOORD0;
        float3 positionOS : TEXCOORD1;
        float fogFactor : TEXCOORD2;
      };

      Varyings vert(Attributes input)
      {
        Varyings output;
        VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
        output.positionCS = positions.positionCS;
        output.positionWS = positions.positionWS;
        output.positionOS = input.positionOS.xyz;
        output.fogFactor = ComputeFogFactor(positions.positionCS.z);
        return output;
      }

      half4 frag(Varyings input) : SV_Target
      {
        float2 coord = lerp(input.positionOS.xz, input.positionWS.xz, _WorldSpace) / _CellSize;
        coord.y -= _Time.y * _ScrollSpeed;

        // Distance to the nearest line in each axis, 0 right on a line
        float2 cell = abs(frac(coord - 0.5) - 0.5);
        // Anti-aliased lines a fixed number of pixels wide, whatever the distance
        float2 pixels = cell / max(fwidth(coord) * _LineWidth, 1e-5);
        float lines = 1.0 - saturate(min(pixels.x, pixels.y));
        float glow = exp(-min(cell.x, cell.y) * 12.0) * _Glow;

        float fade = saturate(1.0 - distance(input.positionWS, _WorldSpaceCameraPos) / _FadeDistance);
        float cycle = sin((coord.y * 0.1 + _Time.y * _ColorCycle) * 6.2831853) * 0.5 + 0.5;
        half3 lineColor = lerp(_LineColorA.rgb, _LineColorB.rgb, cycle);

        half3 color = _BaseColor.rgb + lineColor * (lines + glow) * fade;
        color = lerp(_HorizonColor.rgb, color, saturate(fade * 1.5));
        color = MixFog(color, input.fogFactor);
        return half4(color, 1.0);
      }
      ENDHLSL
    }
  }
}
