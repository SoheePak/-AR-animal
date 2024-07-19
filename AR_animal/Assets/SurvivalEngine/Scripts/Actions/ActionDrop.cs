using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Drop", order =103)]
public class ActionDrop : SAction
{

    public override void DoAction(PlayerCharacter character, ItemSlot slot)
    {
        if (slot.is_equip)
            character.DropEquippedItem(slot.slot_index);
        else
            character.DropItem(slot.slot_index);
    }
}
