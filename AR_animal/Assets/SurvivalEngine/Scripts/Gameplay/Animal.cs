using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Animal behavior script for wandering, escaping, or chasing the player
/// </summary>

public enum AnimalState
{
    Wander=0,
    Alerted=2,
    Escape=4,
    Follow=6,
    Attack=8,
    CustomMove=10,
    Dead=20,
}

public enum AnimalBehavior
{
    None=0,   //Custom behavior from another script
    Escape=5,  //Escape on sight
    PassiveEscape =10,  //Escape if attacked 
    PassiveDefense =15, //Attack if attacked
    Aggressive=20, //Attack on sight
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(Destructible))]
[RequireComponent(typeof(UniqueID))]
public class Animal : MonoBehaviour
{
    [Header("Move")]
    public float wander_speed = 2f;
    public float run_speed = 5f;
    public float rotate_speed = 250f;
    public float wander_range = 10f;
    public float wander_interval = 10f;
    public float fall_speed = 20f;
    public float ground_detect_dist = 0.1f;
    public LayerMask ground_layer = ~0;
    public bool use_navmesh = false;

    [Header("Vision")]
    public float detect_range = 5f;
    public float detect_angle = 360f;
    public float detect_duration = 0f;
    public float alerted_duration = 0.5f;

    [Header("Attack")]
    public float attack_range = 1f;
    public float attack_cooldown = 3f;
    public float attack_windup = 0.5f;
    public float attack_duration = 1f;
    public int attack_damage = 10;
    
    [Header("Behavior")]
    public AnimalBehavior behavior;


    public UnityAction onAttack;
    public UnityAction onDamaged;
    public UnityAction onDeath;

    private AnimalState state;
    private Rigidbody rigid;
    private Collider collide;
    private UniqueID unique_id;
    private Destructible destruct;

    private Vector3 start_pos;
    private Vector3 moving;
    private Vector3 facing;

    private PlayerCharacter target = null;
    private Vector3 wander_target;
    private Vector3 move_target;
    private Vector3 move_target_next;
    private bool is_running = false;
    private float state_timer = 0f;
    private bool attack_hit = false;
    private bool follow_reached = false;
    private bool is_grounded = false;
    private bool is_fronted = false;
    private bool is_fronted_left = false;
    private bool is_fronted_right = false;
    private bool is_active = false;
    private float avoid_angle = 0f;
    private float avoid_side = 1f;

    private Vector3[] nav_paths = new Vector3[0];
    private Vector3 path_destination;
    private int path_index = 0;
    private bool follow_path = false;
    private bool calculating_path = false;

    void Awake()
    {
        rigid = GetComponent <Rigidbody>();
        collide = GetComponentInChildren <Collider>();
        unique_id = GetComponent <UniqueID>();
        destruct = GetComponent <Destructible>();
        move_target = transform.position;
        move_target_next = transform.position;
        start_pos = transform.position;
        facing = transform.forward;
        avoid_side = Random.value < 0.5f ? 1f : -1f;

    }

    private void Start()
    {
        destruct.onDamaged += OnTakeDamage;
        destruct.onDeath += OnKill;

        FindWanderTarget();
    }

    void FixedUpdate()
    {

        if (TheGame.Get().IsPaused())
            return;

        if (state == AnimalState.Dead)
            return;

        //Optimization, dont run if too far
        float dist = (PlayerCharacter.Get().transform.position - transform.position).magnitude;
        is_active = (state != AnimalState.Wander && state != AnimalState.Dead) || dist < Mathf.Max(detect_range * 2f, 20f);
        if (!is_active)
            return;

        //Navmesh
        move_target_next = move_target;
        if (use_navmesh && follow_path && path_index < nav_paths.Length)
        {
            move_target_next = nav_paths[path_index];
            Vector3 dir_total = move_target_next - transform.position;
            dir_total.y = 0f;
            if (dir_total.magnitude < 0.2f)
                path_index++;
        }

        //Rotation
        Quaternion targ_rot = Quaternion.LookRotation(facing, Vector3.up);
        Quaternion nrot = Quaternion.RotateTowards(rigid.rotation, targ_rot, rotate_speed * Time.fixedDeltaTime);
        rigid.MoveRotation(nrot);

        //Navmesh
        if (use_navmesh)
        {
            Vector3 path_dir = path_destination - transform.position;
            Vector3 nav_move_dir = move_target - transform.position;
            float dot = Vector3.Dot(path_dir.normalized, nav_move_dir.normalized);
            if (dot < 0.7f)
                CalculateNavmesh();
        }

        //Moving
        Vector3 move_dir_total = move_target - transform.position;
        Vector3 move_dir_next = move_target_next - transform.position;
        Vector3 move_dir = move_dir_next.normalized * Mathf.Min(move_dir_total.magnitude, 1f);
        move_dir.y = 0f;

        float speed = is_running ? run_speed : wander_speed;
        Vector3 tmove = move_dir.normalized * Mathf.Min(move_dir.magnitude, 1f) * speed;

        if (move_dir.magnitude > 0.1f)
        {
            facing = new Vector3(tmove.normalized.x, 0f, tmove.normalized.z);
        }

        //Detect obstacles and ground
        DetectGrounded();
        DetectFronted();

        //Falling
        if (!is_grounded)
            tmove += Vector3.down * fall_speed;
        if (is_grounded)
            tmove.y = 0f;

        moving = Vector3.Lerp(moving, tmove, 10f * Time.fixedDeltaTime);
        rigid.velocity = moving;
    }

