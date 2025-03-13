using UnityEngine;
// Data Only Read and Value Not Changed
public static class VoxelData
{
    public static readonly int chunkWidth = 16;
    public static readonly int chunkDepth = 16;
    public static readonly int chunkHeight = 16;
    
    //Voxel Vertex
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
    //Voxel Index
    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
    };
    
    // public static readonly int[,] VoxelIndex = new int[6, 6]
    // {
    //     // 윗면
    //     {3, 7, 2, 2, 7, 6},
    //     // 뒷면
    //     {5, 6, 4, 4, 6, 7},
    //     //왼쪽면
    //     {4, 7, 0, 0, 7, 3},
    //     // 오른쪽면
    //     {1, 2, 5, 5, 2, 6},
    //     // 앞면
    //     {0, 3, 1, 1, 3, 2},
    //     // 아랫면
    //     {0, 1, 4, 4, 1, 5}
    // };

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

