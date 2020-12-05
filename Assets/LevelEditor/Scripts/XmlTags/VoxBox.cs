using UnityEngine;

[SelectionBase]
public class VoxBox : GameObjectTag
{
    public bool dynamic = false;
    public Color color;

    private void Start()
    {
        transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
    }

    private void Update()
    {
        updateParent();
        updateTransform();

        if (transform.childCount > 0)
        {            
            transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.color = color;            
        }
        size = transform.localScale * 10;
    }
}
