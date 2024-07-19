using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// PlayerData is the main save file data script. Everything contained in this script is what will be saved. 
/// It also contains a lot of functions to easily access the saved data. Make sure to call Save() to write the data to a file on the disk.
/// The latest save file will be loaded automatically when starting the game
/// </summary>

[System.Serializable]
public struct InventoryItemData
{
    public string item_id;
    public int quantity;

    public InventoryItemData(string id, int q) { item_id = id; quantity = q; }
}

[System.Serializable]
public class DroppedItemData {
    public string uid;
    public string item_id;
    public string scene;
    public Vector3Data pos;
    public int quantity;
}

[System.Serializable]
public class BuiltConstructionData
{
    public string uid;
    public string construction_id;
    public string scene;
    public Vector3Data pos;
}

[System.Serializable]
public class SowedPlantData
{
    public string uid;
    public string plant_id;
    public string scene;
    public Vector3Data pos;
    public int growth_stage;
}

[System.Serializable]
public class PlayerData {

    public const string VERSION = "0.01";

    public const int inventory_size = 15; //Maximum number of items, dont change unless you also change the UI to reflect that

    public string filename;
    public string version;
    public DateTime last_save;

    //-------------------

    public string current_scene = "";
    public int current_entry_index = 0; //-1 means go to current_pos, 0 means default scene pos, >0 means at matching entry index
    public Vector3Data current_pos;

    public int day = 0;
    public float day_time = 0f; // 0 = midnight, 24 = end of day

    public float master_volume = 1f;
    public float music_volume = 1f;
    public float sfx_volume = 1f;

    public Dictionary<AttributeType, float> attributes = new Dictionary<AttributeType, float>();
    public Dictionary<string, int> unique_ids = new Dictionary<string, int>();
    
    public Dictionary<int, InventoryItemData> inventory = new Dictionary<int, InventoryItemData>();
    public Dictionary<int, string> equipped_items = new Dictionary<int, string>(); //0=hand, 1=head, 2=body, 3=boot

    public Dictionary<string, int> removed_objects = new Dictionary<string, int>();
    public Dictionary<string, DroppedItemData> dropped_items = new Dictionary<string, DroppedItemData>();
    public Dictionary<string, BuiltConstructionData> built_constructions = new Dictionary<string, BuiltConstructionData>();
    public Dictionary<string, SowedPlantData> sowed_plants = new Dictionary<string, SowedPlantData>();


    //-------------------

    public static PlayerData player_data = null;
    
    public PlayerData(string name)
    {
        filename = name;
        version = VERSION;
        current_scene = "LargeMap";

        day = 1;
        day_time = 6f; // Start game at 6 in the morning

        master_volume = 1f;
        music_volume = 1f;
        sfx_volume = 1f;

    }

    public void FixData()
    {
        //Fix data to make sure old save files compatible with new game version
        if (attributes == null)
            attributes = new Dictionary<AttributeType, float>();
        if (unique_ids == null)
            unique_ids = new Dictionary<string, int>();
        if (inventory == null)
            inventory = new Dictionary<int, InventoryItemData>();
        if (equipped_items == null)
            equipped_items = new Dictionary<int, string>();
        if (dropped_items == null)
            dropped_items = new Dictionary<string, DroppedItemData>();
        if (removed_objects == null)
            removed_objects = new Dictionary<string, int>();
        if (built_constructions == null)
            built_constructions = new Dictionary<string, BuiltConstructionData>();
        if (sowed_plants == null)
            sowed_plants = new Dictionary<string, SowedPlantData>();
    }
    
    //---- Items -----
    public int AddItem(string item_id, int quantity)
    {
        ItemData idata = ItemData.Get(item_id);
        int max = idata != null ? idata.inventory_max : 999;
        int slot = GetFirstItemSlot(item_id, max - quantity);
        if (slot < 0)
            slot = GetFirstEmptySlot(); 

        if (slot >= 0)
        {
            AddItemAt(item_id, slot, quantity);
        }
        return slot;
    }

    public void RemoveItem(string item_id, int quantity)
    {
        Dictionary<int, int> remove_list = new Dictionary<int, int>(); //Slot, Quantity
        foreach (KeyValuePair<int, InventoryItemData> pair in inventory)
        {
            if (pair.Value.item_id == item_id && pair.Value.quantity > 0 && quantity > 0)
            {
                int remove = Mathf.Min(quantity, pair.Value.quantity);
                remove_list.Add(pair.Key, remove);
                quantity -= remove;
            }
        }

        foreach (KeyValuePair<int, int> pair in remove_list)
        {
            RemoveItemAt(pair.Key, pair.Value);
        }
    }

    public void AddItemAt(string item_id, int slot, int quantity)
    {
        InventoryItemData invt_slot = GetItemSlot(slot);
        if (invt_slot.item_id == item_id)
        {
            int amount = invt_slot.quantity + quantity;
            inventory[slot] = new InventoryItemData(item_id, amount);
        }
        else if (invt_slot.quantity <= 0)
        {
            inventory[slot] = new InventoryItemData(item_id, quantity);
        }
    }

