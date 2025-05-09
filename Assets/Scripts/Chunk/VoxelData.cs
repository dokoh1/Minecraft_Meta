using UnityEngine;

public static class VoxelData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkDepth = 16;
    public static readonly int ChunkHeight = 256;

    public static readonly int TextureAtlasSize = 16;
    public static readonly int TerrainSize = 100;

    //Light Value
    public static float minLight = 0.1f;
    public static float maxLight = 0.9f;
    public static float lightFalloff = 0.1f;

    public static int seed = 0;
    
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
        get {return 1f / TextureAtlasSize; }
    }
    
    public static readonly Vector3[] VoxelVertes = new Vector3[8]
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
