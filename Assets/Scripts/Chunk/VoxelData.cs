using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkDepth = 16;
    public static readonly int ChunkHeight = 256;

    public static readonly int TextureAtlasSize = 16;
    public static readonly int TerrainSize = 100;

    public static readonly int ViewDistance = 5;
    public static int TerrainInVoxelSize
    {
        get { return TerrainSize * ChunkWidth; }
    }
    

    public static float NormalizedBlockTextureSize
    {
        get {return 1f / TextureAtlasSize; }
    }
    
    public static readonly Vector3[] VoxelVertes = new Vector3[]
    {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
    };

    public static readonly Vector3[] FaceChecks = new Vector3[6]
    {
        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
    };

    public static readonly int[,] VoxelIndex = new int[6, 4]
    {
        //앞면
        {0, 3, 1, 2},
        //뒷면
        {5, 6, 4, 7},
        //윗면
        {3, 7, 2, 6},
        //아랫면
        {1, 5, 0, 4},
        //왼쪽면
        {4, 7, 0, 3},
        //오른쪽면
        {1, 2, 5, 6},
    };
    public static readonly Vector2[] VoxelUv = new Vector2[4]
    {
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f),
    };
}

public class Coord
{
    public int X;
    public int Z;

    public Coord(int x, int z)
    {
        X = x;
        Z = z;
    }

    public bool Equals(Coord other)
    {
        if (other == null)
            return false;
        else if (other.X == X && other.Z == Z)
            return true;
        else
            return false;
    }
}