    private void Update()
    {
        if (TheGame.Get().IsPaused())
            return;

        if (state == AnimalState.Dead)
            return;

        if (!is_active)
            return;

        state_timer += Time.deltaTime;

        is_running = (state == AnimalState.Escape || state == AnimalState.Follow);

        //States
        if (state == AnimalState.Wander)
        {
            follow_path = false;

            if (state_timer > wander_interval)
            {
                state_timer = Random.Range(-1f, 1f);
                FindWanderTarget();
            }

            move_target = FindAvoidMoveTarget(wander_target);

            if (behavior == AnimalBehavior.Aggressive || behavior == AnimalBehavior.Escape)
                DetectThreat();
        }

        if (state == AnimalState.Alerted)
        {
            if (state_timer > alerted_duration)
            {
                ReactToThreat();
                state_timer = 0f;
                follow_reached = false;
            }
        }

        if (state == AnimalState.Escape)
        {
            if (target == null)
            {
                ChangeState(AnimalState.Wander);
                return;
            }

            Vector3 targ_dir = (target.transform.position - transform.position);
            targ_dir.y = 0f;

            Vector3 targ_pos = transform.position - targ_dir.normalized * 4f;
            move_target = FindAvoidMoveTarget(targ_pos);

            if (targ_dir.magnitude > detect_range && state_timer > detect_duration)
            {
                state = AnimalState.Wander;
                state_timer = 0f;
            }

        }

        if (state == AnimalState.Follow)
        {
            if (target == null)
            {
                ChangeState(AnimalState.Wander);
                return;
            }

            Vector3 follow_pos = FindAvoidMoveTarget(target.transform.position);
            Vector3 targ_dir = target.transform.position - transform.position;
            Vector3 start_dir = start_pos - transform.position;

            if (!follow_reached)
                move_target = follow_pos;

            if ((targ_dir.magnitude > detect_range || start_dir.magnitude > detect_range) && state_timer > detect_duration)
            {
                state = AnimalState.Wander;
                move_target = transform.position;
                state_timer = 0f;
            }

            if (targ_dir.magnitude < attack_range * 0.8f)
            {
                move_target = transform.position;
                follow_reached = true;
            }

            if (state_timer > attack_cooldown)
            {
                if (targ_dir.magnitude < attack_range)
                {
                    state = AnimalState.Attack;
                    attack_hit = false;
                    state_timer = 0f;

                    if (onAttack != null)
                        onAttack.Invoke();
                }
                else
                {
                    follow_reached = false;
                }
            }
        }

        if (state == AnimalState.Attack)
        {
            if (target == null)
            {
                ChangeState(AnimalState.Wander);
                return;
            }

            move_target = transform.position;
            FaceTorward(target.transform.position);

            if (!attack_hit && state_timer > attack_windup)
            {
                float range = (target.transform.position - transform.position).magnitude;
                if(range < attack_range)
                    target.TakeDamage(attack_damage);
                attack_hit = true;
            }

            if (state_timer > attack_duration)
            {
                state = AnimalState.Follow;
                state_timer = 0f;
                follow_reached = false;
            }
        }

        if (state == AnimalState.CustomMove)
        {
            if (HasReachedTarget())
                state = AnimalState.Wander;
        }

        //Add an offset to escape path when fronted
        if (is_fronted_left && !is_fronted_right)
            avoid_side = 1f;
        if (is_fronted_right && !is_fronted_left)
            avoid_side = -1f;

        float angle = avoid_side * 90f;
        avoid_angle = Mathf.MoveTowards(avoid_angle, is_fronted ? angle : 0f, 45f * Time.deltaTime);

        
    }

    //Find new move target while trying to avoid obstacles
    private Vector3 FindAvoidMoveTarget(Vector3 target)
    {
        Vector3 targ_dir = (target - transform.position);
        targ_dir = Quaternion.AngleAxis(avoid_angle, Vector3.up) * targ_dir; //Rotate if obstacle in front
        return transform.position + targ_dir;
    }

