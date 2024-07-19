using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/Sleep", order =101)]
public class ActionSleep : SAction
{
    public float sleep_hp_hour;
    public float sleep_hunger_hour;
    public float sleep_thirst_hour;
    public float sleep_hapiness_hour;

    public override void DoAction(PlayerCharacter character, Selectable select)
    {
        Construction construct = select.GetComponent<Construction>();
        if(construct != null)
            character.Sleep(this);


    }
}
