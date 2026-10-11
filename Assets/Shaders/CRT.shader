// Old-TV look as a full-screen pass: barrel curvature, scanlines, a faint RGB aperture mask
// and darkened corners. Every effect is scaled by the global _PeggyCRT (0 to 1), which
// scenes set themselves (see MenuStage), so the pass is invisible wherever it's left at 0.
Shader "Peggy/CRT"
{
  Properties
  {
    _Curvature ("Curvature", Range(0, 0.3)) = 0.06
    _Scanlines ("Scanline Strength", Range(0, 1)) = 0.18
    _ScanlineCount ("Scanline Count", Float) = 360
    _Mask ("RGB Mask Strength", Range(0, 1)) = 0.08
    _EdgeDarkening ("Edge Darkening", Range(0, 1)) = 0.4
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
    ZWrite Off
    ZTest Always
    Cull Off
    Blend Off

    Pass
    {
      Name "CRT"

      HLSLPROGRAM
      #pragma vertex Vert
      #pragma fragment Frag
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

      float _Curvature;
      float _Scanlines;
      float _ScanlineCount;
      float _Mask;
      float _EdgeDarkening;
      float _PeggyCRT;

      half4 Frag(Varyings input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float amount = saturate(_PeggyCRT);

        // Barrel distortion: push the picture outwards, more towards the corners
        float2 centred = input.texcoord * 2.0 - 1.0;
        centred *= 1.0 + dot(centred, centred) * _Curvature * amount;
        float2 uv = centred * 0.5 + 0.5;
        if (any(uv < 0.0) || any(uv > 1.0))
          return half4(0.0, 0.0, 0.0, 1.0);

        half3 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;

        float scan = sin(uv.y * _ScanlineCount * PI) * 0.5 + 0.5;
        color *= 1.0 - _Scanlines * amount * (1.0 - scan);

        // Alternating red, green, blue tints across pixel columns, like a shadow mask
        int column = (int)fmod(input.texcoord.x * _ScreenParams.x, 3.0);
        half3 mask = column == 0 ? half3(1.0, 0.75, 0.75) : column == 1 ? half3(0.75, 1.0, 0.75) : half3(0.75, 0.75, 1.0);
        color *= lerp(half3(1.0, 1.0, 1.0), mask, _Mask * amount);

        float2 edge = uv * (1.0 - uv);
        float corners = pow(saturate(edge.x * edge.y * 20.0), 0.3);
        color *= lerp(1.0, corners, _EdgeDarkening * amount);

        return half4(color, 1.0);
      }
      ENDHLSL
    }
  }
}
