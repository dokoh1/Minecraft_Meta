using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 한 청크의 메시 데이터 빌드(백그라운드) -> 메시에 적용(메인 스레드). 충돌체 생성
/// 블록 편집 반영, 청크 경계 변경 전파, 간단한 라이트 전파(전역광).
/// 렌더 구조 :  서브메시 3개(0 = 불투명, 1 = 반투명, 2 = 잎사귀)
/// </summary>
public class Chunk
{
    
    public readonly Coord Coord;
    
    private GameObject _chunkObject;
    private MeshRenderer _renderer;
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    
    private readonly Vector3 _position;
    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _indices = new();
    private readonly List<Vector2> _uvs = new();
    private readonly List<int> _transparentIndices = new();
    private readonly List<int> _leaveIndices = new();
    private readonly List<Color> _colors = new();
    private readonly List<Vector3> _normals = new();
    private readonly Material[] _materials = new Material[3];

    private int _vertexIndex = 0;
    private readonly ChunkData _chunkData;
    private bool _isActive;
    
    /// <summary>
    /// 청크 표시/숨김 토글
    /// </summary>
    public bool IsActive
    {
        get
        {
            return _isActive;
        }
        set
        {
            _isActive = value;
            if (_chunkObject is not null)
                _chunkObject.SetActive(value);
        }
    }
    
