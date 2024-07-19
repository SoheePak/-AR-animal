using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sheep : MonoBehaviour
{
    public float movespeed = 1f;
    private Animator animator;
    private AudioSource audio;
    public AudioClip sheep_sound;
    public GameObject footText;
    public GameObject inforText;
    public AudioClip reading;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();

    }
    // Update is called once per frame
    void Update()
    {
    }
    public void sound()
    {
        animator.SetTrigger("sound");
        audio.PlayOneShot(sheep_sound);
    }
    public void foot()
    {
        footText.SetActive(true);
    }
    public void exit_foot()
    {
        footText.SetActive(false);
    }
    public void infor()
    {
        inforText.SetActive(true);
        audio.PlayOneShot(reading);
    }
    public void exit_infor()
    {
        inforText.SetActive(false);
        audio.Stop();
    }
    // Start is called before the first frame update
}
