using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class RandomizeObjects : ScriptableWizard
{
    public float noise_dist = 1f;
    
    [MenuItem("Tools/Randomize Objects")]
    static void SelectAllOfTagWizard()
    {
        ScriptableWizard.DisplayWizard<RandomizeObjects>("Randomize Objects", "Randomize Objects");
    }
    
    void DoRandomize()
    {
        Undo.RegisterCompleteObjectUndo(Selection.transforms, "randomize");
        foreach (Transform transform in Selection.transforms)
        {
            DoRandomize(transform);

            if (!transform.GetComponent<Selectable>())
            {
                for (int i = 0; i < transform.childCount; i++)
                    DoRandomize(transform.GetChild(i));
            }
        }
    }

    void DoRandomize(Transform transform)
    {
        Vector3 offset = new Vector3(Random.Range(-noise_dist, noise_dist), 0f, Random.Range(-noise_dist, noise_dist));
        transform.position += offset;
    }
    
    void OnWizardCreate()
    {
        DoRandomize();
    }
}