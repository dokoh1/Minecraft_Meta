using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Windows;
using Input = UnityEngine.Input;
using Random = UnityEngine.Random;
using System.IO;
using File = System.IO.File;


public class MinecraftTerrain : MonoBehaviour
{
    public Settings setting;
    public BlockData blockData;
    public BiomeData biomeData;
    public Transform player;
    public GameObject debugUI;

    
    [Range(0f, 1f)]
    public float globalLight;

    public Color Day;
    public Color Night;
    
    //Multi Thread Rendering
    public Queue<Chunk> ChunksQueue = new();
    
    public Coord PlayerCoord;
    
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
    public List<Chunk> _chunksToUpdate = new();
    
    //멀티 쓰레드 방지용 flag
    private bool _isRunningModification = false;
    public object ChunkUpdateLock = new();
    
    Thread ChunkUpdateThread;
    //나무 및 자연 구조물 추가
    private Queue<Queue<VoxelCondition>> _modifications = new();
    private NatureStructure _natureStructure;
    
    private void Start()
    {
        // 게임 세팅 파일 출력
        // string jsonExport = JsonUtility.ToJson(setting);
        // File.WriteAllText(Application.dataPath + "/Resources/settings.cfg", jsonExport);
        
        // 게임 세팅 파일 입력
        string jsonImport = File.ReadAllText(Application.dataPath + "/Resources/settings.cfg");
        setting = JsonUtility.FromJson<Settings>(jsonImport);
        
        Random.InitState(setting.seed);

        Shader.SetGlobalFloat("MinGlobalLight", VoxelData.minLight);
        Shader.SetGlobalFloat("MaxGlobalLight", VoxelData.maxLight);
        
        _natureStructure = new NatureStructure();
        _previousActiveChunk = new List<Coord>();
        if (setting.enabledThread)
        {
            ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            ChunkUpdateThread.Start();
        }
        _spawnPosition = new Vector3
            ((VoxelData.TerrainSize * VoxelData.ChunkWidth) / 2, 
            VoxelData.ChunkHeight - 190,
            (VoxelData.TerrainSize * VoxelData.ChunkDepth) / 2);
        
        GenerateWorld();
        _playerPreviousCoord = Vector3ToCoord(player.position);
        
    }

    public void SetGlobalLight(float globalLight)
    {
        Shader.SetGlobalFloat("GlobalLight", globalLight);
        if (Camera.main != null) 
            Camera.main.backgroundColor = Color.Lerp(Night, Day, globalLight);
    }
    
    private void Update()
    {
        PlayerCoord = Vector3ToCoord(player.transform.position);
        
        if (!PlayerCoord.Equals(_playerPreviousCoord))
            GenerateChunkAroundPlayer();
        
        if (_chunksToCreate.Count > 0)
            CreateChunk();

        if (ChunksQueue.Count > 0)
        {
                if (ChunksQueue.Peek().IsEdit)
                    ChunksQueue.Dequeue().CreateMesh();
        }
        if (!setting.enabledThread)
        {
            if (!_isRunningModification)
                ApplyModifications();
            
            if (_chunksToUpdate.Count > 0)
                UpdateChunk();
        }

        if (Input.GetKeyDown(KeyCode.F3))
            debugUI.SetActive(!debugUI.activeSelf);
    }
    

    private void CreateChunk()
    {
        Coord c = _chunksToCreate[0];
        _chunksToCreate.RemoveAt(0);
        _chunks[c.X, c.Z].Init();
    }

    private void OnDisable()
    {
        if (setting.enabledThread)
        {
            ChunkUpdateThread.Abort();
        }
    }
    
    void ApplyModifications()
    {
        _isRunningModification = true;
        
        while (_modifications.Count > 0)
        {
            Queue<VoxelCondition> queue = _modifications.Dequeue();
            if (queue == null)
            {
                _isRunningModification = false;
                return;
            }
            while (queue.Count > 0)
            {
                VoxelCondition condition = queue.Dequeue();
                
                Coord coord = Vector3ToCoord(condition.Position);
                
                if (_chunks[coord.X, coord.Z] == null)
                {
                    _chunks[coord.X, coord.Z] = new Chunk(coord, blockData, this);
                    _chunksToCreate.Add(coord);
                }
                _chunks[coord.X, coord.Z].Modifications.Enqueue(condition);
                
            }
        }

        _isRunningModification = false;
    }
    private void UpdateChunk()
    {
        bool updated = false;
        int index = 0;

        lock (ChunkUpdateLock)
        {
            while (!updated && index < _chunksToUpdate.Count - 1)
            {
                if (_chunksToUpdate[index].IsEdit)
                {
                    _chunksToUpdate[index].UpdateChunk();
                    if (!_activeChunks.Contains(_chunksToUpdate[index]._coord))
                        _activeChunks.Add(_chunksToUpdate[index]._coord);
                    _chunksToUpdate.RemoveAt(index);
                    updated = true;
                }
                else
                    index++;
            }
        }
    }

