using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
  public List<AudioClip> clips;
  private List<AudioSource> audioSources;

  private void Start()
  {
    audioSources = new List<AudioSource>();
    for (int i = 0; i < clips.Count; i++) {
      AudioSource source = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
      source.clip = clips[i];
      audioSources.Add(source);
    }
  }

  public void playBork()
  {
    playAudio(0);
  }

  public void playStep(float pitch)
  {
    playAudio(3, pitch, 0.5f);
  }

  public void playEat()
  {
    playAudio(1);
  }

  public void playHandbrake()
  {
    playAudio(2);
  }
  public void playBoost()
  {
    playAudio(4);
  }

  private void playAudio(int track, float pitch = 1.0f, float volume = 1.0f)
  {
    if (clips.Count <= track || !clips[track]) {
      Debug.Log("Track does " + track + " not exist! Length is: " + clips.Count);
      return;
    }

    if (!audioSources[track].isPlaying) {
      audioSources[track].pitch = pitch;
      audioSources[track].volume = volume;
      audioSources[track].Play();
    }
  }
}
