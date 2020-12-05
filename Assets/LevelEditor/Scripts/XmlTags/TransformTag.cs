using UnityEngine;

[ExecuteInEditMode]
public class TransformTag : GeneralTag
{
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public float scale;

    public void Update()
    {
        base.Update();
        updateTransform();
    }

    protected void updateTransform()
    {        
        position = transform.localPosition;
        rotation = transform.eulerAngles;
    }
}
