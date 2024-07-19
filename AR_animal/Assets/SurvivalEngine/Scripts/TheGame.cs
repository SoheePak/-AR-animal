using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Game Manager Script for Survival Engine
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>

public class TheGame : MonoBehaviour
{
    [Header("Loader")]
    public GameObject ui_canvas;
    public GameObject ui_canvas_mobile;
    public GameObject audio_manager;
    public GameObject action_selector;

    private bool paused = false;
    private float death_timer = 0f;

    public UnityAction<bool> onPause;

    private static TheGame _instance;

    void Awake()
    {
        _instance = this;
        PlayerData.LoadLast();

        //Load managers
        if (!FindObjectOfType<TheUI>())
            Instantiate(IsMobile() ? ui_canvas_mobile : ui_canvas);
        if (!FindObjectOfType<TheAudio>())
            Instantiate(audio_manager);
        if (!FindObjectOfType<ActionSelector>())
            Instantiate(action_selector);
    }

    private void Start()
    {
        //Load game data
        PlayerData pdata = PlayerData.Get();
        GameData gdata = GameData.Get();
        if (!string.IsNullOrEmpty(pdata.current_scene))
        {
            if (pdata.current_entry_index < 0 && pdata.current_scene == SceneNav.GetCurrentScene())
            {
                PlayerCharacter.Get().transform.position = pdata.current_pos;
                TheCamera.Get().MoveToTarget(pdata.current_pos);
            }
        }

        pdata.current_scene = SceneNav.GetCurrentScene();

        //Spawn dropped items
        foreach (KeyValuePair<string, DroppedItemData> elem in pdata.dropped_items)
        {
            if(elem.Value.scene == SceneNav.GetCurrentScene())
                Item.Spawn(elem.Key);
        }

        //Spawn constructions
        foreach (KeyValuePair<string, BuiltConstructionData> elem in pdata.built_constructions)
        {
            if (elem.Value.scene == SceneNav.GetCurrentScene())
                Construction.Spawn(elem.Key);
        }

        //Spawn plants
        foreach (KeyValuePair<string, SowedPlantData> elem in pdata.sowed_plants)
        {
            if (elem.Value.scene == SceneNav.GetCurrentScene())
                Plant.Spawn(elem.Key);
        }

        BlackPanel.Get().Show(true);
        BlackPanel.Get().Hide();
    }
    
    void Update()
    {
        if (TheGame.Get().IsPaused())
            return;

        //Check if dead
        PlayerCharacter character = PlayerCharacter.Get();
        if (character.IsDead())
        {
            death_timer += Time.deltaTime;
            if (death_timer > 2f)
            {
                enabled = false; //Stop running this loop
                TheUI.Get().ShowGameOver();
            }
        }

        //Game time
        PlayerData pdata = PlayerData.Get();
        GameData gdata = GameData.Get();
        float game_speed = GameData.Get().game_time_mult;
        float hour_to_sec = game_speed / 3600f;
        pdata.day_time += hour_to_sec * Time.deltaTime;
        if (pdata.day_time >= 24f)
        {
            pdata.day_time = 0f;
            pdata.day++; //New day
        }

        //Set music
        AudioClip[] music_playlist = GameData.Get().music_playlist;
        if (music_playlist.Length > 0 && !TheAudio.Get().IsMusicPlaying("music")) {
            AudioClip clip = music_playlist[Random.Range(0, music_playlist.Length)];
            TheAudio.Get().PlayMusic("music", clip, 0.4f, false);
        }
    }

    public bool IsNight() {
        PlayerData pdata = PlayerData.Get();
        return pdata.day_time >= 18f || pdata.day_time < 6f;
    }

    public void Save()
    {
        PlayerData.Get().current_scene = SceneNav.GetCurrentScene();
        PlayerData.Get().current_pos = PlayerCharacter.Get().transform.position;
        PlayerData.Get().current_entry_index = -1; //Go to current_pos
        PlayerData.Get().Save();
    }

    public void Pause()
    {
        paused = true;
        if (onPause != null)
            onPause.Invoke(paused);
    }

    public void Unpause()
    {
        paused = false;
        if (onPause != null)
            onPause.Invoke(paused);
    }

    public bool IsPaused()
    {
        return paused;
    }

    public static bool IsMobile()
    {
#if UNITY_ANDROID || UNITY_IOS
        return true;
#else
        return false;
#endif
    }

    public static TheGame Get()
    {
        return _instance;
    }
}