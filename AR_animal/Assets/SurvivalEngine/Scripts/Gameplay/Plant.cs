using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plants can be sowed (from a seed) and their fruit can be harvested. They can also have multiple growth stages.
/// </summary>

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(UniqueID))]
[RequireComponent(typeof(Destructible))]
public class Plant : MonoBehaviour
{
    public PlantData data;
    public int growth_stage = 0;

    [Header("Growth")]
    public float grow_time = 48f;

    [Header("Harvest")]
    public ItemData fruit;
    public float fruit_grow_time = 24f; //In game hours
    public Transform fruit_model;
    public bool death_on_harvest;

    [Header("FX")]
    public GameObject gather_fx;
    public AudioClip gather_audio;

    [HideInInspector]
    public bool was_built = false; //If true, means it was created by the player

    private UniqueID unique_id;
    private float fruit_progress = 0f;
    private float growth_progress = 0f;
    private Selectable select;
    private Destructible destruct;
    private bool has_fruit = false;
    private bool is_taking = false;

    private static List<Plant> plant_list = new List<Plant>();

    void Awake()
    {
        plant_list.Add(this);
        select = GetComponent<Selectable>();
        destruct = GetComponent<Destructible>();
        unique_id = GetComponent<UniqueID>();
    }

    private void OnDestroy()
    {
        plant_list.Remove(this);
    }

    private void Start()
    {
        select.onDestroy += OnDeath;

        if (fruit_model != null)
            fruit_model.gameObject.SetActive(false);

        //Fruit
        if (PlayerData.Get().HasUniqueID(GetFruitUID()))
            has_fruit = PlayerData.Get().GetUniqueID(GetFruitUID()) > 0;
    }

    void Update()
    {
        if (TheGame.Get().IsPaused())
            return;

        float game_speed = GameData.Get().game_time_mult;
        float hour_to_sec = game_speed / 3600f;

        if (!IsFullyGrown() && grow_time > 0.001f)
        {
            growth_progress += hour_to_sec * Time.deltaTime;
            if (growth_progress > grow_time)
            {
                growth_progress = 0f;
                GrowPlant();
                return;
            }
        }

        if (!has_fruit && fruit != null)
        {
            fruit_progress += hour_to_sec * Time.deltaTime;

            if (fruit_progress > fruit_grow_time)
            {
                has_fruit = true;
                fruit_progress = 0f;
                PlayerData.Get().SetUniqueID(GetFruitUID(), 1);
            }
        }

        //Display
        if (fruit_model != null && has_fruit != fruit_model.gameObject.activeSelf)
            fruit_model.gameObject.SetActive(has_fruit);

    }

    public void GrowPlant()
    {
        if (!IsFullyGrown())
        {
            SowedPlantData sdata = PlayerData.Get().GetSowedPlant(GetUID());
            if (sdata == null) {
                //Remove this plant and create a new one (this one probably was already in the scene)
                if(!was_built)
                    PlayerData.Get().RemoveObject(GetUID()); //Remove Unique id
                sdata = PlayerData.Get().AddPlant(data.id, SceneNav.GetCurrentScene(), transform.position, growth_stage + 1);
            }
            else {
                //Grow current plant from data
                PlayerData.Get().GrowPlant(GetUID(), growth_stage + 1);
            }
            
            Plant plant = Spawn(sdata.uid);
            Destroy(gameObject);
        }
    }

    public void Gather(PlayerCharacter character)
    {
        if (fruit != null && has_fruit && !is_taking && PlayerData.Get().CanTakeItem(fruit.id, 1))
        {
            StartCoroutine(GatherRun(character));
        }
    }

    private IEnumerator GatherRun(PlayerCharacter character)
    {
        is_taking = true;

        GameObject source = fruit_model != null ? fruit_model.gameObject : gameObject;
        character.GainItem(source, fruit, 1);

        yield return new WaitForSeconds(0.4f);

        has_fruit = false;
        PlayerData.Get().SetUniqueID(GetFruitUID(), 0);
        
        if (death_on_harvest && destruct != null)
            destruct.Kill();

        TheAudio.Get().PlaySFX("plant", gather_audio);

        if (gather_fx != null)
            Instantiate(gather_fx, transform.position, Quaternion.identity);

        is_taking = false;
    }

    private void OnDeath()
    {
        PlayerData.Get().RemovePlant(GetUID());
    }

    public bool HasFruit()
    {
        return has_fruit;
    }

    public bool IsFullyGrown()
    {
        return (growth_stage + 1) >= data.growth_stage_prefabs.Length;
    }

    public string GetFruitUID() {
        return GetUID() + "_fruit";
    }

    public string GetUID()
    {
        return unique_id.unique_id;
    }

    public static Plant GetNearest(Vector3 pos, float range = 999f)
    {
        Plant nearest = null;
        float min_dist = range;
        foreach (Plant plant in plant_list)
        {
            float dist = (plant.transform.position - pos).magnitude;
            if (dist < min_dist)
            {
                min_dist = dist;
                nearest = plant;
            }
        }
        return nearest;
    }

    public static List<Plant> GetAll()
    {
        return plant_list;
    }

    //Spawn an existing one in the save file (such as after loading)
    public static Plant Spawn(string uid)
    {
        SowedPlantData sdata = PlayerData.Get().GetSowedPlant(uid);
        if (sdata != null) {
            PlantData pdata = PlantData.Get(sdata.plant_id);
            if (pdata != null)
            {
                GameObject prefab = pdata.GetStagePrefab(sdata.growth_stage);
                GameObject build = Instantiate(prefab, sdata.pos, prefab.transform.rotation);
                Plant plant = build.GetComponent<Plant>();
                plant.data = pdata;
                plant.was_built = true;
                plant.unique_id.unique_id = uid;

                Destructible destruct = plant.GetComponent<Destructible>();
                if (destruct != null)
                    destruct.was_built = true;
                return plant;
            }
        }
        return null;
    }

    //Create a totally new one that will be added to save file
    public static Plant Create(PlantData data, Vector3 pos, int stage)
    {
        SowedPlantData splant = PlayerData.Get().AddPlant(data.id, SceneNav.GetCurrentScene(), pos, stage);
        GameObject prefab = data.GetStagePrefab(stage);
        GameObject build = Instantiate(prefab, pos, prefab.transform.rotation);
        Plant plant = build.GetComponent<Plant>();
        plant.data = data;
        plant.was_built = true;
        plant.unique_id.unique_id = splant.uid;

        Destructible destruct = plant.GetComponent<Destructible>();
        if (destruct != null)
            destruct.was_built = true;

        return plant;
    }
}
