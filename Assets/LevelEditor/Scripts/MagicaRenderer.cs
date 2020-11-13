using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityEngine;
using UnityEngine.Timeline;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MagicaRenderer
{    
    public static float size = 1;

    public static Vector3[] getRotationMatrix(TransformNodeChunk transformNodeChunk)
    {
        Vector3[] matrix = new Vector3[3];
        int rotation = 0;
        
        if (transformNodeChunk.frameAttributes.Count > 0 && transformNodeChunk.frameAttributes[0].ContainsKey("_r"))
        {            
            rotation = int.Parse(transformNodeChunk.frameAttributes[0]["_r"]);
        }
        else
        {
            matrix[0] = new Vector3(1, 0, 0);
            matrix[1] = new Vector3(0, 1, 0);
            matrix[2] = new Vector3(0, 0, 1);
            return matrix;
        }        

        Vector3 aRot = Vector3.zero;
        Vector3 bRot = Vector3.zero;
        Vector3 cRot = Vector3.zero;

        if ((rotation & 2) != 0)
        {
            if ((rotation & 16) != 0)
            {
                aRot = new Vector3(0, 0, -1f);
            }
            else
            {
                aRot = new Vector3(0, 0, 1f);
            }
        }
        else if ((rotation & 1) != 0)
        {
            if ((rotation & 16) != 0)
            {
                aRot = new Vector3(0, -1f, 0);
            }
            else
            {
                aRot = new Vector3(0, 1f, 0);
            }
        }
        else
        {
            if ((rotation & 16) != 0)
            {
                aRot = new Vector3(-1f, 0, 0);
            }
            else
            {
                aRot = new Vector3(1f, 0, 0);
            }
        }

        if ((rotation & 8) != 0)
        {
            if ((rotation & 32) != 0)
            {
                bRot = new Vector3(0, 0, -1f);
            }
            else
            {
                bRot = new Vector3(0, 0, 1f);
            }
        }
        else if ((rotation & 4) != 0)
        {
            if ((rotation & 32) != 0)
            {
                bRot = new Vector3(0, -1f, 0);
            }
            else
            {
                bRot = new Vector3(0, 1f, 0);
            }
        }
        else
        {
            if ((rotation & 32) != 0)
            {
                bRot = new Vector3(-1f, 0, 0);
            }
            else
            {
                bRot = new Vector3(1f, 0, 0);
            }
        }

        float xR = 0;
        float yR = 0;
        float zR = 0;
        float sgn = 1;
        if ((rotation & 64) != 0)
        {
            sgn = -1;
        }
        if (aRot.x == 0 && bRot.x == 0)
        {
            xR = 1f * sgn;
        }
        else if (aRot.y == 0 && bRot.y == 0)
        {
            yR = 1f * sgn;
        }
        else if (aRot.z == 0 && bRot.z == 0)
        {
            zR = 1f * sgn;
        }
        cRot = new Vector3(xR, yR, zR);

        matrix[0] = aRot;
        matrix[1] = bRot;
        matrix[2] = cRot;

        return matrix;
    }

    public void ImportMagicaVoxelFile(string path)
    {
        GameObject levelGo = new GameObject(path.Split('\\')[path.Split('\\').Length - 1]);
        MagicaImportedFile magicaImportedFile = levelGo.AddComponent<MagicaImportedFile>();
        magicaImportedFile.voxFile = path.Split('\\')[path.Split('\\').Length - 1];

        Dictionary<string, GameObject> namedGameObjects = new Dictionary<string, GameObject>();        

        Chunk mainChunk = MagicaVoxelReader.ReadMagicaChunks(path);
        List<Material> colorMaterials = ImportColors(mainChunk);
        List<string> names = new List<string>();
        List<string> doubleNames = new List<string>();

        for (int i = 0; i < mainChunk.children.Count; i++)
        {
            Chunk chunk = mainChunk.children[i];
            if (chunk is VoxelModelChunk)
            {
                VoxelModelChunk voxelChunk = (VoxelModelChunk)chunk;
                List<Material> materials = new List<Material>();

                foreach (ShapeNodeChunk shape in voxelChunk.shapes)
                {
                    MeshAndColors meshAndColors = createMesh(voxelChunk, shape);
                    foreach (int color in meshAndColors.colors)
                    {
                        materials.Add(colorMaterials[color - 1]);
                    }

                    TransformNodeChunk transformNodeChunk = shape.transform;
                    GameObject go = new GameObject();
                    ObjectAttributes script = go.AddComponent<ObjectAttributes>();
                    TeardownProperties teardownProperties = go.AddComponent<TeardownProperties>();

                    MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                    renderer.materials = materials.ToArray();
                    MeshFilter filter = go.AddComponent<MeshFilter>();
                    filter.mesh = meshAndColors.mesh;

                    Vector3 shift = new Vector3(0, 0, 0);
                    while (transformNodeChunk != null)
                    {
                        if (transformNodeChunk.attributes.Count > 0 && transformNodeChunk.attributes.ContainsKey("_name"))
                        {
                            string name = transformNodeChunk.attributes["_name"];
                            script.names.Add(name);                            

                            if (names.Contains(name))
                            {                                
                                if (!doubleNames.Contains(name))
                                {
                                    doubleNames.Add(name);
                                }
                            } else {
                                names.Add(name);
                                namedGameObjects.Add(name, go);
                            }
                        }

                        if (transformNodeChunk.frameAttributes[0].ContainsKey("_r"))
                        {
                            script.rotations.Add(transformNodeChunk.frameAttributes[0]["_r"]);
                        }

                        Vector3[] rotationMatrix = getRotationMatrix(transformNodeChunk);
                        script.aRot = rotationMatrix[0];
                        script.bRot = rotationMatrix[1];
                        script.cRot = rotationMatrix[2];

                        if (transformNodeChunk.frameAttributes[0].ContainsKey("_t"))
                        {
                            string[] coords = transformNodeChunk.frameAttributes[0]["_t"].Split(' ');
                            Vector3 currentShift = new Vector3(float.Parse(coords[0]) / 10f, float.Parse(coords[2]) / 10f, float.Parse(coords[1]) / 10f); ;
                            script.shifts.Add(currentShift);
                            shift += currentShift;
                        }
                        if (transformNodeChunk.group != null && transformNodeChunk.group.transform != null)
                        {
                            transformNodeChunk = transformNodeChunk.group.transform;
                        }
                        else
                        {
                            transformNodeChunk = null;
                        }
                    }

                    Vector3 ve = rotateVector(new Vector3(voxelChunk.sizeChunk.sizeX, voxelChunk.sizeChunk.sizeY, voxelChunk.sizeChunk.sizeZ), shape.transform);

                    script.sizeX = (int) Math.Abs(ve.x);
                    script.sizeY = (int)Math.Abs(ve.z);
                    script.sizeZ = (int)Math.Abs(ve.y);

                    script.trans = new Vector3((float)Math.Floor((double)(Math.Abs(ve.x) / 2f)) / 10f, (float)Math.Floor((double)(Math.Abs(ve.z) / 2f)) / 10f, (float)Math.Floor((double)(Math.Abs(ve.y) / 2f)) / 10f);                    
                    script.parentVoxFile = path.Split('\\')[path.Split('\\').Length - 1];
                    shift -= script.trans;

                    go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    go.transform.position = shift;
                    go.transform.parent = levelGo.transform;
                    if (script.names.Count > 0)
                    {
                        go.name = script.names[0];
                    }                    
                }
            }
        }

        foreach(string name in doubleNames) {
            namedGameObjects.Remove(name);
        }

        GameObject validContainer = new GameObject("ValidAssets");
        foreach(GameObject valid in namedGameObjects.Values)
        {
            valid.transform.parent = validContainer.transform;
            valid.GetComponent<TeardownProperties>().setValid();
        }

        validContainer.transform.parent = levelGo.transform;
        validContainer.transform.SetSiblingIndex(0);
    }

    private static List<Material> ImportColors(Chunk mainChunk)
    {
        List<Material> colorMaterials = new List<Material>();

        for (int i = 0; i < mainChunk.children.Count; i++)
        {
            Chunk chunk = mainChunk.children[i];
            if (chunk is PaletteChunk)
            {
                PaletteChunk palette = (PaletteChunk)chunk;
                foreach (Color c in palette.colors)
                {
                    Material m = new Material(Shader.Find("Standard"));
                    m.color = c;
                    colorMaterials.Add(m);
                }
            }
        }

        return colorMaterials;
    }

    class MeshAndColors
    {
        public Mesh mesh;
        public List<int> colors;
    }
    
    class RenderVoxel
    {
        public static int DOWN = 1;
        public static int UP = 2;
        public static int LEFT = 4;
        public static int RIGHT = 8;
        public static int FRONT = 16;
        public static int BACK = 32;

        public int sides = UP|DOWN|LEFT|RIGHT|FRONT|BACK;
        public int color = 0;

        public RenderVoxel(int color)
        {
            this.color = color;
        }        
    }

    private Vector3 rotateVector(Vector3 vec, TransformNodeChunk transformNodeChunk)
    {
        Vector3 current = new Vector3(vec.x, vec.y, vec.z);

        while (transformNodeChunk != null) {
            Vector3[] matrix = getRotationMatrix(transformNodeChunk);

            float x = current.x * matrix[0].x + current.y * matrix[0].y + current.z * matrix[0].z;
            float y = current.x * matrix[1].x + current.y * matrix[1].y + current.z * matrix[1].z;
            float z = current.x * matrix[2].x + current.y * matrix[2].y + current.z * matrix[2].z;

            current = new Vector3(x, y, z);
            if (transformNodeChunk.group != null)
            {
                transformNodeChunk = transformNodeChunk.group.transform;
            }
            else
            {
                transformNodeChunk = null;
            }
        }

        return current;
    }

    private MeshAndColors createMesh(VoxelModelChunk voxelChunk, ShapeNodeChunk shape)
    {        
        Vector3 ve = rotateVector(new Vector3(voxelChunk.sizeChunk.sizeX, voxelChunk.sizeChunk.sizeY, voxelChunk.sizeChunk.sizeZ), shape.transform);
        
        int sizeX = (int)Math.Abs(ve.x);
        int sizeY = (int)Math.Abs(ve.y);
        int sizeZ = (int)Math.Abs(ve.z);

        Vector3 correction = new Vector3((ve.x < 0) ? Math.Abs(ve.x)-1 : 0, (ve.y < 0) ? Math.Abs(ve.y) - 1 : 0, (ve.z < 0) ? Math.Abs(ve.z) - 1 : 0);        
         
        RenderVoxel[,,] voxelArray = new RenderVoxel[sizeX, sizeY, sizeZ];

        foreach (Voxel voxel in voxelChunk.voxels)
        {
            Vector3 rotVec = rotateVector(new Vector3(voxel.x, voxel.y, voxel.z), shape.transform) + correction;            
            voxelArray[(int)rotVec.x, (int)rotVec.y, (int)rotVec.z] = new RenderVoxel(voxel.colorIndex);
        }      

        // gravity first              
        for (int z=0; z< sizeZ; z++)
        {
            for (int y=0; y < sizeY; y++)
            {
                for (int x=0; x < sizeX; x++)
                {                    
                    if (voxelArray[x, y, z] == null)
                    {
                        continue;
                    }                    

                    if (x > 0 && voxelArray[x-1, y, z] != null)
                    {
                        voxelArray[x, y, z].sides ^= RenderVoxel.LEFT;
                        voxelArray[x-1, y, z].sides ^= RenderVoxel.RIGHT;
                    }
                    if (y > 0 && voxelArray[x, y-1, z] != null)
                    {
                        voxelArray[x, y, z].sides ^= RenderVoxel.BACK;
                        voxelArray[x, y-1, z].sides ^= RenderVoxel.FRONT;
                    }
                    if (z > 0 && voxelArray[x, y , z-1] != null)
                    {
                        voxelArray[x, y, z].sides ^= RenderVoxel.DOWN;
                        voxelArray[x, y, z-1].sides ^= RenderVoxel.UP;
                    }
                }
            }
        }

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int>[] trianglesByColor = new List<int>[256];
        List<int> colorOrder = new List<int>();        

        for (int z = 0; z < sizeZ; z++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {                    
                    RenderVoxel renderVoxel = voxelArray[x, y, z];
                    if (renderVoxel == null)
                    {
                        continue;
                    }

                    if (!colorOrder.Contains(renderVoxel.color))
                    {
                        colorOrder.Add(renderVoxel.color);
                        trianglesByColor[renderVoxel.color] = new List<int>();
                    }

                    List<int> triangles = trianglesByColor[renderVoxel.color];

                    if ((renderVoxel.sides & RenderVoxel.DOWN) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3(x * size, z * size, y * size));
                        vertices.Add(new Vector3((x+1) * size, z * size, y * size));
                        vertices.Add(new Vector3(x * size, z * size, (y+1) * size));
                        vertices.Add(new Vector3((x+1) * size, z * size, (y + 1) * size));

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex + 2);

                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(1, 0));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(0, 1));

                    }
                    if ((renderVoxel.sides & RenderVoxel.UP) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3(x * size, (z+1) * size, y * size));
                        vertices.Add(new Vector3((x + 1) * size, (z+1) * size, y * size));
                        vertices.Add(new Vector3(x * size, (z+1) * size, (y + 1) * size));
                        vertices.Add(new Vector3((x + 1) * size, (z+1) * size, (y + 1) * size));

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex + 1);

                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(1, 0));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(0, 1));
                    }
                    if ((renderVoxel.sides & RenderVoxel.LEFT) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3(x * size, z * size, y * size));
                        vertices.Add(new Vector3(x * size, z * size, (y + 1) * size));
                        vertices.Add(new Vector3(x * size, (z + 1) * size, (y + 1) * size));
                        vertices.Add(new Vector3(x * size, (z + 1) * size, y * size));

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex);

                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(1, 0));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(0, 1));
                    }

                    if ((renderVoxel.sides & RenderVoxel.RIGHT) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3((x + 1) * size, z * size, y * size));
                        vertices.Add(new Vector3((x + 1) * size, z * size, (y + 1) * size));
                        vertices.Add(new Vector3((x + 1) * size, (z + 1) * size, (y + 1) * size));
                        vertices.Add(new Vector3((x + 1) * size, (z + 1) * size, y * size));

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex);

                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(1, 0));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(0, 1));
                    }
                    if ((renderVoxel.sides & RenderVoxel.BACK) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3(x * size, z * size, y * size));
                        vertices.Add(new Vector3(x * size, (z+1) * size, y * size));
                        vertices.Add(new Vector3((x+1) * size, (z+1) * size, y * size));
                        vertices.Add(new Vector3((x+1) * size, z * size, y * size));

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex);

                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(1, 0));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(0, 1));
                    }
                    if ((renderVoxel.sides & RenderVoxel.FRONT) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3(x * size, z * size, (y+1) * size));
                        vertices.Add(new Vector3(x * size, (z + 1) * size, (y+1) * size));
                        vertices.Add(new Vector3((x + 1) * size, (z + 1) * size, (y+1) * size));
                        vertices.Add(new Vector3((x + 1) * size, z * size, (y+1) * size));

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex);

                        uvs.Add(new Vector2(0, 0));
                        uvs.Add(new Vector2(1, 0));
                        uvs.Add(new Vector2(1, 1));
                        uvs.Add(new Vector2(0, 1));
                    }
                }
            }
        }

        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(sizeX-1, sizeY-1, sizeZ-1));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 0));

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.subMeshCount = colorOrder.Count;
        for (int i = 0; i < colorOrder.Count; i++) {
            mesh.SetTriangles(trianglesByColor[colorOrder[i]], i);
        }
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        MeshAndColors meshAndColors = new MeshAndColors();
        meshAndColors.mesh = mesh;
        meshAndColors.colors = colorOrder;

        return meshAndColors;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
