using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Destructibles are objects that can be destroyed. They have HP and can be damaged by the player or by animals. 
/// They often spawn loot items when destroyed (or killed)
/// </summary>

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(UniqueID))]
public class Destructible : MonoBehaviour
{
    [Header("Stats")]
    public int hp = 100;
    public int armor = 0;
    public float hit_size = 1f;

    [Header("Attack")]
    public bool attack_by_default = true;
    public bool attack_melee_only = true;
    public GroupData required_item;

    [Header("Loot")]
    public CraftData[] loots;

    [Header("FX")]
    public bool shake_on_hit = true;
    public float destroy_delay = 0f;
    public GameObject attack_center;
    public GameObject death_fx;
    public AudioClip hit_sound;
    public AudioClip death_sound;

    public UnityAction onDamaged;
    public UnityAction onDeath;

    [HideInInspector]
    public bool was_built = false; //If set to true by another script wont destroy itself because of UID

    private bool dead = false;

    private Selectable select;
    private Collider[] colliders;
    private UniqueID unique_id;
    private Vector3 shake_center;
    private Vector3 shake_vector = Vector3.zero;
    private float shake_timer = 0f;
    private float shake_intensity = 1f;

    void Awake()
    {
        shake_center = transform.position;
        unique_id = GetComponent<UniqueID>();
        select = GetComponent<Selectable>();
        colliders = GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        if (PlayerData.Get().IsObjectRemoved(GetUID()))
            Destroy(gameObject);
    }

    void Update()
    {
        //Shake FX
        if (shake_on_hit)
        {
            shake_timer -= Time.deltaTime;

            if (shake_timer > 0f)
            {
                shake_vector = new Vector3(Mathf.Cos(shake_timer * Mathf.PI * 16f) * 0.02f, 0f, Mathf.Sin(shake_timer * Mathf.PI * 8f) * 0.01f);
                transform.position += shake_vector * shake_intensity;
            }
            else if (shake_timer > -0.5f)
            {
                transform.position = Vector3.Lerp(transform.position, shake_center, 4f * Time.deltaTime);
            }
        }
    }

    //Deal damages to the destructible, if it reaches 0 HP it will be killed
    public void TakeDamage(int damage)
    {
        if (!dead)
        {
            int adamage = Mathf.Max(damage - armor, 1);
            hp -= adamage;

            if (onDamaged != null)
                onDamaged.Invoke();

            if(shake_on_hit)
                ShakeFX();

            if (hp <= 0)
                Kill();
            
            TheAudio.Get().PlaySFX("destruct", hit_sound);
        }
    }

    //Kill the destructible
    public void Kill()
    {
        if (!dead)
        {
            dead = true;

            foreach(Collider collide in colliders)
                collide.enabled = false;

            //Loot
            foreach (CraftData item in loots)
            {
                float radius = Random.Range(0.5f, 1f);
                float angle = Random.Range(0f, 360f) * Mathf.Rad2Deg;
                Vector3 pos = transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (item is ItemData)
                {
                    Item.Create((ItemData)item, pos, 1);
                }
                if (item is ConstructionData)
                {
                    Construction construct = Construction.Create((ConstructionData)item, pos);
                    construct.FinishContruction();
                }
            }

            if(!was_built)
                PlayerData.Get().RemoveObject(GetUID());

            if (onDeath != null)
                onDeath.Invoke();

            //FX
            if (death_fx != null)
                Instantiate(death_fx, transform.position, Quaternion.identity);

            TheAudio.Get().PlaySFX("destruct", death_sound);

            select.Destroy(destroy_delay);
        }
    }

    //Delayed kill (useful if the attacking character doing an animation before destroying this)
    public void KillIn(float delay)
    {
        StartCoroutine(KillInRun(delay));
    }

    private IEnumerator KillInRun(float delay)
    {
        yield return new WaitForSeconds(delay);
        Kill();
    }

    public void ShakeFX(float intensity = 1f, float duration = 0.2f)
    {
        shake_center = transform.position;
        shake_intensity = intensity;
        shake_timer = duration;
    }

    public string GetUID()
    {
        return unique_id.unique_id;
    }

    public bool IsDead()
    {
        return dead;
    }

    public Vector3 GetCenter()
    {
        if (attack_center != null)
            return attack_center.transform.position;
        return transform.position + Vector3.up * 0.1f; //Bit higher than floor
    }

    public bool IsAttackTarget()
    {
        return attack_by_default;
    }

    public bool CanAttackRanged()
    {
        return !attack_melee_only;
    }

    public Selectable GetSelectable()
    {
        return select;
    }
}
