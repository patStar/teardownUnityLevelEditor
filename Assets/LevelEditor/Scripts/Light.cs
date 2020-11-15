using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light : MonoBehaviour
{
    public enum LightType
    {
        cone,
        area
    };

    public LightType lightType = LightType.area;
    public Color color = Color.blue;
    public float scale = 20;
    public float angle = 90;
    public float penumbra = 30;
    public float size = 0.1f;
    public float unshadowed = 0.3f;
    public float glare = 0.3f;  
}
