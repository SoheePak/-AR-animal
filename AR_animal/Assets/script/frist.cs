using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class frist : MonoBehaviour
{
    public GameObject start_button;
    public AudioSource audio;
    public AudioClip button;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("gamestart", 3f);
    }
    public void gamestart()
    {
        start_button.SetActive(true);
    }
    
    public void next()
    {
        audio.PlayOneShot(button);
        Invoke("next_Scene", 1.5f);
    }
    public void next_Scene()
    {
        SceneManager.LoadScene("how");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
