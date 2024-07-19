using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class how : MonoBehaviour
{
    public GameObject go_button;
    public AudioSource audio;
    public AudioClip button_sound;
    public AudioClip reading_sound;
    public VideoPlayer videoPlayer;
    public GameObject startImage;
    // Start is called before the first frame update
    void Start()
    {
        videoPlayer.time = 0;
        Invoke("go", 58f);
        audio.PlayOneShot(reading_sound);
        //videoPlayer = GetComponent<VideoPlayer>();
        Invoke("PlayVideo", 5f);
        Invoke("bye", 6f);
        Invoke("next_Scene", 63f);
    }
    public void bye()
    {
        startImage.SetActive(false);
    }
    public void PlayVideo()
    {
        videoPlayer.Play();
    }
    public void go()
    {
        go_button.SetActive(true);
    }
    public void Button_clk()
    {
        audio.PlayOneShot(button_sound);
        Invoke("next_Scene", 1.5f);
    }
    public void next_Scene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
