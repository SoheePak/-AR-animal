using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Firepits can be fueled with wood or other materials. Will be lit until it run out of fuel
/// </summary>

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(Construction))]
public class Firepit : MonoBehaviour
{
    public GameObject fire_fx;
    public GroupData fire_group;

    public float start_fuel = 10f;
    public float max_fuel = 50f;
    public float fuel_per_hour = 1f; //In Game hours
    public float wood_add_fuel = 2f;

    private Selectable select;
    private Construction construct;
    private UniqueID unique_id;

    private bool is_on = false;
    private float fuel = 0f;

    void Awake()
    {
        select = GetComponent<Selectable>();
        construct = GetComponent<Construction>();
        unique_id = GetComponent<UniqueID>();
        fire_fx.SetActive(false);
    }

    private void Start()
    {
        //select.onUse += OnUse;
        select.RemoveGroup(fire_group);
        construct.onBuild += OnFinishBuild;

        if (PlayerData.Get().HasUniqueID(GetFireUID()))
            fuel = PlayerData.Get().GetUniqueID(GetFireUID());
    }

    void Update()
    {
        if (is_on)
        {
            float game_speed = GameData.Get().game_time_mult;
            float hour_to_sec = game_speed / 3600f;
            fuel -= hour_to_sec * Time.deltaTime;

            PlayerData.Get().SetUniqueID(GetFireUID(), Mathf.RoundToInt(fuel));
        }

        is_on = fuel > 0f;
        fire_fx.SetActive(is_on);

        if (is_on)
            select.AddGroup(fire_group);
        else
            select.RemoveGroup(fire_group);
    }

    public void AddFuel(float value)
    {
        fuel += value;
        is_on = fuel > 0f;

        PlayerData.Get().SetUniqueID(GetFireUID(), Mathf.RoundToInt(fuel));
    }

    private void OnFinishBuild()
    {
        AddFuel(start_fuel);
    }

    public string GetFireUID()
    {
        return unique_id.unique_id + "_fire";
    }

    public bool IsOn() {
        return is_on;
    }
}
