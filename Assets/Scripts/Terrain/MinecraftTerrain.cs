using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Serialization;
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
    //프레임 전 후 player의 Coord(x, z) 위치
    private Coord _playerPreviousCoord;
    private Coord _playerCoord;
    
    private void Start()
    {
        Random.InitState(seed);
        _spawnPosition = new Vector3
            ((VoxelData.TerrainSize * VoxelData.ChunkWidth) / 2, 
            VoxelData.ChunkHeight - 200,
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
    
    void GenerateWorld()
    {
        for (int x = (VoxelData.TerrainSize / 2) - VoxelData.ViewDistance; x < (VoxelData.TerrainSize / 2) + VoxelData.ViewDistance; x++)
        {
            for (int z = (VoxelData.TerrainSize / 2) - VoxelData.ViewDistance; z < (VoxelData.TerrainSize / 2) + VoxelData.ViewDistance; z++)
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
    
    void GenerateChunkAroundPlayer()
    {
        Coord playerPos = Vector3ToCoord(player.transform.position);
        _playerCoord = _playerPreviousCoord;
        List<Coord> previousActiveChunk = new List<Coord>(activeChunks);
        for (int x = playerPos.X - VoxelData.ViewDistance; x < playerPos.X + VoxelData.ViewDistance; x++)
        {
            for (int z = playerPos.Z - VoxelData.ViewDistance; z < playerPos.Z + VoxelData.ViewDistance; z++)
            {
                if (IsChunkInWorld(x, z))
                {
                    if (_chunks[x, z] == null)
                    {
                        CreateNewChunk(x, z);
                    }
                    else if (!_chunks[x, z].isActive)
                    {
                        _chunks[x, z].isActive = true;
                        activeChunks.Add(new Coord(x, z));
                    }
                }
                // 현프레임에서 ActiveChunk와 비교해서 그대로 있다면 해당 청크는 계속 유지
                for (int i = 0; i < previousActiveChunk.Count; i++)
                {
                    if (previousActiveChunk[i].Equals(new Coord(x, z)))
                        previousActiveChunk.RemoveAt(i);
                }
            }
        }
        // 현 프레임에서 전 프레임에서 비교했을때 ActiveChunk가 아닌 것들을 false를 한다.
        foreach (Coord c in previousActiveChunk)
            _chunks[c.X, c.Z].isActive = false;
    }
    
    void CreateNewChunk(int x, int z)
    {
        _chunks[x, z] = new Chunk(new Coord(x, z), blockData, this);
        activeChunks.Add(new Coord(x, z));
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
