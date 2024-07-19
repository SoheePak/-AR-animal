using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioMusic : MonoBehaviour {

    public string channel = "music";
    public AudioClip clip;
    public float volume = 0.4f;
    public bool play_at_start = true;
    public bool loop = true;
    
    private void Start()
    {
        if (play_at_start)
            Play();
    }

    public void Play()
    {
        TheAudio.Get().PlayMusic(channel, clip, volume, loop);
    }
    
}
