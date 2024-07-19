﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum AttributeType
{
    None=0,
    Health=2,
    Happiness=4,
    Hunger=6,
    Thirst=8,
    XP=10
}

[CreateAssetMenu(fileName = "AttributeData", menuName = "Data/AttributeData", order = 11)]
public class AttributeData : ScriptableObject
{
    public AttributeType type;
    public string title;

    [Space(5)]

    public float start_value = 100f; //Starting value
    public float max_value = 100f; //Maximum value

    public float value_per_hour = -100f; //How much is gained (or lost) per in-game hour

    [Header("When reaches zero")]
    public float deplete_hp_loss = -100f; //Per hour
    public float deplete_move_mult = 1f; //1f = normal speed


}
