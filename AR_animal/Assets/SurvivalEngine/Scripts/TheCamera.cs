using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TheCamera : MonoBehaviour
{
    public float move_speed = 2f;
    public float rotate_speed = 90f;
    public float zoom_speed = 0.5f;
    public float zoom_in_max = 0.5f;
    public float zoom_out_max = 1f;
    public GameObject follow_target;
    public Vector3 follow_offset;

    private Vector3 current_vel;
    private Vector3 rotated_offset;
    private Vector3 current_offset;
    private float current_zoom = 0f;
    private Camera cam;

    private Vector3 shake_vector = Vector3.zero;
    private float shake_timer = 0f;
    private float shake_intensity = 1f;

    private static TheCamera _instance;

    void Awake()
    {
        _instance = this;
        cam = GetComponent<Camera>();
        rotated_offset = follow_offset;
        current_offset = follow_offset;
    }

    private void Start()
    {
        if (follow_target == null && PlayerCharacter.Get())
        {
            follow_target = PlayerCharacter.Get().gameObject;
        }
    }

    void LateUpdate()
    {
        //Rotate
        PlayerControls controls = PlayerControls.Get();
        Vector3 rot = controls.GetRotateCam();
        if (rot.magnitude > 0.01)
        {
            rotated_offset = Quaternion.Euler(0, rotate_speed * -rot.x * Time.deltaTime, 0) * rotated_offset;
            transform.RotateAround(follow_target.transform.position, Vector3.up, rotate_speed * -rot.x * Time.deltaTime);
        }

        //Zoom 
        PlayerControlsMouse mouse = PlayerControlsMouse.Get();
        current_zoom += mouse.GetMouseScroll() * zoom_speed;
        current_zoom = Mathf.Clamp(current_zoom, -zoom_out_max, zoom_in_max);
        current_offset = rotated_offset - rotated_offset * current_zoom;

        Vector3 target_pos = follow_target.transform.position + current_offset;
        transform.position = Vector3.SmoothDamp(transform.position, target_pos, ref current_vel, 1f / move_speed);
        Quaternion targ_rot = Quaternion.LookRotation(-current_offset, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targ_rot, 10f * rotate_speed * Time.deltaTime);

        //Shake FX
        if (shake_timer > 0f)
        {
            shake_timer -= Time.deltaTime;
            shake_vector = new Vector3(Mathf.Cos(shake_timer * Mathf.PI * 8f) * 0.02f, Mathf.Sin(shake_timer * Mathf.PI * 7f) * 0.02f, 0f);
            transform.position += shake_vector * shake_intensity;
        }
    }

    public void MoveToTarget(Vector3 target)
    {
        transform.position = target + current_offset;
    }

    public void Shake(float intensity = 2f, float duration = 0.5f)
    {
        shake_intensity = intensity;
        shake_timer = duration;
    }

    public Vector3 GetTargetPos()
    {
        return transform.position - current_offset;
    }

    //Use as center for optimization
    public Vector3 GetTargetPosOffsetFace()
    {
        return transform.position - current_offset + GetFacingFront() * 10f;
    }

    public Quaternion GetRotation()
    {
        return Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    public Vector3 GetFacingFront()
    {
        Vector3 dir = transform.forward;
        dir.y = 0f;
        return dir.normalized;
    }

    public Vector3 GetFacingRight()
    {
        Vector3 dir = transform.right;
        dir.y = 0f;
        return dir.normalized;
    }

    public Camera GetCam()
    {
        return cam;
    }

    public static Camera GetCamera()
    {
        Camera camera = _instance != null ? _instance.GetCam() : Camera.main;
        return camera;
    }

    public static TheCamera Get()
    {
        return _instance;
    }
}
