using TMPro;
using UnityEngine;

/// <summary>
/// Drives the in-game HUD: length, best length, speed, ability meters and leg status.
/// Built by Peggy > Rebuild UI, which wires up the references below.
/// </summary>
public class GameHUD : MonoBehaviour
{
  [Header("Stats")]
  public TMP_Text lengthText;
  public TMP_Text bestText;
  public TMP_Text speedText;
  public Color slowColour = new Color(1f, 0.957f, 0.878f);
  public Color fastColour = new Color(0.902f, 0.224f, 0.275f);
  [Tooltip("Speed at which the speed readout is fully fastColour")]
  public float fastSpeed = 50f;

  [Header("Abilities")]
  public RectTransform boostFill;
  public RectTransform handbrakeFill;
  [Tooltip("Faded out while the legs are missing, since abilities need them")]
  public CanvasGroup abilityBars;
  public TMP_Text legsText;
  public GameObject legsWarning;
  public Color attachedColour = new Color(0.482f, 0.788f, 0.314f);
  public Color missingColour = new Color(0.902f, 0.224f, 0.275f);

  private Movement movement;
  private Rigidbody head;
  private ButtScript butt;
  private int best;

  private void Start()
  {
    GameObject headObject = GameObject.FindGameObjectWithTag("Head");
    movement = headObject.GetComponent<Movement>();
    head = headObject.GetComponent<Rigidbody>();
    butt = GameObject.FindGameObjectWithTag("Butt").GetComponent<ButtScript>();
    best = BestLength.Value;
  }

  private void Update()
  {
    int length = Segment.AliveCount;
    if (length > best)
    {
      best = length;
      BestLength.Submit(best);
    }
    lengthText.text = length.ToString();
    bestText.text = best.ToString();

    float speed = head.linearVelocity.magnitude;
    speedText.text = Mathf.RoundToInt(speed).ToString();
    speedText.color = Color.Lerp(slowColour, fastColour, speed / fastSpeed);

    SetFill(boostFill, movement.currentBoostMeter / movement.maxBoostMeter);
    SetFill(handbrakeFill, movement.currentHandBrakeMeter / movement.maxhandBrakeMeter);

    bool legs = butt.connected;
    legsText.text = legs ? "ATTACHED" : "MISSING";
    legsText.color = legs ? attachedColour : missingColour;
    abilityBars.alpha = legs ? 1f : 0.35f;
    legsWarning.SetActive(!legs);
  }

  private static void SetFill(RectTransform fill, float amount)
  {
    amount = Mathf.Clamp01(amount);
    // Hide when nearly empty, a sliver of sliced sprite looks broken
    fill.gameObject.SetActive(amount > 0.02f);
    fill.anchorMax = new Vector2(amount, 1f);
  }
}
