using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Main Inventory bar that list all items in your inventory
/// </summary>

public class InventoryBar : MonoBehaviour
{
    public ItemSlot[] slots;

    public UnityAction<ItemSlot> onClickSlot;
    public UnityAction<ItemSlot> onRightClickSlot;

    private int selected_slot = -1;
    private int selected_right_slot = -1;

    private float timer = 0f;

    private static InventoryBar _instance;

    void Awake()
    {
        _instance = this;

        for (int i = 0; i < slots.Length; i++)
        {
            int index = i; //Important to copy so not overwritten in loop
            slots[i].slot_index = index;
            slots[i].onClick += (CraftData item) => { OnClickSlot(index, item); };
            slots[i].onClickRight += (CraftData item) => { OnClickSlotRight(index, item); };
            slots[i].onClickLong += (CraftData item) => { OnClickSlotRight(index, item); };
        }
    }

    private void Start()
    {
        if (!TheGame.IsMobile() && ItemSelectedFX.Get() == null)
            Instantiate(GameData.Get().item_select_fx, transform.position, Quaternion.identity);

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
        RefreshInventory();
    }

    private void RefreshInventory()
    {
        PlayerData pdata = PlayerData.Get();
        for (int i = 0; i < slots.Length; i++)
        {
            InventoryItemData invdata = pdata.GetItemSlot(i);
            ItemData idata = ItemData.Get(invdata.item_id);
            if (idata != null)
            {
                slots[i].SetSlot(idata, invdata.quantity, selected_slot == i || selected_right_slot == i);
            }
            else
            {
                slots[i].SetSlot(null, 0, false);
            }
        }
    }

    private void OnClickSlot(int slot, CraftData item)
    {
        ActionSelectorUI.Get().Hide();
        selected_right_slot = -1;

        ItemSlot cslot = GetSlot(slot);
        ItemSlot eslot = EquipBar.Get().GetSelectedSlot();
        ItemSlot sslot = eslot != null ? eslot : GetSelectedSlot();

        //Merge items
        if (sslot != null && cslot != null)
        {
            ItemSlot slot1 = cslot;
            ItemSlot slot2 = sslot;
            ItemData item1 = slot1.GetItem();
            ItemData item2 = slot2.GetItem();
            MAction action1 = item1 != null ? item1.FindMergeAction(item2) : null;
            MAction action2 = item2 != null ? item2.FindMergeAction(item1) : null;

            if (item1 != null && item2 != null) {

                //Same item, combine stacks
                if (item1 == item2)
                {
                    if (slot1.GetQuantity() + slot2.GetQuantity() <= item1.inventory_max)
                    {
                        int quantity = slot2.GetQuantity();
                        PlayerData.Get().RemoveItemAt(slot2.slot_index, quantity);
                        PlayerData.Get().AddItemAt(item1.id, slot1.slot_index, quantity);
                        CancelSelection();
                        return;
                    }
                }
                //Else, use merge action
                else if (action1 != null && action1.CanDoAction(PlayerCharacter.Get(), slot2))
                {
                    action1.DoAction(PlayerCharacter.Get(), slot1, slot2);
                    CancelSelection();
                    return;
                }

                else if (action2 != null && action2.CanDoAction(PlayerCharacter.Get(), slot1))
                {
                    action2.DoAction(PlayerCharacter.Get(), slot2, slot1);
                    CancelSelection();
                    return;
                }
            }
        }

        //Swap equipped with item
        if (eslot != null)
        {
            PlayerData.Get().UnequipItemTo(eslot.slot_index, slot);
            TheUI.Get().CancelSelection();
        }
        //Swap two items
        else if (HasSlotSelected())
        {
            if (slot != selected_slot)
            {
                PlayerData.Get().SwapItemSlots(slot, selected_slot);
            }

            CancelSelection();
        }
        else
        {
            if (item != null)
            {
                TheUI.Get().CancelSelection();
                selected_slot = slot;
            }
        }

        if (onClickSlot != null && cslot != null)
            onClickSlot.Invoke(cslot);
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

        ItemSlot cslot = GetSlot(slot);
        if (onRightClickSlot != null && cslot != null)
            onRightClickSlot.Invoke(cslot);
    }

    public void CancelSelection()
    {
        selected_slot = -1;
        selected_right_slot = -1;
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

    public Vector3 GetSlotWorldPosition(int slot)
    {
        if (slot >= 0 && slot < slots.Length)
        {
            RectTransform slotRect = slots[slot].GetRect();
            return slotRect.position;
        }
        return Vector3.zero;
    }

    public static InventoryBar Get()
    {
        return _instance;
    }
}
