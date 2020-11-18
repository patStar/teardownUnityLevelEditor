using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public enum TargetType
    {
        standard,
        heavy,
        destroy
    }
    
    public bool isOptionalTarget = false;
    public bool isHiddenTarget = false;

    public TargetType targetType = TargetType.standard;
    
    public string targetName = "";
    public string nameInTargetList = "";
    public string targetDescription = "";

    public bool useLevelFolderForImage = true;
    public string targetImage = "";

    public bool startsAnAlarm = false;

}
