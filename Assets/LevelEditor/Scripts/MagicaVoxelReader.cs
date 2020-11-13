using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// After 1h since start of the project I have the default parser and chunk and can read vox properly
// +30m Size Parser and Voxel parser. Got voxel colors and coordinates now.
// +15m Color palettes are read into unity


public class MagicaVoxelReader
{ 
    public static Chunk ReadMagicaChunks(string fileName)
    {
        BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open));

        char[] vox = reader.ReadChars(4);
        int version = reader.ReadInt32();

        Chunk main = Chunk.createChunk(reader);
        Dictionary<int, Chunk> chunksById = new Dictionary<int, Chunk>();       
        int voxelChunkCounter = 0;
        Dictionary<int, VoxelModelChunk> voxelNodesById = new Dictionary<int, VoxelModelChunk>();


        for (int i = 0; i < main.children.Count; i++)
        {
            Chunk chunk = main.children[i];                   
            if (chunk is ShapeNodeChunk)
            {
                chunksById.Add(((ShapeNodeChunk)chunk).nodeId, chunk);
            }
            else if (chunk is GroupNodeChunk)
            {
                chunksById.Add(((GroupNodeChunk)chunk).nodeId, chunk);
            }
            else if (chunk is TransformNodeChunk)
            {
                chunksById.Add(((TransformNodeChunk)chunk).nodeId, chunk);
            }
            else if (chunk is VoxelModelChunk)
            {
                ((VoxelModelChunk)chunk).nodeId = voxelChunkCounter++;
                voxelNodesById.Add(((VoxelModelChunk)chunk).nodeId, (VoxelModelChunk)chunk);
            }
        }

        for (int i = 0; i < main.children.Count; i++)
        {
            Chunk chunk = main.children[i];
            if (chunk is ShapeNodeChunk)
            {
                foreach(MagicaModel model in ((ShapeNodeChunk)chunk).childModels)
                {                    
                    ((VoxelModelChunk) voxelNodesById[model.modelId]).shapes.Add((ShapeNodeChunk) chunk);
                }
            }
            else if (chunk is TransformNodeChunk)
            {
                if (chunksById[((TransformNodeChunk)chunk).childNodeId] is ShapeNodeChunk)
                {
                    ((ShapeNodeChunk)chunksById[((TransformNodeChunk)chunk).childNodeId]).transform = (TransformNodeChunk)chunk;
                }else if (chunksById[((TransformNodeChunk)chunk).childNodeId] is GroupNodeChunk)
                {
                    ((GroupNodeChunk)chunksById[((TransformNodeChunk)chunk).childNodeId]).transform = (TransformNodeChunk)chunk;
                }                
            }
            else if (chunk is GroupNodeChunk)
            {
                foreach (int childNodeId in ((GroupNodeChunk)chunk).childNodeIds)
                {
                    ((TransformNodeChunk)chunksById[childNodeId]).group = (GroupNodeChunk)chunk;
                }
            }
        }

        return main;
    }
}

public class MagicaString
{
    public static string readString(BinaryReader reader)
    {
        int byteLength = reader.ReadInt32();
        return new string(reader.ReadChars(byteLength));
    }
}

public class MagicaDictionary
{
    public static Dictionary<string, string> readDictionary(BinaryReader reader)
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();

        uint numberOfEntries = reader.ReadUInt32();
        for (uint i = 0; i < numberOfEntries; i++)
        {
            string key = MagicaString.readString(reader);
            string value = MagicaString.readString(reader);
            dictionary.Add(key, value);            
        }

        return dictionary;
    }
}


public abstract class ChunkParser
{
    public abstract Chunk parse(BinaryReader reader);    
}

public class DefaultChunkParser : ChunkParser
{
    public override Chunk parse(BinaryReader reader)
    {
        Chunk chunk = new Chunk();
        chunk.id = "Default";
        chunk.bytesInChunk = reader.ReadInt32();
        chunk.bytesInChildren = reader.ReadInt32();
        chunk.content = reader.ReadBytes(chunk.bytesInChunk);

        long currentPosition = reader.BaseStream.Position;
        while (reader.BaseStream.Position < currentPosition + chunk.bytesInChildren)
        {
            Chunk childChunk = Chunk.createChunk(reader);
            if(childChunk is VoxelModelChunk)
            {
                ((VoxelModelChunk)childChunk).sizeChunk = (SizeChunk) chunk.children[chunk.children.Count - 1];
            }
            chunk.children.Add(childChunk);
        }        

        return chunk;
    }
}

