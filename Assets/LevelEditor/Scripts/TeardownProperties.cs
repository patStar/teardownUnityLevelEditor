﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeardownProperties : MonoBehaviour
{
    public bool dynamic = false;
    private bool exportable = true;
    public string tags = "";     

    public enum TeardownTextures
    {
        No_Texture,
        Texture_0,
        Texture_1,
        Texture_2,
        Texture_3,
        Texture_4,
        Texture_5,
        Texture_6,
        Texture_7,
        Texture_8,
        Texture_9,
        Texture_10,
        Texture_11,
        Texture_12,
        Texture_13,
        Texture_14,

    };

    public TeardownTextures teardownTexture = TeardownTextures.No_Texture;
    [Min(0f)]
    public float textureIntensity = 1.0f;

    public int valueableValue = 0;    
    
    public bool isTarget = false;
    public bool isOptionalTarget = false;
    public bool isDestroyTarget = false;
    public bool isHiddenTarget = false;
    public string targetName = "";
    public string targetDetail = "";
    public string targetDescription = "";
    public string targetImage = "";
    public float explosionStrength = 0;

    internal void preventExport()
    {
        exportable = false;
    }

    public bool isExportable()
    {
        return exportable;
    }
}

