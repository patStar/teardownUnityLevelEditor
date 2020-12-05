using UnityEngine;
public class TeardownJoint : TransformTag
{
    public enum Type
    {
        hinge,
        prismatic,
        ball        
    }

    public Type type;
    public float size = 0;
    public float rotstrength = -1;
    public float rotspring = -1;
    public Vector2 limits;

    private void OnDrawGizmos()
    {        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.forward * 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 0.2f);     

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.05f);        
    }
}
