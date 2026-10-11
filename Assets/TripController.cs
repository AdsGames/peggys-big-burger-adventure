using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// The longer Peggy gets, the trippier the world. Works out a single trip value (0 to 1) from
/// her length, plus short spikes from boosting and eating, and drives the post-processing,
/// the rainbow body and (via LampController) the rainbow lamps from it.
/// Lives on Peggy's head.
/// </summary>
public class TripController : MonoBehaviour
{
  public static TripController Instance { get; private set; }

  /// <summary>Current trip, 0 (sober) to 1 (full rainbow), already scaled by Trippiness</summary>
  public static float Trip => Instance ? Instance.trip * Trippiness : 0f;

  /// <summary>Shared rainbow phase, 0 to 1, so everything cycles in step</summary>
  public static float Hue => Instance ? Instance.hue : 0f;

  private const string TrippinessKey = "trippiness";
  private static float trippiness = -1f;

  /// <summary>Player setting, 0 to 1, that scales every effect. Saved between sessions.</summary>
  public static float Trippiness
  {
    get
    {
      if (trippiness < 0f)
        trippiness = PlayerPrefs.GetFloat(TrippinessKey, 1f);
      return trippiness;
    }
    set
    {
      trippiness = Mathf.Clamp01(value);
      PlayerPrefs.SetFloat(TrippinessKey, trippiness);
    }
  }

  [Header("Trip")]
  [Tooltip("Length at which the trip is at full strength")]
  public int fullTripLength = 15;
  [Tooltip("Trip at the very start, so there's a little colour from the first second")]
  public float minimumTrip = 0.35f;
  [Tooltip("How quickly the trip catches up with Peggy's length")]
  public float tripResponse = 1.5f;

  [Header("Rainbow")]
  [Tooltip("Rainbow cycles per second while sober")]
  public float hueSpeed = 0.05f;
  [Tooltip("Rainbow cycles per second at full trip")]
  public float maxHueSpeed = 0.5f;
  [Tooltip("Rainbow cycles faster by this much while boosting")]
  public float boostHueMultiplier = 3f;
  [Tooltip("Hue offset between neighbouring body segments")]
  public float segmentHueStep = 0.06f;

  [Header("Pulses")]
  [Tooltip("How quickly an eat or crash pulse fades, per second")]
  public float pulseDecay = 1.5f;

  [Header("Synthwave grade")]
  [Tooltip("Neon colours the screen tints cycle through. Make the ends match so the loop is seamless")]
  public Gradient neonPalette = DefaultNeonPalette();
  [Tooltip("Colour pushed into the shadows")]
  public Color shadowTone = new Color(0.25f, 0.1f, 0.85f);
  [Tooltip("Colour pushed into the highlights")]
  public Color highlightTone = new Color(1f, 0.3f, 0.55f);
  [Tooltip("How far the whole-screen hue swings at full trip, in degrees. Kept small so the neon palette survives")]
  [Range(0f, 180f)] public float hueSwing = 50f;
  [Tooltip("Film grain at the start and at full trip")]
  public Vector2 grain = new Vector2(0.25f, 0.5f);

  private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

  private float trip;
  private float pulse;
  private float boost;
  private float hue;
  private float wobble;

  private Movement movement;
  private Rigidbody head;
  private ButtScript butt;
  private Renderer buttRenderer;
  private MaterialPropertyBlock block; // created lazily, it isn't kept across script reloads in Play mode
  private readonly Dictionary<Rigidbody, Segment> segmentBehind = new Dictionary<Rigidbody, Segment>();

  private VolumeProfile profile;
  private Bloom bloom;
  private ColorAdjustments colour;
  private ChromaticAberration aberration;
  private LensDistortion distortion;
  private Vignette vignette;
  private SplitToning splitToning;
  private WhiteBalance whiteBalance;
  private FilmGrain filmGrain;

