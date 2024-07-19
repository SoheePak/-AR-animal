using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioSFX : MonoBehaviour {

    public string channel = "sfx";
    public AudioClip clip;
    public float volume = 0.8f;
    public bool play_at_start = true;
    
    private void Start()
    {
        if (play_at_start)
            Play();
    }

    public void Play()
    {
        TheAudio.Get().PlaySFX(channel, clip, volume);
    }
    
}
