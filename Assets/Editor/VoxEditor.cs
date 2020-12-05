using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Vox))]
public class VoxEditor: Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(30);

        Vox myScript = (Vox)target;
        if (GUILayout.Button("Select File"))
        {
            myScript.file  = EditorUtility.OpenFilePanel("Select Vox file", "", "vox");
        }


        GUILayout.Space(30);
        if (GUILayout.Button("Reload"))
        {
            myScript.Reload();
        }
    }
}