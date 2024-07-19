using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optimize rendering and apply visual effects
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>

public class TheRender : MonoBehaviour
{
    private Light dir_light;

    private float update_timer = 0f;

    void Start()
    {
        //Light
        GameData gdata = GameData.Get();
        bool is_night = TheGame.Get().IsNight();
        dir_light = FindObjectOfType<Light>();
        float target = is_night ? gdata.night_light_ambient_intensity : gdata.day_light_ambient_intensity;
        RenderSettings.ambientIntensity = target;
        if (dir_light != null && dir_light.type == LightType.Directional)
        {
            dir_light.intensity = is_night ? gdata.night_light_dir_intensity : gdata.day_light_dir_intensity;
        }
    }

    void Update()
    {

        //Day night
        GameData gdata = GameData.Get();
        bool is_night = TheGame.Get().IsNight();
        float target = is_night ? gdata.night_light_ambient_intensity : gdata.day_light_ambient_intensity;
        RenderSettings.ambientIntensity = Mathf.MoveTowards(RenderSettings.ambientIntensity, target, 0.2f * Time.deltaTime);
        if (dir_light != null && dir_light.type == LightType.Directional)
        {
            float dtarget = is_night ? gdata.night_light_dir_intensity : gdata.day_light_dir_intensity;
            dir_light.intensity = Mathf.MoveTowards(dir_light.intensity, dtarget, 0.2f * Time.deltaTime);
        }

        //Slow update
        update_timer += Time.deltaTime;
        if (update_timer > 0.5f)
        {
            update_timer = 0f;
            SlowUpdate();
        }
    }

    void SlowUpdate()
    {
        //Optimization
        Vector3 center_pos = TheCamera.Get().GetTargetPosOffsetFace();
        foreach (Selectable select in Selectable.GetAll())
        {
            float dist = (select.GetPosition() - center_pos).magnitude;
            select.SetActive(dist < select.active_range);
        }
    }
}
