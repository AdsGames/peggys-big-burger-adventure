using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// The 3, 2, 1, GO! countdown at the start of a round. Peggy is frozen until GO.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Intro : MonoBehaviour
{
  public TMP_Text label;
  [Tooltip("Voice lines for 3, 2, 1 and GO, in that order")]
  [FormerlySerializedAs("audio")]
  public List<AudioClip> voiceClips = new List<AudioClip>();
  public Color[] colours =
  {
    new Color(0.902f, 0.224f, 0.275f),
    new Color(1f, 0.788f, 0.235f),
    new Color(0.482f, 0.788f, 0.314f),
    new Color(1f, 0.957f, 0.878f),
  };

  private AudioSource voice;
  private Coroutine pop;

  private IEnumerator Start()
  {
    voice = GetComponent<AudioSource>();
    label.alpha = 0f;

    Movement movement = GameObject.FindGameObjectWithTag("Head").GetComponent<Movement>();
    GameObject musicObject = GameObject.Find("MainMusic");
    AudioSource music = musicObject ? musicObject.GetComponent<AudioSource>() : null;
    GameObject soundObject = GameObject.FindGameObjectWithTag("SoundEffectManager");
    SoundEffectManager sounds = soundObject ? soundObject.GetComponent<SoundEffectManager>() : null;

    movement.enabled = false;
    if (music)
      music.enabled = false;

    yield return new WaitForSeconds(0.3f);

    for (int i = 0; i < 3; i++)
    {
      Show((3 - i).ToString(), colours[i], false);
      yield return new WaitForSeconds(0.5f);
      PlayVoice(i);
      yield return new WaitForSeconds(0.5f);
    }

    Show("GO!", colours[3], true);
    yield return new WaitForSeconds(0.3f);
    PlayVoice(3);
    movement.enabled = true;

    yield return new WaitForSeconds(0.7f);
    if (music)
      music.enabled = true;
    if (sounds)
      sounds.playBork();
  }

  private void PlayVoice(int index)
  {
    if (index < voiceClips.Count && voiceClips[index])
      voice.PlayOneShot(voiceClips[index]);
  }

  private void Show(string text, Color colour, bool burst)
  {
    if (pop != null)
      StopCoroutine(pop);
    pop = StartCoroutine(Pop(text, colour, burst));
  }

  /// <summary>
  /// Numbers slam in from big to normal size; GO bursts outwards. Both fade at the end.
  /// </summary>
  private IEnumerator Pop(string text, Color colour, bool burst)
  {
    const float duration = 1f;
    label.text = text;
    label.color = colour;

    for (float t = 0f; t < duration; t += Time.deltaTime)
    {
      float k = t / duration;
      float scale = burst
        ? Mathf.Lerp(0.8f, 1.8f, EaseOut(k))
        : Mathf.Lerp(2f, 1f, EaseOut(Mathf.Clamp01(k * 4f)));
      label.rectTransform.localScale = Vector3.one * scale;
      label.alpha = Mathf.Clamp01((1f - k) / 0.3f);
      yield return null;
    }

    label.alpha = 0f;
  }

  private static float EaseOut(float t)
  {
    return 1f - (1f - t) * (1f - t) * (1f - t);
  }
}
