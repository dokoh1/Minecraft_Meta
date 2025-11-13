using UnityEngine;

/// <summary>
/// 한 청크(16x256x16)의 복셀 상태를 담는 순수 데이터 컨테이너
/// </summary>
[System.Serializable]
public class ChunkData
{
    private int _x;
    private int _y;
    public ChunkData(Vector2Int position) {Position = position;}
    public ChunkData(int x, int y) {_x = x;_y = y;}
    public BlockState[,,] Map = new BlockState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkDepth];

    //청크의 월드 좌표 원점
    public Vector2Int Position
    {
        get { return new Vector2Int(_x, _y); }
        set
        {
            _x = value.x;
            _y = value.y;
        }
    }
    
    /*1.삼중 루프(y→x→z)로 청크 전 범위 순회.
    2.월드 보셀 좌표 = (x + Position.x, y, z + Position.y).
    3.MinecraftTerrain.Instance.TerrainCondition(worldPos)로 그 위치의 블록 타입 결정(지형 높이, 바이옴, 광맥/나무 스폰 등).
    4.Map[x,y,z] = new BlockState(그 타입).
    5.끝나면 worldData.AddToModifiedChunks(this)로 “저장 대상” 표시. */
    public void ChunkTypeSetting()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkDepth; z++)
                {
                    Map[x, y, z] =
                        new BlockState(
                            MinecraftTerrain.Instance.TerrainCondition(new Vector3(x + Position.x, y, z + Position.y)));
                }
            }
        }
    
        MinecraftTerrain.Instance.worldData.AddToModifiedChunks(this);
    }
}
