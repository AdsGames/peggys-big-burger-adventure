using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// A spinning searchlight. Creates and owns its own spot light and lens flare from the settings
/// below, so lamps are configured entirely here; the generated light isn't saved and can't be
/// edited directly. Runs in the editor too, so changes show up in the Scene view straight away.
/// </summary>
[ExecuteAlways]
public class LampController : MonoBehaviour
{
  private const string LightName = "Lamp Light (generated)";

  [Header("Light")]
  public Color colour = new Color(1f, 0.632496f, 0f);
  [Min(0f)] public float intensity = 1500f;
  [Min(0f)] public float range = 200f;
  [Range(1f, 179f)] public float spotAngle = 70f;
  [Tooltip("Angle of the fully bright centre of the cone. Lower is softer")]
  [Range(0f, 179f)] public float innerSpotAngle = 30f;
  [Tooltip("Height of the light above the lamp's origin")]
  public float height = 5f;
  [Tooltip("Degrees the beam points down from horizontal. Higher lights a pool around the lamp")]
  [Range(0f, 90f)] public float tilt = 35f;
  public LightShadows shadows = LightShadows.None;

  [Header("Lens Flare")]
  public bool lensFlare = true;
  [Min(0f)] public float flareIntensity = 1f;
  [Min(0f)] public float flareScale = 1f;
  [Tooltip("Flares fade out with distance and are gone beyond this")]
  [Min(0f)] public float flareDistance = 200f;
  [Tooltip("Hide the flare behind walls and other geometry. Needs the camera depth texture")]
  public bool flareOcclusion = true;

  [Header("Spin")]
  [Tooltip("Degrees per frame (at 60 fps) the beam turns while Peggy is short")]
  public float rotationSpeed = 0.1f;
  [Tooltip("Extra degrees per frame (at 60 fps) for every body segment")]
  public float spinPerSegment = 0.1f;

  [Header("Rainbow")]
  [Tooltip("Hue offset per metre of the lamp's position, so colour ripples across the level")]
  public float hueSpread = 0.0015f;
  [Tooltip("How rainbow the lamp is while sober. It reaches full rainbow at full trip")]
  public float minimumRainbow = 0.4f;
  [Tooltip("Extra brightness at full trip, as a multiple of intensity")]
  public float tripBrightness = 6f;
  [Tooltip("Degrees the beam widens by at full trip")]
  public float tripSpread = 30f;

  private static LensFlareDataSRP sharedFlare;

  private Light lamp;
  private LensFlareComponentSRP flare;
  private float huePhase;
  private float yaw;

  public Light Light => lamp;

  private void OnEnable()
  {
    EnsureLight();
    ApplySettings();

    Vector3 position = transform.position;
    huePhase = (position.x + position.z) * hueSpread;
    // Start every lamp facing a different way so the beams don't sweep in lockstep
    yaw = Mathf.Repeat(position.x * 7.3f + position.z * 3.1f, 360f);
    ApplyRotation();
  }

  private void OnValidate()
  {
    innerSpotAngle = Mathf.Min(innerSpotAngle, spotAngle);
    if (lamp)
    {
      ApplySettings();
      ApplyRotation();
    }
  }

  private void Update()
  {
    if (!lamp || !Application.isPlaying)
      return;

    float degreesPerFrame = rotationSpeed + Segment.AliveCount * spinPerSegment;
    yaw = Mathf.Repeat(yaw + degreesPerFrame * 60f * Time.deltaTime, 360f);
    ApplyRotation();

    float trip = TripController.Trip;
    float rainbow = Mathf.Lerp(minimumRainbow, 1f, trip) * TripController.Trippiness;
    Color target = Color.HSVToRGB(Mathf.Repeat(TripController.Hue + huePhase, 1f), 1f, 1f);
    lamp.color = Color.Lerp(colour, target, rainbow);
    lamp.intensity = intensity * (1f + trip * tripBrightness);
    lamp.spotAngle = Mathf.Min(spotAngle + trip * tripSpread, 179f);
    lamp.innerSpotAngle = Mathf.Min(innerSpotAngle + trip * tripSpread, lamp.spotAngle);
    if (flare)
      flare.intensity = flareIntensity * (1f + trip * tripBrightness);
  }

