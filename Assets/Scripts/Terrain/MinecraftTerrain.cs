using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks;

public class MinecraftTerrain : MonoBehaviour
{
    public BlockData blockData;
    public BiomeData biomeData;
    public Transform player;
    public GameObject debugUI;
    
    //Multi Thread Rendering
    public Queue<Chunk> ChunksQueue = new();
    
    public Coord PlayerCoord;
    //seed 값
    public int seed;
    
    //캐릭터 spawn 포지션
    private Vector3 _spawnPosition;
    
    // 청크 생성
    private Chunk[,] _chunks = new Chunk[VoxelData.TerrainSize, VoxelData.TerrainSize];
    
    // 이전 프레임과 이후 프레임의 Coord를 비교하여 Active를 설정하기 위한 List
    private List<Coord> _activeChunks = new();
    private List<Coord> _previousActiveChunk;

    //프레임 전 후 player의 Coord(x, z) 위치
    private Coord _playerPreviousCoord;
    
    //코루틴 청크 프레임 단위 생성을 위한 리스트
    private List<Coord> _chunksToCreate = new();
    private List<Chunk> _chunksToUpdate = new();
    
    //멀티 쓰레드 방지용 flag
    private bool _isRunningModification = false;
    //나무 및 자연 구조물 추가
    private Queue<Queue<VoxelCondition>> _modifications = new();
    private NatureStructure _natureStructure;
    
    private void Start()
    {
        Random.InitState(seed);
        _natureStructure = new NatureStructure();
        _previousActiveChunk = new List<Coord>();
        _spawnPosition = new Vector3
            ((VoxelData.TerrainSize * VoxelData.ChunkWidth) / 2, 
            VoxelData.ChunkHeight - 190,
            (VoxelData.TerrainSize * VoxelData.ChunkDepth) / 2);
        player.transform.position = _spawnPosition;
        
        GenerateWorld();
        
        PlayerCoord = Vector3ToCoord(player.position);
        _playerPreviousCoord = PlayerCoord;
    }
    
    private void Update()
    {
        PlayerCoord = Vector3ToCoord(player.position);
        
        if (!PlayerCoord.Equals(_playerPreviousCoord))
            GenerateChunkAroundPlayer();
        
        if (!_isRunningModification)
           ApplyModifications();
        
        if (_chunksToCreate.Count > 0)
            CreateChunk();
        
        if (_chunksToUpdate.Count > 0)
            UpdateChunk();
        
        if (Input.GetKeyDown(KeyCode.F3))
            debugUI.SetActive(!debugUI.activeSelf);
    }

    private void LateUpdate()
    {
        if (ChunksQueue.Count > 0)
            lock (ChunksQueue)
            {
                if (ChunksQueue.Peek().IsEdit)
                    ChunksQueue.Dequeue().CreateMesh();
            }
    }

    private void CreateChunk()
    {
        Coord c = _chunksToCreate[0];
        _chunksToCreate.RemoveAt(0);
        _activeChunks.Add(c);
        _chunks[c.X_int, c.Z_int].Init();
    }

    void ApplyModifications()
    {
        _isRunningModification = true;
        
        while (_modifications.Count > 0)
        {
            Queue<VoxelCondition> queue = _modifications.Dequeue();
            if (queue == null)
            {
                Debug.Log("NULL!!");
                _isRunningModification = false;
                return;
            }
            while (queue.Count > 0)
            {
                VoxelCondition condition = queue.Dequeue();
                Coord coord = Vector3ToCoord(condition.Position);
                if (_chunks[coord.X_int, coord.Z_int] == null)
                {
                    _chunks[coord.X_int, coord.Z_int] = new Chunk(coord, blockData, this, true);
                    _activeChunks.Add(coord);
                }
                _chunks[coord.X_int, coord.Z_int].Modifications.Enqueue(condition);
                
                if (!_chunksToUpdate.Contains(_chunks[coord.X_int, coord.Z_int]))
                    _chunksToUpdate.Add(_chunks[coord.X_int, coord.Z_int]);
            }
        }

        _isRunningModification = false;
    }
    private void UpdateChunk()
    {
        bool updated = false;
        int index = 0;

        while (!updated && index < _chunksToUpdate.Count - 1)
        {
            if (_chunksToUpdate[index].IsEdit)
            {
                _chunksToUpdate[index].UpdateChunk();
                _chunksToUpdate.RemoveAt(index);
                updated = true;
            }
            else
                index++;
        }
    }
    private void GenerateWorld()
    {
        for (int x = (VoxelData.TerrainSize / 2) - VoxelData.InitSize; x < (VoxelData.TerrainSize / 2) + VoxelData.InitSize; x++)
        {
            for (int z = (VoxelData.TerrainSize / 2) - VoxelData.InitSize; z < (VoxelData.TerrainSize / 2) + VoxelData.InitSize; z++)
            {
                _chunks[x, z] = new Chunk(new Coord(x, z),this.blockData, this, true);
                _activeChunks.Add(new Coord(x, z));
            }
        }
    }
    
