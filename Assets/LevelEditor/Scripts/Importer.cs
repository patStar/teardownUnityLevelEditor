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

    [ContextMenu("Import Magica File")]
    public void Import()
    {
        MagicaRenderer renderer = new MagicaRenderer();
        renderer.ImportMagicaVoxelFile(importPath);
    }
}
