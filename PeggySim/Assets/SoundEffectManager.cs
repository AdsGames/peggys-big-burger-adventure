using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public AudioClip bork;
    public AudioClip step;

    public List<AudioSource> audioSources;

    // Start is called before the first frame update
    void Start()
    {
        audioSources = new List<AudioSource>(GetComponents<AudioSource>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playBork()
    {
        playAudio(bork, 1, 1);
    }

    public void playStep(float pitch)
    {
        playAudioOnSpecific(3, step, pitch, 0.5f);
    }

    private void playAudio(AudioClip clip, float pitch, float volume)
    {
        for(int i = 0; i < 4; i++)
        {
            if(!audioSources[i].isPlaying)
            {
                audioSources[i].clip = clip;
                audioSources[i].pitch = pitch;
                audioSources[i].volume = volume;
                audioSources[i].Play();
                break;
            }
        }
    }

    private void playAudioOnSpecific(int track, AudioClip clip, float pitch, float volume)
    {
        if(!(audioSources[track].time > 0))
        {
            audioSources[track].clip = clip;
            audioSources[track].pitch = pitch;
            audioSources[track].volume = volume;
            audioSources[track].Play();
        }
    }
}
