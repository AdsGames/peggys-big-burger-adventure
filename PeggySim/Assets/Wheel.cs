using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{

    public WheelCollider collider;
    public bool onGround;

    void Start()
    {
        collider = GetComponent<WheelCollider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Segment" || collision.gameObject.tag == "Segment")
        {
            Physics.IgnoreCollision(collision.collider, collider);
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Debug.Log("Bork Brok no longer on the ground");
            onGround = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Ground" && !onGround)
        {
            Debug.Log("Bork bork on the ground");
            onGround = true;
        }
    }
}
