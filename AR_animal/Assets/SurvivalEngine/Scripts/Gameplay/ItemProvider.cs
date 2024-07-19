using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates items over time, that can be picked by the player. Examples include bird nest (create eggs), or a fishing spot (create fishes).
/// </summary>

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(UniqueID))]
public class ItemProvider : MonoBehaviour
{
    public int item_max = 3;
    public float item_spawn_time = 2f; //In game hours
    public ItemData item;
    public bool take_by_default;

    public AudioClip take_sound;

    public GameObject[] item_models;

    private UniqueID unique_id;

    private int nb_item = 1;
    private bool is_taking = false;
    private float item_progress = 0f;

    void Awake()
    {
        unique_id = GetComponent<UniqueID>();
    }

    private void Start()
    {
        if(PlayerData.Get().HasUniqueID(GetAmountUID()))
            nb_item = PlayerData.Get().GetUniqueID(GetAmountUID());

        if (take_by_default)
            GetComponent<Selectable>().onUse += OnUse;
    }

    void Update()
    {
        if (TheGame.Get().IsPaused())
            return;

        float game_speed = GameData.Get().game_time_mult;
        float hour_to_sec = game_speed / 3600f;

        item_progress += hour_to_sec * Time.deltaTime;
        if (item_progress > item_spawn_time)
        {
            item_progress = 0f;
            nb_item += 1;
            nb_item = Mathf.Min(nb_item, item_max);

            PlayerData.Get().SetUniqueID(GetAmountUID(), nb_item);
        }

        for (int i = 0; i < item_models.Length; i++)
        {
            bool visible = (i < nb_item);
            if (item_models[i].activeSelf != visible)
                item_models[i].SetActive(visible);
        }
    }

    public void TakeItem()
    {
        if (!is_taking)
        {
            StartCoroutine(TakeItemRun());
        }
    }

    private IEnumerator TakeItemRun()
    {
        is_taking = true;
        yield return new WaitForSeconds(0.4f);

        if (nb_item > 0)
            nb_item--;

        PlayerData.Get().SetUniqueID(GetAmountUID(), nb_item);
        is_taking = false;

        TheAudio.Get().PlaySFX("item", take_sound);
    }

    private void OnUse(PlayerCharacter player)
    {
        if(HasItem() && !is_taking)
        {
            TakeItem();
            PlayerCharacter.Get().GainItem(gameObject, item, 1);
        }
    }

    public bool HasItem()
    {
        return nb_item > 0;
    }

    public int GetNbItem()
    {
        return nb_item;
    }

    public string GetAmountUID()
    {
        return unique_id.unique_id + "_amount";
    }
}
