using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(XmlImporter))]
public class XmlImportEditor : Editor
{
    public override void OnInspectorGUI()
    {
        XmlImporter myScript = (XmlImporter)target;
        if (GUILayout.Button("Select File"))
        {
            myScript.fileName = EditorUtility.OpenFilePanel("Select Teardown XML file", "", "xml");
        }
        GUILayout.Space(30);
        DrawDefaultInspector();                
        GUILayout.Space(30);
        
        if (GUILayout.Button("Import"))
        {
            myScript.import();
        }
    }
}