  private void ApplyRotation()
  {
    lamp.transform.localRotation = Quaternion.Euler(tilt, yaw, 0f);
  }

  /// <summary>
  /// Finds the generated light (it survives script reloads) or makes a new one
  /// </summary>
  private void EnsureLight()
  {
    if (!lamp)
    {
      Transform existing = transform.Find(LightName);
      GameObject lightObject = existing ? existing.gameObject : new GameObject(LightName);
      // Never saved into the scene or prefab, and greyed out so edits go to this component
      lightObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
      lightObject.transform.SetParent(transform, false);

      lamp = lightObject.GetComponent<Light>();
      if (!lamp)
        lamp = lightObject.AddComponent<Light>();
    }

    if (!flare)
    {
      flare = lamp.GetComponent<LensFlareComponentSRP>();
      if (!flare)
        flare = lamp.gameObject.AddComponent<LensFlareComponentSRP>();
    }
  }

  private void ApplySettings()
  {
    lamp.type = LightType.Spot;
    lamp.transform.localPosition = Vector3.up * height;
    lamp.color = colour;
    lamp.intensity = intensity;
    lamp.range = range;
    lamp.spotAngle = spotAngle;
    lamp.innerSpotAngle = innerSpotAngle;
    lamp.shadows = shadows;

    flare.enabled = lensFlare;
    flare.lensFlareData = SharedFlare();
    flare.intensity = flareIntensity;
    flare.scale = flareScale;
    flare.maxAttenuationDistance = flareDistance;
    flare.maxAttenuationScale = flareDistance;
    // Glow like a bulb from every angle, not only when looking down the beam
    flare.attenuationByLightShape = false;
    flare.useOcclusion = flareOcclusion;
    flare.occlusionRadius = 0.5f;
  }

  /// <summary>
  /// One procedural flare shared by every lamp: a soft core glow, a wide anamorphic streak,
  /// a halo ring and a trail of hexagon ghosts. All tinted by the light, so they go rainbow too.
  /// </summary>
  private static LensFlareDataSRP SharedFlare()
  {
    if (sharedFlare)
      return sharedFlare;

    sharedFlare = ScriptableObject.CreateInstance<LensFlareDataSRP>();
    sharedFlare.name = "Lamp Flare (generated)";
    sharedFlare.hideFlags = HideFlags.DontSave;
    sharedFlare.elements = new[]
    {
      FlareElement(SRPLensFlareType.Circle, 1.2f, 1.4f, Vector2.one, 0.15f),
      FlareElement(SRPLensFlareType.Circle, 0.6f, 1f, new Vector2(10f, 0.12f), 0.5f),
      Ring(),
      Ghosts(),
    };
    return sharedFlare;
  }

  private static LensFlareDataElementSRP FlareElement(SRPLensFlareType type, float intensity, float scale, Vector2 size, float fallOff)
  {
    return new LensFlareDataElementSRP
    {
      flareType = type,
      localIntensity = intensity,
      uniformScale = scale,
      sizeXY = size,
      fallOff = fallOff,
      edgeOffset = 0.1f,
      modulateByLightColor = true,
      blendMode = SRPLensFlareBlendMode.Additive,
      tint = Color.white,
    };
  }

  private static LensFlareDataElementSRP Ring()
  {
    LensFlareDataElementSRP ring = FlareElement(SRPLensFlareType.Ring, 0.25f, 3f, Vector2.one, 0.6f);
    ring.ringThickness = 0.08f;
    return ring;
  }

  private static LensFlareDataElementSRP Ghosts()
  {
    LensFlareDataElementSRP ghosts = FlareElement(SRPLensFlareType.Polygon, 0.15f, 0.35f, Vector2.one, 0.4f);
    ghosts.sideCount = 6;
    ghosts.sdfRoundness = 0.3f;
    ghosts.allowMultipleElement = true;
    ghosts.count = 4;
    ghosts.distribution = SRPLensFlareDistribution.Uniform;
    ghosts.position = -0.4f;
    ghosts.lengthSpread = 1.2f;
    return ghosts;
  }
}
