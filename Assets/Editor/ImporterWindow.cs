using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Importer))]
public class ImporterWindow : Editor
{
    public override void OnInspectorGUI()
    {        
        Importer myScript = (Importer)target;        
        if (GUILayout.Button("Choose File and Import Object"))
        {
            myScript.importPath = EditorUtility.OpenFilePanel("Load vox file", "", "vox");
            myScript.Import();
        }
    }
}