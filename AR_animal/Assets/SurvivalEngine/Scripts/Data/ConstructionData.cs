using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConstructionData", menuName = "Data/ConstructionData", order = 4)]
public class ConstructionData : CraftData
{
    [Header("--- ConstructionData ------------------")]

    [Header("Prefab")]
    public GameObject construction_prefab; //Prefab spawned when the construction is built


    private static List<ConstructionData> construction_data = new List<ConstructionData>();

    public static void Load(string constructions_folder)
    {
        construction_data.Clear();
        construction_data.AddRange(Resources.LoadAll<ConstructionData>(constructions_folder));
    }

    public new static ConstructionData Get(string construction_id)
    {
        foreach (ConstructionData item in construction_data)
        {
            if (item.id == construction_id)
                return item;
        }
        return null;
    }

    public new static List<ConstructionData> GetAll()
    {
        return construction_data;
    }
}
