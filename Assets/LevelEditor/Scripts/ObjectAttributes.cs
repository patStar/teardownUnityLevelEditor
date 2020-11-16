using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class ObjectAttributes : MonoBehaviour
{
    public string parentVoxFile; 
    public List<string> names = new List<string>();

    public Vector3 magicaTotalSize = Vector3.zero;
    
    public Vector3 centerOfMagicaMass = Vector3.zero;
    public Vector3 bottomCenterOfVoxelMass = Vector3.zero;
    public List<Vector3> magicaTransitions = new List<Vector3>();

    public List<string> rotations = new List<string>();
    internal List<Vector3[]> rotationMatrices = new List<Vector3[]>();
}
