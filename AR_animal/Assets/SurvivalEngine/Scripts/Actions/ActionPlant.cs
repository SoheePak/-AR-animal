using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Plant", order =100)]
public class ActionPlant : SAction
{
    public override void DoAction(PlayerCharacter character, ItemSlot slot)
    {
        ItemData item = slot.GetItem();
        if (item != null && item.plant_data != null)
        {
            Vector3 pos = character.transform.position + character.transform.forward * 0.4f;
            Plant nearest = Plant.GetNearest(pos, item.plant_data.sow_radius);
            if (nearest == null) //Make sure theres not already a plant there
            {
                PlayerData.Get().RemoveItemAt(slot.slot_index, 1);
                Plant.Create(item.plant_data, pos, 0);
            }
        }
    }

    public override bool CanDoAction(PlayerCharacter character, ItemSlot slot)
    {
        ItemData item = slot.GetItem();
        return item != null && item.plant_data != null;
    }
}
