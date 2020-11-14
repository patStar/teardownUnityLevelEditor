using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

public class Importer : MonoBehaviour
{
    public string importPath;
    public string outputPath;

    public bool importAssetsOnly = false;

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
        new Vector2(-100, 100)
    };    	

    [ContextMenu("Import Magica File")]
    public void Import()
    {
        MagicaRenderer renderer = new MagicaRenderer();
        renderer.ImportMagicaVoxelFile(importPath, importAssetsOnly);
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

            if (objectAttributes.names.Count > 0 && objectAttributes.gameObject.GetComponent<TeardownProperties>() != null && objectAttributes.gameObject.GetComponent<TeardownProperties>().isExportable())
            {
                float x = objectAttributes.gameObject.transform.position.x;
                float y = objectAttributes.gameObject.transform.position.y;
                float z = objectAttributes.gameObject.transform.position.z;

                x += (float) Math.Floor(objectAttributes.sizeX / 2f) / 10;
                z += (float) (Math.Floor(objectAttributes.sizeZ / 2f) + 1) / 10;

                string rot= (-objectAttributes.gameObject.transform.rotation.eulerAngles.x) + " " + (-objectAttributes.gameObject.transform.rotation.eulerAngles.y) + " " + objectAttributes.gameObject.transform.rotation.eulerAngles.z;

                TeardownProperties teardownProperties = objectAttributes.gameObject.GetComponent<TeardownProperties>();

                string texture = "";
                if(teardownProperties.teardownTexture != TeardownProperties.TeardownTextures.No_Texture)
                {
                    texture = "texture=\""+teardownProperties.teardownTexture.ToString().Split('_')[1] + " " + teardownProperties.textureIntensity+"\"";                    
                }

                string dynamic = "";                
                if (teardownProperties.dynamic)
                {
                    dynamic = " dynamic=\"true\" ";
                }

                string coord = (x + " " + y + " " + (-z)).Replace(",", ".");
                string line = "\t<body rot=\""+rot+"\" pos=\"" + coord + "\"" + dynamic +"><vox "+texture+" file=\"LEVEL\\" + objectAttributes.parentVoxFile + "\" object=\"" + objectAttributes.names[0] + "\"/></body>";
                Debug.Log(line);
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
                string rot = (-magicaImportedFile.gameObject.transform.rotation.eulerAngles.x) + " " + (-magicaImportedFile.gameObject.transform.rotation.eulerAngles.y) + " " + magicaImportedFile.gameObject.transform.rotation.eulerAngles.z;

                TeardownProperties teardownProperties = magicaImportedFile.gameObject.GetComponent<TeardownProperties>();

				string dynamic = "";                
                if (teardownProperties.dynamic)
                {
                    dynamic = " dynamic=\"true\" ";
                }
				
                string texture = "";
                if (teardownProperties.teardownTexture != TeardownProperties.TeardownTextures.No_Texture)
                {
                    texture = "texture=\"" + teardownProperties.teardownTexture.ToString().Split('_')[1] + " " + teardownProperties.textureIntensity + "\"";
                }

                string line = "\t<body rot=\""+rot+"\" "+dynamic+" pos=\"" + coord + "\"><vox "+texture+" file=\"LEVEL\\" + magicaImportedFile.voxFile + "\"/></body>";
                Debug.Log(line);
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
                Debug.Log(line);
                writer.WriteLine(line);
            }
        }

        Object[] joints = FindObjectsOfType(typeof(Joint));
        foreach (Object obj in joints)
        {
            Joint joint = (Joint)obj;

            float x = joint.gameObject.transform.position.x;
            float y = joint.gameObject.transform.position.y;
            float z = joint.gameObject.transform.position.z+0.1f;

            string rot = (-joint.gameObject.transform.rotation.eulerAngles.x) + " " + (-joint.gameObject.transform.rotation.eulerAngles.y) + " " + joint.gameObject.transform.rotation.eulerAngles.z;
            string coord = (x + " " + y + " " + (-z)).Replace(",", ".");

            string limits = "";
            if (joint.useLimits)
            {
                limits = "limits=\""+joint.minLimit.ToString().Replace(",", ".") + " " +joint.maxLimit.ToString().Replace(",", ".") + "\"";
            }

            string inner = "";
            if (joint.showJointHelper)
            {
                inner = "\t\t<voxbox pos=\"-0.05 -0.05 -0.05\" color=\"1 0.0 0.0\" size=\"1 1 1\" prop=\"true\" collide=\"false\" />";
            }

            string line = "\t<joint pos=\"" + coord + "\" rot=\"" + rot + "\" type=\""+joint.jointType+"\" rotstrength=\""+ joint.rotStrength+ "\" rotspring=\"" + joint.rotSpring + "\" sound=\"" + joint.sound + "\" size=\"" + joint.size.ToString().Replace(",",".") + "\" "+limits;

            if(inner.Length == 0)
            {
                line += "/>";
                writer.WriteLine(line);
            }
            else
            {
                line += ">";
                writer.WriteLine(line);
                writer.WriteLine(inner);
                writer.WriteLine("\t</joint>");
            }

        }

        writer.WriteLine("</scene>");
        writer.Close();
    }

}
