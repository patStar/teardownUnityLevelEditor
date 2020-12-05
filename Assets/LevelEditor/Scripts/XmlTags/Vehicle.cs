using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : GameObjectTag
{
    public string vehicleName;
    public bool driven = false;
    public string sound = "";
    public float soundVolume = 1;
    public float spring;
    public float damping;
    public float topspeed;
    public float acceleration;
    public float antiroll;
    public float antispin;
    public float difflock;
    public float steerassist;
    public float friction;
}
