using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all character animations
/// </summary>

[RequireComponent(typeof(PlayerCharacter))]
public class PlayerCharacterAnim : MonoBehaviour
{
    public string move_anim = "Move";
    public string sleep_anim = "Sleep";
    public string attack_anim = "Attack";
    public string take_anim = "Take";
    public string build_anim = "Build";
    public string damaged_anim = "Damaged";
    public string death_anim = "Death";

    private PlayerCharacter character;
    private PlayerCharacterEquip character_equip;
    private Animator animator;

    void Awake()
    {
        character = GetComponent<PlayerCharacter>();
        character_equip = GetComponent<PlayerCharacterEquip>();
        animator = GetComponentInChildren<Animator>();
        
        character.onTakeItem += OnTake;
        character.onGainItem += OnGain;
        character.onBuild += OnBuild;
        character.onAttack += OnAttack;
        character.onAttackHit += OnAttackHit;
        character.onDamaged += OnDamaged;
        character.onDeath += OnDeath;
        character.onTriggerAnim += OnTriggerAnim;
    }

    void Update()
    {
        bool paused = TheGame.Get().IsPaused();
        animator.enabled = !paused;

        if (animator.enabled)
        {
            animator.SetBool(move_anim, character.IsMoving());
            animator.SetBool(sleep_anim, character.IsSleeping());
        }
    }

    private void OnTake(Item item)
    {
        animator.SetTrigger(take_anim);
    }

    private void OnGain(ItemData item)
    {
        animator.SetTrigger(take_anim);
    }

    private void OnBuild(ConstructionData item)
    {
        animator.SetTrigger(build_anim);
    }

    private void OnDamaged()
    {
        animator.SetTrigger(damaged_anim);
    }

    private void OnDeath()
    {
        animator.SetTrigger(death_anim);
    }

    private void OnAttack(Destructible target, bool ranged)
    {
        string anim = attack_anim;

        //Replace anim based on current equipped item
        if (character_equip != null)
        {
            EquipItem equip = character_equip.GetEquippedItem(EquipSlot.Hand);
            if (equip != null)
            {
                if (!ranged && !string.IsNullOrEmpty(equip.attack_melee_anim))
                    anim = equip.attack_melee_anim;
                if (ranged && !string.IsNullOrEmpty(equip.attack_ranged_anim))
                    anim = equip.attack_ranged_anim;
            }
        }

        animator.SetTrigger(anim);
    }

    private void OnAttackHit(Destructible target)
    {

    }

    private void OnTriggerAnim(string anim)
    {
        animator.SetTrigger(anim);
    }
}
