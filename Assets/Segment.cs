using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour
{
  // Segments still attached to the head, so counting them doesn't need a scene search
  private static readonly List<Segment> alive = new List<Segment>();

  public HingeJoint joint;
  private Rigidbody body;

  public static int AliveCount => alive.Count;
  public Rigidbody Body => body;

  private void Awake()
  {
    joint = GetComponent<HingeJoint>();
    body = GetComponent<Rigidbody>();
    if (CompareTag("Segment")) {
      alive.Add(this);
    }
  }

  private void OnDestroy()
  {
    alive.Remove(this);
  }

  private void Update()
  {
    // Fallback for joints broken by physics rather than by HeadCollider
    if (alive.Contains(this) && IsDetached()) {
      PruneDetached();
    }
  }

  private bool IsDetached()
  {
    return joint == null || joint.connectedBody == null || joint.connectedBody.CompareTag("SegmentDead");
  }

  private void Die()
  {
    alive.Remove(this);
    gameObject.tag = "SegmentDead";
    if (joint != null) {
      Destroy(joint);
    }
  }

  /// <summary>
  /// Kills every segment that is no longer connected to the head, all in the same frame
  /// </summary>
  public static void PruneDetached()
  {
    bool changed = true;
    while (changed) {
      changed = false;
      for (int i = alive.Count - 1; i >= 0; i--) {
        if (alive[i].IsDetached()) {
          alive[i].Die();
          changed = true;
        }
      }
    }
  }

  /// <summary>
  /// The last living segment in the chain, which no other segment hangs off
  /// </summary>
  public static Segment FindTail()
  {
    HashSet<Rigidbody> parents = new HashSet<Rigidbody>();
    foreach (Segment segment in alive) {
      if (segment.joint) {
        parents.Add(segment.joint.connectedBody);
      }
    }

    foreach (Segment segment in alive) {
      if (!parents.Contains(segment.body)) {
        return segment;
      }
    }

    return null;
  }
}
