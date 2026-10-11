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
  public float breakTimer = 0.0f;

  private void Start()
  {
    rigidBody = GetComponent<Rigidbody>();
    butt = GameObject.FindGameObjectWithTag("Butt").GetComponent<ButtScript>();
    buttJoint = butt.GetComponent<HingeJoint>();

    // The chain is held together by hinges, not the hierarchy. Left as children, every
    // segment gets dragged rigidly whenever the head's rotation is set directly.
    currentBodyStart.transform.SetParent(transform.parent, true);
    butt.transform.SetParent(transform.parent, true);
  }

  /// <summary>
  /// Grows Peggy by one segment at the back of the chain, like Snake: head -> ... -> tail -> new -> butt
  /// </summary>
  public void AddSegment()
  {
    Segment tail = Segment.FindTail();
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

    // Slide the butt back by one segment and move it onto the new tail.
    // Position first so the hinge auto-configures its anchor from the new pose.
    if (tail && butt.connected && buttJoint.connectedBody == tail.Body) {
      butt.transform.position += newSegment.transform.position - tail.transform.position;
      Physics.SyncTransforms();
      buttJoint.connectedBody = newSegment.GetComponent<Rigidbody>();
    }

    newSegment.GetComponent<HingeJoint>().connectedBody = attachTo;
  }

  public void HandleButtCollision(Collider other)
  {
    if (breakTimer <= 2)
    {
      return;
    }

    Segment tail = Segment.FindTail();
    if (!tail)
    {
      return;
    }

    other.gameObject.transform.localScale = new Vector3(3.91f, 3.91f, 3.91f);
    other.gameObject.transform.position = tail.transform.position;
    other.gameObject.transform.rotation = tail.transform.rotation;
    other.gameObject.GetComponent<HingeJoint>().connectedBody = tail.Body;

    GameObject new_particle = Instantiate(itemParticle);
    new_particle.transform.position = transform.position;
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