public class Chunk
{
    static Dictionary<string, ChunkParser> chunkParser = new Dictionary<string, ChunkParser>
    {
        { "SIZE" , new SizeChunkParser() },
        { "XYZI" , new VoxelChunkParser() },
        { "RGBA" , new PaletteParser() },
        { "nTRN" , new TransformNodeParser() },
        { "nGRP" , new GroupNodeParser() },
        { "nSHP" , new ShapeNodeParser() },
    };

    public static Chunk createChunk(BinaryReader reader)
    {        
        string id = new string( reader.ReadChars(4));
        ChunkParser parser;
        if (chunkParser.Keys.Contains(id))
        {
             parser = chunkParser[id];
        }
        else
        {
            parser = new DefaultChunkParser();
        }

        Chunk chunk = parser.parse(reader);
        chunk.id = id;   

        return chunk;
    }

    public string id;
    public int bytesInChunk;
    public int bytesInChildren;
    public byte[] content;
    public List<Chunk> children = new List<Chunk>();    

    override public string ToString()
    {
        return "ID: " + id + "; ContentLength: " + bytesInChunk + "; Children: " + children.Count;
    }
}

public class SizeChunkParser : ChunkParser
{
    public override Chunk parse(BinaryReader reader)
    {
        SizeChunk chunk = new SizeChunk();        
        chunk.bytesInChunk = reader.ReadInt32();
        chunk.bytesInChildren = reader.ReadInt32();

        chunk.sizeX = reader.ReadInt32();
        chunk.sizeY = reader.ReadInt32();
        chunk.sizeZ = reader.ReadInt32();

        long currentPosition = reader.BaseStream.Position;
        while (reader.BaseStream.Position < currentPosition + chunk.bytesInChildren)
        {
            chunk.children.Add(Chunk.createChunk(reader));
        }

        return chunk;
    }
}

public class VoxelChunkParser : ChunkParser
{
    public override Chunk parse(BinaryReader reader)
    {
        VoxelModelChunk chunk = new VoxelModelChunk();
        chunk.bytesInChunk = reader.ReadInt32();
        chunk.bytesInChildren = reader.ReadInt32();

        chunk.numberOfVoxels = reader.ReadInt32();
        for (long i = 0; i < chunk.numberOfVoxels; i++)
        {
            Voxel voxel = new Voxel();
            voxel.x = reader.ReadByte();
            voxel.y = reader.ReadByte();
            voxel.z = reader.ReadByte();
            voxel.colorIndex = reader.ReadByte();            
            chunk.voxels.Add(voxel);
        }

        long currentPosition = reader.BaseStream.Position;
        while (reader.BaseStream.Position < currentPosition + chunk.bytesInChildren)
        {
            chunk.children.Add(Chunk.createChunk(reader));
        }

        return chunk;
    }
}

public class SizeChunk : Chunk
{
    public int sizeX;
    public int sizeY;
    public int sizeZ; // gravity direction in magica

    override public string ToString()
    {
        return "ID: SIZE; x.y.z: (" + sizeX +", " + sizeY + ", " + sizeZ + "); Children: " + children.Count;
    }
}

public class Voxel
{
    public int x;
    public int y;
    public int z; // gravity direction in magica
    public int colorIndex;

    override public string ToString()
    {
        return "Voxel color: " + colorIndex + "; x.y.z: (" + x + ", " + y + ", " + z + ")";
    }
}

public class VoxelModelChunk : Chunk
{
    public int nodeId = -1;
    public SizeChunk sizeChunk;
    public int numberOfVoxels;
    public List<Voxel> voxels = new List<Voxel>();
    public List<ShapeNodeChunk> shapes = new List<ShapeNodeChunk>();

    override public string ToString()
    {
        return "ID: XYZI; number of voxels: " + numberOfVoxels;
    }
}

public class PaletteChunk : Chunk
{
    public Color[] colors = new Color[256];
    override public string ToString()
    {
        return "ID: RGBA; Palette read";
    }
}

public class PaletteParser : ChunkParser
{
    public override Chunk parse(BinaryReader reader)
    {
        PaletteChunk chunk = new PaletteChunk();
        chunk.bytesInChunk = reader.ReadInt32();
        chunk.bytesInChildren = reader.ReadInt32();

        for (long i = 0; i < 256; i++)
        {
            float r = reader.ReadByte() / 255f;
            float g = reader.ReadByte() / 255f;
            float b = reader.ReadByte() / 255f;
            float a = reader.ReadByte() / 255f;

            chunk.colors[i] = new Color(r,g,b,a);
        }

        long currentPosition = reader.BaseStream.Position;
        while (reader.BaseStream.Position < currentPosition + chunk.bytesInChildren)
        {
            chunk.children.Add(Chunk.createChunk(reader));
        }

        return chunk;
    }
}

