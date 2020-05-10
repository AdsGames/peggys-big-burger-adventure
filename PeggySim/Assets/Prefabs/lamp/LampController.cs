using UnityEngine;

public class LampController : MonoBehaviour
{
  public float rotation = 0.0f;
  public Light lamp = null;
  public float rotationSpeed = 0.1f;
  public GameInfo gameInfo = null;

  private void Update()
  {
    if (!lamp) {
      return;
    }

    lamp.transform.Rotate(0, rotation, 0);
  }

  private void FixedUpdate()
  {
    if (gameInfo != null) {
      rotation = rotationSpeed + gameInfo.getSegments() * 0.1f;
    } else {
      rotation = rotationSpeed;
    }
  }
}
