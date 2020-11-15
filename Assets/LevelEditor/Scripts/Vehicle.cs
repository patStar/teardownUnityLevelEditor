using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public string vehicleName;
    public bool driven = false;
    public string sound = "";
    public float soundVolume = 1;
    public float spring = 1;
    public float damping = 1.5f;
    public float topSpeed= 90;
    public float acceleration = 6;
    public float strength = 4;
    public float antiroll = 0.2f;
    public float difflock = 0.2f;
    public float steerassist = 0.4f;
    public float friction = 1.8f;    
}
