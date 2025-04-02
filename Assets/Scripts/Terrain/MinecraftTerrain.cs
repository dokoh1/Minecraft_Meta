using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Input = UnityEngine.Input;
using File = System.IO.File;
using System.Linq;
using Unity.VisualScripting;


public class MinecraftTerrain : MonoBehaviour
{
    //Shader 변수
    private static readonly int GlobalLight = Shader.PropertyToID("GlobalLight");
    private static readonly int MaxGlobalLight = Shader.PropertyToID("MaxGlobalLight");
    private static readonly int MinGlobalLight = Shader.PropertyToID("MinGlobalLight");
    
    public Settings setting;
    public BlockData blockData;
    public BiomeData biomeData;
    public Transform player;
    public GameObject debugUI;
    
    //빛
    [Range(0f, 1f)]
    public float globalLight;
    public Color Day;
    public Color Night;
    
    //청크 생성 자료 구조
    public Queue<Chunk> ChunksQueue = new();
    public List<Chunk> _chunksToUpdate = new();
    
    //플레이어
    public Coord PlayerCoord;
    private Camera _mainCamera;
    private Vector3 _spawnPosition;

    //구름
    public Clouds _clouds;
    
    
    // 청크 생성
    private Chunk[,] _chunks = new Chunk[VoxelData.TerrainSize, VoxelData.TerrainSize];
    
    // 이전 프레임과 이후 프레임의 Coord를 비교하여 Active를 설정하기 위한 List
    private List<Coord> _activeChunks = new();
    private List<Coord> _previousActiveChunk;
    private Coord _playerPreviousCoord;
    
    
    //멀티 쓰레드 방지용 flag
    private bool _isRunningModification = false;
    
    //쓰레드 락 오브젝트
    public object ChunkUpdateLock = new();
    public object ChunkListThreadLock = new();
    public Thread ChunkUpdateThread;
    
    //나무 및 자연 구조물 추가
    private Queue<Queue<VoxelCondition>> _modifications = new();
    private NatureStructure _natureStructure;
    
    //전역 인스턴스
    private static MinecraftTerrain _instance;
    public static MinecraftTerrain Instance {get {return _instance ;}}
    
    //월드 데이터 저장 앤 로드
    public WorldData worldData;
    public string appPath;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
        {
            _instance = this;
        }