public class TransformNodeChunk : Chunk
{
    public int nodeId;
    public Dictionary<string, string> attributes;
    public int childNodeId;
    public int reservedId = -1;
    public int layerId;
    public int numberOfFrames = 1;
    public List<Dictionary<string, string>> frameAttributes = new List<Dictionary<string, string>>();
    public GroupNodeChunk group;

    override public string ToString()
    {        
        return "ID: nTRN; "+ attributes.Count; 
    }
}

public class TransformNodeParser : ChunkParser
{
    public override Chunk parse(BinaryReader reader)
    {
        TransformNodeChunk chunk = new TransformNodeChunk();
        chunk.bytesInChunk = reader.ReadInt32();
        chunk.bytesInChildren = reader.ReadInt32();
        chunk.nodeId = reader.ReadInt32();
        chunk.attributes = MagicaDictionary.readDictionary(reader);           
        chunk.childNodeId = reader.ReadInt32();
        chunk.reservedId = reader.ReadInt32();
        chunk.layerId = reader.ReadInt32();
        chunk.numberOfFrames = reader.ReadInt32();

        for(int i=0; i<chunk.numberOfFrames; i++)
        {
            chunk.frameAttributes.Add(MagicaDictionary.readDictionary(reader));
        }

        long currentPosition = reader.BaseStream.Position;
        while (reader.BaseStream.Position < currentPosition + chunk.bytesInChildren)
        {
            chunk.children.Add(Chunk.createChunk(reader));
        }

        return chunk;
    }
}

public class GroupNodeChunk: Chunk
{
    public int nodeId;
    public Dictionary<string, string> attributes = new Dictionary<string, string>();
    public List<int> childNodeIds = new List<int>();    
    public TransformNodeChunk transform;

    override public string ToString()
    {
        return "ID: nGRP "+nodeId+"; Children: "+childNodeIds.Count;
    }
}

public class GroupNodeParser : ChunkParser
{
    public override Chunk parse(BinaryReader reader)
    {
        GroupNodeChunk chunk = new GroupNodeChunk();
        chunk.bytesInChunk = reader.ReadInt32();
        chunk.bytesInChildren = reader.ReadInt32();
        chunk.nodeId = reader.ReadInt32();
        chunk.attributes = MagicaDictionary.readDictionary(reader);
        
        int numberOfChildNodes = reader.ReadInt32();
        for(int i=0; i<numberOfChildNodes; i++)
        {
            chunk.childNodeIds.Add(reader.ReadInt32());
        }
        
        long currentPosition = reader.BaseStream.Position;
        while (reader.BaseStream.Position < currentPosition + chunk.bytesInChildren)
        {
            chunk.children.Add(Chunk.createChunk(reader));
        }

        return chunk;
    }
}

public class MagicaModel
{
    public int modelId;
    public Dictionary<string, string> attributes;
}

public class ShapeNodeChunk : Chunk
{
    public int nodeId;
    public Dictionary<string, string> attributes = new Dictionary<string, string>();
    public List<MagicaModel> childModels = new List<MagicaModel>();    
    public TransformNodeChunk transform;

    override public string ToString()
    {
        return "ID: nSHP " + nodeId + "; Children: " + childModels.Count;
    }
}

public class ShapeNodeParser : ChunkParser
{
    public override Chunk parse(BinaryReader reader)
    {
        ShapeNodeChunk chunk = new ShapeNodeChunk();
        chunk.bytesInChunk = reader.ReadInt32();
        chunk.bytesInChildren = reader.ReadInt32();
        chunk.nodeId = reader.ReadInt32();
        chunk.attributes = MagicaDictionary.readDictionary(reader);

        int numberOfChildNodes = reader.ReadInt32();
        for (int i = 0; i < numberOfChildNodes; i++)
        {
            MagicaModel model = new MagicaModel();
            model.modelId = reader.ReadInt32();
            model.attributes = MagicaDictionary.readDictionary(reader);
            chunk.childModels.Add(model);
        }

        long currentPosition = reader.BaseStream.Position;
        while (reader.BaseStream.Position < currentPosition + chunk.bytesInChildren)
        {
            chunk.children.Add(Chunk.createChunk(reader));
        }

        return chunk;
    }
}