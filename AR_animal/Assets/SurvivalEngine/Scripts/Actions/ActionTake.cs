using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Take", order =101)]
public class ActionTake : SAction
{
    public ItemData take_item;

    public override void DoAction(PlayerCharacter character, Selectable select)
    {
        PlayerData pdata = PlayerData.Get();
        if (pdata.CanTakeItem(take_item.id, 1))
        {
            pdata.AddItem(take_item.id, 1);
            select.Destroy();
        }
    }

}
