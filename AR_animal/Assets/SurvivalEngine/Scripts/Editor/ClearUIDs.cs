using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class ClearUIDs : ScriptableWizard
{
    
    [MenuItem("Tools/Clear UIDs")]
    static void SelectAllOfTagWizard()
    {
        ScriptableWizard.DisplayWizard<ClearUIDs>("Clear Unique IDs", "Clear All UIDs");
    }
    
    void OffsetObjects(UniqueID[] objs)
    {
        HashSet<string> existing_ids = new HashSet<string>();

        foreach (UniqueID uid_obj in objs)
        {
            uid_obj.unique_id = "";
            EditorUtility.SetDirty(uid_obj);
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
    
    void OnWizardCreate()
    {
        OffsetObjects(GameObject.FindObjectsOfType<UniqueID>());
    }
}