using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Finishing touches after Unity's Render Pipeline Converter has moved the project to URP:
/// Linear colour space, and URP settings that suit a small WebGL game.
/// Run from Peggy > Configure URP. Safe to run again.
/// </summary>
public static class URPSetup
{
  // Caps rather than fixed values, so higher quality levels never get bumped up
  private const float MaxShadowDistance = 80f;
  private const int MaxShadowCascades = 2;
  private const string CRTShaderPath = "Assets/Shaders/CRT.shader";
  private const string CRTMaterialPath = "Assets/Shaders/CRT.mat";

  [MenuItem("Peggy/Configure URP")]
  public static void Configure()
  {
    List<UniversalRenderPipelineAsset> assets = FindPipelineAssets();
    if (assets.Count == 0) {
      EditorUtility.DisplayDialog("Configure URP",
        "No URP asset is assigned yet.\n\nRun Window > Rendering > Render Pipeline Converter first, choose 'Built-in to URP', tick every converter, then Initialize and Convert.", "OK");
      return;
    }

    StringBuilder report = new StringBuilder();

    if (PlayerSettings.colorSpace != ColorSpace.Linear) {
      // Reimports textures, so it can take a minute
      PlayerSettings.colorSpace = ColorSpace.Linear;
      report.AppendLine("Colour space: Gamma -> Linear");
    }

    foreach (UniversalRenderPipelineAsset asset in assets) {
      // The trip's bloom needs HDR to glow properly; WebGL 2 handles it
      asset.supportsHDR = true;
      // Lamp lens flares sample depth to hide behind walls
      asset.supportsCameraDepthTexture = true;
      // Nothing samples this, and it costs an extra copy of the screen per frame
      asset.supportsCameraOpaqueTexture = false;
      asset.shadowDistance = Mathf.Min(asset.shadowDistance, MaxShadowDistance);
      asset.shadowCascadeCount = Mathf.Min(asset.shadowCascadeCount, MaxShadowCascades);

      // AllanLand has 128 lamp spotlights. Per-pixel, at the maximum per-object count, so the
      // ground (one big mesh) isn't limited to the converter's 1-4 lights.
      SerializedObject serialized = new SerializedObject(asset);
      serialized.FindProperty("m_AdditionalLightsRenderingMode").intValue = (int)LightRenderingMode.PerPixel;
      serialized.FindProperty("m_AdditionalLightsPerObjectLimit").intValue = UniversalRenderPipeline.maxPerObjectLights;
      serialized.ApplyModifiedPropertiesWithoutUndo();

      EditorUtility.SetDirty(asset);
      report.AppendLine($"{asset.name}: HDR on, MSAA {asset.msaaSampleCount}x, shadows {asset.shadowDistance}m / {asset.shadowCascadeCount} cascade(s), per-pixel lights");
    }

    // Forward+ drops the per-object light limit entirely, which suits lots of small lights.
    // On WebGL URP still caps visible lights at 32 per camera, so it keeps the closest ones.
    foreach (UniversalRendererData renderer in FindRenderers(assets)) {
      SerializedObject serialized = new SerializedObject(renderer);
      SerializedProperty mode = serialized.FindProperty("m_RenderingMode");
      if (mode.intValue != (int)RenderingMode.ForwardPlus) {
        mode.intValue = (int)RenderingMode.ForwardPlus;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(renderer);
        report.AppendLine($"{renderer.name}: Forward -> Forward+");
      }

      if (AddCRTPass(renderer)) {
        report.AppendLine($"{renderer.name}: added CRT pass (only visible where a scene sets _PeggyCRT, e.g. the menu)");
      }
    }

    AssetDatabase.SaveAssets();
    Debug.Log("Peggy URP configured\n" + report);
    EditorUtility.DisplayDialog("Configure URP", report.ToString(), "OK");
  }

  /// <summary>
  /// Adds the full-screen CRT pass (scanlines and curvature) to a renderer, once.
  /// Returns whether it was added.
  /// </summary>
  private static bool AddCRTPass(UniversalRendererData renderer)
  {
    Material material = AssetDatabase.LoadAssetAtPath<Material>(CRTMaterialPath);
    if (!material) {
      material = new Material(AssetDatabase.LoadAssetAtPath<Shader>(CRTShaderPath));
      AssetDatabase.CreateAsset(material, CRTMaterialPath);
    }

    foreach (ScriptableRendererFeature existing in renderer.rendererFeatures) {
      if (existing is FullScreenPassRendererFeature fullScreen && fullScreen.passMaterial == material) {
        return false;
      }
    }

    FullScreenPassRendererFeature feature = ScriptableObject.CreateInstance<FullScreenPassRendererFeature>();
    feature.name = "CRT";
    feature.passMaterial = material;
    feature.injectionPoint = FullScreenPassRendererFeature.InjectionPoint.AfterRenderingPostProcessing;
    feature.fetchColorBuffer = true;

    // Same steps as the renderer's "Add Renderer Feature" button: a sub-asset, the feature
    // list, and the map of sub-asset IDs the renderer uses to find its features
    AssetDatabase.AddObjectToAsset(feature, renderer);
    renderer.rendererFeatures.Add(feature);
    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out string _, out long localId);
    SerializedObject serialized = new SerializedObject(renderer);
    SerializedProperty map = serialized.FindProperty("m_RendererFeatureMap");
    map.arraySize++;
    map.GetArrayElementAtIndex(map.arraySize - 1).longValue = localId;
    serialized.ApplyModifiedPropertiesWithoutUndo();

    renderer.SetDirty();
    EditorUtility.SetDirty(renderer);
    return true;
  }

  /// <summary>
  /// Every URP asset in use: the project default plus any per-quality-level overrides
  /// </summary>
  private static List<UniversalRenderPipelineAsset> FindPipelineAssets()
  {
    List<UniversalRenderPipelineAsset> found = new List<UniversalRenderPipelineAsset>();
    AddIfURP(found, GraphicsSettings.defaultRenderPipeline);
    for (int i = 0; i < QualitySettings.names.Length; i++) {
      AddIfURP(found, QualitySettings.GetRenderPipelineAssetAt(i));
    }
    return found;
  }

  private static List<UniversalRendererData> FindRenderers(List<UniversalRenderPipelineAsset> assets)
  {
    List<UniversalRendererData> found = new List<UniversalRendererData>();
    foreach (UniversalRenderPipelineAsset asset in assets) {
      SerializedProperty list = new SerializedObject(asset).FindProperty("m_RendererDataList");
      for (int i = 0; i < list.arraySize; i++) {
        if (list.GetArrayElementAtIndex(i).objectReferenceValue is UniversalRendererData renderer && !found.Contains(renderer)) {
          found.Add(renderer);
        }
      }
    }
    return found;
  }

  private static void AddIfURP(List<UniversalRenderPipelineAsset> found, RenderPipelineAsset asset)
  {
    if (asset is UniversalRenderPipelineAsset urp && !found.Contains(urp)) {
      found.Add(urp);
    }
  }
}
