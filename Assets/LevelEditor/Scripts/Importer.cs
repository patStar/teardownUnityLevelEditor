using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Importer : MonoBehaviour
{
    [Header("Import setup")]
    [Tooltip("Import a vox file from here")]
    public string importPath;

    public void Import(bool importOnlyValidAssets)
    {
        try
        {
            MagicaRenderer renderer = new MagicaRenderer();
            if (importOnlyValidAssets)
            {
                GameObject root = renderer.ImportMagicaVoxelFileAssets(importPath);
                DestroyImmediate(root);
            }
            else
            {
                renderer.ImportMagicaVoxelFile(importPath);
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }
}