    private void GenerateChunkAroundPlayer()
    {
        Coord playerPos = Vector3ToCoord(player.transform.position);
        
        _previousActiveChunk.Clear();
        _previousActiveChunk.AddRange(_activeChunks);
        _activeChunks.Clear();
        
        for (int x = playerPos.X_int - VoxelData.ViewDistance; x < playerPos.X_int + VoxelData.ViewDistance; x++)
        {
            for (int z = playerPos.Z_int - VoxelData.ViewDistance; z < playerPos.Z_int + VoxelData.ViewDistance; z++)
            {
                Coord chunkCoord = new Coord(x, z);
                if (IsChunkInWorld(x, z))
                {
                    if (_chunks[x, z] == null)
                    {
                        _chunks[x, z] = new Chunk(chunkCoord, this.blockData, this, false);
                        _chunksToCreate.Add(new Coord(x, z));
                    }
                    else if (!_chunks[x, z].IsActive)
                        _chunks[x, z].IsActive = true;
                    
                    _activeChunks.Add(new Coord(x, z));
                }
                // 현프레임에서 ActiveChunk와 비교해서 그대로 있다면 해당 청크는 계속 유지
                for (int i = 0; i < _previousActiveChunk.Count; i++)
                {
                    if (_previousActiveChunk[i].Equals(chunkCoord))
                        _previousActiveChunk.RemoveAt(i);
                }
            }
        }
        // 현 프레임에서 전 프레임에서 비교했을때 ActiveChunk가 아닌 것들을 false를 한다.
        foreach (Coord c in _previousActiveChunk)
            _chunks[c.X_int, c.Z_int].IsActive = false;
    }
    
    
    public BlockTypeEnum TerrainCondition(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        BiomeTypeData hillsBiome = biomeData.BiomeTypeDictionary[BiomeTypeEnum.Hills];
        
        // 월드 사이즈 넘는 곳은 air 블록을 배치한다.
        if (IsVoxelInTerrain(pos))
            return BlockTypeEnum.Air; 
        
        // 맨 밑에는 배드락
        if (yPos == 0)
            return BlockTypeEnum.BedRock;
        
        //height PerlinNoise
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
        
        // Mine PerlinNoise
        if (voxelValue == BlockTypeEnum.Stone)
        {
            foreach (Load lode in hillsBiome.loads)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (CustomNoise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        return lode.blockType;
            }
        }
        
        //tree PerlinNoise
        if (yPos == terrainHeight)
        {
            if (CustomNoise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, hillsBiome.treeZoneScale) >
                hillsBiome.treeZoneThreshold)
            {
                if (CustomNoise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, hillsBiome.treePlaceScale) >
                    hillsBiome.treePlaceThreshold)
                    _modifications.Enqueue(_natureStructure.MakeTree(pos, hillsBiome));
            }
                
        }
        return voxelValue;
    }
    
    private Coord Vector3ToCoord(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x) / VoxelData.ChunkWidth;
        int z = Mathf.FloorToInt(pos.z) / VoxelData.ChunkDepth;

        return new Coord(x, z);
    }

    public bool CheckTransparent(Vector3 pos)
    {
        Coord thisChunk = new Coord(pos);

        if (!IsChunkInWorld(thisChunk.X_int, thisChunk.Z_int) || pos.y < 0 || pos.y > VoxelData.ChunkHeight) 
            return false;

        if (_chunks[thisChunk.X_int, thisChunk.Z_int] != null && _chunks[thisChunk.X_int, thisChunk.Z_int].IsEdit)
            return blockData.BlockTypeDictionary[_chunks[thisChunk.X_int, thisChunk.Z_int].GetVoxelFromVector(pos)].isTransparent;

        return blockData.BlockTypeDictionary[TerrainCondition(pos)].isTransparent;
    }
    
    private bool IsChunkInWorld(int x, int z) 
    {
        if (x > 0 && x < VoxelData.TerrainSize - 1 && z > 0 && z < VoxelData.TerrainSize - 1)
            return true;
        return false;
    }

    public bool IsVoxelInTerrain(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.TerrainInVoxelSize && 
            pos.y < VoxelData.ChunkHeight && pos.y >= 0 &&
            pos.z >= 0 && pos.z < VoxelData.TerrainInVoxelSize)
            return false;
        return true;
    }
    
    public Chunk Vector3ToChunk(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x) / VoxelData.ChunkWidth;
        int z = Mathf.FloorToInt(pos.z) / VoxelData.ChunkDepth;

        return _chunks[x, z];
    }
}



