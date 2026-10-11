using UnityEngine;

/// <summary>
/// Speeds the music up as Peggy grows
/// </summary>
public class MusicController : MonoBehaviour
{
  [Tooltip("Falls back to the AudioSource on this object if empty")]
  public AudioSource audioSource;
  public float pitchMultiplier = 0.05f;
  public float maxPitch = 2.0f;
  public float basePitch = 0.5f;
  private float currentSegments = 1;

  private void Awake()
  {
    if (audioSource == null) {
      audioSource = GetComponent<AudioSource>();
    }
  }

  private void Update()
  {
    if (audioSource == null) {
      return;
    }

    currentSegments = Mathf.Lerp(currentSegments, Segment.AliveCount, 0.04f);

    float pitchModifier = (basePitch + currentSegments * pitchMultiplier);
    audioSource.pitch = pitchModifier < maxPitch ? pitchModifier : maxPitch;
  }
}
