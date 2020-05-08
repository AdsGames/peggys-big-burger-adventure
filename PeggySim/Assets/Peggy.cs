using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peggy : MonoBehaviour
{

    private GameObject head;

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
        if (!Input.GetKey(KeyCode.W))
            m_Rigidbody.MovePosition(m_Rigidbody.position + transform.forward * -speed * Time.fixedDeltaTime);


        if (Input.GetKey(KeyCode.A)){
                    Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * -Time.deltaTime);
                     m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);
        }
        if (Input.GetKey(KeyCode.Space)){
           m_Rigidbody.AddRelativeForce(new Vector3(0,100,0));
        }
      
        if (Input.GetKey(KeyCode.D)){
            
        Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.deltaTime);
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);
        }
        
        

    }
}
