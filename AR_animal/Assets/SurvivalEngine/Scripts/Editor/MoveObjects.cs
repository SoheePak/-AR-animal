using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class MoveObjects : ScriptableWizard
{
    public Vector3 move;
    public Vector3 rotate;

    [MenuItem("Tools/Transform Group")]
    static void SelectAllOfTagWizard()
    {
        ScriptableWizard.DisplayWizard<MoveObjects>("Transform Group", "Transform Group");
    }

    void MoveObject(Transform obj, Vector3 move_vect)
    {
        obj.position += move_vect;
        obj.rotation = obj.rotation * Quaternion.Euler(rotate);
    }
    
    void OnWizardCreate()
    {
        Undo.RegisterCompleteObjectUndo(Selection.transforms, "move objects");
        foreach (Transform transform in Selection.transforms)
        {
            MoveObject(transform, move);
        }
    }
}