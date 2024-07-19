using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class AlignObjects : ScriptableWizard
{
    
    [MenuItem("Tools/AlignObjects")]
    static void SelectAllOfTagWizard()
    {
        ScriptableWizard.DisplayWizard<AlignObjects>("AlignObjects", "AlignObjects");
    }
    
    void DoAlignCubes()
    {
        Undo.RegisterCompleteObjectUndo(Selection.transforms, "align objects");
        foreach (Transform transform in Selection.transforms)
        {
            transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), 0f, Mathf.RoundToInt(transform.position.z));
        }
    }
    
    void OnWizardCreate()
    {
        DoAlignCubes();
    }
}