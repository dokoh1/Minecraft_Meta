using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkDepth = 16;
    public static readonly int ChunkHeight = 256;

    public static readonly int TextureAtlasSize = 16;
    public static readonly int TerrainSize = 1000;
    public static readonly int InitSize = 10;

    public static int InitInVoxelSize
    {
        get { return InitSize * ChunkWidth; }
    }

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
    private int _x;
    private int _z;

    public int X_int { get { return _x; } set { _x = value; } }
    public int Z_int { get { return _z; } set { _z = value; } }
    public float X
    {
        get => _x;
        set => _x = Mathf.FloorToInt(value);
    }

    public float Z
    {
        get => _z;
        set => _z = Mathf.FloorToInt(value);
    }
    public Coord(int x, int z)
    {
        X = x;
        _z = z;
    }

    public Coord()
    {
        X = 0;
        _z = 0;
    }

    public Coord(Vector3 pos)
    {
        X = Mathf.FloorToInt(pos.x) / VoxelData.ChunkWidth;
        _z = Mathf.FloorToInt(pos.z) / VoxelData.ChunkDepth;
    }

    public bool Equals(Coord other)
    {
        if (other == null)
            return false;
        else if (other.X == X && other._z == _z)
            return true;
        else
            return false;
    }
}