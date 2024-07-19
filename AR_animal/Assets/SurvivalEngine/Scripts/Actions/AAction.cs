using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automatic Action parent class: Any action performed automatically when the object is clicked on (selectable only)
/// Don't put more than 1 per selectable as only the first one will work
/// </summary>

public class AAction : SAction
{
    //When using an action on a Selectable in the scene
    public override void DoAction(PlayerCharacter character, Selectable select)
    {

    }

    //Condition to check if the action is possible, override to add a condition
    public override bool CanDoAction(PlayerCharacter character, Selectable select)
    {
        return true;
    }
}
