using UnityEngine;

public class HeadCollider : MonoBehaviour
{
  private Peggy parent;

  private void Start()
  {
    parent = transform.parent.GetComponent<Peggy>();
  }

  private void OnTriggerEnter(Collider other)
  {
    if (!other || !parent) {
      Debug.Log("Other or parent is null!");
      return;
    }

    if (other.tag == "Segment" && other.name != "FirstBody") {
      other.GetComponent<HingeJoint>().connectedBody = null;
    }

    if (other.tag == "Food") {
      parent.AddSegment();
      Destroy(other.gameObject);
    }
  }
}
