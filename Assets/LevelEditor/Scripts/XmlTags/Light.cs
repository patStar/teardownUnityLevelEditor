using UnityEngine;
public class Light : TransformTag
{
    public enum Type
    {
        area,
        cone
    }

    public Type type;
    public Color color;
    public float unshadowed = 0.0f;
    public float glare = 0.0f;
    public float angle = 0.0f;
    public float penumbra = 0.0f;
}
