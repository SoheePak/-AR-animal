﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel that contain the detailed information of a single crafting item
/// </summary>

public class CraftInfoPanel : UIPanel
{
    public ItemSlot slot;
    public Text title;
    public Text desc;
    public Button craft_btn;

    public ItemSlot[] craft_slots;

    private CraftData data;

    protected override void Awake()
    {
        base.Awake();

    }

    private void RefreshPanel()
    {
        slot.SetSlot(data, 1, true);
        slot.AnimateGain();
        title.text = data.title;
        desc.text = data.desc;

        foreach (ItemSlot slot in craft_slots)
            slot.Hide();

        CraftCostData cost = data.GetCraftCost();
        int index = 0;
        foreach(KeyValuePair<ItemData, int> pair in cost.craft_items)
        {
            if (index < craft_slots.Length)
            {
                ItemSlot slot = craft_slots[index];
                slot.SetSlot(pair.Key, pair.Value, false);
                slot.ShowTitle();
            }
            index ++;
        }

        if (index < craft_slots.Length)
        {
            ItemSlot slot = craft_slots[index];
            if (cost.craft_near != null)
            {
                slot.SetSlotCustom(cost.craft_near.icon, cost.craft_near.title, false);
                slot.ShowTitle();
            }
        }

        PlayerCharacter player = PlayerCharacter.Get();
        craft_btn.interactable = player.CanCraft(data);
    }

    public void ShowData(CraftData item)
    {
        this.data = item;
        RefreshPanel();
        Show();
    }

    public void OnClickCraft()
    {
        PlayerCharacter player = PlayerCharacter.Get();

        if (player.CanCraft(data)){

            ItemData item = data.GetItem();
            ConstructionData construct = data.GetConstruction();
            PlantData plant = data.GetPlant();

            if (item != null)
                player.CraftItem(item);
            if (construct != null)
                player.SelectCraftConstruction(construct);
            if (plant != null)
                player.CraftPlant(plant);

            craft_btn.interactable = false;
            Hide();
        }
    }

    public override void Hide(bool instant = false)
    {
        base.Hide(instant);
        data = null;
    }

    public CraftData GetData()
    {
        return data;
    }
}
