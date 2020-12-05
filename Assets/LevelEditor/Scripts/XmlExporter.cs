using System;
using System.Xml;
using UnityEngine;

public class XmlExporter
{    
    public void exportToXML(string path, GameObject root)
    {
        XmlDocument xmlDoc = new XmlDocument();

        foreach(Transform child in root.transform)
        {
            exportToXML(xmlDoc, child, null);
        }
        
        xmlDoc.Save(path);
    }

    private void exportToXML(XmlDocument xmlDoc, Transform currentObject, XmlNode parent)
    {
        XmlNode node = null;
        if (currentObject.GetComponent<CommentElement>() != null)
        {
            CommentElement commentElement = (CommentElement)currentObject.GetComponent<CommentElement>();

            node = xmlDoc.CreateComment(commentElement.comment);
        }
        else if (currentObject.GetComponent<Scene>() != null)
        {
            Scene tag = (Scene)currentObject.GetComponent<Scene>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "scene", "");

            enrichXmlWithGeneralAttributes(xmlDoc, tag, node);

            XmlAttribute version = xmlDoc.CreateAttribute("version");
            version.Value = tag.version.ToString().Replace(",", "."); ;
            node.Attributes.Append(version);

            XmlAttribute shadowvolume = xmlDoc.CreateAttribute("shadowVolume");
            shadowvolume.Value = $"{tag.shadowVolume.x} {tag.shadowVolume.y} {tag.shadowVolume.z}".Replace(",", "."); ;
            node.Attributes.Append(shadowvolume);
        }
        else if (currentObject.GetComponent<TeardownEnvironment>() != null)
        {
            TeardownEnvironment tag = (TeardownEnvironment)currentObject.GetComponent<TeardownEnvironment>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "environment", "");
            enrichXmlWithGeneralAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("template");
            attribute.Value = tag.template.ToString().ToLower();
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("skyboxrot");
            attribute.Value = tag.skyboxrot.ToString().Replace(",", "."); ;
            node.Attributes.Append(attribute);

