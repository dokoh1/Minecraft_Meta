using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkDepth = 16;
    public static readonly int ChunkHeight = 256;

    public static readonly int PlayerInitHeight = 70;

    public static readonly int TextureAtlasSize = 16;
    public static readonly int TerrainSize = 100;

    //Light Value
    public static readonly float MinLight = 0.1f;
    public static readonly float MaxLight = 0.9f;
    public static readonly float LightFalloff = 0.1f;

    public static readonly int Seed = 0;
    
    public static int TerrainInVoxelSize
    {
        get { return TerrainSize * ChunkWidth; }
    }

    public static int TerrainMiddle
    {
        get { return (TerrainSize * ChunkWidth / 2); }
    }
    
    public static float NormalizedBlockTextureSize
    {
        get {return (1f / TextureAtlasSize); }
    }
    
    public static readonly Vector3[] VoxelVertes =
    {
        new(0.0f, 0.0f, 0.0f),
        new(1.0f, 0.0f, 0.0f),
        new(1.0f, 1.0f, 0.0f),
        new(0.0f, 1.0f, 0.0f),
        new(0.0f, 0.0f, 1.0f),
        new(1.0f, 0.0f, 1.0f),
        new(1.0f, 1.0f, 1.0f),
        new(0.0f, 1.0f, 1.0f),
    };

    public static readonly Vector3[] FaceChecks =
    {
        new(0.0f, 0.0f, -1.0f),
        new(0.0f, 0.0f, 1.0f),
        new(0.0f, 1.0f, 0.0f),
        new(0.0f, -1.0f, 0.0f),
        new(-1.0f, 0.0f, 0.0f),
        new(1.0f, 0.0f, 0.0f),
    };

    public static readonly int[,] VoxelIndex =
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
}
