using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UniqueID)), CanEditMultipleObjects]
public class UniqueIDEditor : Editor
{

    public override void OnInspectorGUI()
    {
        UniqueID myScript = target as UniqueID;

        DrawDefaultInspector();

        EditorGUILayout.Space();

        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Generate Unique ID", style);
        EditorGUILayout.LabelField("Click on Tools->Generate UIDs to generate\nall empty UIDs in the scene.\nOr click below to generate this one.", GUILayout.Height(50));

        if (GUILayout.Button("Generate UID"))
        {
            Undo.RecordObject(myScript, "Generate UID");
            myScript.GenerateUID();
            EditorUtility.SetDirty(myScript);
        }

        EditorGUILayout.Space();
    }

}
