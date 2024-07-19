using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keyboard controls manager
/// </summary>

public class PlayerControls : MonoBehaviour
{
    public KeyCode action_key1 = KeyCode.Space;
    public KeyCode action_key2 = KeyCode.Return;

    public KeyCode attack_key1 = KeyCode.LeftShift;
    public KeyCode attack_key2 = KeyCode.RightShift;

    public KeyCode cam_rotate_left = KeyCode.Q;
    public KeyCode cam_rotate_right = KeyCode.E;

    private Vector3 move;
    private Vector3 rotate_cam;
    private bool press_action;
    private bool press_attack;
   

    private static PlayerControls _instance;

    void Awake()
    {
        _instance = this;
    }

    void Update()
    {
        move = Vector3.zero;
        rotate_cam = Vector3.zero;
        press_action = false;
        press_attack = false;

        if (Input.GetKey(KeyCode.A))
            move += Vector3.left;
        if (Input.GetKey(KeyCode.D))
            move += Vector3.right;
        if (Input.GetKey(KeyCode.W))
            move += Vector3.forward;
        if (Input.GetKey(KeyCode.S))
            move += Vector3.back;

        if (Input.GetKey(KeyCode.LeftArrow))
            move += Vector3.left;
        if (Input.GetKey(KeyCode.RightArrow))
            move += Vector3.right;
        if (Input.GetKey(KeyCode.UpArrow))
            move += Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow))
            move += Vector3.back;

        move = move.normalized * Mathf.Min(move.magnitude, 1f);

        if (Input.GetKey(cam_rotate_left))
            rotate_cam += Vector3.left;
        if (Input.GetKey(cam_rotate_right))
            rotate_cam += Vector3.right;

        if (Input.GetKeyDown(action_key1) || Input.GetKeyDown(action_key2))
            press_action = true;
        if (Input.GetKeyDown(attack_key1) || Input.GetKeyDown(attack_key2))
            press_attack = true;


        if (press_action || press_attack)
        {
            if (CraftBar.Get())
                CraftBar.Get().CancelSubSelection();
        }
    }

    public bool IsMoving()
    {
        return move.magnitude > 0.1f;
    }

    public bool IsPressAttack()
    {
        return press_attack;
    }

    public bool IsPressAction()
    {
        return press_action;
    }

    public Vector3 GetMove()
    {
        return move;
    }

    public Vector3 GetRotateCam()
    {
        return rotate_cam;
    }

    public static PlayerControls Get()
    {
        return _instance;
    }
}
