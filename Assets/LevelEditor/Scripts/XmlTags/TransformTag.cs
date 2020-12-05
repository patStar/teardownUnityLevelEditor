using UnityEngine;

[ExecuteInEditMode]
public class TransformTag : GeneralTag
{
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;

    private void Update()
    {
        updateTransform();
    }

    protected void updateTransform()
    {
        position = transform.localPosition;
        rotation = transform.localEulerAngles;
    }
}
