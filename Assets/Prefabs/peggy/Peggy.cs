using UnityEngine;

public class Peggy : MonoBehaviour
{
  public GameObject bodySegmentPrefab = null;
  public GameObject currentBodyStart = null;
  public GameObject itemParticle = null;
  public SoundEffectManager soundManager = null;

  // Distance between neighbouring segment centres, in the segment's local units.
  // Each segment's hinge anchor sits at local y = -1, so the next one hangs off y = +1.
  private const float SegmentSpacing = 2.0f;

  private Rigidbody rigidBody = null;
  private ButtScript butt = null;
  private HingeJoint buttJoint = null;
  private Rigidbody buttBody = null;
  private Vector3 buttScale;
  // Where the butt sits relative to the segment it hangs off, recorded from the prefab at start
  private Vector3 buttOffset;
  private Quaternion buttRotationOffset;
  public float breakTimer = 0.0f;

  private void Start()
  {
    rigidBody = GetComponent<Rigidbody>();
    butt = GameObject.FindGameObjectWithTag("Butt").GetComponent<ButtScript>();
    buttJoint = butt.GetComponent<HingeJoint>();
    buttBody = butt.GetComponent<Rigidbody>();

    Transform buttParent = buttJoint.connectedBody ? buttJoint.connectedBody.transform : currentBodyStart.transform;
    buttOffset = buttParent.InverseTransformPoint(butt.transform.position);
    buttRotationOffset = Quaternion.Inverse(buttParent.rotation) * butt.transform.rotation;

    // The chain is held together by hinges, not the hierarchy. Left as children, every
    // segment gets dragged rigidly whenever the head's rotation is set directly.
    currentBodyStart.transform.SetParent(transform.parent, true);
    butt.transform.SetParent(transform.parent, true);
    buttScale = butt.transform.localScale; // after detaching, so it's the real on-screen size
  }

  /// <summary>
  /// Grows Peggy by one segment at the back of the chain, like Snake: head -> ... -> tail -> new -> butt
  /// </summary>
  public void AddSegment()
  {
    Segment tail = Segment.ChainTail(rigidBody);
    GameObject newSegment;
    Rigidbody attachTo;

    if (tail) {
      Transform tailTransform = tail.transform;
      newSegment = Instantiate(bodySegmentPrefab, tailTransform.TransformPoint(Vector3.up * SegmentSpacing), tailTransform.rotation, tailTransform.parent);
      newSegment.transform.localScale = tailTransform.localScale;
      newSegment.name = "Body";
      attachTo = tail.Body;
    } else {
      // No body left at all, so start a fresh chain right behind the head
      newSegment = Instantiate(bodySegmentPrefab, transform);
      newSegment.transform.SetParent(transform.parent, true);
      newSegment.name = "FirstBody";
      attachTo = rigidBody;
    }

    Rigidbody newBody = newSegment.GetComponent<Rigidbody>();
    newBody.linearVelocity = attachTo.linearVelocity;
    Hinge(newSegment.GetComponent<HingeJoint>(), attachTo);

    // The legs always go on the very end, wherever they were hanging before
    if (butt.connected) {
      AttachButt(newBody);
    }
  }

  /// <summary>
  /// Connects a joint to a body at their current relative pose. Sets the anchor explicitly
  /// rather than trusting auto-configure to pick up a pose that was only just moved.
  /// </summary>
  private static void Hinge(HingeJoint joint, Rigidbody to)
  {
    joint.autoConfigureConnectedAnchor = false;
    joint.connectedBody = to;
    joint.connectedAnchor = to.transform.InverseTransformPoint(joint.transform.TransformPoint(joint.anchor));
  }

  /// <summary>
  /// Puts the butt in its usual spot behind a segment and hinges it there
  /// </summary>
  private void AttachButt(Rigidbody segment)
  {
    Transform parent = segment.transform;
    Vector3 position = parent.TransformPoint(buttOffset);
    Quaternion rotation = parent.rotation * buttRotationOffset;

    butt.transform.SetPositionAndRotation(position, rotation);
    buttBody.position = position;
    buttBody.rotation = rotation;
    buttBody.linearVelocity = segment.linearVelocity;
    buttBody.angularVelocity = Vector3.zero;
    Hinge(buttJoint, segment);
  }

  public void HandleButtCollision(Collider other)
  {
    if (breakTimer <= 2)
    {
      return;
    }

    Segment tail = Segment.ChainTail(rigidBody);
    if (!tail)
    {
      return;
    }

    butt.transform.localScale = buttScale;
    AttachButt(tail.Body);

    Effects.PlayOnce(itemParticle, transform.position);
    if (soundManager)
    {
      soundManager.playLegsRetreived();
    }
  }

  private void Update()
  {
    breakTimer += Time.deltaTime;

#if UNITY_EDITOR
    // Debug cheat, editor only
    if (GameInput.DebugAddSegment.WasPressedThisFrame()) {
      AddSegment();
    }
#endif
  }
}
