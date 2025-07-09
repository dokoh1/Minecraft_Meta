using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Input = UnityEngine.Input;
using System.Linq;
using Cysharp.Threading.Tasks;

public class MinecraftTerrain : MonoBehaviour
{
    //Shader 변수
    private static readonly int GlobalLight = Shader.PropertyToID("GlobalLight");
    private static readonly int MaxGlobalLight = Shader.PropertyToID("MaxGlobalLight");
    private static readonly int MinGlobalLight = Shader.PropertyToID("MinGlobalLight");
    
    //Data
    public InGame inGameSetting;
    public BlockData blockData;
    public BiomeData biomeData;
    
    //PerlinNoise
    private CustomNoise _noise;
    
    //Debug
    public GameObject debugUI;
    
    //청크 생성 자료 구조
    public Queue<Chunk> ChunksQueue = new();
    public readonly List<Chunk> ChunksToUpdate = new();
    
    //플레이어
    public Coord PlayerCoord;
    public Transform player;
    private Camera _mainCamera;
    private Vector3 _spawnPosition;

    //빛
    [Range(0f, 1f)]
    public float globalLight;
    
    [SerializeField]
    private Color day;
    
    [SerializeField]
    private Color night;

    //구름
    [SerializeField]
    private Clouds clouds;
    
    // 청크 클래스 생성
    private Chunk[,] _chunks = new Chunk[VoxelData.TerrainSize, VoxelData.TerrainSize];
    
    // 이전 프레임과 이후 프레임의 Coord를 비교하여 Active를 설정하기 위한 List
    private List<Coord> _activeChunks = new();
    private List<Coord> _previousActiveChunk;
    private Coord _playerPreviousCoord;
    
    //밤 낮 조절 변수
    [SerializeField]
    private float cycleDuration;
    
    //Modification Flag
    private bool _isRunningModification;
    
    //쓰레드 락 오브젝트
    public object ChunkUpdateLock = new();
    public object ChunkListThreadLock = new();
    private Thread _chunkUpdateThread;
    
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
            Destroy(gameObject);
        else
        {
            _instance = this;
        }
        appPath = Application.persistentDataPath;
    }
    
    private async void Start()
    {
        _mainCamera = Camera.main;
        worldData = SaveSystem.LoadWorld("Prototype");
        
        _noise = new CustomNoise();
        _natureStructure = new NatureStructure();
        _previousActiveChunk = new List<Coord>();
        
        Random.InitState(worldData.seed);
        
        Shader.SetGlobalFloat(MinGlobalLight, VoxelData.MinLight);
        Shader.SetGlobalFloat(MaxGlobalLight, VoxelData.MaxLight);
        
        _spawnPosition = new Vector3
            (VoxelData.TerrainMiddle, 
            VoxelData.PlayerInitHeight,
            VoxelData.TerrainMiddle);
        
        LoadTerrain();
        player.position = _spawnPosition;
        GenerateChunkAroundPlayer();
        _playerPreviousCoord = Vector3ToCoord(player.position);

        await ThreadedUpdate();
    }   
    
    private void Update()
    {
        float time = Time.time / cycleDuration * Mathf.PI * 2f;
        globalLight = Mathf.Clamp01((Mathf.Sin(time) + 1f) / 2f);
        Shader.SetGlobalFloat(GlobalLight, globalLight);
        _mainCamera.backgroundColor = Color.Lerp(night, day, globalLight);
        Random.InitState(VoxelData.Seed);
        
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
        for (int x = (VoxelData.TerrainSize / 2) - inGameSetting.loadDistance; x < (VoxelData.TerrainSize / 2) + inGameSetting.loadDistance; x++)
        {
            for (int z = (VoxelData.TerrainSize / 2) - inGameSetting.loadDistance; z < (VoxelData.TerrainSize / 2) + inGameSetting.loadDistance; z++)
            {
                worldData.LoadChunk(new Vector2Int(x, z));
            }
        }
    }
    
    private void UpdateChunk()
    {
        lock (ChunkUpdateLock)
        {
            ChunksToUpdate[0].UpdateChunk();
            if (!_activeChunks.Contains(ChunksToUpdate[0].Coord))
                _activeChunks.Add(ChunksToUpdate[0].Coord);
            ChunksToUpdate.RemoveAt(0);
        }
    }
    private async UniTask ThreadedUpdate()
    {
        while (true)
        {
            if (!_isRunningModification)
                ApplyModifications();
            if (ChunksToUpdate.Count > 0)
                await UniTask.RunOnThreadPool(() => UpdateChunk());
            await UniTask.Delay(10);
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
                
                worldData.SetVoxel(condition.Position, condition.BlockType);
            }
        }

        _isRunningModification = false;
    }
    
    private void GenerateChunkAroundPlayer()
    {
        clouds.UpdateCloud();
        Coord playerPos = Vector3ToCoord(player.transform.position);
        _playerPreviousCoord = PlayerCoord;
        PlayerCoord = Vector3ToCoord(player.transform.position);
        
        _previousActiveChunk.Clear();
        _previousActiveChunk.AddRange(_activeChunks);
        _activeChunks.Clear();
        
        for (int x = playerPos.X - inGameSetting.viewDistance; x < playerPos.X + inGameSetting.viewDistance; x++)
        {
            for (int z = playerPos.Z - inGameSetting.viewDistance; z < playerPos.Z + inGameSetting.viewDistance; z++)
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
        
        if (!IsVoxelInTerrain(pos))
            return BlockTypeEnum.Air; 
        
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
            float weight = _noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);

            if (weight > strongestHeight)
            {
                strongestHeight = weight;
                strongestHeightIndex = i;
            }
            float height = biomes[i].terrainHeight * _noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].terrainScale) * weight;

            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }

        }
        BiomeTypeData biome = biomes[strongestHeightIndex];

        sumOfHeights /= count;
        
        //terrain Height
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
                    if (_noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        return lode.blockType;
            }
        }
        
        //tree PerlinNoise
        if (yPos == terrainHeight)
        {
            if (_noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treeZoneScale) >
                biome.treeZoneThreshold)
            {
                if (_noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treePlaceScale) >
                    biome.treePlaceThreshold)
                    _modifications.Enqueue(_natureStructure.MakeTree(pos, biome, _noise));
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

// 게임 세팅 파일 출력
// string jsonExport = JsonUtility.ToJson(setting);
// File.WriteAllText(Application.dataPath + "/Resources/settings.cfg", jsonExport);
        
// 게임 세팅 파일 입력
// string jsonImport = File.ReadAllText(Application.dataPath + "/Resources/settings.cfg");
// setting = JsonUtility.FromJson<Settings>(jsonImport);
