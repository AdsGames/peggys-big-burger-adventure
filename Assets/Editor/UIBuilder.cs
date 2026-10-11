using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Rebuilds the game's UI (splash, menu, HUD) with TextMeshPro and one shared style.
/// Run from Peggy > Rebuild UI. Safe to run again after tweaking the values here.
/// </summary>
public static class UIBuilder
{
  private const string GameUIPath = "Assets/GameUI.prefab";
  private const string MenuScenePath = "Assets/Scenes/Menu.unity";
  private const string SplashScenePath = "Assets/Scenes/Splash.unity";
  private const string SplashImagePath = "Assets/Prefabs/splash/SplashImage.prefab";
  private const string PeggyPrefabPath = "Assets/Peggy.prefab";
  private const string BurgerPrefabPath = "Assets/Prefabs/burger/Fortnut Borger.prefab";
  private const string GrassMaterialPath = "Assets/Materials/MenuGrass.mat";
  private const string IntroSoundPath = "Assets/Sounds/intro.mp3";
  private const string BungeePath = "Assets/Fonts/Bungee-Regular.ttf";
  private const string BungeeAssetPath = "Assets/Fonts/Bungee-Regular SDF.asset";
  private const string ShadowMaterialPath = "Assets/Fonts/Bungee-Regular SDF Shadow.mat";
  private const string OutlineMaterialPath = "Assets/Fonts/Bungee-Regular SDF Outline.mat";

  private static readonly Vector2 ReferenceResolution = new Vector2(1920, 1080);

  // Burger palette
  private static readonly Color Cream = new Color(1f, 0.957f, 0.878f);
  private static readonly Color Mustard = new Color(1f, 0.788f, 0.235f);
  private static readonly Color Ketchup = new Color(0.902f, 0.224f, 0.275f);
  private static readonly Color Sky = new Color(0.298f, 0.788f, 0.941f);
  private static readonly Color Ink = new Color(0.11f, 0.08f, 0.13f);
  private static readonly Color Card = new Color(0.09f, 0.07f, 0.12f, 0.82f);
  private static readonly Color Track = new Color(1f, 1f, 1f, 0.12f);
  private static readonly Color Muted = new Color(1f, 0.957f, 0.878f, 0.6f);

  private static TMP_FontAsset display;
  private static TMP_FontAsset body;
  private static Material shadowMaterial;
  private static Material outlineMaterial;
  private static Sprite rounded;
  private static Sprite circle;

  [MenuItem("Peggy/Rebuild UI")]
  public static void RebuildAll()
  {
    if (EditorApplication.isPlaying)
    {
      EditorUtility.DisplayDialog("Rebuild UI", "Exit Play Mode first.", "OK");
      return;
    }
    if (TMP_Settings.instance == null)
    {
      EditorUtility.DisplayDialog("Rebuild UI",
        "TextMeshPro essentials aren't imported yet.\n\nRun Window > TextMeshPro > Import TMP Essential Resources, then try again.", "OK");
      return;
    }
    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
    {
      return;
    }

    string returnTo = SceneManager.GetActiveScene().path;
    LoadStyle();

    BuildGameUI();
    BuildMenu();
    BuildSplash();

    AssetDatabase.SaveAssets();
    if (!string.IsNullOrEmpty(returnTo))
    {
      EditorSceneManager.OpenScene(returnTo);
    }
    Debug.Log("Peggy UI rebuilt: GameUI.prefab, Menu.unity and Splash.unity");
  }

  // ---------------------------------------------------------------- style

  private static void LoadStyle()
  {
    rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

    display = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BungeeAssetPath);
    if (!display)
    {
      Font font = AssetDatabase.LoadAssetAtPath<Font>(BungeePath);
      display = TMP_FontAsset.CreateFontAsset(font);
      display.name = "Bungee-Regular SDF";
      AssetDatabase.CreateAsset(display, BungeeAssetPath);
      display.atlasTextures[0].name = "Bungee-Regular SDF Atlas";
      AssetDatabase.AddObjectToAsset(display.atlasTextures[0], display);
      display.material.name = "Bungee-Regular SDF Material";
      AssetDatabase.AddObjectToAsset(display.material, display);
      AssetDatabase.SaveAssets();
    }

    // Long-form text reads better in a plain sans than in an all-caps display font
    body = TMP_Settings.defaultFontAsset ? TMP_Settings.defaultFontAsset : display;

