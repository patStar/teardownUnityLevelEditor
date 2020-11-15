using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAttributes : MonoBehaviour
{
    public List<string> names = new List<string>();
    public List<Vector3> shifts = new List<Vector3>();
    public List<string> rotations = new List<string>();
    public int sizeX;
    public int sizeY;
    public int sizeZ;
    public Vector3 trans = Vector3.zero;
    public Vector3 aRot;
    public Vector3 bRot;
    public Vector3 cRot;
    public string parentVoxFile;
    public Vector3 singleCenter;
}
