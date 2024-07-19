using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Constructions are objects that can be placed on the map by the player (by crafting or with items)
/// </summary>

[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(UniqueID))]
[RequireComponent(typeof(Rigidbody))]
public class Construction : MonoBehaviour
{
    public ConstructionData data;

    [Header("FX")]
    public AudioClip build_audio;
    public GameObject build_fx;

    [HideInInspector]
    public bool was_built = false; //If true, means it was created by the player

    public UnityAction onBuild;

    private Selectable selectable;
    private Destructible destruct; //Can be nulls
    private UniqueID unique_id;

    private bool building_mode = false; //Building mode means player is selecting where to build it, but it doesnt really exists yet
    private bool is_overlap = false;
    private Color prev_color = Color.white;

    private List<GameObject> overlap_list = new List<GameObject>();
    private List<Collider> colliders = new List<Collider>();
    private List<MeshRenderer> renders = new List<MeshRenderer>();
    private List<Material> materials = new List<Material>();
    private List<Material> materials_transparent = new List<Material>();
    private List<Color> materials_color = new List<Color>();

    private static List<Construction> construct_list = new List<Construction>();

    void Awake()
    {
        construct_list.Add(this);
        selectable = GetComponent<Selectable>();
        unique_id = GetComponent<UniqueID>();
        renders.AddRange(GetComponentsInChildren<MeshRenderer>());

        foreach (MeshRenderer render in renders)
        {
            foreach (Material material in render.sharedMaterials)
            {
                Material material_normal = new Material(material);
                Material material_trans = new Material(material);
                MaterialTool.ChangeRenderMode(material_trans, BlendMode.Fade);
                materials.Add(material_normal);
                materials_transparent.Add(material_trans);
                materials_color.Add(material.color);
            }
        }
    }

    private void OnDestroy()
    {
        construct_list.Remove(this);
    }

    private void Start()
    {
        selectable.onUse += OnUse;
        selectable.onDestroy += OnDeath;

        if (!was_built && PlayerData.Get().IsObjectRemoved(GetUID()))
            Destroy(gameObject);
    }

    void Update()
    {
        if (building_mode)
        {
            PlayerControlsMouse mouse = PlayerControlsMouse.Get();
            if (mouse.IsUsingMouse())
            {
                transform.position = mouse.GetPointingPos();
            }
            is_overlap = overlap_list.Count > 0;
            Color color = is_overlap ? Color.red : Color.white;
            SetModelColor(new Color(color.r, color.g, color.b, 0.5f));
            //Debug.Log(is_overlap);
        }
    }

    private void OnUse(PlayerCharacter character)
    {
        //Use

    }

    public void StartConstruction()
    {
        building_mode = true;
        GetComponent<Selectable>().enabled = false; //Using GetComponent because this is called right after Instantiate

        destruct = GetComponent<Destructible>();
        if (destruct)
            destruct.enabled = false;

        foreach (Collider collide in GetComponentsInChildren<Collider>())
        {
            if (collide.enabled && !collide.isTrigger)
            {
                colliders.Add(collide);
                collide.isTrigger = true;
            }
        }
    }

    public void FinishContruction()
    {
        building_mode = false;
        is_overlap = false;
        selectable.enabled = true;
        foreach (Collider collide in colliders)
            collide.isTrigger = false;

        destruct = GetComponent<Destructible>();
        if (destruct)
        {
            destruct.enabled = true;
            destruct.was_built = true;
        }

        SetModelColor(Color.white);

        BuiltConstructionData cdata = PlayerData.Get().AddConstruction(data.id, SceneNav.GetCurrentScene(), transform.position);
        unique_id.unique_id = cdata.uid;

        if(build_fx != null)
            Instantiate(build_fx, transform.position, Quaternion.identity);
        TheAudio.Get().PlaySFX("construction", build_audio);

        if (onBuild != null)
            onBuild.Invoke();
    }

    public void SetModelColor(Color color)
    {
        if (color != prev_color)
        {
            int index = 0;
            foreach (MeshRenderer render in renders)
            {
                Material[] mesh_materials = render.sharedMaterials;
                for (int i = 0; i < mesh_materials.Length; i++)
                {
                    if (index < materials.Count && index < materials_transparent.Count)
                    {
                        Material mesh_mat = mesh_materials[i];
                        Material ref_mat = color.a < 0.99f ? materials_transparent[index] : materials[index];
                        ref_mat.color = materials_color[index] * color;
                        if (ref_mat != mesh_mat)
                            mesh_materials[i] = ref_mat;
                    }
                    index++;
                }
                render.sharedMaterials = mesh_materials;
            }
        }

        prev_color = color;
    }

    private void OnDeath()
    {
        PlayerData.Get().RemoveConstruction(GetUID());
        if(!was_built)
            PlayerData.Get().RemoveObject(GetUID());
    }

    public bool IsConstructing()
    {
        return building_mode;
    }

    public bool IsOverlaping()
    {
        return is_overlap;
    }

    public float GetUseRange()
    {
        return selectable.use_range;
    }

    public string GetUID()
    {
        return unique_id.unique_id;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && !other.GetComponent<PlayerCharacter>())
        {
            int other_layer = 1 << other.gameObject.layer;
            int this_layer = 1 << gameObject.layer;
            if((other_layer & this_layer) > 0) //Dont overlap with same layer
                overlap_list.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger && overlap_list.Contains(other.gameObject))
        {
            overlap_list.Remove(other.gameObject);
        }
    }

    public static Construction GetNearest(Vector3 pos, float range = 999f)
    {
        Construction nearest = null;
        float min_dist = range;
        foreach (Construction construction in construct_list)
        {
            float dist = (construction.transform.position - pos).magnitude;
            if (dist < min_dist)
            {
                min_dist = dist;
                nearest = construction;
            }
        }
        return nearest;
    }

    public static List<Construction> GetAll()
    {
        return construct_list;
    }

    //Spawn an existing one in the save file (such as after loading)
    public static Construction Spawn(string uid)
    {
        BuiltConstructionData bdata = PlayerData.Get().GetConstructed(uid);
        if (bdata != null)
        {
            ConstructionData cdata = ConstructionData.Get(bdata.construction_id);
            if (cdata != null)
            {
                GameObject build = Instantiate(cdata.construction_prefab, bdata.pos, cdata.construction_prefab.transform.rotation);
                Construction construct = build.GetComponent<Construction>();
                construct.data = cdata;
                construct.was_built = true;
                construct.unique_id.unique_id = uid;

                Destructible destruct = construct.GetComponent<Destructible>();
                if (destruct != null)
                    destruct.was_built = true;
                return construct;
            }
        }
        return null;
    }

    //Create a totally new one that will be added to save file, but only after constructed by the player
    public static Construction CreateBuildMode(ConstructionData data, Vector3 pos)
    {
        GameObject build = Instantiate(data.construction_prefab, pos, data.construction_prefab.transform.rotation);
        Construction construct = build.GetComponent<Construction>();
        construct.data = data;
        construct.was_built = true;

        Destructible destruct = construct.GetComponent<Destructible>();
        if (destruct != null)
            destruct.was_built = true;

        return construct;
    }

    //Create a totally new one that will be added to save file, already constructed
    public static Construction Create(ConstructionData data, Vector3 pos)
    {
        Construction construct = CreateBuildMode(data, pos);
        construct.FinishContruction();
        return construct;
    }
}
