using UnityEngine;
using UnityEngine.Serialization;

public class Wheel : MonoBehaviour
{
    [FormerlySerializedAs("collider")]
    public WheelCollider wheelCollider;

    void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Segment") || collision.gameObject.CompareTag("Head"))
        {
            Physics.IgnoreCollision(collision.collider, wheelCollider);
        }
    }
}
