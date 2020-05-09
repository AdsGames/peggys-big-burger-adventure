using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    //publics to be provided in editor
    public float turnSpeed;
    public float moveSpeed;
    public bool movementReversed;
    public bool turningReversed;
    public float drag;
    public float angularDrag;
    public float segmentFactor;

    public List<GameObject> wheels;

    private GameObject head;

    private GameInfo info;

    public Rigidbody m_Rigidbody;
    Vector3 m_EulerAngleVelocity;

    private bool onGround = false;

    // Start is called before the first frame update
    void Start()
    {
   //Set the axis the Rigidbody rotates in (100 in the y axis)
        m_EulerAngleVelocity = new Vector3(0, 100, 0);

        //Fetch the Rigidbody from the GameObject with this script attached
        m_Rigidbody = GetComponent<Rigidbody>();

        m_Rigidbody.drag = drag;
        m_Rigidbody.angularDrag = angularDrag;

        wheels = new List<GameObject>();
        foreach(Transform child in transform)
        {

            if (child.tag == "Leg")
            {
                wheels.Add(child.gameObject);
            }
        }

        info = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameInfo>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /* The Danno original
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
        */

        if (onGround)
        {
            m_Rigidbody.AddRelativeForce(new Vector3(0, 0, moveSpeed * (movementReversed ? 1 : -1) * ((info.getSegments()* segmentFactor)+ 1)), ForceMode.Acceleration);

            if (Input.GetAxis("Horizontal") < 0)
            {
                m_Rigidbody.AddRelativeTorque(new Vector3(0, turnSpeed * (turningReversed ? 1 : -1), 0), ForceMode.Acceleration);
            }
            else if (Input.GetAxis("Horizontal") > 0)
            {
                m_Rigidbody.AddRelativeTorque(new Vector3(0, -turnSpeed * (turningReversed ? 1 : -1), 0), ForceMode.Acceleration);
            }
        }
        //Debug.Log(info.getSegments());
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ground")
            onGround = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ground")
            onGround = false;
    }
}
