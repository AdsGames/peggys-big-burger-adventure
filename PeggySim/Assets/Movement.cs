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

  public SoundEffectManager soundManager;
  public GameInfo info;

  private List<GameObject> wheels;
  private Rigidbody m_Rigidbody;
  private Vector3 m_EulerAngleVelocity;
  private bool onGround = true;

  private void Start()
  {
    //Set the axis the Rigidbody rotates in (100 in the y axis)
    m_EulerAngleVelocity = new Vector3(0, 100, 0);

    //Fetch the Rigidbody from the GameObject with this script attached
    m_Rigidbody = GetComponent<Rigidbody>();
    m_Rigidbody.drag = drag;
    m_Rigidbody.angularDrag = angularDrag;

    wheels = new List<GameObject>();
    foreach (Transform child in transform) {
      if (child.tag == "Leg") {
        wheels.Add(child.gameObject);
      }
    }
  }

  private void FixedUpdate()
  {
    if (onGround) {
      m_Rigidbody.drag = drag;
      m_Rigidbody.angularDrag = angularDrag;
      m_Rigidbody.AddRelativeForce(new Vector3(0, 0, moveSpeed * (movementReversed ? 1 : -1) * ((info.getSegments() * segmentFactor) + 1)), ForceMode.Acceleration);

      if (Input.GetAxis("Horizontal") < 0) {
        m_Rigidbody.AddRelativeTorque(new Vector3(0, turnSpeed * (turningReversed ? 1 : -1), 0), ForceMode.Acceleration);
      } else if (Input.GetAxis("Horizontal") > 0) {
        m_Rigidbody.AddRelativeTorque(new Vector3(0, -turnSpeed * (turningReversed ? 1 : -1), 0), ForceMode.Acceleration);
      }

      if (Input.GetButton("Jump")) {
        m_Rigidbody.AddRelativeForce(new Vector3(0, 50, 0), ForceMode.Impulse);
      }


      if (soundManager) {
        soundManager.playStep(m_Rigidbody.velocity.magnitude / 50f);
      } else {
        Debug.Log("No sound manager");
      }
    } else {
      m_Rigidbody.drag = 0;
      m_Rigidbody.angularDrag = 0;
    }
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
