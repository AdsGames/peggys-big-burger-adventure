using System.Text;
using UnityEngine;

// TEMPORARY: logs why Peggy drifts. Delete once the drift bug is fixed.
public class DriftProbe : MonoBehaviour
{
  private Rigidbody head;
  private float nextLog;
  private float lastYaw;
  private float lastTime;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Create()
  {
    GameObject probe = new GameObject("DriftProbe");
    DontDestroyOnLoad(probe);
    probe.AddComponent<DriftProbe>();
  }

  private void FixedUpdate()
  {
    if (!head)
    {
      Movement movement = FindAnyObjectByType<Movement>();
      if (!movement)
      {
        return;
      }
      head = movement.GetComponent<Rigidbody>();
      Debug.Log($"[Drift] start com={head.centerOfMass:F3} inertia={head.inertiaTensor:F3} constraints={head.constraints} scale={head.transform.lossyScale:F3}");
    }

    if (Time.time < nextLog)
    {
      return;
    }
    nextLog = Time.time + 0.25f;

    Vector3 localVel = head.transform.InverseTransformDirection(head.linearVelocity);
    StringBuilder line = new StringBuilder();
    float yaw = head.rotation.eulerAngles.y;
    float yawRate = Mathf.DeltaAngle(lastYaw, yaw) / Mathf.Max(Time.time - lastTime, 0.0001f);
    lastYaw = yaw;
    lastTime = Time.time;
    line.Append($"[Drift] t={Time.time:F2} yaw={yaw:F1} yawRate={yawRate:F1}deg/s ");
    line.Append($"turn={GameInput.Turn.ReadValue<float>():F3} localVel(x={localVel.x:F2} y={localVel.y:F2} z={localVel.z:F2}) ");
    line.Append($"segments={Segment.AliveCount}");

    if (Physics.Raycast(head.worldCenterOfMass, Vector3.down, out RaycastHit hit, 5f))
    {
      line.Append($" ground={hit.normal:F3} on={hit.collider.name}");
    }

    // Joints pulling directly on the head, in head-local space (x = sideways)
    foreach (Joint joint in FindObjectsByType<Joint>(FindObjectsSortMode.None))
    {
      if (joint.connectedBody == head)
      {
        Vector3 force = head.transform.InverseTransformDirection(joint.currentForce);
        Vector3 torque = head.transform.InverseTransformDirection(joint.currentTorque);
        line.Append($" | {joint.name} force={force:F2} torque={torque:F2}");
      }
    }

    Debug.Log(line.ToString());
  }

  private void OnDestroy()
  {
    head = null;
  }
}
