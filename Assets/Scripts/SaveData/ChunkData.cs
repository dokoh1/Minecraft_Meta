using Unity.Mathematics;
using UnityEngine;
[System.Serializable]
public class ChunkData
{
    private int _x;
    private int _y;
    public ChunkData(Vector2Int position) {Position = position;}
    public ChunkData(int x, int y) {_x = x;_y = y;}
    public BlockState[,,] Map = new BlockState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkDepth];

    public Vector2Int Position
    {
        get { return new Vector2Int(_x, _y); }
        set
        {
            _x = value.x;
            _y = value.y;
        }
    }

    //청크 생성
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
