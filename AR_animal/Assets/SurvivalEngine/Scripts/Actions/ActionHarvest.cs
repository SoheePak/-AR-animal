using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Action", menuName = "Data/Actions/Harvest", order = 101)]
public class ActionHarvest : AAction
{
    public override void DoAction(PlayerCharacter character, Selectable select)
    {
        Plant plant = select.GetComponent<Plant>();
        if (plant != null)
        {
            plant.Gather(character);
        }
    }

    public override bool CanDoAction(PlayerCharacter character, Selectable select)
    {
        Plant plant = select.GetComponent<Plant>();
        if (plant != null)
        {
            return plant.HasFruit();
        }
        return false;
    }
}
