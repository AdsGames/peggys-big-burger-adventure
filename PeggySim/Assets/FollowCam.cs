using System.Collections;
using System.Collections.Generic;
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
    void Start()
    {
        cameraGhost = GameObject.FindGameObjectWithTag("CameraGhost");
        player = GameObject.FindGameObjectWithTag("Head");
        info = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameInfo>(); ;
        cam = GetComponent<Camera>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, 
            new Vector3(cameraGhost.transform.position.x, cameraGhost.transform.position.y, 
            cameraGhost.transform.position.z), 
            ref velocity, speed);

        cam.fieldOfView = Mathf.Clamp(Mathf.Lerp(cam.fieldOfView, (player.GetComponent<Rigidbody>().velocity.magnitude * FOVFactor) + baseFov, 0.2f), 45, 85);

        cameraGhost.transform.localPosition = new Vector3(0, height, initialOffset + ((info.getSegments() - 1) * scaleFactor));


        /*
        hyp = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(cameraOffset.x) + lookDistance, 2) + Mathf.Pow(cameraOffset.y, 2));
        theta = Mathf.Asin((Mathf.Abs(cameraOffset.x) + lookDistance) / hyp);

        Vector3 heading = target.transform.position - transform.position;
        Vector3 direction = heading / heading.magnitude;

        float yRot = Mathf.Atan(direction.z / direction.x);
        Debug.Log(yRot);
        */

        transform.eulerAngles = new Vector3(cameraGhost.transform.eulerAngles.x, cameraGhost.transform.eulerAngles.y, cameraGhost.transform.eulerAngles.z);
    }
}
