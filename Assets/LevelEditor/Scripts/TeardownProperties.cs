using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeardownProperties : MonoBehaviour
{
    public bool dynamic = false;
    private bool valid = false;

    internal void setValid()
    {
        valid = true;
    }

    public bool isValid()
    {
        return valid;
    }
}

