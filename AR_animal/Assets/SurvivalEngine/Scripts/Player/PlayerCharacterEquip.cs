using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script spawn and unspawn equipment attachments on the character
/// </summary>

[RequireComponent(typeof(PlayerCharacter))]
public class PlayerCharacterEquip : MonoBehaviour
{
    private PlayerCharacter character;

    private Dictionary<string, EquipItem> equipped_items = new Dictionary<string, EquipItem>();

    void Awake()
    {
        character = GetComponent<PlayerCharacter>();
    }

    void Update()
    {
        PlayerData pdata = PlayerData.Get();
        HashSet<string> equipped_data = new HashSet<string>();
        List<string> remove_list = new List<string>();
        //Equip unequip
        foreach (KeyValuePair<int, string> item in pdata.equipped_items)
        {
            equipped_data.Add(item.Value);
            if (!equipped_items.ContainsKey(item.Value))
                EquipAddedItem(item.Key, item.Value);
        }

        //Create remove list
        foreach (KeyValuePair<string, EquipItem> item in equipped_items)
        {
            if (!equipped_data.Contains(item.Key))
                remove_list.Add(item.Key);
        }

        //Remove
        foreach (string item_id in remove_list)
        {
            UnequipRemovedItem(item_id);
        }
    }

    public void EquipAddedItem(int slot, string item_id)
    {
        ItemData idata = ItemData.Get(item_id);
        if (idata != null && idata.equipped_prefab != null)
        {
            GameObject equip_obj = Instantiate(idata.equipped_prefab, transform.position, Quaternion.identity);
            EquipItem eitem = equip_obj.GetComponent<EquipItem>();
            if (eitem != null)
                eitem.target = character.GetEquipAttach(idata.equip_slot, idata.equip_side);
            equipped_items.Add(item_id, eitem);
        }
        else
        {
            equipped_items.Add(item_id, null);
        }
    }

    public void UnequipRemovedItem(string item_id)
    {
        if (equipped_items.ContainsKey(item_id))
        {
            EquipItem eitem = equipped_items[item_id];
            equipped_items.Remove(item_id);
            if(eitem != null)
                Destroy(eitem.gameObject);
        }
    }

    public EquipItem GetEquippedItem(EquipSlot slot)
    {
        ItemData equipped = character.GetEquippedItem(slot);
        if (equipped != null)
        {
            foreach (KeyValuePair<string, EquipItem> item in equipped_items)
            {
                if (item.Key == equipped.id)
                    return item.Value;
            }
        }
        return null;
    }

}
