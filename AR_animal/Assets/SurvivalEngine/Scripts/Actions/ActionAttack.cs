using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Attack", order =102)]
public class ActionAttack : SAction
{
    public override void DoAction(PlayerCharacter character, Selectable select)
    {
        if (select.GetDestructible())
        {
            character.Attack(select.GetDestructible());
        }
    }
}
