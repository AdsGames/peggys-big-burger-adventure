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
    public bool isBoosting;
    public float handBrakeSpeed;
    public float maxhandBrakeMeter;
    public float boostSpeed;
    public float maxBoostMeter;

        

  public SoundEffectManager soundManager;
  public GameInfo info;

  private List<GameObject> wheels;
  private Rigidbody rigidBody;
  private Vector3 m_EulerAngleVelocity;
  private bool onGround = true;
    private float effectiveTurnSpeed;
    public float currentHandBrakeMeter;
    private GameObject currentBoostParticle;
    public GameObject boostParticle;


    private float effectiveMoveSpeed;
    public float currentBoostMeter;

  private void Start()
  {
    // Set the axis the Rigidbody rotates in (100 in the y axis)
    m_EulerAngleVelocity = new Vector3(0, 100, 0);

    // Fetch the Rigidbody from the GameObject with this script attached
    rigidBody = GetComponent<Rigidbody>();
    rigidBody.drag = drag;
    rigidBody.angularDrag = angularDrag;

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
      rigidBody.drag = drag;
      rigidBody.angularDrag = angularDrag;
      rigidBody.AddRelativeForce(new Vector3(0, 0, effectiveMoveSpeed * (movementReversed ? 1 : -1) * ((info.getSegments() * segmentFactor) + 1)), ForceMode.Acceleration);

      if (Input.GetAxis("Horizontal") < 0) {
        rigidBody.AddRelativeTorque(new Vector3(0, effectiveTurnSpeed * (turningReversed ? 1 : -1), 0), ForceMode.Acceleration);
      } else if (Input.GetAxis("Horizontal") > 0) {
        rigidBody.AddRelativeTorque(new Vector3(0, -effectiveTurnSpeed * (turningReversed ? 1 : -1), 0), ForceMode.Acceleration);
      }

            //Allan is a big dummy with stupid formatting
            if (Input.GetButton("Fire1") && currentHandBrakeMeter > 0)
            {
                effectiveTurnSpeed = handBrakeSpeed;
                currentHandBrakeMeter -= 1;
                soundManager.playHandbrake();
            }
            else
            {
                effectiveTurnSpeed = turnSpeed;
                if (currentHandBrakeMeter < maxhandBrakeMeter)
                    currentHandBrakeMeter += 0.1f;
            }

            if(Input.GetButton("Fire2"))
            {
              if(currentBoostMeter>0){
                if(!isBoosting)
                    currentBoostParticle = Instantiate(boostParticle);
                currentBoostParticle.transform.position = transform.position;
                
                

                  isBoosting = true;
                  effectiveMoveSpeed = boostSpeed;
                  currentBoostMeter -= 0.8f;
                  soundManager.playBoost();
              }else{
                Destroy(currentBoostParticle);
              }
            }
            else
            {
              
               Destroy(currentBoostParticle);
                isBoosting = false;
                effectiveMoveSpeed = moveSpeed;
                if (currentBoostMeter < maxBoostMeter)
                    currentBoostMeter += 1;
            }

      if (Input.GetButton("Jump")) {
        rigidBody.AddRelativeForce(new Vector3(0, 50, 0), ForceMode.Impulse);
      }


      if (soundManager) {
        soundManager.playStep(rigidBody.velocity.magnitude / 50f);
      } else {
        Debug.Log("No sound manager");
      }
    } else {
      rigidBody.drag = 0;
      rigidBody.angularDrag = 0;
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
