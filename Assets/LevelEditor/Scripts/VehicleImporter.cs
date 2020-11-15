using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;

public class VehicleImporter : MonoBehaviour
{
    public string vehicleFilePath
        ;
    [ContextMenu("Import Voxel Vehicle")]
    public void importVoxelVehicle()
    {
        MagicaRenderer renderer = new MagicaRenderer();
        GameObject parent = renderer.ImportMagicaVoxelFile(vehicleFilePath);
        parent.name = parent.name + " - Vehicle";

        GameObject body = null;
        List<GameObject> wheels = new List<GameObject>();
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in parent.transform)
        {
            children.Add(child.gameObject);
            if (child.gameObject.name.StartsWith("wheel_"))
            {
                wheels.Add(child.gameObject);
            }else if (child.gameObject.name == ("body"))
            {
                body = child.gameObject;
            }
        }

        if(body != null)
        {
            parent.AddComponent<Vehicle>();

            Vector3 center = body.transform.position;
            foreach (Transform child in parent.transform)
            {
                child.position -= center; 
            }

            ObjectAttributes bodyAttributes = body.GetComponent<ObjectAttributes>();
            Debug.Log(bodyAttributes.sizeX / 2f);
            Debug.Log(bodyAttributes.sizeZ / 2f);                     
        }        
    }
}
