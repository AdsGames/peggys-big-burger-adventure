// A glowing column of light for marking things from a distance. Additive, so it never hides
// what's behind it. Brightest at the base, fading upwards, with bands scrolling up the beam.
// Made for Unity's cylinder mesh, whose height runs from -1 to 1 on the local Y axis.
Shader "Peggy/Neon Beam"
{
  Properties
  {
    [HDR] _Color ("Colour", Color) = (0.3, 4, 1.2, 1)
    _Stripes ("Stripes", Float) = 12
    _Speed ("Scroll Speed", Float) = 1.5
    _Fade ("Top Fade", Range(0.1, 4)) = 1.5
  }

  SubShader
  {
    Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

    Pass
    {
      Name "NeonBeam"
      Tags { "LightMode" = "UniversalForward" }
      Blend One One
      ZWrite Off
      Cull Off

      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

      CBUFFER_START(UnityPerMaterial)
        half4 _Color;
        float _Stripes;
        float _Speed;
        float _Fade;
      CBUFFER_END

      struct Attributes
      {
        float4 positionOS : POSITION;
        float3 normalOS : NORMAL;
      };

      struct Varyings
      {
        float4 positionCS : SV_POSITION;
        float3 normalWS : TEXCOORD0;
        float3 viewWS : TEXCOORD1;
        float height : TEXCOORD2;
      };

      Varyings vert(Attributes input)
      {
        Varyings output;
        VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
        output.positionCS = positions.positionCS;
        output.normalWS = TransformObjectToWorldNormal(input.normalOS);
        output.viewWS = GetWorldSpaceViewDir(positions.positionWS);
        output.height = saturate(input.positionOS.y * 0.5 + 0.5);
        return output;
      }

      half4 frag(Varyings input) : SV_Target
      {
        // Soft core: bright where the surface faces the camera, fading out at the silhouette
        float facing = abs(dot(normalize(input.normalWS), normalize(input.viewWS)));
        float core = facing * facing;
        float stripes = 0.65 + 0.35 * sin((input.height * _Stripes - _Time.y * _Speed) * 6.2831853);
        float fade = pow(1.0 - input.height, _Fade) * saturate(input.height * 30.0);
        return half4(_Color.rgb * core * stripes * fade, 1.0);
      }
      ENDHLSL
    }
  }
}
