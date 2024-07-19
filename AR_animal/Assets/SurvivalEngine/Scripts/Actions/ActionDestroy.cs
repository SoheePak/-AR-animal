using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Destroy", order =106)]
public class ActionDestroy : AAction
{
    public string animation;

    public override void DoAction(PlayerCharacter character, Selectable select)
    {
        select.GetDestructible().KillIn(0.5f);
        character.TriggerAnim(animation, select.transform.position);
    }

    public override bool CanDoAction(PlayerCharacter character, Selectable select)
    {
        return select.GetDestructible() && !select.GetDestructible().IsDead();
    }
}
