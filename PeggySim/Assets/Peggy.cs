using UnityEngine;

public class Peggy : MonoBehaviour
{
  public GameObject bodySegmentPrefab = null;
  public GameObject currentBodyStart = null;
  private Rigidbody rigidBody = null;

  private void Start()
  {
    rigidBody = GetComponent<Rigidbody>();
  }

  public void AddSegment()
  {
    GameObject newSegment = Instantiate(bodySegmentPrefab, transform);
    newSegment.name = "FirstBody";
    newSegment.GetComponent<HingeJoint>().connectedBody = rigidBody;

    currentBodyStart.transform.position = currentBodyStart.transform.position + transform.forward * 1.0f;
    currentBodyStart.GetComponent<HingeJoint>().connectedBody = newSegment.GetComponent<Rigidbody>();
    currentBodyStart.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    currentBodyStart.name = "Body";
    currentBodyStart = newSegment;
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.F)) {
      AddSegment();
    }
  }
}
