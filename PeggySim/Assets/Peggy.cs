using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peggy : MonoBehaviour
{

    public GameObject bodySegmentPrefab;
    public GameObject currentBodyStart;

    private float speed = 10;
    private float rotation_speed = 2;

    
    public Rigidbody m_Rigidbody;
    Vector3 m_EulerAngleVelocity;
    // Start is called before the first frame update
    void Start()
    {

        //Fetch the Rigidbody from the GameObject with this script attached
        m_Rigidbody = GetComponent<Rigidbody>();



    }
 

    // Update is called once per frame
    void Update()
    {
        if(transform.GetChild(0).GetComponent<HeadCollider>().dead){
            print("you died sir");
        };

        if (Input.GetKeyDown(KeyCode.F)){
           
            Vector3 pos = currentBodyStart.transform.position +  transform.forward * 1.0f;
            currentBodyStart.transform.position = pos;
            GameObject new_segment = Instantiate(bodySegmentPrefab, transform);
            new_segment.name = "FirstBody";
            new_segment.GetComponent<CharacterJoint>().connectedBody = m_Rigidbody;

           currentBodyStart.GetComponent<CharacterJoint>().connectedBody = new_segment.GetComponent<Rigidbody>();
           
           currentBodyStart.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
           currentBodyStart.name = "Body";
           currentBodyStart = new_segment;
        }

    }
}
