using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Fish", order =100)]
public class ActionFish : SAction
{
    public GroupData fishing_rod;

    public override void DoAction(PlayerCharacter character, Selectable select)
    {
        if (select != null)
        {
            character.FaceTorward(select.transform.position);

            ItemProvider pond = select.GetComponent<ItemProvider>();
            if (pond != null)
            {
                if (pond.HasItem())
                {
                    pond.TakeItem();
                    PlayerCharacter.Get().GainItem(select.gameObject, pond.item, 1, false);
                }
            }
        }
    }

    public override bool CanDoAction(PlayerCharacter character, Selectable select)
    {
        ItemProvider pond = select.GetComponent<ItemProvider>();
        return pond != null && pond.HasItem() && character.HasEquippedItemInGroup(fishing_rod);
    }
}
