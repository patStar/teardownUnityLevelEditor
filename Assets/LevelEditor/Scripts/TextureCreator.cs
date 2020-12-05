using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextureCreator : MonoBehaviour
{
    // Start is called before the first frame update
    [ContextMenu("Create")]
    public void Test()
    {
        int size = 256;
        Texture3D texture = new Texture3D(size, size, size, TextureFormat.ARGB32, false);
        Color32[] colors = new Color32[size * size * size];
        float inverseResolution = 1.0f / (size - 1.0f);

        for (int z = 0; z < size; z++)
        {
            int zOffset = z * size * size;
            for (int y = 0; y < size; y++)
            {
                int yOffset = y * size;
                for (int x = 0; x < size; x++)
                {
                    Vector3 v3 = new Vector3(x-size/2, y - size / 2, z - size / 2);
                    float d = v3.magnitude;

                    if (d > size / 2)
                    {
                        colors[x + yOffset + zOffset] = new Color(0, 0, 0, 0);
                    }
                    else
                    {
                        colors[x + yOffset + zOffset] = new Color(d/size,
                            0,0, 1.0f);
                    }
                }
            }
        }
        // Copy the color values to the texture
        texture.SetPixels32(colors);

        // Apply the changes to the texture and upload the updated texture to the GPU
        texture.Apply();

        // Save the texture to your Unity Project
        AssetDatabase.CreateAsset(texture, "Assets/Example3DTexture.asset");
    }
}
