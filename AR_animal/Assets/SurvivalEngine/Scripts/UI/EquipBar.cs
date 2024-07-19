using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Shows currently equipped items
/// </summary>

public class EquipBar : MonoBehaviour
{
    public ItemSlot[] slots;

    public UnityAction<ItemSlot> onClickSlot;
    public UnityAction<ItemSlot> onRightClickSlot;

    private int selected_slot = -1;
    private int selected_right_slot = -1;

    private float timer = 0f;

    private static EquipBar _instance;

    void Awake()
    {
        _instance = this;

        for (int i = 0; i < slots.Length; i++)
        {
            int index = i; //Important to copy so not overwritten in loop
            slots[i].slot_index = index;
            slots[i].is_equip = true;
            slots[i].onClick += (CraftData item) => { OnClickSlot(index, item); };
            slots[i].onClickRight += (CraftData item) => { OnClickSlotRight(index, item); };
            slots[i].onClickLong += (CraftData item) => { OnClickSlotRight(index, item); };
        }
    }

    void Start()
    {
        PlayerControlsMouse.Get().onRightClick += (Vector3) => { CancelSelection(); };
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.1f)
        {
            timer = 0f;
            SlowUpdate();
        }
    }

    void SlowUpdate()
    {
        RefreshEquip();
    }

    private void RefreshEquip()
    {
        PlayerData pdata = PlayerData.Get();
        for (int i = 0; i < slots.Length; i++)
        {
            ItemData idata = pdata.GetEquippedItem(i);
            if (idata != null)
            {
                slots[i].SetSlot(idata, 1, selected_slot == i);
            }
            else
            {
                slots[i].SetSlot(null, 0, false);
            }
        }
    }

    private void OnClickSlot(int slot, CraftData item)
    {
        PlayerControlsMouse controls = PlayerControlsMouse.Get();

        ItemSlot islot = InventoryBar.Get().GetSelectedSlot();
        ItemSlot eslot = GetSlot(slot);
        selected_right_slot = -1;

        //Merge items
        if (eslot != null && islot != null)
        {
            ItemSlot slot1 = eslot;
            ItemSlot slot2 = islot;
            ItemData item1 = slot1.GetItem();
            ItemData item2 = slot2.GetItem();
            MAction action1 = item1 != null ? item1.FindMergeAction(item2) : null;
            MAction action2 = item2 != null ? item2.FindMergeAction(item1) : null;

            if (action1 != null && action1.CanDoAction(PlayerCharacter.Get(), slot2))
            {
                action1.DoAction(PlayerCharacter.Get(), slot1, slot2);
                TheUI.Get().CancelSelection();
                return;
            }

            else if (action2 != null && action2.CanDoAction(PlayerCharacter.Get(), slot1))
            {
                action2.DoAction(PlayerCharacter.Get(), slot2, slot1);
                TheUI.Get().CancelSelection();
                return;
            }
        }

        if (islot != null)
        {
            ItemData idata = islot.GetItem();
            if (idata != null && idata.type == ItemType.Equipment)
            {
                PlayerData.Get().EquipItemTo(islot.slot_index, ItemData.GetEquipIndex(idata.equip_slot));
                TheUI.Get().CancelSelection();
            }
        }
        else if (item != null && slot != selected_slot)
        {
            TheUI.Get().CancelSelection();
            selected_slot = slot;
        }
        else
        {
            CancelSelection();
        }

        if (onClickSlot != null && eslot != null)
            onClickSlot.Invoke(eslot);
    }

    private void OnClickSlotRight(int slot, CraftData item)
    {
        selected_slot = -1;
        selected_right_slot = -1;
        ActionSelectorUI.Get().Hide();

        if (item != null && item.GetItem() != null && item.GetItem().actions.Length > 0)
        {
            selected_right_slot = slot;
            ActionSelectorUI.Get().Show(PlayerCharacter.Get(), slots[slot]);
        }

        ItemSlot eslot = GetSlot(slot);
        if (onRightClickSlot != null && eslot != null)
            onRightClickSlot.Invoke(eslot);
    }

    public void CancelSelection()
    {
        selected_slot = -1;
    }

    public bool HasSlotSelected()
    {
        return selected_slot >= 0;
    }

    public int GetSelectedSlotIndex()
    {
        return selected_slot;
    }

    public ItemSlot GetSlot(int index)
    {
        if (index >= 0 && index < slots.Length)
            return slots[index];
        return null;
    }

    public ItemSlot GetSelectedSlot()
    {
        if (selected_slot >= 0 && selected_slot < slots.Length)
            return slots[selected_slot];
        return null;
    }

    public static EquipBar Get()
    {
        return _instance;
    }
}
