using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An item that will appear on the player to display equipped item. Will be attached to a EquipAttach
/// </summary>

public class EquipItem : MonoBehaviour
{
    public ItemData data;
    public string attack_melee_anim;
    public string attack_ranged_anim;
    public float attack_windup = 0.7f;
    public float attack_windout = 0.7f;

    public Transform[] childs;

    [HideInInspector]
    public EquipAttach target;

    void Start()
    {
        
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.transform.position;
        transform.rotation = target.transform.rotation;

        for (int i = 0; i < childs.Length; i++){
            if (i < target.childs.Length)
            {
                Transform attach_child = childs[i];
                Transform equip_child = target.childs[i];
                attach_child.transform.position = equip_child.transform.position;
                attach_child.transform.rotation = equip_child.transform.rotation;
            }
        }
        
    }
}
