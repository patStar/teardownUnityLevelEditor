using UnityEngine;
public class TeardownEnvironment : GeneralTag
{
    public enum Template
    {
        sunny
    }

    public class FogParams
    {
        public float a1;
        public float a2;
        public float a3;
        public float a4;
    }
    public Template template = Template.sunny;
    public float skyboxrot = -90f;
    public Vector3 sunDir = new Vector3(0, 70, 45);
    public float sunBrightness = 4;
    public float sunFogScale = 0.1f;
    public Color sunColorTint;
}