    public void RemoveItemAt(int slot, int quantity)
    {
        InventoryItemData invt_slot = GetItemSlot(slot);
        if (invt_slot.quantity > 0)
        {
            int amount = invt_slot.quantity - quantity;
            if (amount <= 0)
                inventory.Remove(slot);
            else
                inventory[slot] = new InventoryItemData(invt_slot.item_id, amount);
        }
    }

    public void SwapItemSlots(int slot1, int slot2)
    {
        InventoryItemData invt_slot1 = GetItemSlot(slot1);
        InventoryItemData invt_slot2 = GetItemSlot(slot2);
        inventory[slot1] = invt_slot2;
        inventory[slot2] = invt_slot1;
    }

    public int CountItemType(string item_id)
    {
        int value = 0;
        foreach (KeyValuePair<int, InventoryItemData> pair in inventory)
        {
            if (pair.Value.item_id == item_id)
                value += pair.Value.quantity;
        }
        return value;
    }

    public int GetFirstItemSlot(string item_id, int max=999999)
    {
        foreach (KeyValuePair<int, InventoryItemData> pair in inventory)
        {
            if (pair.Key < inventory_size && pair.Value.item_id == item_id && pair.Value.quantity <= max)
                return pair.Key;
        }
        return -1;
    }

    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < inventory_size; i++)
        {
            InventoryItemData invdata = GetItemSlot(i);
            if (invdata.quantity <= 0)
                return i;
        }
        return -1;
    }

    public InventoryItemData GetItemSlot(int slot)
    {
        if(inventory.ContainsKey(slot))
            return inventory[slot];
        return new InventoryItemData("", 0);
    }

    public bool CanTakeItem(string item_id, int quantity)
    {
        ItemData idata = ItemData.Get(item_id);
        int max = idata != null ? idata.inventory_max : 999;
        int slot = GetFirstItemSlot(item_id, max - quantity);
        if (slot < 0)
            slot = GetFirstEmptySlot(); 
        return slot >= 0;
    }

    public bool HasItem(string item_id, int quantity=1)
    {
        return CountItemType(item_id) >= quantity;
    }


    public bool HasItemIn(int slot)
    {
        return inventory.ContainsKey(slot) && inventory[slot].quantity > 0;
    }

    public bool IsItemIn(string item_id, int slot)
    {
        return inventory.ContainsKey(slot) && inventory[slot].item_id == item_id && inventory[slot].quantity > 0;
    }

    //-------- Dropped items --------

    public DroppedItemData AddDroppedItem(string item_id, string scene, Vector3 pos, int quantity)
    {
        DroppedItemData ditem = new DroppedItemData();
        ditem.uid = UniqueID.GenerateUniqueID();
        ditem.item_id = item_id;
        ditem.scene = scene;
        ditem.pos = pos;
        ditem.quantity = quantity;
        dropped_items[ditem.uid] = ditem;
        return ditem;
    }

    public void RemoveDroppedItem(string uid)
    {
        if (dropped_items.ContainsKey(uid))
            dropped_items.Remove(uid);
    }

    public DroppedItemData GetDroppedItem(string uid)
    {
        if (dropped_items.ContainsKey(uid))
            return dropped_items[uid];
        return null;
    }

    public BuiltConstructionData AddConstruction(string construct_id, string scene, Vector3 pos)
    {
        BuiltConstructionData citem = new BuiltConstructionData();
        citem.uid = UniqueID.GenerateUniqueID();
        citem.construction_id = construct_id;
        citem.scene = scene;
        citem.pos = pos;
        built_constructions[citem.uid] = citem;
        return citem;
    }

    public void RemoveConstruction(string uid)
    {
        if (built_constructions.ContainsKey(uid))
            built_constructions.Remove(uid);
    }

    public BuiltConstructionData GetConstructed(string uid)
    {
        if (built_constructions.ContainsKey(uid))
            return built_constructions[uid];
        return null;
    }

    public SowedPlantData AddPlant(string plant_id, string scene, Vector3 pos, int stage)
    {
        SowedPlantData citem = new SowedPlantData();
        citem.uid = UniqueID.GenerateUniqueID();
        citem.plant_id = plant_id;
        citem.scene = scene;
        citem.pos = pos;
        citem.growth_stage = stage;
        sowed_plants[citem.uid] = citem;
        return citem;
    }

    public void GrowPlant(string plant_uid, int stage)
    {
        if (sowed_plants.ContainsKey(plant_uid))
            sowed_plants[plant_uid].growth_stage = stage;
    }

    public void RemovePlant(string uid)
    {
        if (sowed_plants.ContainsKey(uid))
            sowed_plants.Remove(uid);
    }

    public SowedPlantData GetSowedPlant(string uid)
    {
        if (sowed_plants.ContainsKey(uid))
            return sowed_plants[uid];
        return null;
    }

    // ----- Equip Items ---- (islot=inventory, eslot=equipped)

    public void EquipItemTo(int islot, int eslot)
    {
        InventoryItemData invt_slot = GetItemSlot(islot);
        ItemData idata = ItemData.Get(invt_slot.item_id);
        ItemData edata = GetEquippedItem(eslot);
        if (invt_slot.quantity > 0 && idata != null && eslot >= 0)
        {
            if (edata == null)
            {
                //Equip only
                EquipItem(eslot, idata.id);
                RemoveItemAt(islot, 1);
            }
            else if(invt_slot.quantity ==1 && idata.type == ItemType.Equipment)
            {
                //Swap
                RemoveItemAt(islot, 1);
                UnequipItem(eslot);
                EquipItem(eslot, idata.id);
                AddItemAt(edata.id, islot, 1);
            }
        }
    }

    public void UnequipItemTo(int eslot, int islot)
    {
        InventoryItemData invt_slot = GetItemSlot(islot);
        ItemData idata = ItemData.Get(invt_slot.item_id);
        ItemData edata = GetEquippedItem(eslot);
        if (edata != null)
        {
            bool same_item = invt_slot.quantity > 0 && idata != null && idata.id == edata.id && invt_slot.quantity < idata.inventory_max;
            bool slot_empty = invt_slot.quantity <= 0;
            if (same_item || slot_empty)
            {
                //Unequip
                UnequipItem(eslot);
                AddItemAt(edata.id, islot, 1);
            }
            else if (idata != null && !same_item && idata.type == ItemType.Equipment && idata.equip_slot == edata.equip_slot && invt_slot.quantity == 1)
            {
                //swap
                RemoveItemAt(islot, 1);
                UnequipItem(eslot);
                EquipItem(eslot, idata.id);
                AddItemAt(edata.id, islot, 1);
            }
        }
    }

    public void EquipItem(int eslot, string item_id)
    {
        equipped_items[eslot] = item_id;
    }

    public void UnequipItem(int eslot)
    {
        if(equipped_items.ContainsKey(eslot))
            equipped_items.Remove(eslot);
    }

    public string GetEquippedItemID(int eslot) {
        if (equipped_items.ContainsKey(eslot))
            return equipped_items[eslot];
        return "";
    }

    public ItemData GetEquippedItem(int eslot) {
        string id = GetEquippedItemID(eslot);
        return ItemData.Get(id);
    }

    //---- Destructibles -----

    public void RemoveObject(string uid)
    {
        if(!string.IsNullOrEmpty(uid))
            removed_objects[uid] = 1;
    }

    public bool IsObjectRemoved(string uid)
    {
        if (removed_objects.ContainsKey(uid))
            return removed_objects[uid] > 0;
        return false;
    }

    // ---- Unique Ids (Custom data) ----
    public void SetUniqueID(string unique_id, int val)
    {
        if (!string.IsNullOrEmpty(unique_id))
        {
            if (!unique_ids.ContainsKey(unique_id))
                unique_ids[unique_id] = val;
        }
    }

    public void RemoveUniqueID(string unique_id)
    {
        if (unique_ids.ContainsKey(unique_id))
            unique_ids.Remove(unique_id);
    }

    public int GetUniqueID(string unique_id)
    {
        if (unique_ids.ContainsKey(unique_id))
            return unique_ids[unique_id];
        return 0;
    }

    public bool HasUniqueID(string unique_id)
    {
        return unique_ids.ContainsKey(unique_id);
    }

    //--- Attributes ----

    public bool HasAttribute(AttributeType type)
    {
        return attributes.ContainsKey(type);
    }

    public float GetAttributeValue(AttributeType type)
    {
        if (attributes.ContainsKey(type))
            return attributes[type];
        return 0f;
    }

    public void SetAttributeValue(AttributeType type, float value)
    {
        attributes[type] = value;
    }

    public void AddAttributeValue(AttributeType type, float value, float max)
    {
        if (!attributes.ContainsKey(type))
            attributes[type] = value;
        else
            attributes[type] += value;

        attributes[type] = Mathf.Clamp(attributes[type], 0f, max);
    }

    //--- Save / load -----

    public static void NewGame()
    {
        NewGame("player"); //default name
    }

    public static void NewGame(string name)
    {
        SaveSystem.Unload();
        player_data = new PlayerData(name);
        player_data.FixData();
    }

    public void Save()
    {
        last_save = System.DateTime.Now;
        version = VERSION;
        SaveSystem.Save(filename, player_data);
    }

    public void Restart()
    {
        player_data = new PlayerData(filename);
        player_data.FixData();
    }

    public static void Unload()
    {
        player_data = null;
        SaveSystem.Unload();
    }

    public void Delete()
    {
        SaveSystem.Delete(filename);
        player_data = new PlayerData(filename);
    }

    public static void LoadLast()
    {
        string name = SaveSystem.GetLastSave();
        if (string.IsNullOrEmpty(name))
            name = "player"; //Default name
        Load(name);
    }

    public static void Load(string name)
    {
        if (player_data == null)
            player_data = SaveSystem.Load(name);
        if (player_data == null)
            player_data = new PlayerData(name);
        player_data.FixData();
    }
    
    public static PlayerData Get()
    {
        return player_data;
    }
}
