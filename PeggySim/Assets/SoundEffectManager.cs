using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
  public AudioClip bork;
  public AudioClip step;
  public AudioClip eat;
    public AudioClip handBrake;
    public AudioClip boost;

  private List<AudioSource> audioSources;

  private void Start()
  {
    audioSources = new List<AudioSource>(GetComponents<AudioSource>());
  }

  public void playBork()
  {
    playAudio(bork, 1, 1);
  }

  public void playStep(float pitch)
  {
    playAudioOnSpecific(3, step, pitch, 0.5f);
  }

  public void playEat()
  {
    playAudio(eat, 1, 1);
  }

    public void playHandbrake()
    {
        playAudio(handBrake, 1, 1);
    }
    public void playBoost()
    {
        playAudio(boost, 1, 1);
    }

  private void playAudio(AudioClip clip, float pitch, float volume)
  {
    for (int i = 0; i < audioSources.Count; i++) {
      if (!audioSources[i].isPlaying) {
        playAudioOnSpecific(i, clip, pitch, volume);
        break;
      }
    }
  }

  private void playAudioOnSpecific(int track, AudioClip clip, float pitch, float volume)
  {
    if (audioSources.Count <= track || !audioSources[track]) {
      Debug.Log("Track does " + track + " not exist! Length is: " + audioSources.Count);
      return;
    }

    if (!(audioSources[track].time > 0)) {
      audioSources[track].clip = clip;
      audioSources[track].pitch = pitch;
      audioSources[track].volume = volume;
      audioSources[track].Play();
    }
  }
}
