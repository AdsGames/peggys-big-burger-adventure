using UnityEngine;

public class ButtScript : MonoBehaviour
{
    private HingeJoint joint;
    private GameObject icon;
    private Rigidbody lastConnectedBody;
    public bool connected = true;

    void Start()
    {
        joint = GetComponent<HingeJoint>();
        icon = transform.GetChild(0).gameObject;
        Refresh();
    }

    void Update()
    {
        icon.transform.eulerAngles = Vector3.zero;

        // Only re-check the connection when the joint actually points somewhere new
        if (joint.connectedBody != lastConnectedBody)
        {
            Refresh();
        }
        // A segment that dies keeps its Rigidbody but changes tag, so watch for that too
        else if (connected && !IsAttachedToSegment())
        {
            Refresh();
        }
    }

    private bool IsAttachedToSegment()
    {
        return joint.connectedBody != null && joint.connectedBody.CompareTag("Segment");
    }

    private void Refresh()
    {
        lastConnectedBody = joint.connectedBody;
        bool nowConnected = IsAttachedToSegment();

        if (connected && !nowConnected)
        {
            // Just fell off: pop up and grow so the player can spot it
            icon.SetActive(true);
            transform.position += Vector3.up * 5f;
            transform.localScale = Vector3.one * (3f * 3.91f);
        }
        else if (nowConnected)
        {
            icon.SetActive(false);
        }

        connected = nowConnected;
    }
}
