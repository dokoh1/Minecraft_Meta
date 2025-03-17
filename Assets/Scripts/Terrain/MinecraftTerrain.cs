using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class MinecraftTerrain : MonoBehaviour
{
    // private Vector3 SpawnPosition;
    public BlockData blockData;
    // public Transform player;
    
    private Chunk[,] _chunks = new Chunk[VoxelData.TerrainSize, VoxelData.TerrainSize];
    // private Vector3 _spawnPosition;
    
    private void Awake()
    {
        // _spawnPosition = new Vector3
        //     ((VoxelData.TerrainSize * VoxelData.ChunkWidth) / 2, 
        //     VoxelData.ChunkHeight + 2,
        //     (VoxelData.TerrainSize * VoxelData.ChunkDepth) / 2);
        // player.transform.position = _spawnPosition;
        
        GenerateWorld();
    }
    
    // private void Update()
    // {
    //     GenerateChunkAroundPlayer();
    // }

    public BlockTypeEnum BlockCondition(Vector3 pos)
    {
        if (IsVoxelInTerrain(pos))
            return BlockTypeEnum.Air;
        if (pos.y == VoxelData.ChunkHeight - 1)
            return BlockTypeEnum.Grass;
        else
            return BlockTypeEnum.Stone;
    }
    
    // void GenerateWorld()
    // {
    //     for (int x = (VoxelData.TerrainSize / 2) - VoxelData.ViewDistance; x < (VoxelData.TerrainSize / 2) + VoxelData.ViewDistance; x++)
    //     {
    //         for (int z = (VoxelData.TerrainSize / 2) - VoxelData.ViewDistance; z < (VoxelData.TerrainSize / 2) + VoxelData.ViewDistance; z++)
    //         {
    //             CreateNewChunk(x, z);
    //         }
    //     }
    // }
    
    void GenerateWorld()
    {
        for (int x = 0; x < VoxelData.TerrainSize; x++)
        {
            for (int z = 0; z < VoxelData.TerrainSize; z++)
            {
                CreateNewChunk(x, z);
            }
        }
    }

    Coord Vector3ToCoord(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x) / VoxelData.ChunkWidth;
        int z = Mathf.FloorToInt(pos.z) / VoxelData.ChunkDepth;

        return new Coord(x, z);
    }
    
    // void GenerateChunkAroundPlayer()
    // {
    //     Coord playerPos = Vector3ToCoord(player.transform.position);
    //     for (int x = playerPos.X - VoxelData.ViewDistance; x < playerPos.X + VoxelData.ViewDistance; x++)
    //     {
    //         for (int z = playerPos.Z - VoxelData.ViewDistance; z < playerPos.Z + VoxelData.ViewDistance; z++)
    //         {
    //             if (IsChunkInWorld(x, z))
    //             {
    //                 if (_chunks[x, z] == null)
    //                 {
    //                     CreateNewChunk(x, z);
    //                 }
    //             }
    //         }
    //     }
    // }
    
    void CreateNewChunk(int x, int z)
    {
        Coord coord = new Coord(x, z);
        _chunks[x, z] = new Chunk(coord, blockData, this);
    }
    
    bool IsChunkInWorld(int x, int z) 
    {
        if (x > 0 && x < VoxelData.TerrainSize - 1 && z > 0 && z < VoxelData.TerrainSize - 1)
            return true;
        else
            return false;
    }

    public bool IsVoxelInTerrain(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.TerrainInVoxelSize && 
            pos.y < VoxelData.ChunkHeight && pos.y >= 0 &&
            pos.z >= 0 && pos.z < VoxelData.TerrainInVoxelSize)
        {
            return false;
        }
        else
            return true;
    }
}
