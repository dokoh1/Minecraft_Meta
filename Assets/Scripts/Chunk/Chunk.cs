using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Chunk
{
    public Queue<VoxelCondition> Modifications = new();
    public Vector3 Position;
    
    
    private Coord _coord;
    private BlockData _blockData;
    private MinecraftTerrain _terrain;
    private GameObject _chunkObject;
    
    private MeshRenderer _renderer;
    private MeshFilter _meshFilter;
    private MeshCollider _meshColider;
    
    
    private readonly BlockTypeEnum[,,] _blockNames = new BlockTypeEnum[16,256, 16];
    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _indices = new();
    private readonly List<Vector2> _uvs = new();
    private readonly List<int> _transparentIndices = new();
    private readonly List<int> _leaveIndices = new();
    private Material[] _materials = new Material[3];
    private int _vertexIndex = 0;
    
    //Chunk Acitve Bool
    private bool _isActive;
    
    //청크가 아직 초기화 중이거나, 다른 연산이 진행 중인지 파악하는 bool
    private bool _threadLock = false;
    private bool _isBlockNamePopulated = false;
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
    
    public bool IsEdit
    {
        get
        {
            if (!_isBlockNamePopulated || _threadLock)
                return false;
            else
                return true;
        }
    }
    
    public Chunk(Coord coord, BlockData blockData, MinecraftTerrain terrain, bool onLoad)
    {
        _coord = coord;
        _blockData = blockData;
        _terrain = terrain;
        IsActive = true;
        if (onLoad)
            Init();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void UpdateAroundChunk(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
        for (int i = 0; i < 6; i++)
        {
            Vector3 checkVoxel = thisVoxel + VoxelData.FaceChecks[i];
            if (checkVoxel.x < 0 || checkVoxel.x > VoxelData.ChunkWidth - 1 ||
                checkVoxel.y < 0 || checkVoxel.y > VoxelData.ChunkHeight - 1 ||
                checkVoxel.z < 0 || checkVoxel.z > VoxelData.ChunkDepth - 1)
                _terrain.Vector3ToChunk(checkVoxel + _chunkObject.transform.position).UpdateChunk();
        }
    }
    
    public void EditBlockInChunk(Vector3 pos, BlockTypeEnum blockType)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);
        
        xCheck -= Mathf.FloorToInt(_chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);
        _blockNames[xCheck, yCheck, zCheck] = blockType;
        
        UpdateAroundChunk(xCheck, yCheck, zCheck);
        UpdateChunk();
    }

    public void Init()
    {
        _chunkObject = new GameObject();
        _meshFilter = _chunkObject.AddComponent<MeshFilter>();
        _renderer = _chunkObject.AddComponent<MeshRenderer>();
        _meshColider = _chunkObject.AddComponent<MeshCollider>();

        _materials[0] = _blockData.material;
        _materials[1] = _blockData.transparentMaterial;
        _materials[2] = _blockData.leaveMaterial;
        _renderer.materials = _materials;
        _chunkObject.transform.SetParent(_terrain.transform);
        _chunkObject.transform.position = new Vector3(_coord.X_int * VoxelData.ChunkWidth, 0f, _coord.Z_int * VoxelData.ChunkDepth);
        Position = _chunkObject.transform.position;
        ChunkTypeSetting();

    }



    public void ClearChunk()
    {
        _vertexIndex = 0;
        _vertices.Clear();
        _indices.Clear();
        _uvs.Clear();
        _transparentIndices.Clear();
        _leaveIndices.Clear();
    }

    private void ChunkTypeSetting()
    {
        ThreadChunkTypeSetting();
    }
    private async UniTask ThreadChunkTypeSetting()
    {
        await UniTask.RunOnThreadPool(() =>
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int x = 0; x < VoxelData.ChunkWidth; x++)
                {
                    for (int z = 0; z < VoxelData.ChunkDepth; z++)
                    {
                        //청크의 내부 좌표가 아닌 월드 좌표를 구하기 위해서 더해준다.
                        _blockNames[x, y, z] = _terrain.TerrainCondition(new Vector3(x, y, z) + Position);
                    }
                }
            }
        });

        await ThreadUpdateChunk();
        _isBlockNamePopulated = true;
    }

    /// <summary>
    /// Voxel이 생성되어야할지 말아야할지 판단하는 함수
    /// </summary>
    /// <param name="pos">위치</param>
    /// <returns></returns>
    private bool IsCheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || x > VoxelData.ChunkWidth - 1 ||
            y < 0 || y > VoxelData.ChunkHeight - 1 ||
            z < 0 || z > VoxelData.ChunkDepth - 1)
            return _terrain.CheckTransparent(pos + Position);
        
        return _blockData.BlockTypeDictionary[_blockNames[x, y, z]].isTransparent;
    }

    public void UpdateChunk()
    {
        ThreadUpdateChunk();
    }
    public async UniTask ThreadUpdateChunk()
    {
        _threadLock = true;
        while (Modifications.Count > 0)
        {
            VoxelCondition voxelCondition = Modifications.Dequeue();
            Vector3 pos = voxelCondition.Position -= Position;
            _blockNames[(int)pos.x, (int)pos.y, (int)pos.z] = voxelCondition.BlockType;
        }
        ClearChunk();
        await UniTask.RunOnThreadPool(() =>
        {
            for (int y = 0; y < VoxelData.ChunkHeight; y++)
            {
                for (int x = 0; x < VoxelData.ChunkWidth; x++)
                {
                    for (int z = 0; z < VoxelData.ChunkDepth; z++)
                    {
                        if (_blockData.BlockTypeDictionary[_blockNames[x,y,z]].isSolid)
                            UpdateMeshData(new Vector3(x, y, z));
                    }
                }
            }
        });

        lock (_terrain.ChunksQueue)
        {
           _terrain.ChunksQueue.Enqueue(this);
        }

        _threadLock = false;
    }

    public BlockTypeEnum GetVoxelFromVector(Vector3 vector)
    {
        int xCheck = Mathf.FloorToInt(vector.x);
        int yCheck = Mathf.FloorToInt(vector.y);
        int zCheck = Mathf.FloorToInt(vector.z);
        
        xCheck -= Mathf.FloorToInt(Position.x);
        zCheck -= Mathf.FloorToInt(Position.z);
        return _blockNames[xCheck, yCheck, zCheck];
    }
    
    /// <summary>
    /// Voxel의 Vertex 정보 및 index정보 및 uv 정보를 넣는다.
    /// </summary>
    /// <param name="pos"></param>
    private void UpdateMeshData(Vector3 pos)
    {
        BlockTypeEnum blockKey = _blockNames[(int)pos.x, (int)pos.y, (int)pos.z];
        bool isTransparent = _terrain.blockData.BlockTypeDictionary[blockKey].isTransparent;
        bool isLeave = _terrain.blockData.BlockTypeDictionary[blockKey].isLeave;
        for (int i = 0; i < 6; i++)
        {
            if (IsCheckVoxel(pos + VoxelData.FaceChecks[i]))
            {
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 0]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 1]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 2]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 3]]);

                AddTexture(_blockData.BlockTypeDictionary[blockKey].GetTextureID(i), isLeave);

                if (!isTransparent)
                {
                    _indices.Add(_vertexIndex);
                    _indices.Add(_vertexIndex + 1);
                    _indices.Add(_vertexIndex + 2);
                    _indices.Add(_vertexIndex + 2);
                    _indices.Add(_vertexIndex + 1);
                    _indices.Add(_vertexIndex + 3);
                }
                else if (isLeave)
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
                _vertexIndex += 4;
            }
        }
    }
    
    /// <summary>
    /// Vertex, uv, Index 정보를 Mesh에 넣는다.
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
        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
        _meshColider.sharedMesh = _meshFilter.mesh;
        
    }

    /// <summary>
    /// Atlas 이미지를 uv 좌표에 맞게 매핑하는 로직을 담은 함수
    /// </summary>
    /// <param name="textureID"></param>
    private void AddTexture(int textureID, bool isLeave)
    {
        if (!isLeave)
        {
            float y = textureID / VoxelData.TextureAtlasSize;
            float x = textureID - (y * VoxelData.TextureAtlasSize);

            x *= VoxelData.NormalizedBlockTextureSize;
            y *= VoxelData.NormalizedBlockTextureSize;
            y = 1f - y - VoxelData.NormalizedBlockTextureSize;

            _uvs.Add(new Vector2(x, y));
            _uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
            _uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
            _uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
        }
        else if (isLeave)
        {
            _uvs.Add(new Vector2(0, 0));
            _uvs.Add(new Vector2(0, 1));
            _uvs.Add(new Vector2(1, 0));
            _uvs.Add(new Vector2(1, 1));
        }
    }
}
