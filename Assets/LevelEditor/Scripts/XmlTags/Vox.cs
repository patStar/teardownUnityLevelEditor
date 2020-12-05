using System.IO.Pipes;
using UnityEngine;

[SelectionBase]
public class Vox : GameObjectTag
{
    public bool dynamic = false;
    public string file = "";
    public string voxObject = "";
    private string[] objectNames;

    public void setObjectNames(string[] names)
    {
        objectNames = names;
    }

    private void Start()
    {
        gameObject.name = "<vox " + teardownName + " " + file + " " + this.voxObject + ">";
    }

    public void Reload()
    {
        MagicaRenderer renderer = new MagicaRenderer();
        if (voxObject.Equals(""))
        {
            gameObject.name = "<vox " + teardownName + " " + file + ">";
            GameObject importedObject = renderer.ImportMagicaVoxelFile(file);
            Transform voxObject = importedObject.transform;
            voxObject.parent = transform;
            voxObject.transform.localPosition = Vector3.zero;
            voxObject.transform.localRotation = Quaternion.identity;
        }
        else
        {
            gameObject.name  = "<vox " + teardownName + " " + file + " " + this.voxObject + ">";
            GameObject importedObject = renderer.ImportMagicaVoxelFileObject(file, this.voxObject);
            Transform voxObject = importedObject.transform;
            if (importedObject.transform.childCount > 0)
            {
                voxObject = importedObject.transform.GetChild(0);
            }
            voxObject.transform.parent = transform;
            voxObject.transform.localPosition = Vector3.zero;
            voxObject.transform.localRotation = Quaternion.identity;
            DestroyImmediate(importedObject);
        }

        setObjectNames(renderer.GetObjectNames(file));
    }

}
