using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Split", order =107)]
public class ActionSplit : SAction
{
    public override void DoAction(PlayerCharacter character, ItemSlot slot)
    {
        int new_slot = PlayerData.Get().GetFirstEmptySlot();
        if (new_slot >= 0)
        {
            int half = slot.GetQuantity() / 2;
            ItemData item = slot.GetItem();
            PlayerData.Get().RemoveItemAt(slot.slot_index, half);
            PlayerData.Get().AddItemAt(item.id, new_slot, half);
        }
    }

    public override bool CanDoAction(PlayerCharacter character, ItemSlot slot)
    {
        ItemData item = slot.GetItem();
        return item != null && slot.GetQuantity() > 1 && PlayerData.Get().CanTakeItem(item.id, 1) && PlayerData.Get().GetFirstEmptySlot() >= 0;
    }
}
