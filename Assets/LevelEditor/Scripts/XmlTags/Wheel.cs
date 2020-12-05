using UnityEngine;

public class Wheel : TransformTag
{
    public float drive = 0;
    public float steer = 0;
    public Vector2 travel = Vector2.one;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.right);
        Gizmos.DrawRay(transform.position, -transform.right);
    }
}