    //Check if touching the ground
    private void DetectGrounded()
    {
        float radius = (collide.bounds.extents.x + collide.bounds.extents.z) * 0.5f;
        float hradius = collide.bounds.extents.y + ground_detect_dist;

        Vector3 center = collide.bounds.center;
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

    //Detect if there is an obstacle in front of the animal
    private void DetectFronted()
    {
        float radius = destruct.hit_size * 2f;

        Vector3 center = destruct.GetCenter();
        Vector3 dir = move_target_next - transform.position;
        Vector3 dirl = Quaternion.AngleAxis(-45f, Vector3.up) * dir.normalized; 
        Vector3 dirr = Quaternion.AngleAxis(45f, Vector3.up) * dir.normalized; 

        RaycastHit h, hl, hr;
        bool f1 = Physics.Raycast(center, dir.normalized, out h, radius);
        bool fl = Physics.Raycast(center, dirl.normalized, out hl, radius);
        bool fr = Physics.Raycast(center, dirr.normalized, out hr, radius);
        f1 = f1 && (target == null || h.collider.gameObject != target.gameObject);
        fl = fl && (target == null || hl.collider.gameObject != target.gameObject);
        fr = fr && (target == null || hr.collider.gameObject != target.gameObject);

        is_fronted = f1 || fl || fr;
        is_fronted_left = fl;
        is_fronted_right = fr;
    }

    //Detect if the player is in vision
    private void DetectThreat()
    {
        PlayerCharacter character = PlayerCharacter.Get();
        Vector3 char_dir = (character.transform.position - transform.position);
        if (char_dir.magnitude < detect_range)
        {
            float dangle = detect_angle / 2f; // /2 for each side
            float angle = Vector3.Angle(transform.forward, char_dir.normalized);
            if (angle < dangle)
            {
                target = character;
                state = AnimalState.Alerted;
                state_timer = 0f;
                move_target = transform.position;
                StopMoving();
            }
        }
    }

    //React to player if seen by animal
    private void ReactToThreat() {

        if (state == AnimalState.Wander || state == AnimalState.Alerted)
        {
            if (target == null)
                target = PlayerCharacter.Get();

            if (behavior == AnimalBehavior.Escape || behavior == AnimalBehavior.PassiveEscape)
                state = AnimalState.Escape;
            else if (behavior == AnimalBehavior.Aggressive || behavior == AnimalBehavior.PassiveDefense)
                state = AnimalState.Follow;
            else
                state = AnimalState.Wander;
            state_timer = 0f;
            follow_reached = false;

            if (state != AnimalState.Wander)
                CalculateNavmesh();
        }
    }

    private void FindWanderTarget()
    {
        float range = Random.Range(0f, wander_range);
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 pos = start_pos + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * range;
        wander_target = pos;
    }

    public void MoveToCustomTarget(Vector3 pos, bool run)
    {
        move_target = pos;
        is_running = run;
        state = AnimalState.CustomMove;
        state_timer = 0f;
        CalculateNavmesh();
    }

    public void CalculateNavmesh()
    {
        if (use_navmesh && !calculating_path)
        {
            calculating_path = true;
            path_index = 0;
            NavMeshTool.CalculatePath(transform.position, move_target, 1 << 0, FinishCalculateNavmesh);
            path_destination = move_target;
        }
    }

    private void FinishCalculateNavmesh(NavMeshToolPath path)
    {
        calculating_path = false;
        follow_path = path.success;
        nav_paths = path.path;
        path_index = 0;
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

    public void StopMoving()
    {
        move_target = transform.position;
        is_running = false;
        state_timer = 0f;
        if (state == AnimalState.CustomMove)
            state = AnimalState.Wander;
    }

    public void OnTakeDamage()
    {
        if (IsDead())
            return;

        ReactToThreat();

        if (onDamaged != null)
            onDamaged.Invoke();
    }

    public void OnKill()
    {
        if (IsDead())
            return;

        rigid.velocity = Vector3.zero;
        moving = Vector3.zero;
        state = AnimalState.Dead;
        state_timer = 0f;
        collide.enabled = false;
        rigid.isKinematic = true;

        if (onDeath != null)
            onDeath.Invoke();
    }

    public void ChangeState(AnimalState s)
    {
        state = s;
        state_timer = 0f;
    }

    public bool HasReachedTarget()
    {
        Vector3 diff = move_target - transform.position;
        return (diff.magnitude < 0.11f);
    }

    public bool IsDead()
    {
        return state == AnimalState.Dead;
    }

    public bool IsActive()
    {
        return is_active;
    }

    public bool IsMoving()
    {
        Vector3 moveXZ = new Vector3(moving.x, 0f, moving.z);
        return moveXZ.magnitude > 0.2f;
    }

    public Vector3 GetMove()
    {
        return moving;
    }

    public Vector3 GetFacing()
    {
        return facing;
    }

    public bool IsRunning()
    {
        return IsMoving() && is_running;
    }

    public string GetUID()
    {
        return unique_id.unique_id;
    }
}
