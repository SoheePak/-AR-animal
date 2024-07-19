using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Drink", order =100)]
public class ActionDrink : SAction
{
    public ItemData empty_item;

    public override void DoAction(PlayerCharacter character, ItemSlot slot)
    {
        ItemData item = slot.GetItem();
        if (item != null)
        {
            character.EatItem(item, slot.slot_index);
            PlayerData.Get().AddItem(empty_item.id, 1);
        }
    }

    public override bool CanDoAction(PlayerCharacter character, ItemSlot slot)
    {
        ItemData item = slot.GetItem();
        return item != null && item.type == ItemType.Consumable;
    }
}
