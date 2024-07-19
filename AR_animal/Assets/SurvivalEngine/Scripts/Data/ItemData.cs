using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType {
    
    Basic=0,
    Consumable=10,
    Equipment=20,

}

public enum EquipSlot
{
    None = 0,
    Hand = 10,
    Head = 20,
    Body = 30,
    Feet = 40,
}

public enum EquipSide
{
    Default=0,
    Right=2,
    Left=4,
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Data/ItemData", order = 2)]
public class ItemData : CraftData
{
    [Header("--- ItemData ------------------")]
    public ItemType type;

    [Header("Stats")]
    public int inventory_max = 20;

    [Header("Stats Equip")]
    public EquipSlot equip_slot;
    public EquipSide equip_side;
    public bool weapon;
    public bool ranged;
    public int damage = 0;
    public int armor = 0;
    public float range = 1f;

    [Header("Stats Consume")]
    public int eat_hp = 0;
    public int eat_hunger = 0;
    public int eat_thirst = 0;
    public int eat_happiness = 0;

    [Header("Action")]
    public SAction[] actions;

    [Header("Ref Data")]
    public ItemData container_data;
    public PlantData plant_data;
    public ConstructionData construction_data;
    public GroupData projectile_group;

    [Header("Prefab")]
    public GameObject item_prefab;
    public GameObject equipped_prefab;
    public GameObject projectile_prefab;
   

    private static List<ItemData> item_data = new List<ItemData>(); //For looping
    private static Dictionary<string, ItemData> item_dict = new Dictionary<string, ItemData>(); //Faster access

    public MAction FindMergeAction(ItemData other)
    {
        if (other == null)
            return null;

        foreach (SAction action in actions)
        {
            if (action is MAction)
            {
                MAction maction = (MAction)action;
                if (other.HasGroup(maction.target_groups))
                {
                    return maction;
                }
            }
        }
        return null;
    }

    public MAction FindMergeAction(Selectable other)
    {
        if (other == null)
            return null;

        foreach (SAction action in actions)
        {
            if (action is MAction)
            {
                MAction maction = (MAction)action;
                if (other.HasGroup(maction.target_groups))
                {
                    return maction;
                }
            }
        }
        return null;
    }

    public bool CanBeDropped()
    {
        return item_prefab != null;
    }

    public bool CanBeBuilt()
    {
        return construction_data != null;
    }

    public bool CanBeSowed()
    {
        return plant_data != null;
    }

    public static void Load(string items_folder)
    {
        item_data.Clear();
        item_dict.Clear();
        item_data.AddRange(Resources.LoadAll<ItemData>(items_folder));
        foreach (ItemData item in item_data)
        {
            item_dict.Add(item.id, item);
        }
    }

    public new static ItemData Get(string item_id)
    {
        if (item_dict.ContainsKey(item_id))
            return item_dict[item_id];
        return null;
    }

    public new static List<ItemData> GetAll()
    {
        return item_data;
    }

    public static int GetEquipIndex(EquipSlot slot)
    {
        if (slot == EquipSlot.Hand)
            return 0;
        if (slot == EquipSlot.Head)
            return 1;
        if (slot == EquipSlot.Body)
            return 2;
        if (slot == EquipSlot.Feet)
            return 3;
        return -1;
    }

    public static EquipSlot GetEquipType(int index)
    {
        if (index == 0)
            return EquipSlot.Hand;
        if (index == 1)
            return EquipSlot.Head;
        if (index == 2)
            return EquipSlot.Body;
        if (index == 3)
            return EquipSlot.Feet;
        return EquipSlot.None;
    }
}
