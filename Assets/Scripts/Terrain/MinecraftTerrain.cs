using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MinecraftTerrain : MonoBehaviour
{
    public BlockData blockData;
    public BiomeData biomeData;
    public Transform player;
    //seed 값
    public int seed;
    //캐릭터 spawn 포지션
    private Vector3 _spawnPosition;
    // 청크 생성
    private Chunk[,] _chunks = new Chunk[VoxelData.TerrainSize, VoxelData.TerrainSize];
    // 이전 프레임과 이후 프레임의 Coord를 비교하여 Active를 설정하기 위한 List
    private List<Coord> activeChunks = new List<Coord>();

    private List<Coord> previousActiveChunk;
    //프레임 전 후 player의 Coord(x, z) 위치
    private Coord _playerPreviousCoord;
    public Coord _playerCoord;
    
    List<Coord> chunksToCreate = new List<Coord>();
    private bool isCreating;
    public GameObject debugUI;
    
    private void Start()
    {
        Random.InitState(seed);
        previousActiveChunk = new List<Coord>();
        _spawnPosition = new Vector3
            ((VoxelData.TerrainSize * VoxelData.ChunkWidth) / 2, 
            VoxelData.ChunkHeight - 150,
            (VoxelData.TerrainSize * VoxelData.ChunkDepth) / 2);
        player.transform.position = _spawnPosition;
        
        GenerateWorld();
        _playerCoord = Vector3ToCoord(player.position);
        _playerPreviousCoord = _playerCoord;
    }
    
    private void Update()
    {
        _playerCoord = Vector3ToCoord(player.position);
        if (!_playerCoord.Equals(_playerPreviousCoord))
            GenerateChunkAroundPlayer();
        
        if (chunksToCreate.Count > 0 && !isCreating)
            StartCoroutine(CreateChunks());
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugUI.SetActive(!debugUI.activeSelf);
        }
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.TerrainSize / 2) - VoxelData.InitSize; x < (VoxelData.TerrainSize / 2) + VoxelData.InitSize; x++)
        {
            for (int z = (VoxelData.TerrainSize / 2) - VoxelData.InitSize; z < (VoxelData.TerrainSize / 2) + VoxelData.InitSize; z++)
            {
                _chunks[x, z] = new Chunk(new Coord(x, z),this.blockData, this, true);
                activeChunks.Add(new Coord(x, z));
            }
        }
        // for (int x = 0; x < VoxelData.InitSize; x++)
        // {
        //     for (int z = 0; z < VoxelData.InitSize; z++)
        //     {
        //         _chunks[x, z] = new Chunk(new Coord(x, z),this.blockData, this, true);
        //         activeChunks.Add(new Coord(x, z));
        //     }
        // }
    }
    void GenerateChunkAroundPlayer()
    {
        Coord playerPos = Vector3ToCoord(player.transform.position);
        // previousActiveChunk = activeChunks;
        previousActiveChunk.Clear();
        previousActiveChunk.AddRange(activeChunks);
        activeChunks.Clear();
        for (int x = playerPos.X_int - VoxelData.ViewDistance; x < playerPos.X_int + VoxelData.ViewDistance; x++)
        {
            for (int z = playerPos.Z_int - VoxelData.ViewDistance; z < playerPos.Z_int + VoxelData.ViewDistance; z++)
            {
                Coord ChunkCoord = new Coord(x, z);
                if (IsChunkInWorld(x, z))
                {
                    if (_chunks[x, z] == null)
                    {
                        // Debug.Log($"[청크 생성] ({x}, {z}) 청크를 새로 생성합니다.");
                        _chunks[x, z] = new Chunk(ChunkCoord, this.blockData, this, false);
                        chunksToCreate.Add(new Coord(x, z));
                    }
                    else if (!_chunks[x, z].isActive)
                    {
                        _chunks[x, z].isActive = true;
                    }
                    activeChunks.Add(new Coord(x, z));
                }
                // 현프레임에서 ActiveChunk와 비교해서 그대로 있다면 해당 청크는 계속 유지
                for (int i = 0; i < previousActiveChunk.Count; i++)
                {
                    if (previousActiveChunk[i].Equals(ChunkCoord))
                        previousActiveChunk.RemoveAt(i);
                }
            }
        }
        // 현 프레임에서 전 프레임에서 비교했을때 ActiveChunk가 아닌 것들을 false를 한다.
        foreach (Coord c in previousActiveChunk)
            _chunks[c.X_int, c.Z_int].isActive = false;
    }

    IEnumerator CreateChunks()
    {
        isCreating = true;
        while (chunksToCreate.Count > 0)
        {
            _chunks[chunksToCreate[0].X_int, chunksToCreate[0].Z_int].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;

        }

        isCreating = false;
    }
    public BlockTypeEnum TerrainCondition(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        var hillsBiome = biomeData.BiomeTypeDictionary[BiomeTypeEnum.Hills];
        // 월드 사이즈 넘는 곳은 air 블록을 배치한다.
        if (IsVoxelInTerrain(pos))
            return BlockTypeEnum.Air; 
        
        // 맨 밑에는 배드락
        if (yPos == 0)
            return BlockTypeEnum.BedRock;
        
        //펄린 노이즈
        int terrainHeight = Mathf.FloorToInt(hillsBiome.terrainHeight * CustomNoise.Get2DPerlin(new Vector2(pos.x, pos.z), 500, 
            hillsBiome.terrainScale) + hillsBiome.solidGroundHeight);
        
        BlockTypeEnum voxelValue = BlockTypeEnum.Air;
        
        if (yPos == terrainHeight)
            voxelValue =  BlockTypeEnum.Grass;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            return BlockTypeEnum.Dirt;
        else if (yPos > terrainHeight)
            return BlockTypeEnum.Air;
        else
            voxelValue = BlockTypeEnum.Stone;

        if (voxelValue == BlockTypeEnum.Stone)
        {
            foreach (Load lode in hillsBiome.loads)
            {
                if (yPos > lode.MinHeight && yPos < lode.MaxHeight)
                    if (CustomNoise.Get3DPerlin(pos, lode.noiseOffset, lode.Scale, lode.threshold))
                        return lode.blockType;
            }
        }
        return voxelValue;
        // if (pos.y == VoxelData.ChunkHeight - 1)
        //     return BlockTypeEnum.Grass;
        // else
        //     return BlockTypeEnum.Stone;
    }
    

    // public bool CheckVoxel(Vector3 pos)
    // {
    //     Coord thisChunk = new Coord(pos);
    //
    //     if (!IsChunkInWorld(thisChunk.X_int, thisChunk.Z_int) || pos.y < 0 || pos.y > VoxelData.ChunkHeight) 
    //         return false;
    //
    //     if (_chunks[thisChunk.X_int, thisChunk.Z_int] != null && _chunks[thisChunk.X_int, thisChunk.Z_int].isVoxelMapPopulated)
    //         return blockData.BlockTypeDictionary[_chunks[thisChunk.X_int, thisChunk.Z_int].GetVoxelFromVector(pos)].isSolid;
    //
    //     return blockData.BlockTypeDictionary[TerrainCondition(pos)].isSolid;
    // }
    
    public bool CheckTransparent(Vector3 pos)
    {
        Coord thisChunk = new Coord(pos);

        if (!IsChunkInWorld(thisChunk.X_int, thisChunk.Z_int) || pos.y < 0 || pos.y > VoxelData.ChunkHeight) 
            return false;

        if (_chunks[thisChunk.X_int, thisChunk.Z_int] != null && _chunks[thisChunk.X_int, thisChunk.Z_int].isVoxelMapPopulated)
            return blockData.BlockTypeDictionary[_chunks[thisChunk.X_int, thisChunk.Z_int].GetVoxelFromVector(pos)].isTransparent;

        return blockData.BlockTypeDictionary[TerrainCondition(pos)].isTransparent;
    }
    
    Coord Vector3ToCoord(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x) / VoxelData.ChunkWidth;
        int z = Mathf.FloorToInt(pos.z) / VoxelData.ChunkDepth;

        return new Coord(x, z);
    }

    public Chunk Vector3ToChunk(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x) / VoxelData.ChunkWidth;
        int z = Mathf.FloorToInt(pos.z) / VoxelData.ChunkDepth;

        return _chunks[x, z];
    }
    
    
    // void CreateNewChunk(int x, int z)
    // {
    //     _chunks[x, z] = new Chunk(new Coord(x, z), blockData, this);
    //     activeChunks.Add(new Coord(x, z));
    // }
    
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



