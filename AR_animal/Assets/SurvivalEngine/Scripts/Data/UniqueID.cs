using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helps to generate unique ids for each individual instance of objects in the scene. 
/// Unique IDs are mostly used in the save file to keep track of the state of an object.
/// </summary>

public class UniqueID : MonoBehaviour
{
    public string uid_prefix; //Will be added to the front of every ID of this type of object, set in the prefab

    [TextArea(1, 2)]
    public string unique_id; //The unique ID, should be empty in the prefab. Should only be added to instances in the scene. Can be automatically generated

    private static Dictionary<string, UniqueID> dict_id = new Dictionary<string, UniqueID>();

    void Awake()
    {
        if (unique_id != "")
        {
            dict_id[unique_id] = this;
        }
    }

    private void OnDestroy()
    {
        dict_id.Remove(unique_id);
    }

    public void SetValue(int value)
    {
        PlayerData.Get().SetUniqueID(unique_id, value);
    }

    public int GetValue()
    {
        return PlayerData.Get().GetUniqueID(unique_id);
    }

    public bool HasValue()
    {
        return PlayerData.Get().HasUniqueID(unique_id);
    }

    public bool HasUID()
    {
        return !string.IsNullOrEmpty(unique_id);
    }

    public void GenerateUID()
    {
        unique_id = uid_prefix + GenerateUniqueID();
    }

    public static string GenerateUniqueID()
    {
        int length = Random.Range(11, 17);
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        string unique_id = "";
        for (int i = 0; i < length; i++)
        {
            unique_id += chars[Random.Range(0, chars.Length - 1)];
        }
        return unique_id;
    }

    public static bool HasID(string id)
    {
        return dict_id.ContainsKey(id);
    }

    public static GameObject GetByID(string id)
    {
        if (dict_id.ContainsKey(id))
        {
            return dict_id[id].gameObject;
        }
        return null;
    }
}