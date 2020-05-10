using UnityEngine;

public class FollowCam : MonoBehaviour
{
  public GameObject target;
  public float speed;
  public float baseFov;
  public float FOVFactor;

  private GameObject player;
  private Camera cam;

  private GameObject cameraGhost;
  private GameInfo info;
  private Vector3 velocity;
  private Vector3 rotationVelocity;

  public float initialOffset;
  public float height;
  public float scaleFactor;



  // Start is called before the first frame update
  private void Start()
  {
    cameraGhost = GameObject.FindGameObjectWithTag("CameraGhost");
    player = GameObject.FindGameObjectWithTag("Head");
    info = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameInfo>(); ;
    cam = GetComponent<Camera>();
  }

  // Update is called once per frame
  private void FixedUpdate()
  {
    transform.position = Vector3.SmoothDamp(
      transform.position,
      new Vector3(
        cameraGhost.transform.position.x,
        cameraGhost.transform.position.y,
        cameraGhost.transform.position.z
      ),
      ref velocity,
      speed
    );

    cam.fieldOfView = Mathf.Clamp(Mathf.Lerp(cam.fieldOfView, (player.GetComponent<Rigidbody>().velocity.magnitude * FOVFactor) + baseFov + (player.GetComponent<Movement>().isBoosting ? 10:0), 0.2f), 45, 85);
    cameraGhost.transform.localPosition = new Vector3(0, height, initialOffset + ((info.getSegments() - 1) * scaleFactor));
    transform.eulerAngles = new Vector3(cameraGhost.transform.eulerAngles.x, cameraGhost.transform.eulerAngles.y, cameraGhost.transform.eulerAngles.z);
  }
}
