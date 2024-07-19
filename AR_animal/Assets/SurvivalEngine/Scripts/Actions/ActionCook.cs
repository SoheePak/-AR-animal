using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Action", menuName = "Data/Actions/Cook", order = 100)]
public class ActionCook : MAction
{
    public ItemData cooked_item;

    public override void DoAction(PlayerCharacter character, ItemSlot slot, Selectable select)
    {
        if (PlayerData.Get().CanTakeItem(cooked_item.id, 1))
        {
            PlayerData.Get().RemoveItemAt(slot.slot_index, 1);
            int islot = PlayerData.Get().AddItem(cooked_item.id, 1);

            //Take fx
            ItemTakeFX.DoTakeFX(select.transform.position, cooked_item, islot);
        }

    }
}
