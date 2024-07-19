using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager script that will load all scriptable objects for use at runtime
/// </summary>

public class TheData : MonoBehaviour
{
    public GameData data;

    [Header("Resources")]
    public string items_folder = "Items";
    public string constructions_folder = "Constructions";
    public string plants_folder = "Plants";

    private static TheData _instance;

    void Awake()
    {
        _instance = this;
        ItemData.Load(items_folder);
        ConstructionData.Load(constructions_folder);
        PlantData.Load(plants_folder);
        CraftData.Load();
    }

    public static TheData Get()
    {
        return _instance;
    }
}
