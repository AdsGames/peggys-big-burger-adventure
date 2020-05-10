using UnityEngine;

public class Peggy : MonoBehaviour
{
  public GameObject bodySegmentPrefab = null;
  public GameObject currentBodyStart = null;
  public float breakTimer=0;
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
   public void handleButtCollision(Collider other){
        if(breakTimer>2){
        GameObject[] list = GameObject.FindGameObjectsWithTag("Segment");
        GameObject oldest = list[0];
        for(int i=0; i<list.Length; i++){
            if(oldest.GetComponent<Segment>().getAge()>list[i].GetComponent<Segment>().getAge())
                oldest = list[i];
        }
        other.gameObject.transform.position = oldest.transform.position;
        other.gameObject.transform.rotation = oldest.transform.rotation;


        other.gameObject.GetComponent<HingeJoint>().connectedBody = oldest.GetComponent<Rigidbody>();

        //GameObject new_particle = Instantiate(explodeParticle);
        //new_particle.transform.position = other.transform.position;
        }
    }

  private void Update()
  {
      breakTimer+=Time.deltaTime;
    if (Input.GetKeyDown(KeyCode.F)) {
      AddSegment();
    }
  }
}
