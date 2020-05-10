using UnityEngine;

public class MusicController : MonoBehaviour
{
  public AudioSource audioSource;
  public GameInfo gameInfo;
  public float pitchMultiplier = 0.05f;
  public float maxPitch = 2.0f;
  public float basePitch = 0.5f;
  private float currentSegments = 1;

  private void Update()
  {
    if (audioSource == null || gameInfo == null) {
      Debug.Log("Must set Audio Source and Game Info");
      return;
    }

    currentSegments = Mathf.Lerp(currentSegments, gameInfo.getSegments(), 0.04f);

    float pitchModifier = (basePitch + currentSegments * pitchMultiplier);
    audioSource.pitch = pitchModifier < maxPitch ? pitchModifier : maxPitch;
  }
}
