using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Items are objects that can be picked, dropped and held into the player's inventory. Some item can also be crafted or used as crafting material.
/// </summary>

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(UniqueID))]
public class Item : MonoBehaviour
{
    public ItemData data;
    public int quantity = 1;

    [Header("FX")]
    public AudioClip take_audio;
    public GameObject take_fx;

    [HideInInspector]
    public bool was_dropped = false; //If true, item was dropped by the player

    private Selectable selectable;
    private UniqueID unique_id;

    private static List<Item> item_list = new List<Item>();

    void Awake()
    {
        item_list.Add(this);
        selectable = GetComponent<Selectable>();
        unique_id = GetComponent<UniqueID>();
    }

    private void OnDestroy()
    {
        item_list.Remove(this);
    }

    private void Start()
    {
        selectable.onUse += OnUse;

        if (!was_dropped && PlayerData.Get().IsObjectRemoved(GetUID()))
            Destroy(gameObject);
    }

    void Update()
    {

    }

    private void OnUse(PlayerCharacter character)
    {
        //Take
        character.TakeItem(this);
    }

    public void TakeItem()
    {
        PlayerData pdata = PlayerData.Get();
        if (CanTakeItem())
        {
            int slot = pdata.AddItem(data.id, quantity); //Add to inventory

            if(was_dropped)
                pdata.RemoveDroppedItem(unique_id.unique_id); //Removed from dropped items
            else
                pdata.RemoveObject(GetUID()); //Taken from map
            
            Destroy(gameObject);

            //Take fx
            ItemTakeFX.DoTakeFX(transform.position, data, slot);

            TheAudio.Get().PlaySFX("item", take_audio);
            if (take_fx != null)
                Instantiate(take_fx, transform.position, Quaternion.identity);
        }
    }

    public bool CanTakeItem()
    {
        PlayerData pdata = PlayerData.Get();
        return gameObject.activeSelf && pdata.CanTakeItem(data.id, quantity);
    }

    public string GetUID()
    {
        return unique_id.unique_id;
    }

    public static Item GetNearest(Vector3 pos, float range = 999f)
    {
        Item nearest = null;
        float min_dist = range;
        foreach (Item item in item_list)
        {
            float dist = (item.transform.position - pos).magnitude;
            if (dist < min_dist)
            {
                min_dist = dist;
                nearest = item;
            }
        }
        return nearest;
    }

    public static List<Item> GetAll()
    {
        return item_list;
    }

    //Spawn an existing one in the save file (such as after loading)
    public static Item Spawn(string uid)
    {
        DroppedItemData ddata = PlayerData.Get().GetDroppedItem(uid);
        if (ddata != null)
        {
            ItemData idata = ItemData.Get(ddata.item_id);
            if (idata != null)
            {
                GameObject build = Instantiate(idata.item_prefab, ddata.pos, idata.item_prefab.transform.rotation);
                Item item = build.GetComponent<Item>();
                item.data = idata;
                item.was_dropped = true;
                item.unique_id.unique_id = uid;
                item.quantity = ddata.quantity;
                return item;
            }
        }
        return null;
    }

    //Create a totally new one that will be added to save file
    public static Item Create(ItemData data, Vector3 pos, int quantity)
    {
        DroppedItemData ditem = PlayerData.Get().AddDroppedItem(data.id, SceneNav.GetCurrentScene(), pos, quantity);
        GameObject obj = Instantiate(data.item_prefab, pos, data.item_prefab.transform.rotation);
        Item item = obj.GetComponent<Item>();
        item.data = data;
        item.was_dropped = true;
        item.unique_id.unique_id = ditem.uid;
        item.quantity = quantity;
        return item;
    }
}
