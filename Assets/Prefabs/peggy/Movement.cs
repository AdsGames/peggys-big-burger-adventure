using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives Peggy's head. She always runs forward; the player steers, handbrakes into tighter
/// turns, boosts and jumps. Handbrake and boost need the legs (butt) attached and share
/// meters that refill over time.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
  [Header("Running")]
  [Tooltip("Forward acceleration while running")]
  public float moveSpeed = 30f;
  [Tooltip("Forward acceleration while boosting")]
  public float boostSpeed = 60f;
  [Tooltip("Extra acceleration per body segment, so a longer Peggy keeps up")]
  public float segmentFactor = 0.2f;
  [Tooltip("Linear damping on the ground. Top speed is roughly acceleration / drag")]
  public float drag = 1f;

  [Header("Turning")]
  [Tooltip("Turn rate in degrees per second")]
  public float turnRate = 145f;
  [Tooltip("Turn rate in degrees per second while the handbrake is held")]
  public float handbrakeTurnRate = 345f;
  [Tooltip("How quickly turning reaches full rate and stops again. Higher feels snappier")]
  public float turnSharpness = 5f;
  [Tooltip("How fast Peggy tilts to match ramps, in degrees per second")]
  public float pitchSpeed = 360f;
  [Tooltip("Slopes steeper than this are treated as walls, not ramps")]
  public float maxPitch = 45f;

  [Header("Jumping")]
  [Tooltip("Roughly how high a jump goes on flat ground, in metres. The body chain pulls it down a little")]
  public float jumpHeight = 5f;

  [Header("Abilities")]
  public float maxhandBrakeMeter = 50f;
  public float maxBoostMeter = 50f;
  [Tooltip("Meter drained per second while the handbrake is held")]
  public float handbrakeDrainPerSecond = 50f;
  [Tooltip("Meter drained per second while boosting")]
  public float boostDrainPerSecond = 40f;
  [Tooltip("Meter refilled per second while not in use")]
  public float meterRefillPerSecond = 5f;
  public GameObject boostParticle;

  public SoundEffectManager soundManager;

  public bool isBoosting { get; private set; }
  public float currentHandBrakeMeter { get; private set; }
  public float currentBoostMeter { get; private set; }

  private Rigidbody rigidBody;
  private ButtScript butt;
  private GameObject currentBoostParticle;
  private bool jumpRequested;
  // Degrees per second. Kept by the script rather than the Rigidbody so the body chain
  // pulling on the head can't twist her off course.
  private float yawRate;

  // Ground triggers Peggy is currently inside. A set, so overlapping ground pieces don't
  // flip her airborne when she leaves just one of them.
  private readonly HashSet<Collider> groundContacts = new HashSet<Collider>();
  private Vector3 groundNormal = Vector3.up;
  private Vector3 groundNormalSum;
  private int groundNormalCount;

  private bool OnGround
  {
    get
    {
      groundContacts.RemoveWhere(c => c == null || !c.enabled);
      return groundContacts.Count > 0;
    }
  }

  private void Start()
  {
    rigidBody = GetComponent<Rigidbody>();
    // Rotation is driven entirely by Steer and ApplyRotation
    rigidBody.constraints |= RigidbodyConstraints.FreezeRotation;
    butt = GameObject.FindGameObjectWithTag("Butt").GetComponent<ButtScript>();
    currentHandBrakeMeter = maxhandBrakeMeter;
    currentBoostMeter = maxBoostMeter;
  }

  private void OnDisable()
  {
    SetBoosting(false);
    jumpRequested = false;
    yawRate = 0f;
  }

  private void Update()
  {
    // Button presses are per-frame, so catch them here and act on them in FixedUpdate
    if (GameInput.Jump.WasPressedThisFrame())
    {
      jumpRequested = true;
    }
  }

  private void FixedUpdate()
  {
    float dt = Time.fixedDeltaTime;
    bool onGround = OnGround;
    bool canUseAbilities = onGround && butt.connected;

    UpdateGroundNormal(onGround);

    bool handbraking = UpdateMeter(GameInput.Handbrake.IsPressed() && canUseAbilities, currentHandBrakeMeter, maxhandBrakeMeter, handbrakeDrainPerSecond, dt, out float handbrakeMeter);
    currentHandBrakeMeter = handbrakeMeter;

    bool boosting = UpdateMeter(GameInput.Boost.IsPressed() && canUseAbilities, currentBoostMeter, maxBoostMeter, boostDrainPerSecond, dt, out float boostMeter);
    currentBoostMeter = boostMeter;
    SetBoosting(boosting);

    rigidBody.linearDamping = onGround ? drag : 0f;

    if (onGround)
    {
      Run();
      Steer(handbraking ? handbrakeTurnRate : turnRate, dt);
      PlaySounds(handbraking);
    }

    ApplyRotation(onGround, dt);

    if (jumpRequested && onGround)
    {
      // Launch speed that reaches jumpHeight under gravity, independent of mass
      float jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * jumpHeight);
      rigidBody.AddForce(transform.up * jumpSpeed, ForceMode.VelocityChange);
    }
    jumpRequested = false;
  }

  /// <summary>
  /// Drains the meter while the ability is held and has charge, otherwise refills it.
  /// Returns whether the ability is active this step.
  /// </summary>
  private bool UpdateMeter(bool held, float meter, float max, float drainPerSecond, float dt, out float newMeter)
  {
    bool active = held && meter > 0f;
    // Holding the button on an empty meter doesn't refill it; you have to let go
    float rate = active ? -drainPerSecond : held ? 0f : meterRefillPerSecond;
    newMeter = Mathf.Clamp(meter + rate * dt, 0f, max);
    return active;
  }

  private void Run()
  {
    float acceleration = (isBoosting ? boostSpeed : moveSpeed) * (1.2f + Segment.AliveCount * segmentFactor);

    // Peggy's model faces down her local -Z
    rigidBody.AddRelativeForce(Vector3.back * acceleration, ForceMode.Acceleration);
  }

  private void Steer(float rate, float dt)
  {
    float targetYawRate = GameInput.Turn.ReadValue<float>() * rate;

    // Ease towards the target rate rather than applying torque, so turning feels the same
    // regardless of mass, inertia or how many segments are pulling on the head
    yawRate = Mathf.Lerp(yawRate, targetYawRate, 1f - Mathf.Exp(-turnSharpness * dt));
  }

  /// <summary>
  /// Turns Peggy by the current yaw rate, tilts the nose to follow ramps on the ground and
  /// levels out in the air
  /// </summary>
  private void ApplyRotation(bool onGround, float dt)
  {
    Vector3 euler = rigidBody.rotation.eulerAngles;
    float targetPitch = 0f;

    if (onGround)
    {
      // Negative X tilts the nose (local -Z) down, so measure the slope along local +Z
      Vector3 heading = Quaternion.Euler(0f, euler.y, 0f) * Vector3.forward;
      Vector3 alongSlope = Vector3.ProjectOnPlane(heading, groundNormal).normalized;
      float slopePitch = -Mathf.Asin(Mathf.Clamp(alongSlope.y, -1f, 1f)) * Mathf.Rad2Deg;
      if (Mathf.Abs(slopePitch) < maxPitch)
      {
        targetPitch = slopePitch;
      }
    }

    float pitch = Mathf.MoveTowardsAngle(euler.x, targetPitch, pitchSpeed * dt);
    rigidBody.rotation = Quaternion.Euler(pitch, euler.y + yawRate * dt, 0f);
  }

  private void UpdateGroundNormal(bool onGround)
  {
    if (groundNormalCount > 0)
    {
      groundNormal = (groundNormalSum / groundNormalCount).normalized;
    }
    else if (!onGround)
    {
      groundNormal = Vector3.up;
    }
    groundNormalSum = Vector3.zero;
    groundNormalCount = 0;
  }

  private void SetBoosting(bool boosting)
  {
    if (boosting && !currentBoostParticle && boostParticle)
    {
      currentBoostParticle = Instantiate(boostParticle, transform.position, Quaternion.identity);
    }
    else if (!boosting && currentBoostParticle)
    {
      Destroy(currentBoostParticle);
    }

    if (currentBoostParticle)
    {
      currentBoostParticle.transform.position = transform.position;
    }
    isBoosting = boosting;
  }

  private void PlaySounds(bool handbraking)
  {
    if (!soundManager)
    {
      return;
    }

    soundManager.playStep(rigidBody.linearVelocity.magnitude / 50f);
    if (handbraking)
    {
      soundManager.playHandbrake();
    }
    if (isBoosting)
    {
      soundManager.playBoost();
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Ground"))
    {
      groundContacts.Add(other);
    }
  }

  private void OnTriggerExit(Collider other)
  {
    groundContacts.Remove(other);
  }

  private void OnCollisionStay(Collision collision)
  {
    if (!collision.gameObject.CompareTag("Ground"))
    {
      return;
    }

    for (int i = 0; i < collision.contactCount; i++)
    {
      Vector3 normal = collision.GetContact(i).normal;
      // Ignore walls and ceilings so they don't tilt Peggy
      if (Vector3.Angle(normal, Vector3.up) < maxPitch)
      {
        groundNormalSum += normal;
        groundNormalCount++;
      }
    }
  }
}
