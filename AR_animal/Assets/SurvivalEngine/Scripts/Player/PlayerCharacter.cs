using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Main character script
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCharacter : MonoBehaviour
{
    [Header("Movement")]
    public float move_speed = 4f;
    public float move_accel = 8;
    public float rotate_speed = 180f;
    public float fall_speed = 20f;
    public float ground_detect_dist = 0.1f;
    public LayerMask ground_layer = ~0;
    public bool use_navmesh = false;

    [Header("Combat")]
    public int hand_damage = 5;
    public int base_armor = 0;
    public float attack_speed = 150f;
    public float attack_windup = 0.7f;
    public float attack_windout = 0.7f;
    public float attack_range = 1.2f;

    [Header("Attributes")]
    public AttributeData[] attributes;

    [Header("Craft")]
    public float construct_range = 4f;

    [Header("Audio")]
    public AudioClip hit_sound;

    public UnityAction<Item> onTakeItem;
    public UnityAction<Item> onDropItem;
    public UnityAction<ItemData> onGainItem;
    public UnityAction<ConstructionData> onBuild;
    public UnityAction<Destructible, bool> onAttack;
    public UnityAction<Destructible> onAttackHit;
    public UnityAction onDamaged;
    public UnityAction onDeath;
    public UnityAction<string> onTriggerAnim;

    private Rigidbody rigid;
    private CapsuleCollider collide;
    private PlayerCharacterEquip character_equip;

    private Vector3 move;
    private Vector3 facing;
    private Vector3 move_average;
    private Vector3 prev_pos;

    private bool auto_move = false;
    private Vector3 auto_move_target;
    private Vector3 auto_move_target_next;
    private Selectable auto_move_select = null;
    private Destructible auto_move_attack = null;
    private int auto_move_drop = -1;
    private int auto_move_drop_equip = -1;
    private float auto_move_timer = 0f;
    private float move_speed_mult = 1f;

    private bool is_grounded = false;
    private bool is_fronted = false;
    private bool is_action = false;
    private bool is_sleep = false;
    private bool is_dead = false;

    private float attack_timer = 0f;

    private Construction current_construction = null;
    private ActionSleep sleep_target = null;
    private bool clicked_build = false;

    private Vector3[] nav_paths = new Vector3[0];
    private int path_index = 0;
    private bool calculating_path = false;
    private bool path_found = false;

    private EquipAttach[] equip_attachments;

    private static PlayerCharacter _instance;

    void Awake()
    {
        _instance = this;
        rigid = GetComponent<Rigidbody>();
        collide = GetComponentInChildren<CapsuleCollider>();
        character_equip = GetComponent<PlayerCharacterEquip>();
        equip_attachments = GetComponentsInChildren<EquipAttach>();
        facing = transform.forward;
        prev_pos = transform.position;
    }

    private void Start()
    {
        PlayerControlsMouse mouse_controls = PlayerControlsMouse.Get();
        mouse_controls.onClickFloor += OnClickFloor;
        mouse_controls.onClickObject += OnClickObject;
        mouse_controls.onClick += OnClick;
        mouse_controls.onRightClick += OnRightClick;

        //Init attributes
        foreach (AttributeData attr in attributes)
        {
            if (!PlayerData.Get().HasAttribute(attr.type))
                PlayerData.Get().SetAttributeValue(attr.type, attr.start_value);
        }
    }

    void FixedUpdate()
    {
        if (TheGame.Get().IsPaused())
        {
            rigid.velocity = Vector3.zero;
            return;
        }

        if (is_dead)
            return;

        PlayerControls controls = PlayerControls.Get();
        PlayerControlsMouse mcontrols = PlayerControlsMouse.Get();
        Vector3 tmove = Vector3.zero;

        //Navmesh
        if (auto_move && use_navmesh && path_found && path_index < nav_paths.Length)
        {
            auto_move_target_next = nav_paths[path_index];
            Vector3 move_dir_total = auto_move_target_next - transform.position;
            move_dir_total.y = 0f;
            if (move_dir_total.magnitude < 0.2f)
                path_index++;
        }

        //Moving
        auto_move_timer += Time.fixedDeltaTime;
        if (auto_move && auto_move_timer > 0.02f)
        {
            if (!use_navmesh || !calculating_path)
            {
                Vector3 move_dir_total = auto_move_target - transform.position;
                Vector3 move_dir_next = auto_move_target_next - transform.position;
                Vector3 move_dir = move_dir_next.normalized * Mathf.Min(move_dir_total.magnitude, 1f);
                move_dir.y = 0f;

                float move_dist = Mathf.Min(move_speed * move_speed_mult, move_dir.magnitude * 10f);
                tmove = move_dir.normalized * move_dist;
            }
        }
        else
        {
            Vector3 cam_move = TheCamera.Get().GetRotation() * controls.GetMove();
            if (mcontrols.IsJoystickActive())
            {
                Vector2 joystick = mcontrols.GetJoystickDir();
                cam_move = TheCamera.Get().GetRotation() * new Vector3(joystick.x, 0f, joystick.y);
            }
            tmove = cam_move * move_speed * move_speed_mult;
        }

        if (is_action)
            tmove = Vector3.zero;

        DetectGrounded();

        //Falling
        if (!is_grounded)
        {
            tmove += Vector3.down * fall_speed;
        }

        //Do move
        move = Vector3.Lerp(move, tmove, move_accel * Time.fixedDeltaTime);
        rigid.velocity = move;

        //Facing
        if (!is_action && IsMoving())
        {
            facing = new Vector3(move.x, 0f, move.z).normalized;
        }

        Quaternion targ_rot = Quaternion.LookRotation(facing, Vector3.up);
        rigid.MoveRotation(Quaternion.RotateTowards(rigid.rotation, targ_rot, rotate_speed * Time.fixedDeltaTime));

        //Fronted
        DetectFronted();

        //Traveled calcul
        Vector3 last_frame_travel = transform.position - prev_pos;
        move_average = Vector3.MoveTowards(move_average, last_frame_travel, 1f * Time.fixedDeltaTime);
        prev_pos = transform.position;

        //Stop auto move
        bool stuck_somewhere = move_average.magnitude < 0.02f && auto_move_timer > 1f;
        if (stuck_somewhere)
            StopMove();

        if (controls.IsMoving() || mcontrols.IsJoystickActive())
            StopAction();
    }

    private void Update()
    {
        if (TheGame.Get().IsPaused())
            return;

        if (is_dead)
            return;

        PlayerControls controls = PlayerControls.Get();

        //Update attributes
        ResolveAttributeEffects();

        //Stop sleep
        if (is_action || IsMoving() || sleep_target == null)
            is_sleep = false;

        //Sleep
        if (is_sleep)
        {
            float game_speed = GameData.Get().game_time_mult;
            float hour_to_sec = game_speed / 3600f;
            PlayerData.Get().AddAttributeValue(AttributeType.Health, sleep_target.sleep_hp_hour * hour_to_sec * Time.deltaTime, GetAttributeMax(AttributeType.Health));
            PlayerData.Get().AddAttributeValue(AttributeType.Hunger, sleep_target.sleep_hunger_hour * hour_to_sec * Time.deltaTime, GetAttributeMax(AttributeType.Hunger));
            PlayerData.Get().AddAttributeValue(AttributeType.Thirst, sleep_target.sleep_thirst_hour * hour_to_sec * Time.deltaTime, GetAttributeMax(AttributeType.Thirst));
            PlayerData.Get().AddAttributeValue(AttributeType.Happiness, sleep_target.sleep_hapiness_hour * hour_to_sec * Time.deltaTime, GetAttributeMax(AttributeType.Happiness));
        }

        //Activate Selectable
        Vector3 move_dir = auto_move_target - transform.position;
        if (auto_move && auto_move_select != null && move_dir.magnitude < GetTargetInteractRange(auto_move_select))
        {
            auto_move = false;
            auto_move_select.Use(this);
            auto_move_select = null;
        }

        //Stop move & drop
        if (auto_move && move_dir.magnitude < 0.35f)
        {
            auto_move = false;
            DropItem(auto_move_drop);
            DropEquippedItem(auto_move_drop_equip);
        }

        //Finish construction
        if (auto_move && clicked_build && move_dir.magnitude < 1.5f)
        {
            auto_move = false;
            CompleteCraftConstruction(auto_move_target);
        }

        //Attack
        float speed = GetAttackSpeed();
        attack_timer += speed * Time.deltaTime;
        if (auto_move_attack != null && !is_action && IsAttackTargetInRange())
        {
            FaceTorward(auto_move_attack.transform.position);

            if (attack_timer > 100f)
            {
                DoAttack(auto_move_attack);
            }
        }

        if(!CanAttack(auto_move_attack))
            auto_move_attack = null;

        //Press Action button
        if (controls.IsPressAction())
        {
            if (current_construction != null)
            {
                CompleteCraftConstruction(current_construction.transform.position);
            }
            else
            {
                Selectable nearest = Selectable.GetNearestInteractable(transform.position, 4f);
                if (nearest != null)
                {
                    InteractWith(nearest);
                }
            }
        }
    }

    //Detect if character is on the floor
    private void DetectGrounded()
    {
        Vector3 scale = transform.lossyScale;
        float hradius = collide.height * scale.y * 0.5f + ground_detect_dist; //radius is half the height minus offset
        float radius = collide.radius * (scale.x + scale.y) * 0.5f;

        Vector3 center = collide.transform.position + Vector3.Scale(collide.center, scale);
        Vector3 p1 = center;
        Vector3 p2 = center + Vector3.left * radius;
        Vector3 p3 = center + Vector3.right * radius;
        Vector3 p4 = center + Vector3.forward * radius;
        Vector3 p5 = center + Vector3.back * radius;

        RaycastHit h1, h2, h3, h4, h5;
        bool f1 = Physics.Raycast(p1, Vector3.down, out h1, hradius, ground_layer.value);
        bool f2 = Physics.Raycast(p2, Vector3.down, out h2, hradius, ground_layer.value);
        bool f3 = Physics.Raycast(p3, Vector3.down, out h3, hradius, ground_layer.value);
        bool f4 = Physics.Raycast(p4, Vector3.down, out h4, hradius, ground_layer.value);
        bool f5 = Physics.Raycast(p5, Vector3.down, out h5, hradius, ground_layer.value);

        is_grounded = f1 || f2 || f3 || f4 || f5;

        //Debug.DrawRay(p1, Vector3.down * hradius);
        //Debug.DrawRay(p2, Vector3.down * hradius);
        //Debug.DrawRay(p3, Vector3.down * hradius);
        //Debug.DrawRay(p4, Vector3.down * hradius);
        //Debug.DrawRay(p5, Vector3.down * hradius);
    }

    //Detect if there is an obstacle in front of the character
    private void DetectFronted() {
        Vector3 scale = transform.lossyScale;
        float hradius = collide.height * scale.y * 0.5f - 0.02f; //radius is half the height minus offset
        float radius = collide.radius * (scale.x + scale.y) * 0.5f + 0.5f;

        Vector3 center = collide.transform.position + Vector3.Scale(collide.center, scale);
        Vector3 p1 = center;
        Vector3 p2 = center + Vector3.up * hradius;
        Vector3 p3 = center + Vector3.down * hradius;

        RaycastHit h1, h2, h3;
        bool f1 = Physics.Raycast(p1, facing, out h1, radius);
        bool f2 = Physics.Raycast(p2, facing, out h2, radius);
        bool f3 = Physics.Raycast(p3, facing, out h3, radius);

        is_fronted = f1 || f2 || f3;

        //Debug.DrawRay(p1, facing * radius);
        //Debug.DrawRay(p2, facing * radius);
        //Debug.DrawRay(p3, facing * radius);
    }

    //Update attribute and apply effects for having empty attribute
    private void ResolveAttributeEffects()
    {
        float game_speed = GameData.Get().game_time_mult;
        float hour_to_sec = game_speed / 3600f;

        //Update Attributes
        foreach (AttributeData attr in attributes)
        {
            float update_value = attr.value_per_hour * hour_to_sec * Time.deltaTime;
            PlayerData.Get().AddAttributeValue(attr.type, update_value, attr.max_value);
        }

        //Penalty for depleted attributes
        float health_max = GetAttributeMax(AttributeType.Health);
        float health = GetAttributeValue(AttributeType.Health);

        move_speed_mult = 1f;

        foreach (AttributeData attr in attributes)
        {
            if (GetAttributeValue(attr.type) < 0.01f)
            {
                move_speed_mult = move_speed_mult * attr.deplete_move_mult;
                float update_value = attr.deplete_hp_loss * hour_to_sec * Time.deltaTime;
                PlayerData.Get().AddAttributeValue(AttributeType.Health, update_value, health_max);
            }
        }

        //Dying
        health = GetAttributeValue(AttributeType.Health);
        if (health < 0.01f)
            Kill();
    }

    //Perform one attack
    private void DoAttack(Destructible resource)
    {
        attack_timer = -10f;
        StartCoroutine(AttackRun(resource));
    }

    private IEnumerator AttackRun(Destructible target)
    {
        is_action = true;

        bool is_ranged = target != null && CanAttackRanged(target);

        //Start animation
        if (onAttack != null)
            onAttack.Invoke(target, is_ranged);

        //Face target
        FaceTorward(target.transform.position);

        //Wait for windup
        float windup = GetAttackWindup();
        yield return new WaitForSeconds(windup);

        attack_timer = 0f;

        //Ranged attack
        if (target != null && is_ranged)
        {
            ItemData equipped = GetEquippedItem(EquipSlot.Hand);
            ItemData projectile = GetFirstItemInGroup(equipped.projectile_group);
            if (projectile != null)
            {
                PlayerData.Get().RemoveItem(projectile.id, 1);
                Vector3 pos = GetProjectileSpawnPos(equipped);
                Vector3 dir = target.GetCenter() - pos;
                GameObject proj = Instantiate(projectile.projectile_prefab, pos, Quaternion.LookRotation(dir.normalized, Vector3.up));
                proj.GetComponent<Projectile>().dir = dir.normalized;
                proj.GetComponent<Projectile>().damage = equipped.damage;
            }
        }
        //Melee attack
        else if (IsAttackTargetInRange())
        {
            target.TakeDamage(GetAttackDamage());

            if (onAttackHit != null)
                onAttackHit.Invoke(target);
        }

        //Wait for the end of the attack before character can move again
        float windout = GetAttackWindout();
        yield return new WaitForSeconds(windout);

        is_action = false;
    }

    public void Sleep(ActionSleep sleep_target)
    {
        this.sleep_target = sleep_target;
        is_sleep = true;
        auto_move = false;
        auto_move_attack = null;
    }

    public void TakeDamage(int damage)
    {
        if (is_dead)
            return;

        int dam = damage - GetArmor();
        dam = Mathf.Max(dam, 1);

        PlayerData.Get().AddAttributeValue(AttributeType.Health, -dam, GetAttributeMax(AttributeType.Health));

        TheCamera.Get().Shake();
        TheAudio.Get().PlaySFX("character", hit_sound);

        if (onDamaged != null)
            onDamaged.Invoke();
    }

    public void Kill()
    {
        if (is_dead)
            return;

        rigid.velocity = Vector3.zero;
        move = Vector3.zero;
        is_dead = true;

        if (onDeath != null)
            onDeath.Invoke();
    }

    public void FaceTorward(Vector3 pos)
    {
        Vector3 face = (pos - transform.position);
        face.y = 0f;
        if (face.magnitude > 0.01f)
        {
            facing = face.normalized;
        }
    }

    public void TriggerAnim(string anim)
    {
        if (onTriggerAnim != null)
            onTriggerAnim.Invoke(anim);
    }

    public void TriggerAnim(string anim, Vector3 face_at, float duration = 0.5f)
    {
        FaceTorward(face_at);
        if (onTriggerAnim != null)
            onTriggerAnim.Invoke(anim);
        StartCoroutine(RunAction(duration));
    }

    private IEnumerator RunAction(float action_duration)
    {
        is_action = true;
        yield return new WaitForSeconds(action_duration);
        is_action = false;
    }

    //------- Items ----------

    //Take an Item on the floor
    public void TakeItem(Item item)
    {
        if (item != null && !is_action && item.CanTakeItem())
        {
            StartCoroutine(TakeItemRoutine(item));
        }
    }

    private IEnumerator TakeItemRoutine(Item item)
    {
        is_action = true;

        FaceTorward(item.transform.position);

        if (onTakeItem != null)
            onTakeItem.Invoke(item);

        yield return new WaitForSeconds(0.4f);

        //Make sure wasnt destroyed during the 0.4 sec
        if(item != null && item.CanTakeItem())
            item.TakeItem();

        is_action = false;
    }

    //Gain an new item directly to inventory
    public void GainItem(GameObject source, ItemData item, int quantity, bool animate=true)
    {
        if (item != null && PlayerData.Get().CanTakeItem(item.id, quantity))
        {
            if (animate)
            {
                StartCoroutine(GainItemRoutine(source, item, quantity));
            }
            else
            {
                int islot = PlayerData.Get().AddItem(item.id, quantity);
                ItemTakeFX.DoTakeFX(source.transform.position, item, islot);
            }
        }
    }

    private IEnumerator GainItemRoutine(GameObject source, ItemData item, int quantity)
    {
        is_action = true;

        if(source != null)
            FaceTorward(source.transform.position);

        if (onGainItem != null)
            onGainItem.Invoke(item);

        yield return new WaitForSeconds(0.4f);

        int islot = PlayerData.Get().AddItem(item.id, quantity);

        ItemTakeFX.DoTakeFX(source.transform.position, item, islot);

        is_action = false;
    }

    //Drop item on the floor
    public void DropItem(int slot)
    {
        InventoryItemData invdata = PlayerData.Get().GetItemSlot(slot);
        ItemData idata = ItemData.Get(invdata.item_id);
        if (idata != null && invdata.quantity > 0)
        {
            if (idata.CanBeDropped())
            {
                PlayerData.Get().RemoveItemAt(slot, invdata.quantity);
                Item iitem = Item.Create(idata, transform.position, invdata.quantity);

                TheUI.Get().CancelSelection();

                if (onDropItem != null)
                    onDropItem.Invoke(iitem);
            }
            else if(idata.CanBeBuilt())
            {
                BuildItem(slot, transform.position);
            }
        }
    }

    //Drop equipped item on the floor
    public void DropEquippedItem(int slot)
    {
        ItemData idata = PlayerData.Get().GetEquippedItem(slot);
        if (idata != null)
        {
            PlayerData.Get().UnequipItem(slot);
            Item iitem = Item.Create(idata, transform.position, 1);

            TheUI.Get().CancelSelection();

            if (onDropItem != null)
                onDropItem.Invoke(iitem);
        }
    }

    public void EatItem(ItemData item, int slot)
    {
        if (item.type == ItemType.Consumable)
        {
            if (PlayerData.Get().IsItemIn(item.id, slot))
            {
                PlayerData.Get().RemoveItemAt(slot, 1);

                if (item.container_data)
                    PlayerData.Get().AddItem(item.container_data.id, 1);

                PlayerData.Get().AddAttributeValue(AttributeType.Health, item.eat_hp, GetAttributeMax(AttributeType.Health));
                PlayerData.Get().AddAttributeValue(AttributeType.Hunger, item.eat_hunger, GetAttributeMax(AttributeType.Hunger));
                PlayerData.Get().AddAttributeValue(AttributeType.Thirst, item.eat_thirst, GetAttributeMax(AttributeType.Thirst));
                PlayerData.Get().AddAttributeValue(AttributeType.Happiness, item.eat_happiness, GetAttributeMax(AttributeType.Happiness));

            }
        }
    }

    public void EquipItem(ItemData item, int eslot)
    {
        if (item.type == ItemType.Equipment)
        {
            int index = ItemData.GetEquipIndex(item.equip_slot);
            PlayerData.Get().EquipItemTo(eslot, index);
        }
    }

    public void UnEquipItem(ItemData item, int eslot)
    {
        if (item.type == ItemType.Equipment)
        {
            if (PlayerData.Get().CanTakeItem(item.id, 1))
            {
                PlayerData.Get().UnequipItem(eslot);
                PlayerData.Get().AddItem(item.id, 1);
            }
        }
    }

    public bool HasItem(ItemData item)
    {
        PlayerData pdata = PlayerData.Get();
        return pdata.HasItem(item.id);
    }

    public bool HasItemInGroup(GroupData group)
    {
        PlayerData pdata = PlayerData.Get();
        foreach (KeyValuePair<int, InventoryItemData> pair in pdata.inventory)
        {
            ItemData idata = ItemData.Get(pair.Value.item_id);
            if (idata != null && pair.Value.quantity > 0)
            {
                if (idata.HasGroup(group))
                    return true;
            }
        }
        foreach (KeyValuePair<int, string> pair in pdata.equipped_items)
        {
            ItemData idata = ItemData.Get(pair.Value);
            if (idata != null)
            {
                if (idata.HasGroup(group))
                    return true;
            }
        }
        return false;
    }

    public ItemData GetFirstItemInGroup(GroupData group)
    {
        PlayerData pdata = PlayerData.Get();
        foreach (KeyValuePair<int, InventoryItemData> pair in pdata.inventory)
        {
            ItemData idata = ItemData.Get(pair.Value.item_id);
            if (idata != null && pair.Value.quantity > 0)
            {
                if (idata.HasGroup(group))
                    return idata;
            }
        }
        return null;
    }

    public ItemData GetEquippedItem(EquipSlot slot)
    {
        return PlayerData.Get().GetEquippedItem(ItemData.GetEquipIndex(slot));
    }

    public bool HasEquippedItemInGroup(GroupData group)
    {
        PlayerData pdata = PlayerData.Get();
        foreach (KeyValuePair<int, string> pair in pdata.equipped_items)
        {
            ItemData idata = ItemData.Get(pair.Value);
            if (idata != null)
            {
                if (idata.HasGroup(group))
                    return true;
            }
        }
        return false;
    }

    //---- Crafting ----

    public bool CanCraft(CraftData item)
    {
        CraftCostData cost = item.GetCraftCost();
        bool can_craft = true;
        foreach (KeyValuePair<ItemData, int> pair in cost.craft_items)
        {
            if (!PlayerData.Get().HasItem(pair.Key.id, pair.Value))
                can_craft = false; //Dont have required items
        }

        if (cost.craft_near != null && !IsNearGroup(cost.craft_near) && !HasItemInGroup(cost.craft_near))
            can_craft = false; //Not near required construction

        ItemData idata = item.GetItem();
        if (idata != null && !PlayerData.Get().CanTakeItem(idata.id, 1))
            can_craft = false; //Inventory is full

        return can_craft;
    }

    public void CraftItem(ItemData item)
    {
        if (CanCraft(item) && PlayerData.Get().CanTakeItem(item.id, 1))
        {
            CraftCostData cost = item.GetCraftCost();
            foreach (KeyValuePair<ItemData, int> pair in cost.craft_items)
            {
                PlayerData.Get().RemoveItem(pair.Key.id, pair.Value);
            }
            PlayerData.Get().AddItem(item.id, 1);

            //Gain container back after gaining the craft item
            foreach (KeyValuePair<ItemData, int> pair in cost.craft_items)
            {
                if (pair.Key.container_data)
                    PlayerData.Get().AddItem(pair.Key.container_data.id, pair.Value);
            }

            TheAudio.Get().PlaySFX("craft", item.craft_sound);
        }
    }

    public void CraftPlant(PlantData plant)
    {
        if (CanCraft(plant))
        {
            CraftCostData cost = plant.GetCraftCost();
            foreach (KeyValuePair<ItemData, int> pair in cost.craft_items)
            {
                PlayerData.Get().RemoveItem(pair.Key.id, pair.Value);
                if (pair.Key.container_data)
                    PlayerData.Get().AddItem(pair.Key.container_data.id, pair.Value);
            }
            Vector3 pos = transform.position + transform.forward * 0.4f;
            Plant.Create(plant, pos, 0);

            TheAudio.Get().PlaySFX("craft", plant.craft_sound);
        }
    }

    public void SelectCraftConstruction(ConstructionData item)
    {
        current_construction = Construction.CreateBuildMode(item, transform.position);
        current_construction.StartConstruction();
        clicked_build = false;

        TheAudio.Get().PlaySFX("craft", item.craft_sound);
    }

    public void CompleteCraftConstruction(Vector3 pos)
    {
        if (current_construction != null && !current_construction.IsOverlaping() && CanCraft(current_construction.data)){

            ConstructionData item = current_construction.data;
            CraftCostData cost = item.GetCraftCost();
            foreach (KeyValuePair<ItemData, int> pair in cost.craft_items)
            {
                PlayerData.Get().RemoveItem(pair.Key.id, pair.Value);
                if (pair.Key.container_data)
                    PlayerData.Get().AddItem(pair.Key.container_data.id, pair.Value);
            }

            current_construction.transform.position = pos;
            current_construction.FinishContruction();
            current_construction = null;
            clicked_build = false;
            auto_move = false;
            TheUI.Get().CancelSelection();
        }
    }

    public void CancelConstruction()
    {
        if (current_construction != null)
        {
            Destroy(current_construction.gameObject);
            current_construction = null;
            clicked_build = false;
        }
    }

    //Use an item in your inventory and build it on the map
    public void BuildItem(int slot, Vector3 pos)
    {
        InventoryItemData invdata = PlayerData.Get().GetItemSlot(slot);
        ItemData idata = ItemData.Get(invdata.item_id);
        if (idata != null)
        {
            ConstructionData construct = idata.construction_data;
            if (construct != null)
            {
                PlayerData.Get().RemoveItemAt(slot, 1);
                Construction.Create(construct, pos);
                TheUI.Get().CancelSelection();
            }
        }
    }

    //----- Player Orders ----------

    public void MoveTo(Vector3 pos)
    {
        auto_move = true;
        auto_move_target = pos;
        auto_move_target_next = pos;
        auto_move_select = null;
        auto_move_drop = -1;
        auto_move_drop_equip = -1;
        auto_move_timer = 0f;
        path_found = false;
        calculating_path = false;
        auto_move_attack = null;

        CalculateNavmesh();
    }

    public void InteractWith(Selectable selectable)
    {
        auto_move = true;
        auto_move_select = selectable;
        auto_move_target = selectable.transform.position;
        auto_move_target_next = selectable.transform.position;
        auto_move_drop = -1;
        auto_move_drop_equip = -1;
        auto_move_timer = 0f;
        clicked_build = false;
        path_found = false;
        calculating_path = false;
        auto_move_attack = null;

        CalculateNavmesh();
    }

    public void Attack(Destructible target)
    {
        if (CanAttack(target))
        {
            auto_move = true;
            auto_move_select = null;
            auto_move_target = target.transform.position;
            auto_move_target_next = target.transform.position;
            auto_move_drop = -1;
            auto_move_drop_equip = -1;
            auto_move_timer = 0f;
            clicked_build = false;
            path_found = false;
            calculating_path = false;
            auto_move_attack = target;

            CalculateNavmesh();
        }
    }

    public void StopMove()
    {
        auto_move = false;
    }

    public void StopAction()
    {
        auto_move = false;
        auto_move_select = null;
        auto_move_attack = null;
    }
    
    //------- Mouse Clicks --------

    private void OnClick(Vector3 pos)
    {
        
    }

    private void OnRightClick(Vector3 pos)
    {
        TheUI.Get().CancelSelection();
    }

    private void OnClickFloor(Vector3 pos)
    {
        MoveTo(pos);

        auto_move_drop = PlayerControlsMouse.Get().GetInventorySelectedSlotIndex();
        auto_move_drop_equip = PlayerControlsMouse.Get().GetEquippedSelectedSlotIndex();
        clicked_build = (current_construction != null);

    }

    private void OnClickObject(Selectable selectable)
    {
        selectable.Select();

        //Attack target ?
        Destructible target = selectable.GetDestructible();
        if (target != null && target.attack_by_default && CanAttack(target))
        {
            Attack(target);
        }
        else
        {
            InteractWith(selectable);
        }
    }

    //---- Navmesh ----

    public void CalculateNavmesh()
    {
        if (auto_move && use_navmesh && !calculating_path)
        {
            calculating_path = true;
            path_found = false;
            path_index = 0;
            auto_move_target_next = auto_move_target; //Default
            NavMeshTool.CalculatePath(transform.position, auto_move_target, 1 << 0, FinishCalculateNavmesh);
        }
    }

    private void FinishCalculateNavmesh(NavMeshToolPath path)
    {
        calculating_path = false;
        path_found = path.success;
        nav_paths = path.path;
        path_index = 0;
    }

    //---- Getters ----

    //Check if character is near an object of that group
    public bool IsNearGroup(GroupData group)
    {
        foreach (Selectable select in Selectable.GetAllActive())
        {
            if (select.HasGroup(group))
            {
                float dist = (select.transform.position - transform.position).magnitude;
                if (dist < select.use_range)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool CanAttack(Destructible target)
    {
        return target != null && !target.IsDead() && (target.required_item == null || HasEquippedItemInGroup(target.required_item));
    }

    public int GetAttackDamage()
    {
        PlayerData pdata = PlayerData.Get();
        int damage = hand_damage;
        foreach (KeyValuePair<int, string> pair in pdata.equipped_items)
        {
            ItemData idata = ItemData.Get(pair.Value);
            if (idata != null)
            {
                damage = Mathf.Max(damage, idata.damage);
            }
        }
        return damage;
    }

    public float GetAttackRange(Destructible target)
    {
        ItemData equipped = GetEquippedItem(EquipSlot.Hand);
        if (equipped != null && equipped.ranged && CanAttackRanged(target))
            return equipped.range;
        else if(equipped != null && !equipped.ranged)
            return equipped.range;
        return attack_range;
    }

    public float GetAttackWindup()
    {
        if (character_equip) {
            EquipItem item_equip = character_equip.GetEquippedItem(EquipSlot.Hand);
            if (item_equip != null)
                return item_equip.attack_windup;
        }
        return attack_windup;
    }

    public float GetAttackWindout()
    {
        if (character_equip)
        {
            EquipItem item_equip = character_equip.GetEquippedItem(EquipSlot.Hand);
            if (item_equip != null)
                return item_equip.attack_windout;
        }
        return attack_windout;
    }

    public Vector3 GetProjectileSpawnPos(ItemData weapon) {

        EquipAttach attach = GetEquipAttach(EquipSlot.Hand, weapon.equip_side);
        if (attach != null)
            return attach.transform.position;

        return transform.position + Vector3.up;
    }

    public float GetAttackSpeed()
    {
        return attack_speed;
    }

    public bool CanAttackRanged(Destructible destruct)
    {
        if (destruct == null)
            return false;

        ItemData equipped = GetEquippedItem(EquipSlot.Hand);
        if (equipped != null && equipped.ranged && destruct.CanAttackRanged())
        {
            ItemData projectile = GetFirstItemInGroup(equipped.projectile_group);
            if (projectile != null && HasItem(projectile))
            {
                return true;
            }
        }
        return false;
    }

    public float GetTargetInteractRange(Selectable target)
    {
        return target.use_range;
    }

    public float GetTargetAttackRange(Destructible target)
    {
        return GetAttackRange(target) + target.hit_size;
    }

    public bool IsAttackTargetInRange()
    {
        if (auto_move_attack != null)
        {
            float dist = (auto_move_attack.transform.position - transform.position).magnitude;
            return dist < GetTargetAttackRange(auto_move_attack);
        }
        return false;
    }

    public int GetArmor()
    {
        int armor = base_armor;
        foreach (KeyValuePair<int, string> pair in PlayerData.Get().equipped_items)
        {
            ItemData idata = ItemData.Get(pair.Value);
            if(idata != null)
                armor += idata.armor;
        }
        return armor;
    }

    public float GetAttributeValue(AttributeType type)
    {
        return PlayerData.Get().GetAttributeValue(type);
    }

    public float GetAttributeMax(AttributeType type)
    {
        AttributeData adata = GetAttribute(type);
        if (adata != null)
            return adata.max_value;
        return 0f;
    }

    public AttributeData GetAttribute(AttributeType type)
    {
        foreach (AttributeData attr in attributes)
        {
            if (attr.type == type)
                return attr;
        }
        return null;
    }

    public EquipAttach GetEquipAttach(EquipSlot slot, EquipSide side) {
        foreach (EquipAttach attach in equip_attachments)
        {
            if (attach.slot == slot)
            {
                if(attach.side == EquipSide.Default || side == EquipSide.Default || attach.side == side)
                    return attach;
            }
        }
        return null;
    }

    public bool IsDead()
    {
        return is_dead;
    }

    public bool IsSleeping()
    {
        return is_sleep;
    }

    public bool IsMoving()
    {
        Vector3 moveXZ = new Vector3(move.x, 0f, move.z);
        return moveXZ.magnitude > move_speed * move_speed_mult * 0.25f;
    }

    public Vector3 GetMove()
    {
        return move;
    }

    public Vector3 GetFacing()
    {
        return facing;
    }

    public bool IsFronted()
    {
        return is_fronted;
    }

    public static PlayerCharacter Get()
    {
        return _instance;
    }
}
