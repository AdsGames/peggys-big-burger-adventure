using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peggy : MonoBehaviour
{

    public GameObject bodySegmentPrefab;
    public GameObject currentBodyStart;
    public GameObject explodeParticle;
    public GameObject itemParticle;


    private float speed = 10;
    private float rotation_speed = 2;

    private Random rnd;

    
    public Rigidbody m_Rigidbody;
    public GameObject Food;
    Vector3 m_EulerAngleVelocity;
    // Start is called before the first frame update
    void Start()
    {

        //Fetch the Rigidbody from the GameObject with this script attached
        m_Rigidbody = GetComponent<Rigidbody>();

        rnd = new Random();



    }
    public void handleCollision(GameObject other){
        if(other.tag == "Food"){
            addSegment();
            Destroy(other);
 
            GameObject new_particle = Instantiate(itemParticle);
            new_particle.transform.position = other.transform.position;


      
        }
    }
    void addSegment(){
            Vector3 pos = currentBodyStart.transform.position +  transform.forward * 1.0f;
            currentBodyStart.transform.position = pos;
            GameObject new_segment = Instantiate(bodySegmentPrefab, transform);
            new_segment.name = "FirstBody";
            new_segment.GetComponent<HingeJoint>().connectedBody = m_Rigidbody;

           currentBodyStart.GetComponent<HingeJoint>().connectedBody = new_segment.GetComponent<Rigidbody>();
           
           currentBodyStart.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
           currentBodyStart.name = "Body";
           currentBodyStart = new_segment;

    }
 

    // Update is called once per frame
    void Update()
    {
        if(transform.GetChild(0).GetComponent<HeadCollider>().dead){
           // print("you died sir");
        };

        if (Input.GetKeyDown(KeyCode.F)){
           
            addSegment();
        }

    }
}
