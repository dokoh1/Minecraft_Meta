using UnityEngine;
/// <summary>
///  월드 규격과 정육면체 한 칸의 기하 정보를 한데 모아둔 상수/테이블 집합
/// </summary>
public static class VoxelData
{
    // 청크의 Voxel 그리드 크기
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkDepth = 16;
    public static readonly int ChunkHeight = 256;
    
    // 초기 플레이어 Y 높이
    public static readonly int PlayerInitHeight = 70;

    // 아틀라스 사이즈 16X16 타일
    public static readonly int TextureAtlasSize = 16;
    // 100X100 Chunk
    public static readonly int TerrainSize = 100;

    //Light Value
    // 셰이더의 전역 광량 범위를 고정
    public static readonly float MinLight = 0.1f;
    public static readonly float MaxLight = 0.9f;
    public static readonly float LightFalloff = 0.1f;
    
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
