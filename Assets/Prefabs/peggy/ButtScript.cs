using UnityEngine;

public class ButtScript : MonoBehaviour
{
    [Header("Dropped legs beacon")]
    [Tooltip("Height of the glowing beam above the dropped legs, in metres")]
    public float beaconHeight = 40f;
    [Tooltip("Width of the beam, in metres")]
    public float beaconWidth = 1.2f;
    [Tooltip("Peak brightness of the light under the beam")]
    public float beaconLightIntensity = 60f;
    public float beaconLightRange = 20f;
    [Tooltip("Pulses per second")]
    public float beaconPulseSpeed = 1.5f;

    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private HingeJoint joint;
    private GameObject icon;
    private Renderer iconRenderer;
    private Light beaconLight;
    private MaterialPropertyBlock block;
    private Collider buttCollider;
    private Collider[] headColliders;
    private Rigidbody lastConnectedBody;
    public bool connected = true;

    void Start()
    {
        joint = GetComponent<HingeJoint>();
        icon = transform.GetChild(0).gameObject;
        iconRenderer = icon.GetComponent<Renderer>();
        buttCollider = GetComponent<Collider>();

        GameObject head = GameObject.FindGameObjectWithTag("Head");
        headColliders = head ? head.GetComponentsInChildren<Collider>() : new Collider[0];

        GameObject lightObject = new GameObject("Beacon Light");
        lightObject.transform.SetParent(transform, false);
        beaconLight = lightObject.AddComponent<Light>();
        beaconLight.type = LightType.Point;
        beaconLight.range = beaconLightRange;
        beaconLight.shadows = LightShadows.None;
        beaconLight.enabled = false;

        Refresh();
    }

    void Update()
    {
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

        if (!connected)
        {
            AnimateBeacon();
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
            // Just fell off: pop up so it lands somewhere visible
            transform.position += Vector3.up * 5f;
        }

        icon.SetActive(!nowConnected);
        beaconLight.enabled = !nowConnected;
        // Peggy shouldn't bump into her own dropped legs. Her head's trigger still picks them up.
        IgnoreHead(!nowConnected);

        connected = nowConnected;
    }

    private void IgnoreHead(bool ignore)
    {
        if (!buttCollider)
            return;

        foreach (Collider headCollider in headColliders)
        {
            if (headCollider && !headCollider.isTrigger)
                Physics.IgnoreCollision(buttCollider, headCollider, ignore);
        }
    }

    /// <summary>
    /// Keeps the beam standing straight up out of the legs however they tumble, and pulses
    /// it and the light through the rainbow in time with the trip
    /// </summary>
    private void AnimateBeacon()
    {
        Vector3 basePosition = transform.position;
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * beaconPulseSpeed * Mathf.PI * 2f);

        // Unity's cylinder is 1 wide and 2 tall, centred on its origin
        Vector3 parentScale = transform.lossyScale;
        icon.transform.SetPositionAndRotation(basePosition + Vector3.up * (beaconHeight * 0.5f), Quaternion.identity);
        float width = beaconWidth * (0.85f + 0.3f * pulse);
        icon.transform.localScale = new Vector3(width / parentScale.x, beaconHeight * 0.5f / parentScale.y, width / parentScale.z);

        Color colour = Color.HSVToRGB(Mathf.Repeat(TripController.Hue + 0.33f, 1f), 0.8f, 1f);
        block ??= new MaterialPropertyBlock();
        iconRenderer.GetPropertyBlock(block);
        block.SetColor(ColorId, colour * (2.5f + 2.5f * pulse));
        iconRenderer.SetPropertyBlock(block);

        beaconLight.transform.position = basePosition + Vector3.up * 2f;
        beaconLight.color = colour;
        beaconLight.intensity = beaconLightIntensity * (0.5f + 0.5f * pulse);
    }
}
