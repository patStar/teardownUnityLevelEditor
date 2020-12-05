using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Xml;
using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class XmlImporter : MonoBehaviour
{
    public string fileName;    

    [ContextMenu("Import")]
    void import()
    {
        readXML(fileName);
    }

    public class XmlExportable : MonoBehaviour
    {
        public string path = "";

        [ContextMenu("Export To Teardown XML")]
        public void export()
        {
            XmlExporter xmlExporter = new XmlExporter();
            string savePath = EditorUtility.SaveFilePanel(
                "Save Teardown scene to XML",
                Directory.GetParent(path).FullName,
                Path.GetFileName(path),
            "xml");
            xmlExporter.exportToXML(savePath, gameObject);
        }
    }

    GameObject readXML(string path)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);
        GameObject go = new GameObject(path);
        XmlExportable xmlExportable = go.AddComponent<XmlExportable>();
        xmlExportable.path = path;
        foreach (XmlNode xmlNode in xmlDoc.ChildNodes)
        {
            readXmlNode(xmlNode, go, path);
        }
        return go;
    }

    float readFloat(XmlNode xmlNode, string attrName)
    {
        return xmlNode.Attributes[attrName] != null ? float.Parse(xmlNode.Attributes[attrName].Value.Replace(".", ",")) : -1f;
    }

    Vector3 readVec3(XmlNode xmlNode, string attrName)
    {
        Vector3 result = xmlNode.Attributes[attrName] != null ? parsePosition(xmlNode.Attributes[attrName].Value) : Vector3.zero;
        if (attrName.Equals("pos")) result.z = -result.z;
        if (attrName.Equals("rot")) result.y = -result.y;
        return result;
    }

    Vector2 readVec2(XmlNode xmlNode, string attrName)
    {
        return xmlNode.Attributes[attrName] != null ? parseVec2(xmlNode.Attributes[attrName].Value) : Vector2.zero;
    }

    bool readBool(XmlNode xmlNode, string attrName, bool defaultValue)
    {
        return xmlNode.Attributes[attrName] != null ? Boolean.Parse(xmlNode.Attributes[attrName].Value) : defaultValue;
    }

    string readString(XmlNode xmlNode, string attrName)
    {
        return xmlNode.Attributes[attrName] != null ? xmlNode.Attributes[attrName].Value : "";
    }

    string readString(XmlNode xmlNode, string attrName, string defaultValue)
    {
        return xmlNode.Attributes[attrName] != null ? xmlNode.Attributes[attrName].Value : defaultValue;
    }

    int readInt(XmlNode xmlNode, string attrName)
    {
        return xmlNode.Attributes[attrName] != null ? int.Parse(xmlNode.Attributes[attrName].Value) : 0;
    }

    void attachTransformProperties(TransformTag tag, XmlNode xmlNode)
    {
        attachGeneralProperties(tag, xmlNode);
        tag.position = readVec3(xmlNode, "pos");
        tag.rotation = readVec3(xmlNode, "rot");
    }

    void attachGeneralProperties(GeneralTag tag, XmlNode xmlNode)
    {
        tag.tags = readString(xmlNode, "tags");
        tag.teardownName = readString(xmlNode, "name");
    }

    void attachGameObjectProperties(GameObjectTag tag, XmlNode xmlNode)
    {
        attachTransformProperties(tag, xmlNode);
        tag.collide = readBool(xmlNode, "collide", true);
        tag.size = readVec3(xmlNode, "size");
        tag.density = readFloat(xmlNode, "density");
        tag.strength = readFloat(xmlNode, "strength");
    }

    Color readColor(XmlNode xmlNode, string attrName)
    {
        return xmlNode.Attributes[attrName] != null ? parseColor(xmlNode.Attributes[attrName].Value) : new Color(0.1f, 0.1f, 0.1f, 1f);
    }

    void addToParent(GameObject parent, GeneralTag tag)
    {
        if(parent.GetComponent<GeneralTag>() != null){
            parent.GetComponent<GeneralTag>().children.Add(tag);
        }
    }

    GameObject readXmlNode(XmlNode xmlNode, GameObject parent, string fileName)
    {
        GameObject go = new GameObject(xmlNode.Name);
        if (parent != null)
        {
            go.transform.parent = parent.transform;
        }

        if (xmlNode.NodeType == XmlNodeType.Comment)
        {
            CommentElement comment = go.AddComponent<CommentElement>();
            go.transform.parent = parent.transform;

            comment.comment = xmlNode.InnerText;
        }
        else if (xmlNode.Name == "vehicle")
        {
            Vehicle tag = go.AddComponent<Vehicle>();
            go.transform.parent = parent.transform;

            attachGameObjectProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            tag.sound = readString(xmlNode, "sound");
            tag.spring = readFloat(xmlNode, "spring");
            tag.acceleration = readFloat(xmlNode, "acceleration");
            tag.antiroll = readFloat(xmlNode, "antiroll");
            tag.damping = readFloat(xmlNode, "damping");
            tag.friction = readFloat(xmlNode, "friction");
            tag.difflock = readFloat(xmlNode, "difflock");
            tag.steerassist = readFloat(xmlNode, "steerassist");
            tag.topspeed = readFloat(xmlNode, "topspeed");
            tag.driven = readBool(xmlNode, "driven", false);
            tag.antispin = readFloat(xmlNode, "antispin");
            tag.soundVolume = readFloat(xmlNode, "soundvolume");

            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "wheel")
        {
            Wheel tag = go.AddComponent<Wheel>();
            go.transform.parent = parent.transform;

            attachTransformProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            tag.steer = readFloat(xmlNode, "steer");
            tag.drive = readFloat(xmlNode, "drive");
            tag.travel = readVec2(xmlNode, "travel");
            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "spawnpoint")
        {
            SpawnPoint tag = go.AddComponent<SpawnPoint>();
            go.transform.parent = parent.transform;

            attachTransformProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "location")
        {
            Location tag = go.AddComponent<Location>();
            go.transform.parent = parent.transform;

            attachTransformProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "rope")
        {
            Rope tag = go.AddComponent<Rope>();
            go.transform.parent = parent.transform;

            attachTransformProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            tag.strength = readFloat(xmlNode, "strength");
            tag.slack = readFloat(xmlNode, "slack");

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "screen")
        {
            Screen tag = go.AddComponent<Screen>();
            go.transform.parent = parent.transform;

            attachTransformProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            tag.size = readVec2(xmlNode, "size");
            tag.isEnabled = readBool(xmlNode, "enabled", false);
            tag.interactive = readBool(xmlNode, "interactive", false);
            tag.emissive = readFloat(xmlNode, "emissive");
            tag.color = readColor(xmlNode, "color");
            tag.resolution = readVec2(xmlNode, "resolution");
            tag.bulge = readVec2(xmlNode, "bulge");
            tag.script = readString(xmlNode, "script");

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "vox")
        {
            Vox tag = go.AddComponent<Vox>();
            go.transform.parent = parent.transform;

            attachGameObjectProperties(tag, xmlNode);
            tag.file = readString(xmlNode, "file");
            tag.voxObject = readString(xmlNode, "object");
            tag.dynamic = readBool(xmlNode, "prop", false);

            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            MagicaRenderer renderer = new MagicaRenderer();
            if (tag.voxObject.Equals(""))
            {
                go.name = "<" + xmlNode.Name + " " + tag.teardownName + " " + tag.file + ">";
                GameObject importedObject = renderer.ImportMagicaVoxelFile(tag.file.Replace("LEVEL", getLevelFolder(fileName)));
                Transform voxObject = importedObject.transform;
                voxObject.parent = go.transform;
                voxObject.transform.localPosition = Vector3.zero;
                voxObject.transform.localRotation = Quaternion.identity;
            }
            else
            {
                go.name = "<" + xmlNode.Name + " " + tag.teardownName + " " + tag.file + " " + tag.voxObject + ">";
                GameObject importedObject = renderer.ImportMagicaVoxelFileObject(tag.file.Replace("LEVEL", getLevelFolder(fileName)), tag.voxObject);
                Transform voxObject = importedObject.transform;
                if (importedObject.transform.childCount > 0)
                {
                    voxObject = importedObject.transform.GetChild(0);
                }
                voxObject.transform.parent = go.transform;
                voxObject.transform.localPosition = Vector3.zero;
                voxObject.transform.localRotation = Quaternion.identity;
                DestroyImmediate(importedObject);
            }

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "joint")
        {
            TeardownJoint tag = go.AddComponent<TeardownJoint>();
            go.transform.parent = parent.transform;

            attachTransformProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            tag.type = (TeardownJoint.Type)Enum.Parse(typeof(TeardownJoint.Type), readString(xmlNode, "type", "ball"), false);
            tag.size = readFloat(xmlNode, "size");
            tag.rotspring = readFloat(xmlNode, "rotspring");
            tag.rotstrength = readFloat(xmlNode, "rotstrength");
            tag.limits = readVec2(xmlNode, "limits");

            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "voxbox")
        {
            VoxBox tag = go.AddComponent<VoxBox>();
            go.transform.parent = parent.transform;

            attachGameObjectProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            tag.color = readColor(xmlNode, "color");            
            tag.dynamic = readBool(xmlNode, "prop", false);
            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = go.transform;
            cube.transform.localPosition = new Vector3(0.5f, 0.5f, -0.5f);
            cube.transform.localRotation = Quaternion.identity;
            cube.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
            go.transform.localScale = tag.size/10;
            

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "body")
        {
            Body tag = go.AddComponent<Body>();
            go.transform.parent = parent.transform;

            attachGameObjectProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            tag.dynamic = readBool(xmlNode, "dynamic", false);
            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "water")
        {
            go.name = "<" + xmlNode.Name + ">";
            Water tag = go.AddComponent<Water>();
            go.transform.parent = parent.transform;

            attachTransformProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            addToParent(parent, tag);

        }
        else if (xmlNode.Name == "environment")
        {
            TeardownEnvironment tag = go.AddComponent<TeardownEnvironment>();
            go.transform.parent = parent.transform;

            attachGeneralProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            tag.skyboxrot = readFloat(xmlNode, "skyboxrot");
            tag.sunBrightness = readFloat(xmlNode, "sunBrightness");
            tag.sunFogScale = readFloat(xmlNode, "sunFogScale");
            tag.sunDir = readVec3(xmlNode, "sunDir");
            tag.sunColorTint = readColor(xmlNode, "sunColorTint");

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "scene")
        {
            Scene tag = go.AddComponent<Scene>();
            go.transform.parent = parent.transform;
            attachGeneralProperties(tag, xmlNode);

            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            tag.shadowVolume = readVec3(xmlNode, "shadowVolume");
            tag.version = readInt(xmlNode, "version");

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "boundary")
        {
            Boundary tag = go.AddComponent<Boundary>();
            attachGeneralProperties(tag, xmlNode);
            go.transform.parent = parent.transform;
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "script")
        {
            Script tag = go.AddComponent<Script>();
            attachGeneralProperties(tag, xmlNode);
            go.transform.parent = parent.transform;

            tag.file = readString(xmlNode, "file");
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + " " + tag.file + ">";

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "instance")
        {
            Instance tag = go.AddComponent<Instance>();
            go.transform.parent = parent.transform;
            attachGeneralProperties(tag, xmlNode);

            tag.file = readString(xmlNode, "file");
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + " " + tag.file + ">";

            if (!fileName.Contains("create") && tag.file.StartsWith("LEVEL"))
            {
                Debug.LogError("Your source file has no create folder in its path but an instance is referencing the level folder. Cannot import!");
            }
            else if (!tag.file.StartsWith("LEVEL"))
            {
                GameObject xmlRoot = readXML(tag.file);
                xmlRoot.GetComponentInChildren<Prefab>().gameObject.transform.transform.parent = go.transform;
                DestroyImmediate(xmlRoot);
            }
            else
            {
                GameObject xmlRoot = readXML(tag.file.Replace("LEVEL", getLevelFolder(fileName)));
                xmlRoot.GetComponentInChildren<Prefab>().gameObject.transform.transform.parent = go.transform;
                DestroyImmediate(xmlRoot);
            }
            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            addToParent(parent, tag);

        }
        else if (xmlNode.Name == "prefab")
        {
            Prefab tag = go.AddComponent<Prefab>();
            go.transform.parent = parent.transform;

            attachGameObjectProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "vertex")
        {
            Vertex tag = go.AddComponent<Vertex>();
            go.transform.parent = parent.transform;
            attachGeneralProperties(tag, xmlNode);
            tag.pos = readVec2(xmlNode, "pos");

            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";
            go.transform.localPosition = new Vector3(tag.pos.x, 0 ,tag.pos.y);

            addToParent(parent, tag);
        }
        else if (xmlNode.Name == "light")
        {
            Light tag = go.AddComponent<Light>();
            go.transform.parent = parent.transform;

            attachGeneralProperties(tag, xmlNode);
            attachTransformProperties(tag, xmlNode);
            go.name = "<" + xmlNode.Name + " " + tag.teardownName + ">";

            tag.penumbra = readFloat(xmlNode, "penumbra");
            tag.unshadowed = readFloat(xmlNode, "unshadowed");
            tag.angle = readFloat(xmlNode, "angle");
            tag.glare = readFloat(xmlNode, "glare");
            tag.color = readColor(xmlNode, "color");
            tag.type = (Light.Type)Enum.Parse(typeof(Light.Type), readString(xmlNode, "type", "area"), false);

            go.transform.localRotation = Quaternion.Euler(tag.rotation);
            go.transform.localPosition = tag.position;

            addToParent(parent, tag);
        }

        foreach (XmlNode child in xmlNode.ChildNodes)
        {
            GameObject childGameObject = readXmlNode(child, go, fileName);            
        }

        return go;
    }

    private static string getLevelFolder(string fileName)
    {
        string[] folders = Directory.GetParent(fileName).FullName.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.None);
        List<string> levelFolderParts = new List<string>();
        bool foundCreate = false;
        bool hasLevelFolder = false;
        foreach (string p in folders)
        {
            levelFolderParts.Add(p);
            if (foundCreate)
            {
                hasLevelFolder = true;
                break;
            }
            if (p.Equals("create")) { foundCreate = true; }
        }

        string levelFolder = String.Join(Path.DirectorySeparatorChar + "", levelFolderParts.ToArray());
        if (!hasLevelFolder)
        {
            levelFolder = Path.Combine(levelFolder, Path.GetFileNameWithoutExtension(fileName));
        }

        return levelFolder;
    }

    Color parseColor(string data)
    {        
        string[] colorData = data.Replace(".",",").Split(' ');
        return new Color(float.Parse("0"+colorData[0]), float.Parse("0" + colorData[1]), float.Parse("0" + colorData[2]), 1f);
    }

    Vector3 parsePosition(string pos)
    {
        if (pos == null) return Vector3.zero;
        string[] posData = pos.Replace(".", ",").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return new Vector3(float.Parse(posData[0]), float.Parse(posData[1]), float.Parse(posData[2]));
    }

    Vector2 parseVec2(string pos)
    {
        if (pos == null) return Vector2.zero;
        string[] posData = pos.Replace(".", ",").Split(' ');
        return new Vector2(float.Parse(posData[0]), float.Parse(posData[1]));
    }
}
