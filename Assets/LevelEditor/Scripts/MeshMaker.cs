using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MagicaRenderer;

public class MeshMaker : MonoBehaviour
{
    public Vector3Int size = Vector3Int.one;
    public string input;

    [ContextMenu("run")]
    void Run()
    {
        string[] splits = input.Split(' ');        
        size = new Vector3Int(
            Convert.ToInt32(splits[3] + splits[2]+ splits[1] + splits[0], 16),
            Convert.ToInt32(splits[7] + splits[6] + splits[5] + splits[4], 16),
            Convert.ToInt32(splits[11] + splits[10] + splits[9] + splits[8], 16));
        RenderVoxel[,,] voxelArray = new RenderVoxel[size.x, size.y, size.z];
        int x = 0;
        int y = 0;
        int z = 0;

        for (int i = 16; i < splits.Length; i += 2)
        {
            int n = Convert.ToInt32(splits[i], 16) + 1;
            int color = Convert.ToInt32(splits[i + 1], 16);
            for (int k = 0; k < n; k++)
            {
                x++;
                if (x == size.x)
                {
                    y++;
                    x = 0;
                    if (y == size.y)
                    {
                        y = 0;
                        z++;
                    }
                }
                if (color == 0)
                {
                    continue;
                }
                if (z >= size.z) break;
                voxelArray[x, y, z] = new RenderVoxel(color);                
            }
        }

        MagicaRenderer renderer = new MagicaRenderer();
        MeshAndColors meshAndColors = renderer.createMeshFromVoxelArray(voxelArray, Vector3.zero);
        GameObject go = new GameObject();

        Color32[] colors = new Color32[meshAndColors.colors.Count];
        for (int i=0; i < meshAndColors.colors.Count; i++)
        {
            int c = meshAndColors.colors[i];
            colors[i] = new Color(c/256f, c / 256f, c / 256f, 1);
        }
        meshAndColors.mesh.colors32 = colors;
        (go.AddComponent<MeshFilter>()).mesh = meshAndColors.mesh;
        (go.AddComponent<MeshRenderer>()).material = Resources.Load("VertexShading", typeof(Material)) as Material;
    }
}
