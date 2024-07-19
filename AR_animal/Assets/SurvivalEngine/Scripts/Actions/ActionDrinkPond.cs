using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Action", menuName ="Data/Actions/DrinkPond", order =101)]
public class ActionDrinkPond : SAction
{
    public ItemData drink_item;

    public override void DoAction(PlayerCharacter character, Selectable select)
    {
        PlayerData.Get().AddAttributeValue(AttributeType.Health, drink_item.eat_hp, character.GetAttributeMax(AttributeType.Health));
        PlayerData.Get().AddAttributeValue(AttributeType.Hunger, drink_item.eat_hunger, character.GetAttributeMax(AttributeType.Hunger));
        PlayerData.Get().AddAttributeValue(AttributeType.Thirst, drink_item.eat_thirst, character.GetAttributeMax(AttributeType.Thirst));
        PlayerData.Get().AddAttributeValue(AttributeType.Happiness, drink_item.eat_happiness, character.GetAttributeMax(AttributeType.Happiness));

        character.TriggerAnim("Take");
    }
}
