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
   //Set the axis the Rigidbody rotates in (100 in the y axis)
        m_EulerAngleVelocity = new Vector3(0, 100, 0);

        //Fetch the Rigidbody from the GameObject with this script attached
        m_Rigidbody = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {   
        print(currentBodyStart.transform.position);
        if (!Input.GetKey(KeyCode.W))
            m_Rigidbody.MovePosition(m_Rigidbody.position + transform.forward * -speed * Time.deltaTime);


        if (Input.GetKey(KeyCode.A)){
                    Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * -Time.deltaTime);
                     m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);
        }
        if (Input.GetKeyDown(KeyCode.Space)){
           m_Rigidbody.AddRelativeForce(new Vector3(0,1000,0));
        }
        if (Input.GetKeyDown(KeyCode.F)){
            Vector3 pos = currentBodyStart.transform.position + transform.forward * 1.0f;
            currentBodyStart.transform.position = pos;
            GameObject new_segment = Instantiate(bodySegmentPrefab, transform);
            new_segment.GetComponent<CharacterJoint>().connectedBody = m_Rigidbody;

           currentBodyStart.GetComponent<CharacterJoint>().connectedBody = new_segment.GetComponent<Rigidbody>();
           
           currentBodyStart.transform.position = new Vector3(transform.position.x, 0, transform.position.z);

           currentBodyStart = new_segment;
        }
        if (Input.GetKey(KeyCode.G)){
            
            currentBodyStart.transform.rotation = Quaternion.Euler(0, 0, 0);
            
        }
      
        if (Input.GetKey(KeyCode.D)){
            
        Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.deltaTime);
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);
        }
        
        

    }
}