            if (tag.sunDir != null && !$"{tag.sunDir.x} {tag.sunDir.y} {tag.sunDir.z}".Equals("0 0 0"))
            {
                attribute = xmlDoc.CreateAttribute("sunDir");
                attribute.Value = $"{tag.sunDir.x} {tag.sunDir.y} {tag.sunDir.z}".Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.sunBrightness >= 0)
            {
                attribute = xmlDoc.CreateAttribute("sunBrightness");
                attribute.Value = tag.sunBrightness.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.sunFogScale >= 0)
            {
                attribute = xmlDoc.CreateAttribute("sunFogScale");
                attribute.Value = tag.sunFogScale.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            attribute = xmlDoc.CreateAttribute("sunColorTint");
            attribute.Value = $"{tag.sunColorTint.r} {tag.sunColorTint.g} {tag.sunColorTint.b}".Replace(",", ".");
            node.Attributes.Append(attribute);
        }
        else if (currentObject.GetComponent<Body>() != null)
        {
            Body tag = (Body)currentObject.GetComponent<Body>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "body", "");
            enrichXmlWithGameObjectAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("dynamic");
            attribute.Value = tag.dynamic ? "true" : "false";
            node.Attributes.Append(attribute);
        }
        else if (currentObject.GetComponent<Boundary>() != null)
        {
            Boundary tag = (Boundary)currentObject.GetComponent<Boundary>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "boundary", "");
            enrichXmlWithGeneralAttributes(xmlDoc, tag, node);
        }
        else if (currentObject.GetComponent<Vertex>() != null)
        {
            Vertex tag = (Vertex)currentObject.GetComponent<Vertex>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "vertex", "");
            enrichXmlWithGeneralAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("pos");
            attribute.Value = $"{tag.pos.x} {tag.pos.y}".Replace(",", "."); ;
            node.Attributes.Append(attribute);
        }
        else if (currentObject.GetComponent<Script>() != null)
        {
            Script tag = (Script)currentObject.GetComponent<Script>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "script", "");
            enrichXmlWithGeneralAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("file");
            attribute.Value = tag.file.Replace(XmlImporter.getLevelFolder(tag.file),"LEVEL");
            node.Attributes.Append(attribute);
        }
        else if (currentObject.GetComponent<Instance>() != null)
        {
            Instance tag = (Instance)currentObject.GetComponent<Instance>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "instance", "");
            enrichXmlWithGeneralAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("file");
            attribute.Value = tag.file.Replace(XmlImporter.getLevelFolder(tag.file), "LEVEL");
            node.Attributes.Append(attribute);
        }
        else if (currentObject.GetComponent<SpawnPoint>() != null)
        {
            SpawnPoint tag = (SpawnPoint)currentObject.GetComponent<SpawnPoint>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "spawnpoint", "");
            enrichXmlWithTransformAttributes(xmlDoc, tag, node);
        }
        else if (currentObject.GetComponent<Location>() != null)
        {
            Location tag = (Location)currentObject.GetComponent<Location>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "location", "");
            enrichXmlWithTransformAttributes(xmlDoc, tag, node);
        }
        else if (currentObject.GetComponent<Group>() != null)
        {
            Group tag = (Group)currentObject.GetComponent<Group>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "group", "");
            enrichXmlWithTransformAttributes(xmlDoc, tag, node);
        }
        else if (currentObject.GetComponent<Water>() != null)
        {
            Water tag = (Water)currentObject.GetComponent<Water>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "water", "");
            enrichXmlWithTransformAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("size");
            attribute.Value = $"{tag.size.x} {tag.size.y}".Replace(",", ".");
            node.Attributes.Append(attribute);

            if (tag.wave > -1)
            {
                attribute = xmlDoc.CreateAttribute("wave");
                attribute.Value = tag.wave.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.ripple > -1)
            {
                attribute = xmlDoc.CreateAttribute("ripple");
                attribute.Value = tag.ripple.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.foam > -1)
            {
                attribute = xmlDoc.CreateAttribute("foam");
                attribute.Value = tag.foam.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.motion > -1)
            {
                attribute = xmlDoc.CreateAttribute("motion");
                attribute.Value = tag.motion.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (!String.IsNullOrEmpty(tag.type))
            {
                attribute = xmlDoc.CreateAttribute("type");
                attribute.Value = tag.type.ToString().ToLower();
                node.Attributes.Append(attribute);
            }
        }
        else if (currentObject.GetComponent<Screen>() != null)
        {
            Screen tag = (Screen)currentObject.GetComponent<Screen>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "water", "");
            enrichXmlWithTransformAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("size");
            attribute.Value = $"{tag.size.x} {tag.size.y}".Replace(",", ".");
            node.Attributes.Append(attribute);

            if (!tag.resolution.Equals(Vector2.zero))
            {
                attribute = xmlDoc.CreateAttribute("resolution");
                attribute.Value = $"{tag.resolution.x} {tag.resolution.y}".Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (!tag.bulge.Equals(Vector2.zero))
            {
                attribute = xmlDoc.CreateAttribute("bulge");
                attribute.Value = $"{tag.bulge.x} {tag.bulge.y}".Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (!String.IsNullOrEmpty(tag.script))
            {
                attribute = xmlDoc.CreateAttribute("script");
                attribute.Value = tag.script;
                node.Attributes.Append(attribute);
            }

            attribute = xmlDoc.CreateAttribute("color");
            attribute.Value = $"{tag.color.r} {tag.color.g} {tag.color.b}".Replace(",", ".");
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("enabled");
            attribute.Value = tag.enabled ? "true" : "false";
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("interactive");
            attribute.Value = tag.interactive ? "true" : "false";
            node.Attributes.Append(attribute);

            if (tag.emissive > -1)
            {
                attribute = xmlDoc.CreateAttribute("resolution");
                attribute.Value = tag.emissive.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }
        }
        else if (currentObject.GetComponent<Light>() != null)
        {
            Light tag = (Light)currentObject.GetComponent<Light>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "light", "");
            enrichXmlWithTransformAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("type");
            attribute.Value = tag.type.ToString().ToLower();
            node.Attributes.Append(attribute);

            if (tag.glare > -1)
            {
                attribute = xmlDoc.CreateAttribute("glare");
                attribute.Value = tag.glare.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            attribute = xmlDoc.CreateAttribute("angle");
            attribute.Value = tag.angle.ToString().Replace(",", ".");
            node.Attributes.Append(attribute);

            if (tag.penumbra > -1)
            {
                attribute = xmlDoc.CreateAttribute("penumbra");
                attribute.Value = tag.penumbra.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.unshadowed > -1)
            {
                attribute = xmlDoc.CreateAttribute("unshadowed");
                attribute.Value = tag.unshadowed.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            attribute = xmlDoc.CreateAttribute("color");
            attribute.Value = $"{tag.color.r} {tag.color.g} {tag.color.b}".Replace(",", ".");
            node.Attributes.Append(attribute);
        }
        else if (currentObject.GetComponent<Rope>() != null)
        {
            Rope tag = (Rope)currentObject.GetComponent<Rope>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "rope", "");
            enrichXmlWithTransformAttributes(xmlDoc, tag, node);

            if (tag.slack > -1)
            {
                XmlAttribute attribute = xmlDoc.CreateAttribute("friction");
                attribute.Value = tag.slack.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.strength > -1)
            {
                XmlAttribute attribute = xmlDoc.CreateAttribute("steerassist");
                attribute.Value = tag.strength.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }
        }
        else if (currentObject.GetComponent<Wheel>() != null)
        {
            Wheel tag = (Wheel)currentObject.GetComponent<Wheel>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "wheel", "");
            enrichXmlWithTransformAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("drive");
            attribute.Value = tag.drive.ToString().Replace(",", ".");
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("steer");
            attribute.Value = tag.steer.ToString().Replace(",", ".");
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("travel");
            attribute.Value = $"{tag.travel.x} {tag.travel.y}".Replace(",", ".");
            node.Attributes.Append(attribute);
        }
        else if (currentObject.GetComponent<Vehicle>() != null)
        {
            Vehicle tag = (Vehicle)currentObject.GetComponent<Vehicle>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "vehicle", "");
            enrichXmlWithGameObjectAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("driven");
            attribute.Value = tag.driven ? "true" : "false";
            node.Attributes.Append(attribute);

            if (tag.friction > -1)
            {
                attribute = xmlDoc.CreateAttribute("friction");
                attribute.Value = tag.friction.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.steerassist > -1)
            {
                attribute = xmlDoc.CreateAttribute("steerassist");
                attribute.Value = tag.steerassist.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.difflock > -1)
            {
                attribute = xmlDoc.CreateAttribute("difflock");
                attribute.Value = tag.difflock.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.antiroll > -1)
            {
                attribute = xmlDoc.CreateAttribute("antiroll");
                attribute.Value = tag.antiroll.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.antispin > -1)
            {
                attribute = xmlDoc.CreateAttribute("antispin");
                attribute.Value = tag.antispin.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }


            if (tag.acceleration > -1)
            {
                attribute = xmlDoc.CreateAttribute("acceleration");
                attribute.Value = tag.acceleration.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.spring > -1)
            {
                attribute = xmlDoc.CreateAttribute("spring");
                attribute.Value = tag.spring.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.damping > -1)
            {
                attribute = xmlDoc.CreateAttribute("damping");
                attribute.Value = tag.damping.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            if (tag.topspeed > -1)
            {
                attribute = xmlDoc.CreateAttribute("topspeed");
                attribute.Value = tag.topspeed.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            attribute = xmlDoc.CreateAttribute("sound");
            attribute.Value = tag.sound;
            node.Attributes.Append(attribute);
        }
        else if (currentObject.GetComponent<Vox>() != null)
        {
            Vox tag = (Vox)currentObject.GetComponent<Vox>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "vox", "");
            enrichXmlWithGameObjectAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("file");
            attribute.Value = tag.file.Replace(XmlImporter.getLevelFolder(tag.file), "LEVEL");
            node.Attributes.Append(attribute);

            if (!String.IsNullOrEmpty(tag.voxObject))
            {
                attribute = xmlDoc.CreateAttribute("object");
                attribute.Value = tag.voxObject;
                node.Attributes.Append(attribute);
            }


            attribute = xmlDoc.CreateAttribute("prop");
            attribute.Value = tag.dynamic ? "true" : "false";
            node.Attributes.Append(attribute);
        }
        else if (currentObject.GetComponent<VoxBox>() != null)
        {
            VoxBox tag = (VoxBox)currentObject.GetComponent<VoxBox>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "voxbox", "");
            enrichXmlWithGameObjectAttributes(xmlDoc, tag, node);
            Debug.Log(tag.position);

            XmlAttribute attribute = xmlDoc.CreateAttribute("color");
            attribute.Value = $"{tag.color.r} {tag.color.g} {tag.color.b}".Replace(",", ".");
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("prop");
            attribute.Value = tag.dynamic ? "true" : "false";
            node.Attributes.Append(attribute);
        }
        else if (currentObject.GetComponent<TeardownJoint>() != null)
        {
            TeardownJoint tag = (TeardownJoint)currentObject.GetComponent<TeardownJoint>();
            node = xmlDoc.CreateNode(XmlNodeType.Element, "joint", "");
            enrichXmlWithTransformAttributes(xmlDoc, tag, node);

            XmlAttribute attribute = xmlDoc.CreateAttribute("type");
            attribute.Value = tag.type.ToString().ToLower();
            node.Attributes.Append(attribute);

            if (tag.limits != null && !$"{tag.limits.x} {tag.limits.y}".Replace(",", ".").Equals("0 0"))
            {
                attribute = xmlDoc.CreateAttribute("limits");
                attribute.Value = $"{tag.limits.x} {tag.limits.y}".Replace(",", ".");
                node.Attributes.Append(attribute);
            }

            attribute = xmlDoc.CreateAttribute("size");
            attribute.Value = tag.size.ToString().Replace(",", ".");
            node.Attributes.Append(attribute);

            if (tag.rotstrength >= 0)
            {
                attribute = xmlDoc.CreateAttribute("rotstrength");
                attribute.Value = tag.rotstrength.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }
            if (tag.rotspring >= 0)
            {
                attribute = xmlDoc.CreateAttribute("rotspring");
                attribute.Value = tag.rotspring.ToString().Replace(",", ".");
                node.Attributes.Append(attribute);
            }
        }

        if (node != null)
        {
            foreach (Transform child in currentObject)
            {
                exportToXML(xmlDoc, child, node);
            }

            if (parent == null)
            {
                xmlDoc.AppendChild(node);
            }
            else {
                parent.AppendChild(node);
            }
        }
    }

    private static void enrichXmlWithGameObjectAttributes(XmlDocument xmlDoc, GameObjectTag tag, XmlNode node)
    {        
        enrichXmlWithTransformAttributes(xmlDoc, tag, node);

        if (tag.strength >= 0)
        {
            XmlAttribute attribute = xmlDoc.CreateAttribute("strength");
            attribute.Value = tag.strength.ToString().Replace(",", "."); ;
            node.Attributes.Append(attribute);
        }

        if (tag.density >= 0)
        {
            XmlAttribute attribute = xmlDoc.CreateAttribute("density");
            attribute.Value = tag.density.ToString().Replace(",", "."); ;
            node.Attributes.Append(attribute);
        }

        if (tag.size != null && !$"{tag.size.x} {tag.size.y} {tag.size.z}".Equals("0 0 0"))
        {
            XmlAttribute attribute = xmlDoc.CreateAttribute("size");
            attribute.Value = $"{tag.size.x} {tag.size.y} {tag.size.z}".Replace(",", "."); ;
            node.Attributes.Append(attribute);
        }

        if(!tag.collide)
        {
            XmlAttribute attribute = xmlDoc.CreateAttribute("collide");
            attribute.Value = "false";
            node.Attributes.Append(attribute);
        }
    }

    private static void enrichXmlWithTransformAttributes(XmlDocument xmlDoc, TransformTag tag, XmlNode node)
    {
        enrichXmlWithGeneralAttributes(xmlDoc, tag, node);
        if (tag.position != null && !$"{tag.position.x} {tag.position.y} {tag.position.z}".Equals("0 0 0"))
        {
            XmlAttribute attribute = xmlDoc.CreateAttribute("pos");
            attribute.Value = $"{tag.position.x} {tag.position.y} {-tag.position.z}".Replace(",", ".");
            node.Attributes.Append(attribute);
        }

        Vector3 rotation = tag.gameObject.transform.localEulerAngles;
        if (Math.Abs(rotation.x) < 0.001) rotation.x = 0;
        if (Math.Abs(rotation.y) < 0.001) rotation.y = 0;
        if (Math.Abs(rotation.z) < 0.001) rotation.z = 0;
        if (rotation != null && !rotation.Equals(Vector3.zero))
        {
            XmlAttribute attribute = xmlDoc.CreateAttribute("rot");
            attribute.Value = $"{-rotation.x} {-rotation.y} {rotation.z}".Replace(",", "."); ;
            node.Attributes.Append(attribute);
        }
    }

    private static void enrichXmlWithGeneralAttributes(XmlDocument xmlDoc, GeneralTag tag, XmlNode node)
    {
        if (!String.IsNullOrEmpty(tag.teardownName))
        {
            XmlAttribute attribute = xmlDoc.CreateAttribute("name");
            attribute.Value = tag.teardownName;
            node.Attributes.Append(attribute);
        }

        if (!String.IsNullOrEmpty(tag.tags))
        {
            XmlAttribute attribute = xmlDoc.CreateAttribute("tags");
            attribute.Value = tag.tags;
            node.Attributes.Append(attribute);
        }
    }
}
