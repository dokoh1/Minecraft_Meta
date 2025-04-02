using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldData
{
    //월드 이름
    public string worldName = "Prototype";
    //시드
    public int seed;
    
    //월드에 로드된 청크 저장
    [System.NonSerialized]
    public Dictionary<Vector2Int, ChunkData> Chunks = new();
    
    //변경된 청크
    [System.NonSerialized]
    public List<ChunkData> modifiedChunks = new();
    
    //변경된 청크 추가
    
    public WorldData(string _worldName, int _seed)
    {
        worldName = _worldName;
        seed = _seed;
        
    }
    
    public WorldData(WorldData WorldData)
    {
        worldName = WorldData.worldName;
        seed = WorldData.seed;
    }
    public void AddToModifiedChunks(ChunkData chunk)
    {
        if (!modifiedChunks.Contains(chunk))
            modifiedChunks.Add(chunk);
    }
    
    //청크 요청 없으면 생성
    public ChunkData RequestChunk(Vector2Int coord, bool create)
    {
        ChunkData c;
        lock (MinecraftTerrain.Instance.ChunkListThreadLock)
        {
            if (Chunks.ContainsKey(coord))
                c =  Chunks[coord];
            
            else if (!create)
                c = null;
            
            else
            {
                LoadChunk(coord);
                c = Chunks[coord];
            }
        }

        return c;
    }

    //청크 로드 없으면 생성
    public void LoadChunk(Vector2Int coord)
    {
        if (Chunks.ContainsKey(coord))
            return;
        ChunkData chunk = SaveSystem.LoadChunk(worldName, coord);
        if (chunk != null)
        {
            Chunks.Add(coord, chunk);
            return;
        }
        Chunks.Add(coord, new ChunkData(coord));
        Chunks[coord].ChunkTypeSetting();
    }
    
    // 해당 좌표가 Terrain 범위인지 확인
    public bool IsVoxelInTerrain(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.TerrainInVoxelSize && 
            pos.y < VoxelData.ChunkHeight && pos.y >= 0 &&
            pos.z >= 0 && pos.z < VoxelData.TerrainInVoxelSize)
            return true;
        return false;
    }

    //특정 좌표의 블록을 설정하고 변경된 청크 목록 추가
    public void SetVoxel(Vector3 pos, BlockTypeEnum value)
    {
        if (!IsVoxelInTerrain(pos))
            return;
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkDepth);
        
        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkDepth;
        
        ChunkData chunk = RequestChunk(new Vector2Int(x,z), true);
        
        Vector3Int voxel = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        chunk.Map[voxel.x, voxel.y, voxel.z].BlockType = value;
        AddToModifiedChunks(chunk);
        

    }
    
    //특정 좌표의 블록 데이터를 가져옴
    public BlockState GetVoxel (Vector3 pos)
    {
        if (!IsVoxelInTerrain(pos))
            return null;
        
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkDepth);
        
        x *= VoxelData.ChunkWidth;
        z *= VoxelData.ChunkDepth;
        
        ChunkData chunk = RequestChunk(new Vector2Int(x,z), true);
        
        Vector3Int voxel = new Vector3Int((int)(pos.x - x), (int)pos.y, (int)(pos.z - z));

        return chunk.Map[voxel.x, voxel.y, voxel.z];
    }
}