        appPath = Application.persistentDataPath;
    }
    
    private void Start()
    {
        // Application.OpenURL(Application.persistentDataPath);
        _mainCamera = Camera.main;
        worldData = SaveSystem.LoadWorld("Prototype");

        Random.InitState(worldData.seed);
        
        Shader.SetGlobalFloat(MinGlobalLight, VoxelData.minLight);
        Shader.SetGlobalFloat(MaxGlobalLight, VoxelData.maxLight);
        
        _spawnPosition = new Vector3
            (VoxelData.TerrainMiddle, 
            VoxelData.ChunkHeight - 190,
            VoxelData.TerrainMiddle);
        
        _natureStructure = new NatureStructure();
        _previousActiveChunk = new List<Coord>();
        
        LoadTerrain();
        player.position = _spawnPosition;
        GenerateChunkAroundPlayer();
        _playerPreviousCoord = Vector3ToCoord(player.position);
        

        ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
        ChunkUpdateThread.Start();
        
        
    }   
    
    private void Update()
    {
        Shader.SetGlobalFloat(GlobalLight, globalLight);
        _mainCamera.backgroundColor = Color.Lerp(Night, Day, globalLight);
        Random.InitState(VoxelData.seed);
        
        PlayerCoord = Vector3ToCoord(player.transform.position);
        
        if (!PlayerCoord.Equals(_playerPreviousCoord))
            GenerateChunkAroundPlayer();

        if (ChunksQueue.Count > 0)
            ChunksQueue.Dequeue().CreateMesh();

        if (Input.GetKeyDown(KeyCode.F3))
            debugUI.SetActive(!debugUI.activeSelf);
        
        if (Input.GetKeyDown(KeyCode.F4))
            SaveSystem.SaveWorld(worldData);
    }
    
    private void LoadTerrain()
    {
        for (int x = (VoxelData.TerrainSize / 2) - setting.loadDistance; x < (VoxelData.TerrainSize / 2) + setting.loadDistance; x++)
        {
            for (int z = (VoxelData.TerrainSize / 2) - setting.loadDistance; z < (VoxelData.TerrainSize / 2) + setting.loadDistance; z++)
            {
                worldData.LoadChunk(new Vector2Int(x, z));
            }
        }
    }
    
    private void UpdateChunk()
    {
        lock (ChunkUpdateLock)
        {
            _chunksToUpdate[0].UpdateChunk();
            if (!_activeChunks.Contains(_chunksToUpdate[0]._coord))
                _activeChunks.Add(_chunksToUpdate[0]._coord);
            _chunksToUpdate.RemoveAt(0);
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
    
    private void OnDisable()
    {
        ChunkUpdateThread.Abort();
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
                
                worldData.SetVoxel(condition.Position, condition.BlockType);
            }
        }

        _isRunningModification = false;
    }

    
    private void GenerateChunkAroundPlayer()
    {
        _clouds.UpdateCloud();
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
                        _chunks[x, z] = new Chunk(playerCoord);
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
        
        BiomeTypeData[] biomes = biomeData.BiomeTypeDictionary.Values.ToArray();
        
        // 월드 사이즈 넘는 곳은 air 블록을 배치한다.
        if (!IsVoxelInTerrain(pos))
            return BlockTypeEnum.Air; 
        
        // 맨 밑에는 배드락
        if (yPos == 0)
            return BlockTypeEnum.BedRock;
        
        //Biome PelinNoise
        int solidGroundHeight = 42;
        float sumOfHeights = 0f;
        int count = 0;
        float strongestHeight = 0f;
        int strongestHeightIndex = 0;

        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = CustomNoise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);

            if (weight > strongestHeight)
            {
                strongestHeight = weight;
                strongestHeightIndex = i;
            }
            float height = biomes[i].terrainHeight * CustomNoise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].terrainScale) * weight;

            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }

        }
        BiomeTypeData biome = biomes[strongestHeightIndex];

        sumOfHeights /= count;
        
        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);
        
        BlockTypeEnum voxelValue = BlockTypeEnum.Air;
        
        if (yPos == terrainHeight)
            voxelValue = biome.surfaceBlock;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = biome.subSurfaceBlock;
        else if (yPos > terrainHeight)
            return BlockTypeEnum.Air;
        else
            voxelValue = BlockTypeEnum.Stone;
        
        // Mine PerlinNoise
        if (voxelValue == BlockTypeEnum.Stone)
        {
            foreach (Load lode in biome.loads)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (CustomNoise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        return lode.blockType;
            }
        }
        
        //tree PerlinNoise
        if (yPos == terrainHeight)
        {
            if (CustomNoise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treeZoneScale) >
                biome.treeZoneThreshold)
            {
                if (CustomNoise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treePlaceScale) >
                    biome.treePlaceThreshold)
                    _modifications.Enqueue(_natureStructure.MakeTree(pos, biome));
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
        BlockState block = worldData.GetVoxel(pos);
        if (blockData.BlockTypeDictionary[block.BlockType].isSolid)
            return true;
        return false;

    }
    
    public BlockState GetBlockState(Vector3 pos)
    {
        return worldData.GetVoxel(pos);
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
    public int ViewDistance = 8;

    public int loadDistance = 16;
    public bool enabledThread = true;
    
    [Header("Controls")]
    [Range(0.1f, 10f)]
    public float mouseSensitivity = 2.0f;
    
    [Header("World Gen")]
    public int seed = 0;
      
}

// 게임 세팅 파일 출력
// string jsonExport = JsonUtility.ToJson(setting);
// File.WriteAllText(Application.dataPath + "/Resources/settings.cfg", jsonExport);
        
// 게임 세팅 파일 입력
// string jsonImport = File.ReadAllText(Application.dataPath + "/Resources/settings.cfg");
// setting = JsonUtility.FromJson<Settings>(jsonImport);
