using UnityEngine;

public class Segment : MonoBehaviour
{
  private HingeJoint joint;

  private void Start()
  {
    joint = GetComponent<HingeJoint>();
  }

  private void Update()
  {
    if (joint == null) {
      return;
    }

    if (joint.connectedBody == null || joint.connectedBody.gameObject.tag == "SegmentDead") {
      gameObject.tag = "SegmentDead";
      Destroy(joint);
    }
  }
}
