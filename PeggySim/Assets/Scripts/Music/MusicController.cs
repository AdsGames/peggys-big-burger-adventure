using UnityEngine;

public class MusicController : MonoBehaviour
{
  public AudioSource audioSource;
  public GameInfo gameInfo;
  public float pitchMultiplier = 0.1f;
  public float maxPitch = 2.0f;
  public float basePitch = 0.5f;

  private void Update()
  {
    if (audioSource == null || gameInfo == null) {
      Debug.Log("Must set Audio Source and Game Info");
      return;
    }

    float pitchModifier = (basePitch + gameInfo.getSegments() * pitchMultiplier);
    audioSource.pitch = pitchModifier < maxPitch ? pitchModifier : maxPitch;
  }
}
