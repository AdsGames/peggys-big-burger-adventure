using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour
{
    public CapsuleCollider collider;

    void Start()
    {
        collider = GetComponent<CapsuleCollider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Segment" || collision.gameObject.tag == "Head")
        {
            Physics.IgnoreCollision(collision.collider, collider);
        }

    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Segment" || collision.gameObject.tag == "Head")
        {
            Physics.IgnoreCollision(collision.collider, collider);
        }
    }
}
