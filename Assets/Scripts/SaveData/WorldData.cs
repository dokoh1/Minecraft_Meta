using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 영속 메타: worldName, seed.
/// 런타임 상태: 메모리에 로드된 청크들(Chunks)과 저장 필요한 청크들(modifiedChunks).
/// 좌표 → 청크/보셀 매핑: 월드 보셀 좌표를 청크 원점/로컬 보셀로 바꿔 접근(Set/Get).
/// 로드 파이프라인: 요청한 청크가 없으면 디스크에서 로드하거나 새로 생성.
/// </summary>

[System.Serializable]
public class WorldData
{
    //월드 이름
    public string worldName = "Prototype";
    //시드
    public int seed;
    
    // 메모리에 로드된 청크들. 키는 "청크 원점의 월드 복셀 좌표"(x,z가 ChunkWidth/Detph의 배수);
    [System.NonSerialized]
    public Dictionary<Vector2Int, ChunkData> Chunks = new();
    
    // 편집 등으로 저장 대상이 된 청크 목록.
    [System.NonSerialized]
    public List<ChunkData> modifiedChunks = new();
    
    // 이름/시드로 새 월드 만들거나, 다른 worldData에서 복사.
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
    /// <summary>
    /// 같은 청크가 중복되지 않게 저장 목록에 등록
    /// </summary>
    /// <param name="chunk"></param>
    public void AddToModifiedChunks(ChunkData chunk)
    {
        if (!modifiedChunks.Contains(chunk))
            modifiedChunks.Add(chunk);
    }
    
    /// <summary>
    /// 1. 이미 로드됨 -> 그대로 반환
    /// 2. 없고 create==false -> null
    /// 3. 없고 create==true -> LoadChunk(coord) 호출 후 반환
    /// </summary>
    /// <param name="coord"></param>
    /// <param name="create"></param>
    /// <returns></returns>
    public ChunkData RequestChunk(Vector2Int coord, bool create)
    {
        ChunkData c;
            if (Chunks.ContainsKey(coord))
                c =  Chunks[coord];
            
            else if (!create)
                c = null;
            
            else
            {
                LoadChunk(coord);
                c = Chunks[coord];
            }

        return c;
    }

    /// <summary>
    /// 1. 이미 있으면 종료
    /// 2. SaveSystem.LoadChunk(worldName, coord) 시도
    /// - 성공 -> 그대로 등록
    /// - 실패 -> new ChunkData(coord)로 생성 후 ChunkData.ChunkTypeSetting() (절차적 생성)
    /// </summary>
    /// <param name="coord"></param>
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

    /// <summary>
    /// 월드 복셀 좌표의 블록 타입을 바꾸고 저장 대상으로 표시.
    /// 1. 범위 밖이면 return
    /// 2. x = floor(pos.x/ChunkW) * ChunkW, z = floor(pos.z/ChunkD) * ChunkD
    /// -> 해당 복셀이 속한 청크의 원점(월드 복셀 좌표)
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="value"></param>
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
