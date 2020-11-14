﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;
using System.Diagnostics;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public class Importer : MonoBehaviour
{
    public string importPath;
    public string vehiclePath;
    public string outputPath;

    [SerializeField] public TMP_Text WorldFile;
    [SerializeField] public TMP_Text ObjectFolder;
    [SerializeField] public Button WorldImportButton;
    [SerializeField] public Button FolderImportButton;
    [SerializeField] public Button ExportButton;

    public bool useMainLua = true;
    public bool useHeistLua= false;

    public enum EnvironmentTeplates
    {
        sunny,
        night,
        sunset, 
        sunrise, 
        foggy, 
        rain        
    };

    public EnvironmentTeplates environmentTeplate = EnvironmentTeplates.sunny;

    public Vector2[] boundaries = new Vector2[4]
    {
        new Vector2(-100, -100),
        new Vector2(100, -100),
        new Vector2(100, 100),
        new Vector2(100, -100)
    };    	

    [ContextMenu("Import Main World Magica File")]
    public void ImportWorld()
    {
        var file = WorldFile.text.Remove(WorldFile.text.Length - 1);
        if (File.Exists(file))
        {
            MagicaRenderer renderer = new MagicaRenderer();
            if (file.EndsWith(".vox"))
                renderer.ImportMagicaVoxelFile(file);
        }
    }

    [ContextMenu("Import Magica Files From Folder")]
    public void ImportFolder()
    {
        var folder = ObjectFolder.text.Remove(ObjectFolder.text.Length - 1);
        if (Directory.Exists(folder))
        {
            MagicaRenderer renderer = new MagicaRenderer();
            foreach (string file in Directory.GetFiles(folder))
            {
                if(file.EndsWith(".vox"))
                    renderer.ImportMagicaVoxelFile(file);
            }
        }
    }

    [ContextMenu("Export To Teardown")]
    public void Export()
    {
        StreamWriter writer = new StreamWriter(outputPath, false);

        writer.WriteLine("<scene version = \"3\" shadowVolume = \"200 100 200\" >");
        writer.WriteLine("\t<environment template = \""+environmentTeplate+"\" />");
        writer.WriteLine("\t<boundary>");
        writer.WriteLine("\t\t<vertex pos=\"" + boundaries[0].x + " " + boundaries[0].y + "\"/>");
        writer.WriteLine("\t\t<vertex pos=\"" + boundaries[1].x + " " + boundaries[1].y + "\"/>");
        writer.WriteLine("\t\t<vertex pos=\"" + boundaries[2].x + " " + boundaries[2].y + "\"/>");
        writer.WriteLine("\t\t<vertex pos=\"" + boundaries[3].x + " " + boundaries[3].y + "\"/>");
        writer.WriteLine("\t</boundary>");

        // Todo - make customizable
        writer.WriteLine("\t<body>");
        writer.WriteLine("\t\t<voxbox pos=\"-100 -0.5 -100\" size=\"2000 1 2000\"/>");
        writer.WriteLine("\t</body>");

        if (useMainLua)
        {
            writer.WriteLine("\t<script file=\"main.lua\"/>");
        }
        if (useHeistLua)
        {
            writer.WriteLine("\t<script file=\"heist.lua\"/>");
        }

        Object[] gameObjects = FindObjectsOfType(typeof(ObjectAttributes));
        foreach(Object obj in gameObjects)
        {
            ObjectAttributes objectAttributes = (ObjectAttributes) obj;
            
            if (objectAttributes.gameObject.GetComponent<TeardownProperties>().isValid())
            {
                float x = objectAttributes.gameObject.transform.position.x;
                float y = objectAttributes.gameObject.transform.position.y;
                float z = objectAttributes.gameObject.transform.position.z;

                x += (float) Math.Floor(objectAttributes.sizeX / 2f) / 10;
                z += (float) (Math.Floor(objectAttributes.sizeZ / 2f) + 1) / 10;

                string rot= (-objectAttributes.gameObject.transform.rotation.eulerAngles.x) + " " + (-objectAttributes.gameObject.transform.rotation.eulerAngles.y) + " " + objectAttributes.gameObject.transform.rotation.eulerAngles.z;

                TeardownProperties teardownProperties = objectAttributes.gameObject.GetComponent<TeardownProperties>();

                string dynamic = "";                
                if (teardownProperties.dynamic)
                {
                    dynamic = " dynamic=\"true\" ";
                }

                string coord = (x + " " + y + " " + (-z)).Replace(",", ".");
                string line = "\t<body rot=\""+rot+"\" pos=\"" + coord + "\"" + dynamic +"><vox file=\"LEVEL\\" + objectAttributes.parentVoxFile + "\" object=\"" + objectAttributes.names[0] + "\"/></body>";
                UnityEngine.Debug.Log(line);
                writer.WriteLine(line);
            }

        }

        Object[] magicaImportedFiles = FindObjectsOfType(typeof(MagicaImportedFile));
        foreach (Object obj in magicaImportedFiles)
        {
            MagicaImportedFile magicaImportedFile = (MagicaImportedFile)obj;
            if (magicaImportedFile.exportToTeardown)
            {
                float x = magicaImportedFile.gameObject.transform.position.x;
                float y = magicaImportedFile.gameObject.transform.position.y;
                float z = magicaImportedFile.gameObject.transform.position.z;

                string coord = (x + " " + y + " " + (-z)).Replace(",", ".");

                string line = "\t<body pos=\"" + coord + "\"><vox file=\"LEVEL\\" + magicaImportedFile.voxFile + "\"/></body>";
                UnityEngine.Debug.Log(line);
                writer.WriteLine(line);
            }
        }

        Object[] waterPlanes = FindObjectsOfType(typeof(Water));
        foreach (Object obj in waterPlanes)
        {
            Water water = (Water)obj;
            if (water.exportToTeardown)
            {
                float x = water.gameObject.transform.position.x;
                float y = water.gameObject.transform.position.y;
                float z = water.gameObject.transform.position.z;

                string coord = (x + " " + y + " " + (-z)).Replace(",", ".");

                string line = "\t<water pos=\"" + coord + "\"/>";
                UnityEngine.Debug.Log(line);
                writer.WriteLine(line);
            }
        }

        writer.WriteLine("</scene>");
        writer.Close();
    }

}
