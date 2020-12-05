using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GeneralTag : MonoBehaviour
{
    public string teardownName = "";
    public string tags = "";
    public GeneralTag parent;
    public List<GeneralTag> children = new List<GeneralTag>();

    private void Update()
    {
        updateParent();
    }

    protected void updateParent()
    {
        if (parent != null && transform.parent && transform.parent.gameObject.GetComponent<GeneralTag>() == parent) return;

        if (transform.parent != null)
        {
            parent = transform.parent.gameObject.GetComponent<GeneralTag>();
        }
        else
        {
            parent = null;
        }
    }
}
