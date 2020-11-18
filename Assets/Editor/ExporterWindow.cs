using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TeardownExporter))]
public class ExporterWindow : Editor
{
    public override void OnInspectorGUI()
    {
        TeardownExporter myScript = (TeardownExporter)target;
        if (GUILayout.Button("Select Teardown create folder"))
        {
            myScript.createFolder = EditorUtility.OpenFolderPanel("Select Teardown create folder", "", "");
        }

        GUILayout.Space(30);
        DrawDefaultInspector();        
        
        
        GUILayout.Space(30);
        if (GUILayout.Button("Export To Teardown"))
        {
            myScript.Export();
        }
    }
}