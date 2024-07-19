using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Action", menuName = "Data/Actions/AddFuel", order = 100)]
public class ActionAddFuel : MAction
{
    public override void DoAction(PlayerCharacter character, ItemSlot slot, Selectable select)
    {
        PlayerData pdata = PlayerData.Get();
        Firepit fire = select.GetComponent<Firepit>();
        if (fire != null && slot.GetItem() && pdata.HasItem(slot.GetItem().id))
        {
            fire.AddFuel(fire.wood_add_fuel);
            pdata.RemoveItemAt(slot.slot_index, 1);
        }

    }
}
