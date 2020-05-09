using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{

    public WheelCollider collider;

    void Start()
    {
        collider = GetComponent<WheelCollider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Segment" || collision.gameObject.tag == "Head")
        {
            Physics.IgnoreCollision(collision.collider, collider);
        }

    }
}