    /// <summary>
    /// 청크 게임오브젝트 생성, 필수 컴포넌트 등록(MeshFilter/Renderer/Collider)
    /// Material 세팅, 위치 지정, ChunkData 요청, 업데이트 큐 등록
    /// MinecraftTerrain.GenerateChunkAroundPlayer() 내부에서 신규 청크 필요 시 호출
    /// </summary>
    /// <param name="coord"></param>
    public Chunk(Coord coord)
    {
        Coord = coord;

        _chunkObject = new GameObject();
        _meshFilter = _chunkObject.AddComponent<MeshFilter>();
        _renderer = _chunkObject.AddComponent<MeshRenderer>();
        _meshCollider = _chunkObject.AddComponent<MeshCollider>();

        _materials[0] = MinecraftTerrain.Instance.blockData.material;
        _materials[1] = MinecraftTerrain.Instance.blockData.transparentMaterial;
        _materials[2] = MinecraftTerrain.Instance.blockData.leaveMaterial;
        _renderer.materials = _materials;
        
        _chunkObject.transform.SetParent(MinecraftTerrain.Instance.transform);
        _chunkObject.transform.position = new Vector3(Coord.X * VoxelData.ChunkWidth, 0f, Coord.Z * VoxelData.ChunkDepth);
        
        _position = _chunkObject.transform.position;
        _chunkData = MinecraftTerrain.Instance.worldData.RequestChunk(new Vector2Int((int)_position.x, (int)_position.z),
            true);
        lock (MinecraftTerrain.Instance.ChunkUpdateLock)
            MinecraftTerrain.Instance.ChunksToUpdate.Add(this);
    }
    /// <summary>
    /// 전역광 간단 전파.
    /// 위에서 아래로 스캔: 블록의 transparency를 누적해 GlobalLightPercent 설정, 임계치 LightFalloff 이상이면 큐에 등록
    /// 큐 BFS: 6방향 이웃으로 감쇠(-LightFalloff) 전파
    /// </summary>
    void CalculateLight()
    {
        Queue<Vector3Int> litBlocks = new Queue<Vector3Int>();
        
        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.ChunkDepth; z++)
            {
                float lightRay = 1f;
                for (int y = VoxelData.ChunkHeight - 1; y >= 0; y--)
                {
                    BlockState thisBlock = _chunkData.Map[x, y, z];

                    if (thisBlock.BlockType != BlockTypeEnum.Air && MinecraftTerrain.Instance.blockData.BlockTypeDictionary[thisBlock.BlockType].transparency < lightRay)
                    {
                        lightRay = MinecraftTerrain.Instance.blockData.BlockTypeDictionary[thisBlock.BlockType].transparency;
                    }
                    thisBlock.GlobalLightPercent = lightRay;
                    _chunkData.Map[x, y, z] = thisBlock;
                    if (lightRay > VoxelData.LightFalloff)
                        litBlocks.Enqueue(new Vector3Int(x, y, z));
                }
            }
        }
        
        while (litBlocks.Count > 0)
        {
            Vector3Int block = litBlocks.Dequeue();
            
            for (int i = 0; i < 6; i++)
            {
                Vector3 currentBlock = block + VoxelData.FaceChecks[i];
                Vector3Int neighbor = new Vector3Int((int)currentBlock.x, (int)currentBlock.y, (int)currentBlock.z);
                if (IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z))
                {
                    if (_chunkData.Map[neighbor.x, neighbor.y, neighbor.z].GlobalLightPercent <
                        _chunkData.Map[block.x, block.y, block.z].GlobalLightPercent - VoxelData.LightFalloff)
                    {
                        _chunkData.Map[neighbor.x, neighbor.y, neighbor.z].GlobalLightPercent =
                            _chunkData.Map[block.x, block.y, block.z].GlobalLightPercent - VoxelData.LightFalloff;
                        if (_chunkData.Map[neighbor.x, neighbor.y, neighbor.z].GlobalLightPercent > VoxelData.LightFalloff)
                            litBlocks.Enqueue(neighbor);
                    }
                }
            }
        }
    
    }
    /// <summary>
    /// 메시 버퍼 생성 단계
    /// 1. ClearChunk()
    /// 2. CalculateLight()
    /// 3. 모든 Voxel 순회 : isSolid면 UpdateMeshData()로 면 추가
    /// 4. 완료되면 자기 자신을 ChunksQueue에 Enqueue -> (메인 스레드) CreateMesh()로 실제 적용
    /// </summary>
    public void UpdateChunk()
    {
        ClearChunk();
        CalculateLight();
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkDepth; z++)
                {
                    if (MinecraftTerrain.Instance.blockData.BlockTypeDictionary[_chunkData.Map[x, y, z].BlockType].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }
        MinecraftTerrain.Instance.ChunksQueue.Enqueue(this);
    }
    /// <summary>
    /// 하나의 복셀에 대해 보이는 면만 골라 버텍스/인덱스/UV/노말/색을 버퍼에 Push
    /// 1. 현재 블록 타입 -> blockKey.
    /// 2. neighbor = IsCheckVoxel(pos + faceDir)가 그려지는 블록인지에 따라 "현재 Voxel의 해당 면"을 추가
    /// 3. 면 추가 시
    /// - 4 버텍스, 면 노말 4개, UV 4개(AddTexture), 색상 4개(알파=라이트)
    /// - 인덱스 6개(두 삼각형). 서브메시에 따라 _indices/_transparentIndices/_leaveIndices 중 하나에 push
    /// </summary>
    /// <param name="pos"></param>
    
    private void UpdateMeshData(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        
        BlockTypeEnum blockKey = _chunkData.Map[x, y, z].BlockType;
        bool isTransparent = MinecraftTerrain.Instance.blockData.BlockTypeDictionary[blockKey].isDrawing;
        bool isLeave = MinecraftTerrain.Instance.blockData.BlockTypeDictionary[blockKey].isLeave;
        
        for (int i = 0; i < 6; i++)
        {
            BlockState neighbor = IsCheckVoxel(pos + VoxelData.FaceChecks[i]);
            if (neighbor != null && MinecraftTerrain.Instance.blockData.BlockTypeDictionary[neighbor.BlockType].isDrawing)
            {
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 0]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 1]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 2]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 3]]);
                for (int n = 0; n < 4; n++)
                {
                    _normals.Add(VoxelData.FaceChecks[i]);
                }
                AddTexture(MinecraftTerrain.Instance.blockData.BlockTypeDictionary[blockKey].GetTextureID(i), isLeave);
                float lightLevel = neighbor.GlobalLightPercent;
                
                _colors.Add(new Color(0, 0, 0, lightLevel));
                _colors.Add(new Color(0, 0, 0, lightLevel));
                _colors.Add(new Color(0, 0, 0, lightLevel));
                _colors.Add(new Color(0, 0, 0,  lightLevel));
                if (isLeave)
                {
                    _leaveIndices.Add(_vertexIndex);
                    _leaveIndices.Add(_vertexIndex + 1);
                    _leaveIndices.Add(_vertexIndex + 2);
                    _leaveIndices.Add(_vertexIndex + 2);
                    _leaveIndices.Add(_vertexIndex + 1);
                    _leaveIndices.Add(_vertexIndex + 3);
                }
                else if (isTransparent)
                {
                    _transparentIndices.Add(_vertexIndex);
                    _transparentIndices.Add(_vertexIndex + 1);
                    _transparentIndices.Add(_vertexIndex + 2);
                    _transparentIndices.Add(_vertexIndex + 2);
                    _transparentIndices.Add(_vertexIndex + 1);
                    _transparentIndices.Add(_vertexIndex + 3);
                }
                else
                {
                    _indices.Add(_vertexIndex);
                    _indices.Add(_vertexIndex + 1);
                    _indices.Add(_vertexIndex + 2);
                    _indices.Add(_vertexIndex + 2);
                    _indices.Add(_vertexIndex + 1);
                    _indices.Add(_vertexIndex + 3);
                }
                _vertexIndex += 4;
            }
        }
    }
    /// <summary>
    /// 빌드 버퍼 초기화
    /// </summary>
    public void ClearChunk()
    {
        _vertexIndex = 0;
        _vertices.Clear();
        _indices.Clear();
        _uvs.Clear();
        _transparentIndices.Clear();
        _leaveIndices.Clear();
        _colors.Clear();
        _normals.Clear();
    }
    


    /// <summary>
    /// 경계에 닿는 수정이 있을 때 인접 청크도 업데이트 큐 맨 앞에 넣어 우선 갱신.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    private void UpdateAroundChunk(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
        for (int i = 0; i < 6; i++)
        {
            Vector3 checkVoxel = thisVoxel + VoxelData.FaceChecks[i];
            if (!IsVoxelInChunk((int)checkVoxel.x, (int)checkVoxel.y, (int)checkVoxel.z))
                MinecraftTerrain.Instance.ChunksToUpdate.Insert(0, MinecraftTerrain.Instance.Vector3ToChunk(checkVoxel + _position));
        }
    }
    /// <summary>
    /// 월드 좌표의 블록을 현재 청크의 로컬 인덱스로 변환해서 타입 교체 -> 수정된 청크 등록 -> 본인/경계 이웃 청크 우선 업데이트 큐에 push
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="blockType"></param>
    public void EditBlockInChunk(Vector3 pos, BlockTypeEnum blockType)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(_chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);
        
        _chunkData.Map[xCheck, yCheck, zCheck].BlockType = blockType;
        
        MinecraftTerrain.Instance.worldData.AddToModifiedChunks(_chunkData);

        lock (MinecraftTerrain.Instance.ChunkUpdateLock)
        {
            MinecraftTerrain.Instance.ChunksToUpdate.Insert(0, this);
            UpdateAroundChunk(xCheck, yCheck, zCheck);
        }
    }





    /// <summary>
    /// "그 면을 그릴지" 판단용으로 이웃 Voxel 상태 조회
    /// - 청크 내부면 _chunkData.Map
    /// - 밖이면 MinecraftTerrain.Instance.GetBlockState(pos + _position) 로 월드 좌표에서 조회
    /// </summary>
    /// <param name="pos">위치</param>
    /// <returns></returns>
    private BlockState IsCheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
            return MinecraftTerrain.Instance.GetBlockState(pos + _position);
        
        return _chunkData.Map[x, y, z];
    }
    
    /// <summary>
    /// 메인 스레드에서 빌드된 버퍼로 Mesh/Collider 실제 적용
    /// - mesh.subMeshCount = 3 -> 서브 메시별로 인덱스 적용
    /// - UV/색/노말 적용
    /// - _meshFilter.mesh = mesh; -> _meshCollider.sharedMesh 에 동일 메시 사용
    /// </summary>
    public void CreateMesh()
    {
        Mesh mesh = new();
        mesh.vertices = _vertices.ToArray();
        mesh.subMeshCount = 3;
        
        mesh.SetTriangles(_indices.ToArray(), 0);
        mesh.SetTriangles(_transparentIndices.ToArray(), 1);
        mesh.SetTriangles(_leaveIndices.ToArray(), 2);
        
        mesh.uv = _uvs.ToArray();
        mesh.colors = _colors.ToArray();
        mesh.normals = _normals.ToArray();
        _meshFilter.mesh = mesh;    
        _meshCollider.sharedMesh = _meshFilter.mesh;
        
    }

    /// <summary>
    /// Atlas 이미지를 uv 좌표에 맞게 매핑하는 로직을 담은 함수
    /// </summary>
    /// <param name="textureID"></param>
    private void AddTexture(int textureID, bool isLeave)
    {
        Vector2 uv0 = new Vector2(0, 0);
        
        if (!isLeave)
        {
            float y = textureID / VoxelData.TextureAtlasSize;
            float x = textureID - (y * VoxelData.TextureAtlasSize);

            x *= VoxelData.NormalizedBlockTextureSize;
            y *= VoxelData.NormalizedBlockTextureSize;
            y = 1f - y - VoxelData.NormalizedBlockTextureSize;
            
            uv0.Set(x, y);
            _uvs.Add(uv0);
            uv0.Set(x, y + VoxelData.NormalizedBlockTextureSize);
            _uvs.Add(uv0);
            uv0.Set(x + VoxelData.NormalizedBlockTextureSize, y);
            _uvs.Add(uv0);
            uv0.Set(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize);
            _uvs.Add(uv0);
        }
        else
        {
            uv0.Set(0, 0);
            _uvs.Add(uv0);
            uv0.Set(0, 1);
            _uvs.Add(uv0);
            uv0.Set(1, 0);
            _uvs.Add(uv0);
            uv0.Set(1, 1);
            _uvs.Add(uv0);
        }
    }
    /// <summary>
    /// 로컬 복셀 인덱스 범위 체크
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 ||
            y < 0 || y > VoxelData.ChunkHeight - 1 ||
            z < 0 || z > VoxelData.ChunkDepth - 1)
            return false;
        return true;
    }
}
