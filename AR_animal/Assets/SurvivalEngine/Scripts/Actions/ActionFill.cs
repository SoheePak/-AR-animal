using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Fill", order =100)]
public class ActionFill : MAction
{
    public ItemData filled_item;
    public float fill_range = 2f;

    public override void DoAction(PlayerCharacter character, ItemSlot slot)
    {
        if (PlayerData.Get().CanTakeItem(filled_item.id, 1))
        {
            PlayerData.Get().RemoveItemAt(slot.slot_index, 1);
            PlayerData.Get().AddItem(filled_item.id, 1);
        }
    }

    public override void DoAction(PlayerCharacter character, ItemSlot slot, Selectable select)
    {
        if (select.HasGroup(target_groups))
        {
            if (PlayerData.Get().CanTakeItem(filled_item.id, 1))
            {
                PlayerData.Get().RemoveItemAt(slot.slot_index, 1);
                int islot = PlayerData.Get().AddItem(filled_item.id, 1);

                //Take fx
                ItemTakeFX.DoTakeFX(select.transform.position, filled_item, islot);
            }
        }
    }

    public override bool CanDoAction(PlayerCharacter character, ItemSlot slot)
    {
        Selectable water_source = Selectable.GetNearestGroup(target_groups, character.transform.position, fill_range);
        return water_source != null && PlayerData.Get().CanTakeItem(filled_item.id, 1);
    }
}