    void ThreadedUpdate()
    {
        while (true)
        {
            if (!_isRunningModification)
                ApplyModifications();
            if (_chunksToUpdate.Count > 0)
                UpdateChunk();
        }
    }
    private void GenerateWorld()
    {
        for (int x = (VoxelData.TerrainSize / 2) - setting.ViewDistance; x < (VoxelData.TerrainSize / 2) + setting.ViewDistance; x++)
        {
            for (int z = (VoxelData.TerrainSize / 2) - setting.ViewDistance; z < (VoxelData.TerrainSize / 2) + setting.ViewDistance; z++)
            {
                Coord newCoord = new Coord(x, z);
                _chunks[x, z] = new Chunk(newCoord, blockData, this);
                _chunksToCreate.Add(newCoord);
            }
        }
        player.transform.position = _spawnPosition;
        GenerateChunkAroundPlayer();
    }
    
    private void GenerateChunkAroundPlayer()
    {
        Coord playerPos = Vector3ToCoord(player.transform.position);
        _playerPreviousCoord = PlayerCoord;
        PlayerCoord = Vector3ToCoord(player.transform.position);
        
        _previousActiveChunk.Clear();
        _previousActiveChunk.AddRange(_activeChunks);
        _activeChunks.Clear();
        
        for (int x = playerPos.X - setting.ViewDistance; x < playerPos.X + setting.ViewDistance; x++)
        {
            for (int z = playerPos.Z - setting.ViewDistance; z < playerPos.Z + setting.ViewDistance; z++)
            {
                Coord playerCoord = new Coord(x, z);
                if (IsChunkInWorld(x, z))
                {
                    if (_chunks[x, z] == null)
                    {
                        _chunks[x, z] = new Chunk(playerCoord, blockData, this);
                        _chunksToCreate.Add(playerCoord);
                    }
                    else if (!_chunks[x, z].IsActive)
                        _chunks[x, z].IsActive = true;
                    
                    _activeChunks.Add(playerCoord);
                }
                // 현프레임에서 ActiveChunk와 비교해서 그대로 있다면 해당 청크는 계속 유지
                for (int i = 0; i < _previousActiveChunk.Count; i++)
                {
                    if (_previousActiveChunk[i].Equals(playerCoord))
                        _previousActiveChunk.RemoveAt(i);
                }
            }
        }
        // 현 프레임에서 전 프레임에서 비교했을때 ActiveChunk가 아닌 것들을 false를 한다.
        foreach (Coord c in _previousActiveChunk)
            _chunks[c.X, c.Z].IsActive = false;
    }
    
    
    public BlockTypeEnum TerrainCondition(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        BiomeTypeData hillsBiome = biomeData.BiomeTypeDictionary[BiomeTypeEnum.Hills];
        
        // 월드 사이즈 넘는 곳은 air 블록을 배치한다.
        if (!IsVoxelInTerrain(pos))
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
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkDepth);

        return new Coord(x, z);
    }

    public bool CheckVoxel(Vector3 pos)
    {
        Coord thisChunk = new Coord(pos);

        if (!IsChunkInWorld(thisChunk.X, thisChunk.Z) || pos.y < 0 || pos.y > VoxelData.ChunkHeight) 
            return false;

        if (_chunks[thisChunk.X, thisChunk.Z] != null && _chunks[thisChunk.X, thisChunk.Z].IsEdit)
            return blockData.BlockTypeDictionary[_chunks[thisChunk.X, thisChunk.Z].GetVoxelFromVector(pos).BlockType].isSolid;

        return blockData.BlockTypeDictionary[TerrainCondition(pos)].isSolid;
    }
    
    public BlockState GetBlockState(Vector3 pos)
    {
        Coord thisChunk = new Coord(pos);

        if (!IsChunkInWorld(thisChunk.X, thisChunk.Z) || pos.y < 0 || pos.y > VoxelData.ChunkHeight) 
            return null;

        if (_chunks[thisChunk.X, thisChunk.Z] != null && _chunks[thisChunk.X, thisChunk.Z].IsEdit)
            return _chunks[thisChunk.X, thisChunk.Z].GetVoxelFromVector(pos);

        return new BlockState(TerrainCondition(pos));
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
            return true;
        return false;
    }
    
    public Chunk Vector3ToChunk(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkDepth);

        return _chunks[x, z];
    }
}

[System.Serializable]
public class Settings
{
    [Header("Optimization")]
    public int ViewDistance;
    public bool enabledThread;
    
    [Header("Controls")]
    [Range(0.1f, 10f)]
    public float mouseSensitivity;
    
    [Header("World Gen")]
    public int seed;
      
}

