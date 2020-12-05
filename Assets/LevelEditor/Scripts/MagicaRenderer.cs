using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Timeline;
using Debug = UnityEngine.Debug;
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

    public string[] GetObjectNames(string path)
    {
        Chunk mainChunk = MagicaVoxelReader.ReadMagicaChunks(path);

        HashSet<string> names = new HashSet<string>();

        for (int i = 0; i < mainChunk.children.Count; i++)
        {
            Chunk chunk = mainChunk.children[i];
            if (chunk is TransformNodeChunk)
            {
                TransformNodeChunk transformNodeChunk = (TransformNodeChunk) chunk;
                if (transformNodeChunk.attributes.Keys.Contains("_name"))
                {
                    names.Add(transformNodeChunk.attributes["_name"]);
                }
            }
        }

        return names.ToArray<string>();
    }

    public GameObject ImportMagicaVoxelFileObject(string path, string objectName)
    {
        Material vertexMaterial = Resources.Load("VertexShading", typeof(Material)) as Material;

        GameObject levelGo = new GameObject(Path.GetFileName(path));
        levelGo.AddComponent<TeardownProperties>();
        MagicaImportedFile magicaImportedFile = levelGo.AddComponent<MagicaImportedFile>();
        magicaImportedFile.voxFile = Path.GetFileName(path);

        Chunk mainChunk = MagicaVoxelReader.ReadMagicaChunks(path);
        List<Material> colorMaterials = ImportColors(mainChunk);

        for (int i = 0; i < mainChunk.children.Count; i++)
        {
            Chunk chunk = mainChunk.children[i];
            if (chunk is VoxelModelChunk)
            {
                VoxelModelChunk voxelChunk = (VoxelModelChunk)chunk;

                foreach (ShapeNodeChunk shape in voxelChunk.shapes)
                {
                    if (!shape.transform.attributes.Keys.Contains("_name") || !shape.transform.attributes["_name"].Equals(objectName))
                    {
                        continue;
                    }
                                    
                    MeshAndColors meshAndColors = createMesh(voxelChunk, shape);
                    Color[] colors = new Color[meshAndColors.colors.Count];
                    for (int c = 0; c < meshAndColors.colors.Count; c++)
                    {
                        colors[c] = colorMaterials[meshAndColors.colors[c] - 1].color;
                    }
                    meshAndColors.mesh.colors = colors;

                    TransformNodeChunk transformNodeChunk = shape.transform;
                    GameObject go = new GameObject();
                    ObjectAttributes script = go.AddComponent<ObjectAttributes>();
                    MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                    renderer.material = vertexMaterial;
                    MeshFilter filter = go.AddComponent<MeshFilter>();
                    filter.mesh = meshAndColors.mesh;

                    Vector3 shift = Vector3.zero; //new Vector3(script.singleCenter.x, 0, script.singleCenter.y);
                    while (transformNodeChunk != null)
                    {
                        if (transformNodeChunk.attributes.Count > 0 && transformNodeChunk.attributes.ContainsKey("_name"))
                        {
                            string name = transformNodeChunk.attributes["_name"];
                            script.names.Add(name);
                        }

                        if (transformNodeChunk.frameAttributes[0].ContainsKey("_r"))
                        {
                            script.rotations.Add(transformNodeChunk.frameAttributes[0]["_r"]);
                        }

                        Vector3[] rotationMatrix = getRotationMatrix(transformNodeChunk);
                        script.rotationMatrices.Add(rotationMatrix);

                        if (transformNodeChunk.frameAttributes[0].ContainsKey("_t"))
                        {
                            string[] coords = transformNodeChunk.frameAttributes[0]["_t"].Split(' ');
                            Vector3 currentShift = new Vector3(float.Parse(coords[0]) / 10f, float.Parse(coords[2]) / 10f, float.Parse(coords[1]) / 10f); ;
                            script.magicaTransitions.Add(currentShift);
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

                    script.magicaTotalSize = new Vector3(Math.Abs(ve.x), Math.Abs(ve.z), Math.Abs(ve.y));

                    script.bottomCenterOfVoxelMass = shape.singleCenter / 10f;

                    script.centerOfMagicaMass = new Vector3((float)Math.Floor((double)(Math.Abs(ve.x) / 2f)) / 10f, (float)Math.Floor((double)(Math.Abs(ve.z) / 2f)) / 10f, (float)Math.Floor((double)(Math.Abs(ve.y) / 2f)) / 10f);
                    script.parentVoxFile = Path.GetFileName(path);

                    shift += new Vector3(script.bottomCenterOfVoxelMass.x - script.centerOfMagicaMass.x, script.bottomCenterOfVoxelMass.y - script.centerOfMagicaMass.y, script.bottomCenterOfVoxelMass.z - script.centerOfMagicaMass.z);

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

        return levelGo;
    }

    public GameObject ImportMagicaVoxelFile(string path)
    {
        Material vertexMaterial = Resources.Load("VertexShading", typeof(Material)) as Material;

        GameObject levelGo = new GameObject(Path.GetFileName(path));
        levelGo.AddComponent<TeardownProperties>();
        MagicaImportedFile magicaImportedFile = levelGo.AddComponent<MagicaImportedFile>();
        magicaImportedFile.voxFile = Path.GetFileName(path);

        List<GameObject> gameObjects = new List<GameObject>();
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

                foreach (ShapeNodeChunk shape in voxelChunk.shapes)
                {
                    MeshAndColors meshAndColors = createMesh(voxelChunk, shape);
                    Color[] colors = new Color[meshAndColors.colors.Count];
                    for(int c=0; c<meshAndColors.colors.Count; c++)
                    {
                        colors[c] = colorMaterials[meshAndColors.colors[c]-1].color;
                    }
                    meshAndColors.mesh.colors = colors;

                    TransformNodeChunk transformNodeChunk = shape.transform;
                    GameObject go = new GameObject();
                    gameObjects.Add(go);
                    ObjectAttributes script = go.AddComponent<ObjectAttributes>();
                    MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                    renderer.material = vertexMaterial;
                    MeshFilter filter = go.AddComponent<MeshFilter>();
                    filter.mesh = meshAndColors.mesh;

                    Vector3 shift = Vector3.zero; //new Vector3(script.singleCenter.x, 0, script.singleCenter.y);
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
                            }
                            else
                            {
                                names.Add(name);
                                namedGameObjects.Add(name, go);
                            }
                        }

                        if (transformNodeChunk.frameAttributes[0].ContainsKey("_r"))
                        {
                            script.rotations.Add(transformNodeChunk.frameAttributes[0]["_r"]);
                        }

                        Vector3[] rotationMatrix = getRotationMatrix(transformNodeChunk);
                        script.rotationMatrices.Add(rotationMatrix);

                        if (transformNodeChunk.frameAttributes[0].ContainsKey("_t"))
                        {
                            string[] coords = transformNodeChunk.frameAttributes[0]["_t"].Split(' ');
                            Vector3 currentShift = new Vector3(float.Parse(coords[0]) / 10f, float.Parse(coords[2]) / 10f, float.Parse(coords[1]) / 10f); ;
                            script.magicaTransitions.Add(currentShift);
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

                    script.magicaTotalSize = new Vector3(Math.Abs(ve.x), Math.Abs(ve.z), Math.Abs(ve.y));

                    script.bottomCenterOfVoxelMass = shape.singleCenter / 10f;

                    script.centerOfMagicaMass = new Vector3((float)Math.Floor((double)(Math.Abs(ve.x) / 2f)) / 10f, (float)Math.Floor((double)(Math.Abs(ve.z) / 2f)) / 10f, (float)Math.Floor((double)(Math.Abs(ve.y) / 2f)) / 10f);
                    script.parentVoxFile = Path.GetFileName(path);
                    //shift -= script.trans;
                    shift += new Vector3(script.bottomCenterOfVoxelMass.x - script.centerOfMagicaMass.x, script.bottomCenterOfVoxelMass.y - script.centerOfMagicaMass.y, script.bottomCenterOfVoxelMass.z - script.centerOfMagicaMass.z);
                    //shift += (script.bottomCenterOfVoxelMass - script.centerOfMagicaMass);

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

        // remove double named objects since we cannot insert them properly
        foreach (string name in doubleNames)
        {
            namedGameObjects.Remove(name);
        }

        foreach (GameObject g in gameObjects)
        {
            if (namedGameObjects.Values.Contains(g))
            {
                TeardownProperties teardownProperties = g.AddComponent<TeardownProperties>();
            }
        }

        return levelGo;
    }

    public GameObject ImportMagicaVoxelFileAssets(string path)
    {
        GameObject levelGo = new GameObject(Path.GetFileName(path));
        
        string fileName = Path.GetFileName(path);
       
        List<GameObject> gameObjects = new List<GameObject>();
        Dictionary<string, GameObject> namedGameObjects = new Dictionary<string, GameObject>();        

        Chunk mainChunk = MagicaVoxelReader.ReadMagicaChunks(path);
        List<Material> colorMaterials = ImportColors(mainChunk);
        List<string> names = new List<string>();
        List<string> doubleNames = new List<string>();

        string localPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine("Assets", "ImportedPrefabs", fileName));
        string materialFolder = Path.Combine("Assets", "ImportedPrefabs", fileName, "Materials");
        string meshFolder = Path.Combine("Assets", "ImportedPrefabs", fileName, "Meshes");
        if (!AssetDatabase.IsValidFolder(localPath))
        {
            AssetDatabase.CreateFolder(Path.Combine("Assets", "ImportedPrefabs"), fileName);
        }
        if (!AssetDatabase.IsValidFolder(materialFolder))
        {
            AssetDatabase.CreateFolder(localPath, "Materials");
        }
        if (!AssetDatabase.IsValidFolder(meshFolder))
        {
            AssetDatabase.CreateFolder(localPath, "Meshes");
        }

        for (int m = 0; m < colorMaterials.Count; m++)
        {
            AssetDatabase.CreateAsset(colorMaterials[m], Path.Combine(materialFolder , "Color_" + m + ".mat"));
        }

        for (int i = 0; i < mainChunk.children.Count; i++)
        {
            Chunk chunk = mainChunk.children[i];
            if (chunk is VoxelModelChunk)
            {
                VoxelModelChunk voxelChunk = (VoxelModelChunk)chunk;
                
                foreach (ShapeNodeChunk shape in voxelChunk.shapes)
                {                               
                    TransformNodeChunk transformNodeChunk = shape.transform;

                    bool isPotentialAsset = false;
                    while (transformNodeChunk != null)
                    {
                        if (transformNodeChunk.attributes.Count > 0 && transformNodeChunk.attributes.ContainsKey("_name"))
                        {
                            isPotentialAsset = true;
                            break;
                        }
                    }
                    if (!isPotentialAsset) continue;

                    MeshAndColors meshAndColors = createMesh(voxelChunk, shape);
                    List<Material> materials = new List<Material>();
                    foreach (int color in meshAndColors.colors)
                    {
                        materials.Add(colorMaterials[color - 1]);
                    }

                    GameObject go = new GameObject();
                    gameObjects.Add(go);
                    ObjectAttributes script = go.AddComponent<ObjectAttributes>();
                    
                    MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                    renderer.materials = materials.ToArray();
                    MeshFilter filter = go.AddComponent<MeshFilter>();
                    filter.mesh = meshAndColors.mesh;

                    Vector3 shift = Vector3.zero; //new Vector3(script.singleCenter.x, 0, script.singleCenter.y);
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
                        script.rotationMatrices.Add(rotationMatrix);                        

                        if (transformNodeChunk.frameAttributes[0].ContainsKey("_t"))
                        {
                            string[] coords = transformNodeChunk.frameAttributes[0]["_t"].Split(' ');
                            Vector3 currentShift = new Vector3(float.Parse(coords[0]) / 10f, float.Parse(coords[2]) / 10f, float.Parse(coords[1]) / 10f); ;
                            script.magicaTransitions.Add(currentShift);
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

                    script.magicaTotalSize = new Vector3(Math.Abs(ve.x), Math.Abs(ve.z), Math.Abs(ve.y));

                    script.bottomCenterOfVoxelMass = shape.singleCenter/10f;

                    script.centerOfMagicaMass = new Vector3((float)Math.Floor((double)(Math.Abs(ve.x) / 2f)) / 10f, (float)Math.Floor((double)(Math.Abs(ve.z) / 2f)) / 10f, (float)Math.Floor((double)(Math.Abs(ve.y) / 2f)) / 10f);                    
                    script.parentVoxFile = Path.GetFileName(path);
                    //shift -= script.trans;
                    shift += new Vector3(script.bottomCenterOfVoxelMass.x - script.centerOfMagicaMass.x, script.bottomCenterOfVoxelMass.y- script.centerOfMagicaMass.y, script.bottomCenterOfVoxelMass.z - script.centerOfMagicaMass.z);
                    //shift += (script.bottomCenterOfVoxelMass - script.centerOfMagicaMass);

                    go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    go.transform.position = shift;
                    go.name = script.names[0];
                    go.transform.parent = levelGo.transform;
                }
            }
        }
    
        // remove double named objects since we cannot insert them properly
        foreach(string name in doubleNames){
            namedGameObjects.Remove(name);
        }

        foreach(GameObject g in namedGameObjects.Values)
        {
            TeardownProperties teardownProperties = g.AddComponent<TeardownProperties>();

            // Create the new Prefab.                        
            AssetDatabase.CreateAsset(g.GetComponent<MeshFilter>().sharedMesh, Path.Combine(meshFolder, g.name.Replace(" ", "_") + ".mesh"));
            AssetDatabase.SaveAssets();
            PrefabUtility.SaveAsPrefabAssetAndConnect(g, Path.Combine("Assets", "ImportedPrefabs", fileName, g.name.Replace(" ", "_") + ".prefab"), InteractionMode.UserAction);                       
        }

        PrefabUtility.SaveAsPrefabAssetAndConnect(levelGo, Path.Combine("Assets", "ImportedPrefabs", fileName, fileName + ".prefab"), InteractionMode.UserAction);

        return levelGo;
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

    public class MeshAndColors
    {
        public Mesh mesh;
        public List<int> colors;
    }

    public class RenderVoxel
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

    public MeshAndColors createMesh(VoxelModelChunk voxelChunk, ShapeNodeChunk shape)
    {
        Vector3 ve = rotateVector(new Vector3(voxelChunk.sizeChunk.sizeX, voxelChunk.sizeChunk.sizeY, voxelChunk.sizeChunk.sizeZ), shape.transform);

        int sizeX = (int)Math.Abs(ve.x);
        int sizeY = (int)Math.Abs(ve.y);
        int sizeZ = (int)Math.Abs(ve.z);

        // WTF? Dont drink and code!!!
        Vector3 correction = new Vector3((ve.x < 0) ? Math.Abs(ve.x) - 1 : 0, (ve.y < 0) ? Math.Abs(ve.y) - 1 : 0, (ve.z < 0) ? Math.Abs(ve.z) - 1 : 0);

        RenderVoxel[,,] voxelArray = new RenderVoxel[sizeX, sizeY, sizeZ];

        float maxX = 0;
        float maxY = 0;
        float maxZ = 0;
        float minX = sizeX;
        float minY = sizeY;
        float minZ = sizeZ;
        foreach (Voxel voxel in voxelChunk.voxels)
        {
            Vector3 rotVec = rotateVector(new Vector3(voxel.x, voxel.y, voxel.z), shape.transform) + correction;
            voxelArray[(int)rotVec.x, (int)rotVec.y, (int)rotVec.z] = new RenderVoxel(voxel.colorIndex);
            if (rotVec.x < minX) minX = rotVec.x;
            if (rotVec.y < minY) minY = rotVec.y;
            if (rotVec.z < minZ) minZ = rotVec.z;
            if (rotVec.x > maxX) maxX = rotVec.x;
            if (rotVec.y > maxY) maxY = rotVec.y;
            if (rotVec.z > maxZ) maxZ = rotVec.z;
        }

        shape.singleCenter = new Vector3((float)Math.Ceiling(minX + (maxX - minX) / 2f), minZ, (float)Math.Ceiling(minY + (maxY - minY) / 2f));
        Vector3 shifts = shape.singleCenter;
      
        MeshAndColors meshAndColors = createMeshFromVoxelArray(voxelArray, shifts);
        
        return meshAndColors;
    }

    public MeshAndColors createMeshFromVoxelArray(RenderVoxel[,,] voxelArray, Vector3 shifts)
    {
        // gravity first              
        for (int z = 0; z < voxelArray.GetLength(2); z++)
        {
            for (int y = 0; y < voxelArray.GetLength(1); y++)
            {
                for (int x = 0; x < voxelArray.GetLength(0); x++)
                {
                    if (voxelArray[x, y, z] == null)
                    {
                        continue;
                    }

                    if (x > 0 && voxelArray[x - 1, y, z] != null)
                    {
                        voxelArray[x, y, z].sides ^= RenderVoxel.LEFT;
                        voxelArray[x - 1, y, z].sides ^= RenderVoxel.RIGHT;
                    }
                    if (y > 0 && voxelArray[x, y - 1, z] != null)
                    {
                        voxelArray[x, y, z].sides ^= RenderVoxel.BACK;
                        voxelArray[x, y - 1, z].sides ^= RenderVoxel.FRONT;
                    }
                    if (z > 0 && voxelArray[x, y, z - 1] != null)
                    {
                        voxelArray[x, y, z].sides ^= RenderVoxel.DOWN;
                        voxelArray[x, y, z - 1].sides ^= RenderVoxel.UP;
                    }
                }
            }
        }


        List<int> colors;
        Mesh mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<int> colorOrder = new List<int>();
        colors = new List<int>();
        for (int _z = 0; _z < voxelArray.GetLength(2); _z++)
        {
            for (int _y = 0; _y < voxelArray.GetLength(1); _y++)
            {
                for (int _x = 0; _x < voxelArray.GetLength(0); _x++)
                {
                    RenderVoxel renderVoxel = voxelArray[_x, _y, _z];

                    if (renderVoxel == null || renderVoxel.sides == 0)
                    {
                        continue;
                    }

                    float x = _x - shifts.x;
                    float y = _y - shifts.z;
                    float z = _z - shifts.y;

                    if (!colorOrder.Contains(renderVoxel.color))
                    {
                        colorOrder.Add(renderVoxel.color);
                    }

                    if ((renderVoxel.sides & RenderVoxel.DOWN) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3(x * size, z * size, y * size));
                        vertices.Add(new Vector3((x + 1) * size, z * size, y * size));
                        vertices.Add(new Vector3(x * size, z * size, (y + 1) * size));
                        vertices.Add(new Vector3((x + 1) * size, z * size, (y + 1) * size));
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex + 2);

                    }
                    if ((renderVoxel.sides & RenderVoxel.UP) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3(x * size, (z + 1) * size, y * size));
                        vertices.Add(new Vector3((x + 1) * size, (z + 1) * size, y * size));
                        vertices.Add(new Vector3(x * size, (z + 1) * size, (y + 1) * size));
                        vertices.Add(new Vector3((x + 1) * size, (z + 1) * size, (y + 1) * size));
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex + 1);
                    }
                    if ((renderVoxel.sides & RenderVoxel.LEFT) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3(x * size, z * size, y * size));
                        vertices.Add(new Vector3(x * size, z * size, (y + 1) * size));
                        vertices.Add(new Vector3(x * size, (z + 1) * size, (y + 1) * size));
                        vertices.Add(new Vector3(x * size, (z + 1) * size, y * size));
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex);
                    }

                    if ((renderVoxel.sides & RenderVoxel.RIGHT) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3((x + 1) * size, z * size, y * size));
                        vertices.Add(new Vector3((x + 1) * size, z * size, (y + 1) * size));
                        vertices.Add(new Vector3((x + 1) * size, (z + 1) * size, (y + 1) * size));
                        vertices.Add(new Vector3((x + 1) * size, (z + 1) * size, y * size));
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex);
                    }
                    if ((renderVoxel.sides & RenderVoxel.BACK) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3(x * size, z * size, y * size));
                        vertices.Add(new Vector3(x * size, (z + 1) * size, y * size));
                        vertices.Add(new Vector3((x + 1) * size, (z + 1) * size, y * size));
                        vertices.Add(new Vector3((x + 1) * size, z * size, y * size));
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex);
                    }
                    if ((renderVoxel.sides & RenderVoxel.FRONT) != 0)
                    {
                        int startIndex = vertices.Count;

                        vertices.Add(new Vector3(x * size, z * size, (y + 1) * size));
                        vertices.Add(new Vector3(x * size, (z + 1) * size, (y + 1) * size));
                        vertices.Add(new Vector3((x + 1) * size, (z + 1) * size, (y + 1) * size));
                        vertices.Add(new Vector3((x + 1) * size, z * size, (y + 1) * size));
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);
                        colors.Add(renderVoxel.color);

                        triangles.Add(startIndex);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex);
                    }


                }
            }
        }

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        MeshAndColors meshAndColors = new MeshAndColors();
        meshAndColors.mesh = mesh;
        meshAndColors.colors = colors;

        return meshAndColors;
    }


}
