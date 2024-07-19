﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="GameData", menuName ="Data/GameData", order =0)]
public class GameData : ScriptableObject
{
    [Header("Game")]
    public float game_time_mult = 24f; //A value of 1 means time follows real life time. Value of 24 means 1 hour of real time will be one day in game

    [Header("Day/Night")]
    public float day_light_dir_intensity = 1f; //Directional light at day
    public float day_light_ambient_intensity = 1f;  //Ambient light at day
    public float night_light_dir_intensity = 0.2f; //Directional light at night
    public float night_light_ambient_intensity = 0.5f; //Ambient light at night

    [Header("Music")]
    public AudioClip[] music_playlist;

    [Header("FX")]
    public GameObject item_take_fx;
    public GameObject item_select_fx;

    public static GameData Get()
    {
        return TheData.Get().data;
    }
}
