using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joint : MonoBehaviour
{
    public enum JointType
    {
        ball,
        hinge,
        prismatic
    }

    public JointType jointType = JointType.ball;
    public float size = 0.1f;
    public float rotStrength = 0;
    public float rotSpring = 0;
    public bool sound = true;
    public bool useLimits = false;
    public float minLimit = 0;
    public float maxLimit = 0;
    public bool showJointHelper = false;
}
