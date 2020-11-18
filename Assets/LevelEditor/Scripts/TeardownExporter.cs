using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using Boo.Lang.Environments;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class TeardownExporter : MonoBehaviour
{
    [Header("Output paths setup")]
    [Tooltip("The path to the create folder of Teardown")]
    public string createFolder;
    [Tooltip("The name of the level which is also the folder name and the name of the xml.")]
    public string levelName;
    [Space(10)]    
    [Tooltip("The name of the output file inside the create/levename/ folder. If empty it is levelname.xml by default.")]
    public string outputFileName;
    [Space(10)]
    [Tooltip("Import the main.lua into the level to have tools and stuff")]
    public bool useMainLua = true;
    [Space(10)]
    [Header("Heist")]
    [Tooltip("Import the heist.lua into the level to create basic heist missions")]
    public bool useHeistLua = false;
    [Tooltip("Import the escape.lua into the level to create escape vehicle extraction")]
    public bool useEscapeVehicles = false;
    [Tooltip("Write the customHeist.lua so the heist can be completed and alarms can be done")]
    public bool writeCustomHeistLua = false;

    [Space(10)]
    [Header("Level Data")]
    [Tooltip("Write the level informations to the missions.lua")]
    public bool writeLevelDataLua = false;
    [Tooltip("The title of the level to show at the beginning and on the map")]
    public string levelTitle = "";
    [Tooltip("The description of the level shown on the map")]
    public string levelDescription= "";
    [Tooltip("The security title of the level to show on the map")]
    public string securityTitle = "";
    [Tooltip("The security description shown on the map")]
    public string securityDescription = "";
    [Tooltip("The number of targets required to win the game. If it is 0, it will be calculated by the targets in the game.")]
    public int numberOfRequiredTargets = 0;

    public enum EnvironmentTeplates
    {
        sunny,
        night,
        sunset, 
        sunrise, 
        foggy, 
        rain        
    };

    [Space(10)]
    [Header("Environment")]
    [Tooltip("Choose the weather for the level")]
    public EnvironmentTeplates environmentTeplate = EnvironmentTeplates.sunny;
    [Tooltip("Set the boundaries for this level")]
    public Vector2[] boundaries = new Vector2[4]
    {
        new Vector2(-100, -100),
        new Vector2(100, -100),
        new Vector2(100, 100),
        new Vector2(-100, 100)
    };

    public void exportAssets(StreamWriter writer)
    {
        Object[] gameObjects = FindObjectsOfType(typeof(ObjectAttributes));
        foreach (Object obj in gameObjects)
        {                          
            ObjectAttributes objectAttributes = (ObjectAttributes)obj;
            if (objectAttributes.gameObject.GetComponent<EscapeVehicle>() != null) continue;

            MagicaImportedFile wholeFile = objectAttributes.gameObject.GetComponentInParent<MagicaImportedFile>();
            if(wholeFile != null)
            {
                return;
            }

            if (objectAttributes.names.Count > 0 && objectAttributes.gameObject.GetComponent<TeardownProperties>() != null && objectAttributes.gameObject.GetComponent<TeardownProperties>().isExportable())
            {
                Debug.Log(objectAttributes.bottomCenterOfVoxelMass);

                float x = objectAttributes.gameObject.transform.position.x;
                float y = objectAttributes.gameObject.transform.position.y;
                float z = objectAttributes.gameObject.transform.position.z + 0.1f;
                string coord = (x + " " + y + " " + (-z)).Replace(",", ".");

                string rot = ((-objectAttributes.gameObject.transform.rotation.eulerAngles.x) + " " + (-objectAttributes.gameObject.transform.rotation.eulerAngles.y) + " " + objectAttributes.gameObject.transform.rotation.eulerAngles.z).Replace(",", "."); ;

                TeardownProperties teardownProperties = objectAttributes.gameObject.GetComponent<TeardownProperties>();

                string texture = "";
                if (teardownProperties.teardownTexture != TeardownProperties.TeardownTextures.No_Texture)
                {
                    texture = "texture=\"" + teardownProperties.teardownTexture.ToString().Split('_')[1] + " " + teardownProperties.textureIntensity + "\"";
                }

                string description = "";
                Target target = objectAttributes.gameObject.GetComponent<Target>();
                if (target != null)
                {
                    string imgPath = "";
                    if (target.targetImage != "")
                    {
                        if (target.useLevelFolderForImage)
                        {
                            imgPath = "../../../../create/" + target.targetImage;
                        }
                        else
                        {
                            imgPath = target.targetImage;
                        }
                    }
                    description = " desc=\"" + target.targetName + ";" + target.nameInTargetList + ";" + target.targetDescription + ";" + imgPath + "\" ";
                }

                string tags = createTags(target, teardownProperties.tags);

                string dynamic = "";
                if (teardownProperties.dynamic)
                {
                    dynamic = " dynamic=\"true\" ";
                }
                
                string line = "\t<body " + tags + " " + description + " rot=\"" + rot + "\" pos=\"" + coord + "\"" + dynamic + "><vox " + texture + " file=\"LEVEL\\" + objectAttributes.parentVoxFile + "\" object=\"" + objectAttributes.names[0] + "\"/></body>";
                writer.WriteLine(line);
            }

        }
    }

    private static string createTags(Target target, string objectTags)
    {
        string tags = "";
        if (objectTags.Length > 0 || target != null)
        {
            string tagsInternal = objectTags;

            if (target != null)
            {
                string targetTag = " target";
                if (!target.targetType.Equals(Target.TargetType.standard))
                {
                    targetTag += target.targetType.ToString().ToLower();
                }
                tagsInternal += targetTag;
                if (target.isOptionalTarget)
                {
                    tagsInternal += " optional";
                }
                if (target.isHiddenTarget)
                {
                    tagsInternal += " hidden";
                }
                if (target.startsAnAlarm)
                {
                    tagsInternal += " alarmstarter";
                }
            }
            tags = "tags=\"" + tagsInternal + "\"";
        }

        return tags;
    }

    public void exportVoxFiles(StreamWriter writer)
    {
        Object[] magicaImportedFiles = FindObjectsOfType(typeof(MagicaImportedFile));
        foreach (Object obj in magicaImportedFiles)
        {
            MagicaImportedFile magicaImportedFile = (MagicaImportedFile)obj;
            if (magicaImportedFile.gameObject.GetComponent<EscapeVehicle>() != null) continue;

            if (magicaImportedFile.exportToTeardown)
            {
                float x = magicaImportedFile.gameObject.transform.position.x;
                float y = magicaImportedFile.gameObject.transform.position.y;
                float z = magicaImportedFile.gameObject.transform.position.z;

                string coord = (x + " " + y + " " + (-z)).Replace(",", ".");
                string rot = ((-magicaImportedFile.gameObject.transform.rotation.eulerAngles.x) + " " + (-magicaImportedFile.gameObject.transform.rotation.eulerAngles.y) + " " + magicaImportedFile.gameObject.transform.rotation.eulerAngles.z).Replace(",", "."); ;

                TeardownProperties teardownProperties = magicaImportedFile.gameObject.GetComponent<TeardownProperties>();

                string dynamic = "";
                if (teardownProperties.dynamic)
                {
                    dynamic = " dynamic=\"true\" ";
                }

                string description = "";
                Target target = magicaImportedFile.gameObject.GetComponent<Target>();
                if (target != null)
                {
                    string imgPath = "";
                    if (target.targetImage != "")
                    {
                        if (target.useLevelFolderForImage)
                        {
                            imgPath = "../../../../create/" + target.targetImage;
                        }
                        else
                        {
                            imgPath = target.targetImage;
                        }
                    }
                    description = " desc=\"" + target.targetName + ";" + target.nameInTargetList + ";" + target.targetDescription + ";" + imgPath + "\" ";
                }

                string tags = createTags(target, teardownProperties.tags);

                string texture = "";
                if (teardownProperties.teardownTexture != TeardownProperties.TeardownTextures.No_Texture)
                {
                    texture = "texture=\"" + teardownProperties.teardownTexture.ToString().Split('_')[1] + " " + teardownProperties.textureIntensity + "\"";
                }

                string line = "\t<body " + tags + " " + description+" rot=\"" + rot + "\" " + dynamic + " pos=\"" + coord + "\"><vox " + texture + " file=\"LEVEL\\" + magicaImportedFile.voxFile + "\"/></body>";

                writer.WriteLine(line);
            }
        }
    }

    [ContextMenu("Export To Teardown")]
    public void Export()
    {
        if (outputFileName == "")
        {
            outputFileName = levelName.Trim() + ".xml";
        }
        
        StreamWriter writer = new StreamWriter(Path.Combine(createFolder.Trim(), outputFileName).Normalize(), false);

        writer.WriteLine("<scene version = \"3\" shadowVolume = \"200 100 200\" >");
        if (writeCustomHeistLua)
        {
            File.Copy(Path.Combine(Application.dataPath, "lua", "customHeist.lua"), Path.Combine(createFolder.Trim(), levelName.Trim(), "customHeist.lua").Normalize(), true);
            writer.WriteLine("\t<script file=\"LEVEL/customHeist.lua\"/>");
        }
        if (writeLevelDataLua) { 
            
            StreamWriter missionLevelWriter = new StreamWriter(Path.Combine(createFolder.Trim(), levelName, "missionLevel.lua").Normalize(), false);
            missionLevelWriter.WriteLine("function init()");
            missionLevelWriter.WriteLine("\tSetString(\"game.levelid\", \"" + levelName + "\")");
            missionLevelWriter.WriteLine("\tSetString(\"game.levelpath\", \"\")");
            missionLevelWriter.WriteLine("end");
            missionLevelWriter.Close();
            writer.WriteLine("\t<script file=\"LEVEL/missionLevel.lua\"/>");

            StreamReader missionsReader = new StreamReader(Path.Combine(createFolder.Trim(), "..", "data", "missions.lua").Normalize());
            StreamWriter missionsBackupWriter = new StreamWriter(Path.Combine(createFolder.Trim(), "..", "data", "missions_backup.lua").Normalize(), false);
            string missionsContent = missionsReader.ReadToEnd();
            missionsReader.Close();
            missionsBackupWriter.WriteLine(missionsContent);
            missionsBackupWriter.Close();

            int numberOfSecondaryTargets = 0;
            int numberOfPrimaryTargets = 0;
            Object[] targets = FindObjectsOfType(typeof(Target));
            foreach (Object obj in targets)
            {
                Target target = (Target)obj;
                if (target.isOptionalTarget)
                {
                    numberOfSecondaryTargets++;
                }
                else
                {
                    numberOfPrimaryTargets++;
                }
            }
            int totalNumberOfRequiredTargets = numberOfRequiredTargets;
            if(totalNumberOfRequiredTargets == 0)
            {
                totalNumberOfRequiredTargets = numberOfPrimaryTargets;
            }

            string levelCode = "gMissions[\"" + levelName + "\"] = { level = \"" + levelName + "\", title = \"" + levelTitle + "\", desc = \"" + levelDescription + "\", primary = " + numberOfPrimaryTargets + ", secondary = " + numberOfSecondaryTargets + ", required = " + totalNumberOfRequiredTargets + ", bonus = { }, securityTitle=\"" + securityTitle + "\", securityDesc=\"" + securityDescription + "\"}";        

            StreamWriter missionsWriter = new StreamWriter(Path.Combine(createFolder.Trim(), "..", "data", "missions.lua").Normalize(), false);
            if (missionsContent.IndexOf("gMissions[\"" + levelName + "\"]") < 0)
            {
                missionsWriter.WriteLine(missionsContent);
                missionsWriter.WriteLine("");
                missionsWriter.WriteLine(levelCode);
                missionsWriter.WriteLine("");
            }
            else
            {
                missionsContent = Regex.Replace(missionsContent, "gMissions\\[\"" + levelName + "\"\\].*\n", levelCode);                
                missionsWriter.WriteLine(missionsContent);
            }
            missionsWriter.Close();
            

            writer.WriteLine("\t<script file=\"LEVEL/missionLevel.lua\"/>");
        }

        if (useEscapeVehicles)
        {
            writer.WriteLine("\t<script file=\"escape.lua\">");
            writeEscapeCamera(writer);
            writeEscapeVehicle(writer);
            writer.WriteLine("\t</script>");
        }

        exportEnvironment(writer);
        exportBoundary(writer);
        exportBasePlate(writer);
        exportAssets(writer);
        exportVoxFiles(writer);
        exportWater(writer);
        exportJoints(writer);
        exportVehicles(writer);
        exportScripts(writer);

        exportLights(writer);
        exportSpawnPoint(writer);
        exportVoxBox(writer);

        writer.WriteLine("</scene>");
        writer.Close();
    }

    private static void writeEscapeVehicle(StreamWriter writer)
    {
        EscapeVehicle escapeVehicle = (EscapeVehicle)FindObjectOfType(typeof(EscapeVehicle));

        if (escapeVehicle != null)
        {
            float x = escapeVehicle.gameObject.transform.position.x;
            float y = escapeVehicle.gameObject.transform.position.y;
            float z = escapeVehicle.gameObject.transform.position.z + 0.1f;
            string coord = (x + " " + y + " " + (-z)).Replace(",", ".");

            string rot = ((-escapeVehicle.gameObject.transform.rotation.eulerAngles.x) + " " + (-escapeVehicle.gameObject.transform.rotation.eulerAngles.y) + " " + escapeVehicle.gameObject.transform.rotation.eulerAngles.z).Replace(",", "."); ;

            string file = "LEVEL/";
            if (escapeVehicle.gameObject.GetComponent<MagicaImportedFile>() != null)
            {
                file += escapeVehicle.gameObject.GetComponent<MagicaImportedFile>().voxFile;
            }
            else
            {
                file += escapeVehicle.gameObject.GetComponent<ObjectAttributes>().parentVoxFile + "\" object=\"" + escapeVehicle.gameObject.GetComponent<ObjectAttributes>().names[0];
            }

            writer.WriteLine("\t\t<body tags=\"escapevehicle\" pos=\"" + coord + "\" dynamic = \"true\">");
            writer.WriteLine("\t\t\t<vox density=\"2\" strength=\"3\" rot=\"" + rot + "\" file=\"" + file + "\"/>");
            writer.WriteLine("\t\t\t<location pos=\"0 3 0\" tags=\"arrow\"/>");
            writer.WriteLine("\t\t</body>");
        }
    }

    private static void writeEscapeCamera(StreamWriter writer)
    {
        EscapeCamera escapeCamera = (EscapeCamera)FindObjectOfType(typeof(EscapeCamera));

        if (escapeCamera != null)
        {
            float x = escapeCamera.gameObject.transform.position.x;
            float y = escapeCamera.gameObject.transform.position.y;
            float z = escapeCamera.gameObject.transform.position.z + 0.1f;
            string coord = (x + " " + y + " " + (-z)).Replace(",", ".");

            string rot = ((-escapeCamera.gameObject.transform.rotation.eulerAngles.x) + " " + (-escapeCamera.gameObject.transform.rotation.eulerAngles.y) + " " + escapeCamera.gameObject.transform.rotation.eulerAngles.z).Replace(",", "."); ;

            writer.WriteLine("\t\t<location tags=\"escapecamera\" pos=\"" + coord + "\" rot=\"" + rot + "\"/>");
        }
    }

    private void exportVoxBox(StreamWriter writer)
    {
        Object[] voxBoxes = FindObjectsOfType(typeof(VoxBox));
        foreach (Object obj in voxBoxes)
        {
            VoxBox voxBox = (VoxBox)obj;

            string tags = "";
            if (voxBox.tags.Length > 0)
            {
                tags = "tags=\"" + voxBox.tags + "\"";
            }

            float x = voxBox.gameObject.transform.position.x;
            float y = voxBox.gameObject.transform.position.y;
            float z = voxBox.gameObject.transform.position.z + 0.1f;

            string rot = ((-voxBox.gameObject.transform.rotation.eulerAngles.x) + " " + (-voxBox.gameObject.transform.rotation.eulerAngles.y) + " " + voxBox.gameObject.transform.rotation.eulerAngles.z).Replace(",", "."); ;
            string coord = (x + " " + y + " " + (-z)).Replace(",", ".");

            string color = (voxBox.color.r + " " + voxBox.color.g + " " + voxBox.color.b).Replace(",", ".");
            string size = ((int)Math.Round(voxBox.gameObject.transform.localScale.x*10) + " " + (int)Math.Round(voxBox.gameObject.transform.localScale.y * 10) + " " + (int)Math.Round(voxBox.gameObject.transform.localScale.z * 10)).Replace(",", ".");

            writer.WriteLine("\t<body><voxbox " + tags + " rot=\"" + rot + "\" pos=\"" + coord + "\" color=\""+color+"\" size=\""+size+"\" prop=\"" + voxBox.dynamic.ToString().ToLower() + "\" collide=\"" + voxBox.collide.ToString().ToLower() + "\" /></body>");
        }
    }

    private void exportLights(StreamWriter writer)
    {
        Object[] lights = FindObjectsOfType(typeof(Light));
        foreach (Object obj in lights)
        {
            Light light = (Light)obj;

            string tags = "";
            if (light.tags.Length > 0)
            {
                tags = "tags=\"" + light.tags + "\"";
            }

            float x = light.gameObject.transform.position.x;
            float y = light.gameObject.transform.position.y;
            float z = light.gameObject.transform.position.z + 0.1f;

            string rot = ((-light.gameObject.transform.rotation.eulerAngles.x) + " " + (-light.gameObject.transform.rotation.eulerAngles.y) + " " + light.gameObject.transform.rotation.eulerAngles.z).Replace(",", "."); ;
            string coord = (x + " " + y + " " + (-z)).Replace(",", ".");


            string color = (light.color.r + " " + light.color.g + " " + light.color.b).Replace(",", ".");

            writer.WriteLine("\t<light " + tags + " rot=\"" + rot + "\" pos=\"" + coord + "\" type=\""+light.lightType+ "\" color=\"" + color + "\" scale=\"" + light.scale.ToString().Replace(",", ".") + "\" glare=\"" + light.glare.ToString().Replace(",", ".") + "\" unshadowed=\"" + light.unshadowed.ToString().Replace(",", ".") + "\" size=\"" + light.size.ToString().Replace(",", ".") + "\" penumbra=\"" + light.penumbra.ToString().Replace(",", ".") + "\" angle=\"" + light.angle.ToString().Replace(",", ".") + "\"/>");       
        }
    }

    private void exportSpawnPoint(StreamWriter writer)
    {
        Object[] spawnPoints = FindObjectsOfType(typeof(SpawnPoint));
        if (spawnPoints.Length == 0) return;

        SpawnPoint spawnPoint = (SpawnPoint) spawnPoints[0];
            
        float x = spawnPoint.gameObject.transform.position.x;
        float y = spawnPoint.gameObject.transform.position.y;
        float z = spawnPoint.gameObject.transform.position.z;

        string coord = (x + " " + y + " " + (-z)).Replace(",", "."); 

        string line = "\t<spawnpoint pos=\"" + coord + "\"/>";
        Debug.Log(line);
        writer.WriteLine(line);                    
    }

    private void exportEnvironment(StreamWriter writer)
    {
        writer.WriteLine("\t<environment template = \"" + environmentTeplate + "\" />");
    }

    private void exportScripts(StreamWriter writer)
    {
        if (useMainLua)
        {
            writer.WriteLine("\t<script file=\"main.lua\"/>");
        }
        if (useHeistLua)
        {
            writer.WriteLine("\t<script file=\"heist.lua\"/>");
        }
    }

    private void exportBoundary(StreamWriter writer)
    {
        writer.WriteLine("\t<boundary>");
        writer.WriteLine("\t\t<vertex pos=\"" + boundaries[0].x + " " + boundaries[0].y + "\"/>");
        writer.WriteLine("\t\t<vertex pos=\"" + boundaries[1].x + " " + boundaries[1].y + "\"/>");
        writer.WriteLine("\t\t<vertex pos=\"" + boundaries[2].x + " " + boundaries[2].y + "\"/>");
        writer.WriteLine("\t\t<vertex pos=\"" + boundaries[3].x + " " + boundaries[3].y + "\"/>");
        writer.WriteLine("\t</boundary>");
    }

    private static void exportBasePlate(StreamWriter writer)
    {
        writer.WriteLine("\t<body>");
        writer.WriteLine("\t\t<voxbox tags=\"unbreakable\" pos=\"-100 -0.5 -100\" size=\"2000 1 2000\"/>");
        writer.WriteLine("\t</body>");
    }

    private void exportJoints(StreamWriter writer)
    {
        Object[] joints = FindObjectsOfType(typeof(Joint));
        foreach (Object obj in joints)
        {
            Joint joint = (Joint)obj;

            float x = joint.gameObject.transform.position.x;
            float y = joint.gameObject.transform.position.y;
            float z = joint.gameObject.transform.position.z + 0.1f;

            string tags = "";
            if (joint.tags.Length > 0)
            {
                tags = "tags=\"" + joint.tags + "\"";
            }

            string rot = ((-joint.gameObject.transform.rotation.eulerAngles.x) + " " + (-joint.gameObject.transform.rotation.eulerAngles.y) + " " + joint.gameObject.transform.rotation.eulerAngles.z).Replace(",", "."); ;
            string coord = (x + " " + y + " " + (-z)).Replace(",", ".");

            string limits = "";
            if (joint.useLimits)
            {
                limits = "limits=\"" + joint.minLimit.ToString().Replace(",", ".") + " " + joint.maxLimit.ToString().Replace(",", ".") + "\"";
            }

            string inner = "";
            if (joint.showJointHelper)
            {
                inner = "\t\t<voxbox pos=\"-0.05 -0.05 -0.05\" color=\"1 0.0 0.0\" size=\"1 1 1\" prop=\"true\" collide=\"false\" />";
            }

            string line = "\t<joint "+tags+" pos=\"" + coord + "\" rot=\"" + rot + "\" type=\"" + joint.jointType + "\" rotstrength=\"" + joint.rotStrength.ToString().Replace(",", ".") + "\" rotspring=\"" + joint.rotSpring.ToString().Replace(",", ".") + "\" sound=\"" + joint.sound.ToString().ToLower() + "\" size=\"" + joint.size.ToString().Replace(",", ".") + "\" " + limits;

            if (inner.Length == 0)
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
    }

    private void exportVehicles(StreamWriter writer)
    {
        Object[] vehicles = FindObjectsOfType(typeof(Vehicle));
        foreach (Object obj in vehicles)
        {
            Vehicle vehicle = (Vehicle)obj;

            GameObject body = null;
            List<GameObject> wheels = new List<GameObject>();

            foreach (Transform child in vehicle.transform)
            {
                if (child.gameObject.name.StartsWith("wheel"))
                {
                    wheels.Add(child.gameObject);
                }
                else if (child.gameObject.name.StartsWith("body"))
                {
                    body = child.gameObject;
                }
            }
            if (body == null)
            {
                Debug.LogError("could not export vehicle " + vehicle.gameObject.name);
            }

            Vector3 vehiclePosition = vehicle.gameObject.transform.position;
            Vector3 centerOfBodyPosition = new Vector3(body.transform.position.x, 0, body.transform.position.z);
            Vector3 bodySize = new Vector3(body.GetComponent<ObjectAttributes>().magicaTotalSize.x, 0, body.GetComponent<ObjectAttributes>().magicaTotalSize.z) / 10f;

            string vehiclePositionString = (vehiclePosition.x + " " + vehiclePosition.y + " " + -vehiclePosition.z).Replace(",", ".");


            writer.WriteLine("\t<vehicle pos=\"" + vehiclePositionString + "\">");
            writer.WriteLine("\t\t<body rot=\"0 0 0\" dynamic=\"true\">");
            writer.WriteLine("\t\t\t<vox rot=\"0 180 0\" density=\"2\" strength=\"3\" file=\"LEVEL/" + vehicle.gameObject.GetComponent<MagicaImportedFile>().voxFile + "\" object=\"body\"></vox>");


            foreach (GameObject wheel in wheels)
            {
                ObjectAttributes wheelAttributes = wheel.GetComponent<ObjectAttributes>();

                float dx = wheelAttributes.magicaTotalSize.x / 20f;
                float dy = -wheelAttributes.magicaTotalSize.y / 20f;
                float dz = wheelAttributes.magicaTotalSize.z % 2 == 0 ? 0 : 0.05f;

                Vector3 wheelPosition = wheel.transform.position - centerOfBodyPosition - bodySize / 2f + new Vector3(0.05f, -dy + dz, dz + wheelAttributes.magicaTotalSize.z / 20f);

                string wheelPositionString = (wheelPosition.x + " " + wheelPosition.y + " " + -wheelPosition.z).Replace(",", ".");

                writer.WriteLine("\t\t\t<wheel name=\"" + wheel.name.Split('_')[1] + "\" pos=\"" + wheelPositionString + "\" steer=\"1\" drive=\"1\" travel=\"-0.1 0.2\">");
                writer.WriteLine("\t\t\t\t<voxbox pos =\"-0.05 -0.05 -0.05\" color=\"1 0.0 0.0\" size=\"1 1 1\" prop=\"true\" collide=\"false\" />");
                writer.WriteLine("\t\t\t\t<vox pos=\"0 " + dy.ToString().Replace(",", ".") + " " + dz.ToString().Replace(",", ".") + "\" rot=\"0 0 0\" file=\"LEVEL/" + vehicle.gameObject.GetComponent<MagicaImportedFile>().voxFile + "\" object=\"" + wheel.name + "\" />");
                writer.WriteLine("\t\t\t</wheel >");
            }

            writer.WriteLine("\t\t</body>");
            writer.WriteLine("\t</vehicle>");

        }
    }

    private void exportWater(StreamWriter writer)
    {

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
    }
}
