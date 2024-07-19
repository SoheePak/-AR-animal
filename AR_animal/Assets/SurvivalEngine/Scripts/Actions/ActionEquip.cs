using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Equip", order =100)]
public class ActionEquip : SAction
{

    public override void DoAction(PlayerCharacter character, ItemSlot slot)
    {
        ItemData item = slot.GetItem();
        if (item != null && item.type == ItemType.Equipment)
        {
            if (slot.is_equip)
            {
                character.UnEquipItem(item, slot.slot_index);
            }
            else
            {
                character.EquipItem(item, slot.slot_index);
            }
        }
    }

    public override bool CanDoAction(PlayerCharacter character, ItemSlot slot)
    {
        ItemData item = slot.GetItem();
        return item != null && item.type == ItemType.Equipment;
    }
}
