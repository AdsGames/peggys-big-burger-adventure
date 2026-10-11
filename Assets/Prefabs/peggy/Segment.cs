using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour
{
  // Segments still attached to the head, so counting them doesn't need a scene search
  private static readonly List<Segment> alive = new List<Segment>();

  public HingeJoint joint;
  private Rigidbody body;

  public static int AliveCount => alive.Count;
  public static IReadOnlyList<Segment> Alive => alive;
  public Rigidbody Body => body;
  public Renderer Renderer { get; private set; }

  private void Awake()
  {
    joint = GetComponent<HingeJoint>();
    body = GetComponent<Rigidbody>();
    Renderer = GetComponent<Renderer>();
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
  /// The real end of the chain, found by walking segment to segment from the head.
  /// Unlike FindTail, a stray segment that isn't connected to the head can never be picked.
  /// </summary>
  public static Segment ChainTail(Rigidbody head)
  {
    Dictionary<Rigidbody, Segment> behind = new Dictionary<Rigidbody, Segment>();
    foreach (Segment segment in alive) {
      if (segment.joint && segment.joint.connectedBody) {
        behind[segment.joint.connectedBody] = segment;
      }
    }

    Segment tail = null;
    Rigidbody current = head;
    // Bounded by the segment count, so a broken loop can't hang the game
    for (int i = 0; i <= alive.Count && behind.TryGetValue(current, out Segment next); i++) {
      tail = next;
      current = next.body;
    }
    return tail;
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
