using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Input = UnityEngine.Input;
using System.Linq;
using Cysharp.Threading.Tasks;
// 월드(청크/바이옴/광맥/나무) 생성 규칙을 제공하고, 플레이어 주변 청크의 생성/활성화 관리
// 주야간 광원(Shader Global) 설정
// 백그라운드 청크 업데이트/자연 구조물 적용을 조율하는 "월드 매니저"

//메인 스레드 : 입력/카메라/Shader 전역값/ 큐 소비(ChunksQueue.Dequeue().CreateMesh())
//백그라운드 :  ChunksToUpdate의 Chunk.UpdateChunk() 계산(메쉬 데이터 빌드 전용) -> 메인에서 CreateMesh().
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
    
    //싱글톤 보장
    //appPath 초기화
    //appPath란? : 앱이 껐다 켜져도 유지되는 파일을 저장하는 전용 폴더의 경로(문자열)입니다.
    //1.세이브 데이터, 설정 파일(JSON), 진행도, 다운로드해 둔 리소스(썸네일/캐시), 로그 등 사용자 데이터를 넣습니다.
    //2. 앱을 삭제하거나 설정에서 데이터 삭제를 하지 않는 이상 유지됩니다.
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
    
    /// <summary>
    /// 1. 카메라/월드 로드와 랜덤 시드 세팅
    /// 2. 전역 셰이더 파라미터 초기화
    /// 3. 스폰 위치/플레이어 배치
    /// 4. 초기 시야 내 청크 생성 요청
    /// 5. 백그라운드 루프(ThreadedUpdate) 기동
    /// </summary>
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
        var ct = this.GetCancellationTokenOnDestroy();
        ThreadedUpdate(ct)
            .AttachExternalCancellation(ct)
            .SuppressCancellationThrow()
            .Forget();
    }   
    
    /// <summary>
    /// 1. 주 야간 사이클에 따라 globalLight 셰이더 파라미터/카메라 배경색 갱신
    /// 2. 플레이어 좌표 변화 감지 -> 주변 청크 로딩/활성화(GenerateChunkAroundPlayer())
    /// 3. ChunksQueue에서 프레임당 1개 꺼내 CreateMesh()
    /// 4. F3 디버그 UI 토글, F4 월드 저장
    /// </summary>
    private void Update()
    {
        float time = Time.time / cycleDuration * Mathf.PI * 2f;
        globalLight = Mathf.Clamp01((Mathf.Sin(time) + 1f) / 2f);
        Shader.SetGlobalFloat(GlobalLight, globalLight);
        _mainCamera.backgroundColor = Color.Lerp(night, day, globalLight); 
        //Random.InitState(VoxelData.Seed);
        
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
    /// <summary>
    /// 초기 로딩 거리 범위 내의 청크 데이터를 WorldData에서 미리 로드(디스크 -> 메모리)
    /// 리팩토링 힌트: 큰 맵에서는 IO를 분산(코루틴/Job)하거나 스플래시 로딩 화면 고려 (추후에 넣을 기능)
    /// </summary>
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
    /// <summary>
    /// ChunksToUpdate[0]에 대해 데이터 갱신 (Chunk.UpdateChunk() 호출) 후, 해당 청크를 활성 목록에 등록하고 큐에서 제거.
    /// </summary>
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
    /// <summary>
    /// 자연 구조물 변경사항 적용(ApplyModifications() - 메인스레드)
    /// ChunksToUpdate가 있으면 백그라운드에서 UpdateChunk() 실행
    /// UniTask.Delay(10)로 양보 .스로틀링
    /// 왜 Delay(10)?
    /// CPU 점유 과다 방지(한 프레임/틱마다 잠깐 쉼)
    /// 대량 생성 시 “조금씩” 분배하여 스파이크를 완화
    /// 적 생성 10개가 몰릴 경우, 10ms 간격으로 처리가 퍼짐(프레임 타임 안정)
    /// </summary>
    /// <param name="ct"></param>
    private async UniTask ThreadedUpdate(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (!_isRunningModification)
                ApplyModifications();
            if (ChunksToUpdate.Count > 0)
                await UniTask.RunOnThreadPool(() => UpdateChunk(), cancellationToken: ct);
            await UniTask.Delay(10, cancellationToken: ct);
        }
    }
    /// <summary>
    /// _modifications 큐에 누적된 나무/구조물을 월드 데이터에 메인스레드에서 적용
    /// 리팩토링 힌트 : 큰 변경 묶음은 청크 단위로 배치해서 최소한의 리빌드만 유도.
    /// </summary>
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
    /// <summary>
    ///  플레이어 좌표 기준 ViewDistance 사각 영역 내에  청크를 생성/활성화, 범위 밖은 비활성화.
    /// clouds.UpdateCloud() 호출(구름 동기화)
    /// 현재 프레임의 활성 후보를 _activeChunks에 채우고, 전 프레임의 활성 목록(_previousActiveChunk)과 비교하여 사라진 청크만 IsActive = false;
    /// </summary>
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
    
    /// <summary>
    /// 특정 월드 좌표의 Voxel이 무엇인지 결정(지표/지층/광맥/공기/나무 스폰 트리거).
    /// 1. 경계/바닥 체크(0=BedRock)
    /// 2. 각 바이옴에 대해 PerlinNoise로 가중치와 바이옴별 지형 높이 계산
    /// 3. 가장 강한 가중치 바이옴 선택 -> 평균 높이(sumOfHeights/count) + solidGroundHeight로 최종 지형고도 산출
    /// 4. 현재 y가 지형고도와의 관계로 표면/중간층/암석/공기 결정
    /// 5. 암석이면 3DPerlinNoise로 광맥(loads) 체크
    /// 6. 표면이면 2DPerlinNoise로 나무 스폰 확률 검사 -> _modifications 큐에 나무 생성 작업 enqueue
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
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
    /// <summary>
    /// 월드 좌표 -> 청크 좌표(정수) 변환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private Coord Vector3ToCoord(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkDepth);

        return new Coord(x, z);
    }
    /// <summary>
    /// 해당 월드 좌표가 Solid 블록인지 체크
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckVoxel(Vector3 pos)
    {
        BlockState block = worldData.GetVoxel(pos);
        if (blockData.BlockTypeDictionary[block.BlockType].isSolid)
            return true;
        return false;

    }
    /// <summary>
    /// Voxel 그대로 조회
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public BlockState GetBlockState(Vector3 pos)
    {
        return worldData.GetVoxel(pos);
    }
    /// <summary>
    /// 청크 좌표가 월드 범위 내인지.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private bool IsChunkInWorld(int x, int z) 
    {
        if (x > 0 && x < VoxelData.TerrainSize - 1 && z > 0 && z < VoxelData.TerrainSize - 1)
            return true;
        return false;
    }
    
    /// <summary>
    /// Voxel 좌표가 월드 Voxel 범위 내인지 확인
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool IsVoxelInTerrain(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.TerrainInVoxelSize && 
            pos.y < VoxelData.ChunkHeight && pos.y >= 0 &&
            pos.z >= 0 && pos.z < VoxelData.TerrainInVoxelSize)
            return true;
        return false;
    }
    
    /// <summary>
    /// 좌표로 해당 청크 인스턴스 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Chunk Vector3ToChunk(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkDepth);

        return _chunks[x, z];
    }
}