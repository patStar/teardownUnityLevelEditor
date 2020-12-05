using System.Collections.Generic;
using UnityEngine;

public class Boundary : GeneralTag
{        
    private void OnDrawGizmos()
    {
        List<Vertex> vertices = new List<Vertex>();
        foreach (GeneralTag child in children)
        {
            if(child is Vertex)
            {
                vertices.Add((Vertex)child);
            }                
        }
            
        Gizmos.color = Color.yellow;
        for (int i=1; i<vertices.Count; i++)
        {
            Gizmos.DrawLine(new Vector3(vertices[i-1].pos.x,0, vertices[i - 1].pos.y), new Vector3(vertices[i].pos.x, 0, vertices[i].pos.y));
        }
        Gizmos.DrawLine(new Vector3(vertices[0].pos.x, 0, vertices[0].pos.y), new Vector3(vertices[vertices.Count-1].pos.x, 0, vertices[vertices.Count - 1].pos.y));
    }
}
