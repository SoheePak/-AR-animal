using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Birds are alternate version of the animal script but with flying!
/// </summary>

public enum BirdState
{
    Sit = 0,
    Fly = 2,
    FlyDown = 4,
    Alerted = 5,
    Dead = 10,
}

public class Bird : MonoBehaviour
{
    [Header("Fly")]
    public float fly_speed = 10f;
    public float wander_radius = 10f;
    public float fly_duration = 20f;
    public float sit_duration = 20f;
    public LayerMask ground_layer = ~0;

    [Header("Vision")]
    public float detect_range = 5f;
    public float detect_angle = 360f;
    public float alerted_duration = 0.5f;

    [Header("Models")]
    public Animator sit_model;
    public Animator fly_model;

    private Destructible destruct;
    private BirdState state = BirdState.Sit;
    private float state_timer = 0f;
    private Vector3 start_pos;
    private Vector3 target_pos;
    private Vector3 move;
    private Vector3 facing;

    void Start()
    {
        destruct = GetComponent<Destructible>();
        start_pos = transform.position;
        target_pos = transform.position;
        facing = transform.forward;

        destruct.onDeath += OnDeath;
    }

    void Update()
    {
        if (TheGame.Get().IsPaused())
            return;

        state_timer += Time.deltaTime;

        if (state == BirdState.Sit)
        {
            DetectThreat();

            if (state_timer > sit_duration)
            {
                state_timer = 0f;
                FindFlyPosition(transform.position, wander_radius, out target_pos);
                state = BirdState.Fly;
                sit_model.gameObject.SetActive(false);
                fly_model.gameObject.SetActive(true);
            }
        }

        if (state == BirdState.Alerted)
        {
            if (state_timer > alerted_duration)
            {
                state_timer = 0f;
                FindFlyPosition(transform.position, wander_radius, out target_pos);
                state = BirdState.Fly;
                sit_model.gameObject.SetActive(false);
                fly_model.gameObject.SetActive(true);
            }
        }
        
        if (state == BirdState.Fly)
        {
            if (state_timer > fly_duration)
            {
                state_timer = 0f;
                Vector3 npos;
                bool succes = FindGroundPosition(start_pos, wander_radius, out npos);
                if (succes)
                {
                    state = BirdState.FlyDown;
                    target_pos = npos;
                    fly_model.gameObject.SetActive(true);
                    sit_model.gameObject.SetActive(false);
                }
            }

            if(fly_model.gameObject.activeSelf && HasReachedTarget())
                fly_model.gameObject.SetActive(false);

            DoMovement();
        }

        if (state == BirdState.FlyDown)
        {
            if (HasReachedTarget())
            {
                state_timer = Random.Range(-1f, 1f);
                state = BirdState.Sit;
                sit_model.gameObject.SetActive(true);
                fly_model.gameObject.SetActive(false);
                facing.y = 0f;
                facing.Normalize();
            }

            //Move
            DoMovement();
        }

        Quaternion face_rot = Quaternion.LookRotation(facing, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, face_rot, 200 * Time.deltaTime);
    }

    private void DoMovement()
    {
        Vector3 dir = target_pos - transform.position;
        float dist = Mathf.Min(fly_speed * Time.deltaTime, dir.magnitude);
        transform.position += dir.normalized * dist;
        move = dir.normalized * Mathf.Min(dir.magnitude, 1f);

        bool face_y = state == BirdState.Fly || state == BirdState.FlyDown;
        if (move.magnitude > 0.1f)
            facing = move.normalized;

        if (!face_y)
            facing.y = 0f;
    }

    private void OnDeath()
    {
        StopMoving();
        state = BirdState.Dead;
        state_timer = 0f;
        sit_model.gameObject.SetActive(true);
        fly_model.gameObject.SetActive(false);
        sit_model.SetTrigger("Death");
    }

    private bool FindFlyPosition(Vector3 pos, float radius, out Vector3 fly_pos)
    {
        Vector3 offest = new Vector3(Random.Range(-radius, radius), 20f, Random.Range(radius, radius));
        fly_pos = pos + offest;
        return true;
    }

    //Find landing position to make sure it wont land on an obstacle
    private bool FindGroundPosition(Vector3 pos, float radius, out Vector3 ground_pos)
    {
        Vector3 offest = new Vector3(Random.Range(-radius, radius), 20f, Random.Range(radius, radius));
        Vector3 center = pos + offest;
        RaycastHit h1;
        bool f1 = Physics.Raycast(center, Vector3.down, out h1, 50f);
        bool is_in_layer = h1.collider != null && ((1 << h1.collider.gameObject.layer) & ground_layer.value) > 0;
        ground_pos = h1.point;
        return f1 && is_in_layer;
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
                state = BirdState.Alerted;
                state_timer = 0f;
                StopMoving();
            }
        }
    }

    private bool HasReachedTarget()
    {
        Vector3 diff = target_pos - transform.position;
        return diff.magnitude < 0.2f;
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
        target_pos = transform.position;
        state_timer = 0f;
    }

    public Vector3 GetMove()
    {
        return move;
    }

    public Vector3 GetFacing()
    {
        return facing;
    }
}
