using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioAnim : MonoBehaviour {

    public string channel = "animation";
    public float volume = 0.8f;
    
    public void PlaySound(AudioClip clip)
    {
        TheAudio.Get().PlaySFX(channel, clip, volume);
    }

    public void SetChannel(string channel)
    {
        this.channel = channel;
    }

    public void SetVolume(float vol)
    {
        this.volume = vol;
    }
    
}