    shadowMaterial = MaterialPreset(ShadowMaterialPath, mat =>
    {
      mat.EnableKeyword(ShaderUtilities.Keyword_Underlay);
      mat.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.6f));
      mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.6f);
      mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.6f);
      mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.3f);
    });
    outlineMaterial = MaterialPreset(OutlineMaterialPath, mat =>
    {
      mat.EnableKeyword(ShaderUtilities.Keyword_Outline);
      mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.25f);
      mat.SetColor(ShaderUtilities.ID_OutlineColor, Ink);
      mat.EnableKeyword(ShaderUtilities.Keyword_Underlay);
      mat.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.5f));
      mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.8f);
      mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.4f);
    });
  }

  private static Material MaterialPreset(string path, System.Action<Material> configure)
  {
    Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
    if (!mat)
    {
      mat = new Material(display.material);
      AssetDatabase.CreateAsset(mat, path);
    }
    mat.CopyPropertiesFromMaterial(display.material);
    mat.shaderKeywords = display.material.shaderKeywords;
    configure(mat);
    EditorUtility.SetDirty(mat);
    return mat;
  }

  // ---------------------------------------------------------------- HUD

  private static void BuildGameUI()
  {
    GameObject root = PrefabUtility.LoadPrefabContents(GameUIPath);
    try
    {
      Intro oldIntro = root.GetComponentInChildren<Intro>(true);
      List<AudioClip> voiceClips = oldIntro ? new List<AudioClip>(oldIntro.voiceClips) : new List<AudioClip>();

      ClearChildren(root.transform);
      foreach (MonoBehaviour behaviour in root.GetComponents<MonoBehaviour>())
      {
        if (behaviour is GameHUD)
        {
          Object.DestroyImmediate(behaviour);
        }
      }
      GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
      ConfigureCanvas(root);

      GameHUD hud = root.AddComponent<GameHUD>();
      RectTransform safe = Stretch("HUD", root.transform, 40, 40, 40, 40);

      // Top left: length and best
      RectTransform stats = Rect("Stats", safe, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(400, 170));
      Panel(stats, Card);
      Label(Rect("LengthLabel", stats, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(28, -22), new Vector2(200, 30)),
        "LENGTH", 24, Mustard, TextAlignmentOptions.TopLeft, display).characterSpacing = 6;
      hud.lengthText = Label(Rect("Length", stats, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(26, -50), new Vector2(220, 100)),
        "1", 88, Cream, TextAlignmentOptions.TopLeft, display, shadowMaterial);
      Label(Rect("BestLabel", stats, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-28, -22), new Vector2(140, 30)),
        "BEST", 24, Muted, TextAlignmentOptions.TopRight, display).characterSpacing = 6;
      hud.bestText = Label(Rect("Best", stats, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-28, -56), new Vector2(160, 60)),
        "1", 52, Mustard, TextAlignmentOptions.TopRight, display, shadowMaterial);

      // Bottom centre: speed
      RectTransform speed = Rect("Speed", safe, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(260, 140));
      Panel(speed, Card);
      hud.speedText = Label(Rect("Value", speed, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(0, 96)),
        "0", 80, Cream, TextAlignmentOptions.Center, display, shadowMaterial);
      Label(Rect("Label", speed, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(0, 30)),
        "SPEED", 22, Muted, TextAlignmentOptions.Center, display).characterSpacing = 6;

      // Bottom left: legs and ability meters
      RectTransform abilities = Rect("Abilities", safe, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(480, 230));
      Panel(abilities, Card);
      Label(Rect("LegsLabel", abilities, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(28, -24), new Vector2(160, 32)),
        "LEGS", 24, Mustard, TextAlignmentOptions.TopLeft, display).characterSpacing = 6;
      hud.legsText = Label(Rect("Legs", abilities, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-28, -24), new Vector2(240, 32)),
        "ATTACHED", 24, Cream, TextAlignmentOptions.TopRight, display);

      RectTransform bars = Rect("Bars", abilities, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(-56, 140));
      hud.abilityBars = bars.gameObject.AddComponent<CanvasGroup>();
      hud.boostFill = Meter(bars, "Boost", "BOOST", "ALT", Mustard, 70);
      hud.handbrakeFill = Meter(bars, "Handbrake", "HANDBRAKE", "CTRL", Sky, 0);

      RectTransform warning = Rect("LegsWarning", abilities, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(0, 52));
      Panel(warning, Ketchup);
      Label(Stretch("Text", warning, 16, 16, 0, 0),
        "GRAB YOUR LEGS TO USE ABILITIES!", 22, Cream, TextAlignmentOptions.Center, display);
      hud.legsWarning = warning.gameObject;

      // Centre: countdown
      RectTransform countdown = Rect("Countdown", root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(900, 400));
      AudioSource voice = countdown.gameObject.AddComponent<AudioSource>();
      voice.playOnAwake = false;
      Intro intro = countdown.gameObject.AddComponent<Intro>();
      intro.label = Label(Stretch("Label", countdown, 0, 0, 0, 0), "", 280, Cream, TextAlignmentOptions.Center, display, outlineMaterial);
      intro.voiceClips = voiceClips;

      PrefabUtility.SaveAsPrefabAsset(root, GameUIPath);
    }
    finally
    {
      PrefabUtility.UnloadPrefabContents(root);
    }
  }

  /// <summary>
  /// A labelled meter bar. Returns the fill rect, whose anchorMax.x is the fill amount.
  /// </summary>
  private static RectTransform Meter(RectTransform parent, string name, string title, string key, Color colour, float y)
  {
    RectTransform row = Rect(name, parent, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, y), new Vector2(0, 62));
    Label(Rect("Title", row, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(300, 30)),
      title, 24, Cream, TextAlignmentOptions.TopLeft, display);
    Label(Rect("Key", row, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), Vector2.zero, new Vector2(120, 30)),
      key, 20, Muted, TextAlignmentOptions.TopRight, display);

    RectTransform track = Rect("Track", row, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 22));
    Panel(track, Track).pixelsPerUnitMultiplier = 1f;
    RectTransform fill = Stretch("Fill", track, 0, 0, 0, 0);
    Panel(fill, colour).pixelsPerUnitMultiplier = 1f;
    return fill;
  }

  // ---------------------------------------------------------------- menu

  private static void BuildMenu()
  {
    Scene scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
    UpgradeEventSystems();

    GameObject canvas = FindRoot(scene, "Menu Canvas");
    Texture title = canvas.GetComponentsInChildren<RawImage>(true).Select(r => r.texture).FirstOrDefault(t => t);
    ClearChildren(canvas.transform);
    ConfigureCanvas(canvas);

    // Everything sits in a left-hand column, leaving the right for the turntable stage
    const float column = 0.3f;

    // Title
    RectTransform titleRect = Rect("Title", canvas.transform, new Vector2(column, 1), new Vector2(column, 1), new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(900, 300));
    RawImage titleImage = titleRect.gameObject.AddComponent<RawImage>();
    titleImage.texture = title;
    titleImage.raycastTarget = false;
    if (title)
    {
      titleRect.sizeDelta = new Vector2(900, 900f * title.height / title.width);
    }

    // Buttons
    RectTransform buttons = Rect("Buttons", canvas.transform, new Vector2(column, 0.5f), new Vector2(column, 0.5f), new Vector2(0.5f, 1), new Vector2(0, 20), new Vector2(460, 400));
    Button start = MakeButton("Start Game", buttons, "PLAY", new Vector2(460, 110), 0, Mustard, Ink, 52);
    Button help = MakeButton("Open Help", buttons, "HOW TO PLAY", new Vector2(460, 86), -136, Card, Cream, 34);
    Button about = MakeButton("Open About", buttons, "ABOUT", new Vector2(460, 86), -240, Card, Cream, 34);

    // Best length, filled in by MenuStartController
    TextMeshProUGUI best = Label(Rect("Best", buttons, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -360), new Vector2(800, 50)),
      "", 30, Mustard, TextAlignmentOptions.Center, display, shadowMaterial);
    best.characterSpacing = 4;

    // Dialogs
    Button helpClose;
    GameObject helpDialog = Dialog(canvas.transform, "Help Dialog", "HOW TO PLAY", HelpText(), out helpClose);
    Button aboutClose;
    GameObject aboutDialog = Dialog(canvas.transform, "About Dialog", "ABOUT", AboutText(), out aboutClose);

    // Wire up the existing controllers
    GameObject controller = FindRoot(scene, "Controller");
    MenuStartController starter = controller.GetComponent<MenuStartController>();
    if (!starter)
    {
      starter = controller.AddComponent<MenuStartController>();
    }
    starter.startButton = start;
    starter.bestText = best;
    EditorUtility.SetDirty(starter);

    // Intro jingle, played once when the menu opens
    AudioSource jingle = controller.GetComponent<AudioSource>();
    if (!jingle)
    {
      jingle = controller.AddComponent<AudioSource>();
    }
    jingle.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(IntroSoundPath);
    jingle.playOnAwake = true;
    jingle.loop = false;
    jingle.spatialBlend = 0f;

    MenuDialogController[] dialogs = controller.GetComponents<MenuDialogController>();
    for (int i = dialogs.Length; i < 2; i++)
    {
      controller.AddComponent<MenuDialogController>();
    }
    dialogs = controller.GetComponents<MenuDialogController>();
    SetDialog(dialogs[0], aboutDialog, about, aboutClose);
    SetDialog(dialogs[1], helpDialog, help, helpClose);

    BuildMenuStage(scene);

    EditorSceneManager.MarkSceneDirty(scene);
    EditorSceneManager.SaveScene(scene);
  }

  private static void SetDialog(MenuDialogController dialog, GameObject canvas, Button open, Button close)
  {
    dialog.canvas = canvas;
    dialog.openButton = open;
    dialog.closeButton = close;
    EditorUtility.SetDirty(dialog);
  }

  /// <summary>
  /// Peggy spinning on a patch of grass while burgers rain down, to the right of the menu.
  /// Replaces the old static Peggy and burger props.
  /// </summary>
  private static void BuildMenuStage(Scene scene)
  {
    foreach (GameObject old in scene.GetRootGameObjects())
    {
      if (old.name == "Menu Stage" || old.name == "Peggy" || old.name == "Fortnut Borger")
      {
        Object.DestroyImmediate(old);
      }
    }

    GameObject stageObject = new GameObject("Menu Stage");
    SceneManager.MoveGameObjectToScene(stageObject, scene);
    stageObject.transform.position = new Vector3(3f, 0f, 0f);

    GameObject turntableObject = new GameObject("Turntable");
    turntableObject.transform.SetParent(stageObject.transform, false);
    Rigidbody turntable = turntableObject.AddComponent<Rigidbody>();
    turntable.isKinematic = true;
    turntable.interpolation = RigidbodyInterpolation.Interpolate;

    // A flat disc whose top surface sits at the stage's origin
    GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    grass.name = "Grass";
    grass.transform.SetParent(turntableObject.transform, false);
    grass.transform.localPosition = new Vector3(0f, -0.15f, 0f);
    grass.transform.localScale = new Vector3(6f, 0.15f, 6f);
    Object.DestroyImmediate(grass.GetComponent<Collider>());
    MeshCollider grassCollider = grass.AddComponent<MeshCollider>();
    grassCollider.convex = true;
    grass.GetComponent<MeshRenderer>().sharedMaterial = GrassMaterial();

    MenuStage stage = stageObject.AddComponent<MenuStage>();
    stage.peggyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PeggyPrefabPath);
    stage.burgerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BurgerPrefabPath);
    stage.turntable = turntable;
    stage.ground = grassCollider;

    // Look slightly down at the stage, which lands right of centre
    Camera camera = FindRoot(scene, "Main Camera").GetComponent<Camera>();
    camera.transform.SetPositionAndRotation(new Vector3(0f, 3f, -11f), Quaternion.Euler(8f, 0f, 0f));
    camera.fieldOfView = 40f;
    EditorUtility.SetDirty(camera.gameObject);
  }

  private static Material GrassMaterial()
  {
    Material grass = AssetDatabase.LoadAssetAtPath<Material>(GrassMaterialPath);
    if (!grass)
    {
      grass = new Material(Shader.Find("Standard"));
      AssetDatabase.CreateAsset(grass, GrassMaterialPath);
    }
    grass.color = new Color(0.36f, 0.64f, 0.25f);
    grass.SetFloat("_Glossiness", 0.1f);
    EditorUtility.SetDirty(grass);
    return grass;
  }

  private static string HelpText()
  {
    string heading = "<b><color=#FFC93C><size=110%>";
    string end = "</size></color></b>";
    return
      heading + "GOAL" + end + "\n" +
      "Peggy always runs forward, like in Snake. Eat burgers to grow longer, and don't run into your own body.\n\n" +
      heading + "CONTROLS" + end + "\n" +
      "Steer<pos=45%><b>A / D</b>  or  <b>Arrow keys</b>\n" +
      "Jump<pos=45%><b>Space</b>\n" +
      "Handbrake (sharp turns)<pos=45%><b>Ctrl</b>  or  <b>Left mouse</b>\n" +
      "Boost<pos=45%><b>Alt</b>  or  <b>Right mouse</b>\n" +
      "Gamepad<pos=45%>Stick to steer, <b>A</b> handbrake, <b>B</b> boost, <b>Y</b> jump\n\n" +
      heading + "ABILITIES" + end + "\n" +
      "Handbrake and boost only work while Peggy has her legs. Lose them and you'll have to run back and grab them.";
  }

  private static string AboutText()
  {
    return
      "Made lovingly by <b>Lyra Stobo</b>, <b>Allan Legemaate</b> and <b>Danny Van Stemp</b> for <color=#FFC93C>TOJam: Hindsight</color>.\n\n" +
      "We hope you enjoy it!";
  }

  /// <summary>
  /// A full-screen dimmed overlay with a centred card, a title, body text and a close button. Starts hidden.
  /// </summary>
  private static GameObject Dialog(Transform parent, string name, string title, string text, out Button close)
  {
    RectTransform overlay = Stretch(name, parent, 0, 0, 0, 0);
    Image dim = overlay.gameObject.AddComponent<Image>();
    dim.color = new Color(0f, 0f, 0f, 0.65f);
    dim.raycastTarget = true; // blocks clicks to the menu behind

    RectTransform card = Rect("Card", overlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 760));
    Panel(card, new Color(Ink.r, Ink.g, Ink.b, 0.97f));

    Label(Rect("Title", card, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -48), new Vector2(-120, 80)),
      title, 64, Mustard, TextAlignmentOptions.Top, display, shadowMaterial);

    TextMeshProUGUI bodyText = Label(Stretch("Body", card, 80, 80, 160, 60), text, 30, Cream, TextAlignmentOptions.TopLeft, body);
    bodyText.textWrappingMode = TextWrappingModes.Normal;
    bodyText.lineSpacing = 8;
    bodyText.richText = true;

    RectTransform closeRect = Rect("Close Button", card, new Vector2(1, 1), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(-48, -48), new Vector2(72, 72));
    Image closeImage = closeRect.gameObject.AddComponent<Image>();
    closeImage.sprite = circle;
    close = closeRect.gameObject.AddComponent<Button>();
    close.targetGraphic = closeImage;
    close.colors = Tint(Ketchup);
    // Arrow keys mustn't wander off to the menu buttons hidden behind the dialog
    close.navigation = new Navigation { mode = Navigation.Mode.None };
    closeRect.gameObject.AddComponent<SelectedPop>();
    Label(Stretch("Text", closeRect, 0, 0, 0, 4), "X", 34, Cream, TextAlignmentOptions.Center, display);

    overlay.gameObject.SetActive(false);
    return overlay.gameObject;
  }

  private static Button MakeButton(string name, Transform parent, string text, Vector2 size, float y, Color background, Color foreground, float fontSize)
  {
    RectTransform rect = Rect(name, parent, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, y), size);
    Image image = Panel(rect, Color.white);
    image.raycastTarget = true;
    Button button = rect.gameObject.AddComponent<Button>();
    button.targetGraphic = image;
    button.colors = Tint(background);
    rect.gameObject.AddComponent<SelectedPop>();
    Label(Stretch("Text", rect, 0, 0, 0, 4), text, fontSize, foreground, TextAlignmentOptions.Center, display).characterSpacing = 4;
    return button;
  }

  private static ColorBlock Tint(Color colour)
  {
    ColorBlock colours = ColorBlock.defaultColorBlock;
    colours.normalColor = colour;
    colours.highlightedColor = Color.Lerp(colour, Color.white, 0.2f);
    colours.selectedColor = colours.highlightedColor;
    colours.pressedColor = Color.Lerp(colour, Color.black, 0.2f);
    colours.disabledColor = new Color(colour.r, colour.g, colour.b, 0.4f);
    colours.fadeDuration = 0.08f;
    return colours;
  }

  // ---------------------------------------------------------------- splash

  private static void BuildSplash()
  {
    // The splash image prefab carries its own copy of the controller, on top of the one on
    // the canvas. One is enough, and prefab instances can't have components destroyed directly.
    GameObject splashImage = PrefabUtility.LoadPrefabContents(SplashImagePath);
    try
    {
      foreach (SplashController extra in splashImage.GetComponentsInChildren<SplashController>(true))
      {
        Object.DestroyImmediate(extra);
      }
      PrefabUtility.SaveAsPrefabAsset(splashImage, SplashImagePath);
    }
    finally
    {
      PrefabUtility.UnloadPrefabContents(splashImage);
    }

    Scene scene = EditorSceneManager.OpenScene(SplashScenePath, OpenSceneMode.Single);
    UpgradeEventSystems();

    GameObject canvas = FindRoot(scene, "Canvas");
    ConfigureCanvas(canvas);

    SplashController controller = canvas.GetComponent<SplashController>();
    RawImage image = canvas.GetComponentInChildren<RawImage>(true);

    if (image)
    {
      RectTransform rect = image.rectTransform;
      rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
      rect.anchoredPosition = Vector2.zero;
      if (image.texture)
      {
        rect.sizeDelta = new Vector2(1100, 1100f * image.texture.height / image.texture.width);
      }
      Color colour = image.color;
      colour.a = 0f; // faded in by SplashController
      image.color = colour;
    }
    if (controller)
    {
      controller.image = image;
      EditorUtility.SetDirty(controller);
    }

    EditorSceneManager.MarkSceneDirty(scene);
    EditorSceneManager.SaveScene(scene);
  }

  // ---------------------------------------------------------------- helpers

  /// <summary>
  /// The old StandaloneInputModule reads the legacy Input Manager, which is now disabled
  /// </summary>
  private static void UpgradeEventSystems()
  {
    foreach (EventSystem system in Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include))
    {
      foreach (StandaloneInputModule old in system.GetComponents<StandaloneInputModule>())
      {
        Object.DestroyImmediate(old);
      }
      InputSystemUIInputModule module = system.GetComponent<InputSystemUIInputModule>();
      if (!module)
      {
        module = system.gameObject.AddComponent<InputSystemUIInputModule>();
      }
      module.AssignDefaultActions();
      EditorUtility.SetDirty(system.gameObject);
    }
  }

  private static void ConfigureCanvas(GameObject canvasObject)
  {
    Canvas canvas = canvasObject.GetComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.pixelPerfect = false;

    CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
    if (!scaler)
    {
      scaler = canvasObject.AddComponent<CanvasScaler>();
    }
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = ReferenceResolution;
    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    scaler.matchWidthOrHeight = 0.5f;

    if (!canvasObject.GetComponent<GraphicRaycaster>())
    {
      canvasObject.AddComponent<GraphicRaycaster>();
    }
  }

  private static GameObject FindRoot(Scene scene, string name)
  {
    GameObject found = scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
    if (!found)
    {
      throw new System.InvalidOperationException($"Couldn't find '{name}' in {scene.path}");
    }
    return found;
  }

  private static void ClearChildren(Transform parent)
  {
    for (int i = parent.childCount - 1; i >= 0; i--)
    {
      Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }
  }

  private static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
  {
    GameObject go = new GameObject(name, typeof(RectTransform));
    go.layer = LayerMask.NameToLayer("UI");
    RectTransform rect = (RectTransform)go.transform;
    rect.SetParent(parent, false);
    rect.anchorMin = anchorMin;
    rect.anchorMax = anchorMax;
    rect.pivot = pivot;
    rect.anchoredPosition = position;
    rect.sizeDelta = size;
    return rect;
  }

  private static RectTransform Stretch(string name, Transform parent, float left, float right, float top, float bottom)
  {
    RectTransform rect = Rect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
    rect.offsetMin = new Vector2(left, bottom);
    rect.offsetMax = new Vector2(-right, -top);
    return rect;
  }

  private static Image Panel(RectTransform rect, Color colour)
  {
    Image image = rect.gameObject.AddComponent<Image>();
    image.sprite = rounded;
    image.type = Image.Type.Sliced;
    image.pixelsPerUnitMultiplier = 0.35f; // bigger, softer corners than the default skin
    image.color = colour;
    image.raycastTarget = false;
    return image;
  }

  private static TextMeshProUGUI Label(RectTransform rect, string text, float size, Color colour, TextAlignmentOptions alignment, TMP_FontAsset font, Material material = null)
  {
    TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
    label.font = font;
    if (material)
    {
      label.fontSharedMaterial = material;
    }
    label.text = text;
    label.fontSize = size;
    label.color = colour;
    label.alignment = alignment;
    label.textWrappingMode = TextWrappingModes.NoWrap;
    label.overflowMode = TextOverflowModes.Overflow;
    label.raycastTarget = false;
    return label;
  }
}