  /// <summary>
  /// Hot pink, sunset orange, magenta, purple, cyan and back to pink
  /// </summary>
  private static Gradient DefaultNeonPalette()
  {
    Color pink = new Color(1f, 0.165f, 0.427f);
    Gradient palette = new Gradient();
    palette.SetKeys(
      new[]
      {
        new GradientColorKey(pink, 0f),
        new GradientColorKey(new Color(1f, 0.424f, 0.067f), 0.2f),
        new GradientColorKey(new Color(0.827f, 0f, 0.773f), 0.4f),
        new GradientColorKey(new Color(0.478f, 0.016f, 0.922f), 0.6f),
        new GradientColorKey(new Color(0.02f, 0.851f, 0.91f), 0.8f),
        new GradientColorKey(pink, 1f),
      },
      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
    return palette;
  }

  private Color Neon(float position)
  {
    return neonPalette.Evaluate(Mathf.Repeat(position, 1f));
  }

  /// <summary>
  /// A short burst of extra trip, for eating a burger or crashing into the tail
  /// </summary>
  public static void Pulse(float amount = 1f)
  {
    if (Instance)
      Instance.pulse = Mathf.Min(Instance.pulse + amount, 1.5f);
  }

  private void OnEnable()
  {
    // OnEnable rather than Awake so Instance comes back after a script reload in Play mode
    Instance = this;
  }

  private void OnDestroy()
  {
    if (Instance == this)
      Instance = null;
    if (profile)
      Destroy(profile);
  }

  private void Start()
  {
    movement = GetComponent<Movement>();
    head = GetComponent<Rigidbody>();
    GameObject buttObject = GameObject.FindGameObjectWithTag("Butt");
    if (buttObject)
    {
      butt = buttObject.GetComponent<ButtScript>();
      buttRenderer = buttObject.GetComponent<Renderer>();
    }

    CreateVolume();

    // Every camera, not just Camera.main: the game camera prefab isn't tagged MainCamera
    foreach (Camera cam in Camera.allCameras)
      cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
  }

  private void CreateVolume()
  {
    GameObject volumeObject = new GameObject("Trip Volume");
    volumeObject.transform.SetParent(transform, false);
    Volume volume = volumeObject.AddComponent<Volume>();
    volume.isGlobal = true;
    volume.priority = 10;

    profile = ScriptableObject.CreateInstance<VolumeProfile>();
    bloom = profile.Add<Bloom>(true);
    colour = profile.Add<ColorAdjustments>(true);
    aberration = profile.Add<ChromaticAberration>(true);
    distortion = profile.Add<LensDistortion>(true);
    vignette = profile.Add<Vignette>(true);
    splitToning = profile.Add<SplitToning>(true);
    whiteBalance = profile.Add<WhiteBalance>(true);
    filmGrain = profile.Add<FilmGrain>(true);
    volume.sharedProfile = profile;

    bloom.scatter.value = 0.8f; // wide, soft neon glow
    bloom.highQualityFiltering.value = false; // cheaper, and WebGL needs every bit
    vignette.smoothness.value = 0.5f;
    whiteBalance.temperature.value = -15f; // cool, night-time base
    filmGrain.type.value = FilmGrainLookup.Medium3;
    filmGrain.response.value = 0.7f;
  }

  private void Update()
  {
    float dt = Time.deltaTime;

    float target = Mathf.Lerp(minimumTrip, 1f, Mathf.Clamp01((float)Segment.AliveCount / fullTripLength));
    trip = Mathf.Lerp(trip, target, 1f - Mathf.Exp(-tripResponse * dt));
    boost = Mathf.MoveTowards(boost, movement && movement.isBoosting ? 1f : 0f, dt * 4f);
    pulse = Mathf.MoveTowards(pulse, 0f, dt * pulseDecay);

    float cycleSpeed = Mathf.Lerp(hueSpeed, maxHueSpeed, trip) * Mathf.Lerp(1f, boostHueMultiplier, boost);
    hue = Mathf.Repeat(hue + cycleSpeed * dt, 1f);
    wobble += dt * Mathf.Lerp(0.5f, 2.5f, trip);

    float amount = Trippiness;
    float t = trip * amount;
    float b = boost * amount;
    float p = pulse * amount;

    UpdatePostProcessing(t, b, p);
    TintBody(Mathf.Lerp(0.5f, 1f, t) * amount);
  }

  private void UpdatePostProcessing(float t, float b, float p)
  {
    // With HDR on, lights and flares go well above 1 and bloom picks them up properly
    bloom.intensity.value = Mathf.Lerp(1f, 4f, t) + p * 3f;
    bloom.threshold.value = Mathf.Lerp(1f, 0.6f, t);
    // Always pink-ish glow, drifting round the neon palette as the trip builds
    bloom.tint.value = Color.Lerp(highlightTone, Neon(hue), t);

    colour.hueShift.value = Mathf.Sin(wobble * 0.5f) * hueSwing * t;
    colour.saturation.value = Mathf.Min(Mathf.Lerp(30f, 70f, t) + p * 20f, 100f);
    colour.contrast.value = Mathf.Lerp(15f, 35f, t);

    // The synthwave grade: indigo shadows, hot pink highlights, a magenta cast and grain
    splitToning.shadows.value = Color.Lerp(Color.grey, shadowTone, Mathf.Lerp(0.6f, 1f, t));
    splitToning.highlights.value = Color.Lerp(Color.grey, Color.Lerp(highlightTone, Neon(hue + 0.2f), t * 0.5f), Mathf.Lerp(0.5f, 0.9f, t));
    splitToning.balance.value = -15f; // favour the shadow tint, it's what sells the look
    whiteBalance.tint.value = Mathf.Lerp(15f, 35f, t);
    filmGrain.intensity.value = Mathf.Clamp01(Mathf.Lerp(grain.x, grain.y, t) + p * 0.2f);

    aberration.intensity.value = Mathf.Clamp01(Mathf.Lerp(0.15f, 0.8f, t) + b * 0.5f + p * 0.6f);

    // Breathing that grows with the trip, plus a punch inwards on boosts and pulses
    float lens = Mathf.Clamp(Mathf.Sin(wobble) * 0.35f * t - b * 0.4f - p * 0.35f, -1f, 1f);
    distortion.intensity.value = lens;
    distortion.scale.value = 1f - Mathf.Abs(lens) * 0.25f; // zoom in a touch to hide the stretched edges

    vignette.intensity.value = Mathf.Min(Mathf.Lerp(0.25f, 0.5f, t) + p * 0.2f, 1f);
    Color edge = Neon(hue + 0.5f) * Mathf.Lerp(0.3f, 0.8f, t);
    edge.a = 1f;
    vignette.color.value = edge;
  }

  /// <summary>
  /// Walks the chain from the head and tints each segment one step further round the rainbow
  /// </summary>
  private void TintBody(float blend)
  {
    segmentBehind.Clear();
    foreach (Segment segment in Segment.Alive)
    {
      if (segment.joint && segment.joint.connectedBody)
        segmentBehind[segment.joint.connectedBody] = segment;
    }

    Rigidbody current = head;
    int index = 0;
    while (current && segmentBehind.TryGetValue(current, out Segment next) && index < 1000)
    {
      index++;
      Tint(next.Renderer, index, blend);
      current = next.Body;
    }

    if (butt && butt.connected)
      Tint(buttRenderer, index + 1, blend);
  }

  private void Tint(Renderer target, int index, float blend)
  {
    if (!target)
      return;

    Color original = target.sharedMaterial ? target.sharedMaterial.GetColor(BaseColorId) : Color.white;
    Color rainbow = Color.HSVToRGB(Mathf.Repeat(hue - index * segmentHueStep, 1f), 0.85f, 1f);
    block ??= new MaterialPropertyBlock();
    target.GetPropertyBlock(block);
    block.SetColor(BaseColorId, Color.Lerp(original, rainbow, blend));
    target.SetPropertyBlock(block);
  }
}
