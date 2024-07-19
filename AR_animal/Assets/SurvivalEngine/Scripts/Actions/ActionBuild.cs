using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Build", order =103)]
public class ActionBuild : SAction
{
    public override void DoAction(PlayerCharacter character, ItemSlot slot)
    {
        character.BuildItem(slot.slot_index, character.transform.position);
    }
}
