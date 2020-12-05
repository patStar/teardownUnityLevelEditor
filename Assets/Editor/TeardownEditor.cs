using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TeardownEditor : EditorWindow
{
    string levelXmlPath = "Hello World";
    bool groupEnabled;
    GameObject exportRootNode;
    bool myBool = true;
    float myFloat = 1.23f;
    int tab = 0;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Teardown")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        TeardownEditor window = (TeardownEditor)EditorWindow.GetWindow(typeof(TeardownEditor));
        window.Show();
    }

    void OnGUI()
    {
        tab = GUILayout.Toolbar(tab, new string[] { "Import XML", "Export XML", "Vox Files" });

        if (tab == 0)
        {
            drawImportDialog();            
        }
        else if (tab == 1)
        {
            GUILayout.Label("Export Level XML", EditorStyles.boldLabel);
            exportRootNode = ((GameObject)EditorGUILayout.ObjectField("Scene to export", exportRootNode, typeof(GameObject)));
            XmlExporter xmlExporter = new XmlExporter();            
            if (GUILayout.Button("Export"))
            {
                string savePath = EditorUtility.SaveFilePanel(
                "Save Teardown scene to XML",
                "","","xml");
                xmlExporter.exportToXML(savePath, exportRootNode);
            }
        }

        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        myBool = EditorGUILayout.Toggle("Toggle", myBool);
        myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup();
        
    }

    private void drawImportDialog()
    {
        GUILayout.Label("Import Level XML", EditorStyles.boldLabel);
        levelXmlPath = EditorGUILayout.TextField("Level XML", levelXmlPath);
        if (GUILayout.Button("Select XML"))
        {
            levelXmlPath = EditorUtility.OpenFilePanel("Select Teardown XML file", "", "xml");
        }
        GUILayout.Space(10);
        XmlImporter xmlImporter = new XmlImporter();
        xmlImporter.fileName = levelXmlPath;
        if (GUILayout.Button("Import"))
        {
            xmlImporter.import();
        }
    }
}