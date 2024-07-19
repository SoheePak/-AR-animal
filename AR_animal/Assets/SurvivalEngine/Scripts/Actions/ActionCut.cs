using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Action", menuName = "Data/Actions/Cut", order = 100)]
public class ActionCut : MAction
{
    public ItemData cut_item;

    public override void DoAction(PlayerCharacter character, ItemSlot slot1, ItemSlot slot2)
    {
        if (PlayerData.Get().CanTakeItem(cut_item.id, 1))
        {
            PlayerData.Get().RemoveItemAt(slot1.slot_index, 1);
            int islot = PlayerData.Get().AddItem(cut_item.id, 1);

            //Take fx
            ItemTakeFX.DoTakeFX(character.transform.position, cut_item, islot);
        }
    }
}
