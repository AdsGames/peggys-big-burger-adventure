using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peggy : MonoBehaviour
{

    private GameObject head;

    private float speed = 2;
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

        m_Rigidbody.transform.Translate(Vector3.back * Time.deltaTime * speed);

    
        if (Input.GetKey(KeyCode.A)){
                    Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * -Time.deltaTime);
                     m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);
        }
        if (Input.GetKey(KeyCode.S)){
           
        }
        if (Input.GetKey(KeyCode.D)){
            
        Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.deltaTime);
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);
        }
        
        

    }
}
