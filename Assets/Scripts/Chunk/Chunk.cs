using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Chunk
{
    public Coord _coord;
    
    private BlockData BlockData;
    private GameObject _chunkObject;
    private MeshRenderer _renderer;
    private MeshFilter _meshFilter;
    private MeshCollider _meshColider;
    
    private int _vertexIndex = 0;
    private readonly BlockTypeEnum[,,] _blockNames = new BlockTypeEnum[16,256, 16];
    
    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _indices = new();
    private readonly List<Vector2> _uvs = new();

    private MinecraftTerrain _terrain;
    private bool _isActive;
    public bool isVoxelMapPopulated = false;
    public Chunk(Coord coord, BlockData blockData, MinecraftTerrain terrain, bool OnLoad)
    {
        _coord = coord;
        BlockData = blockData;
        _terrain = terrain;
        isActive = true;
        if (OnLoad)
            Init();
    }

    void UpdateAroundChunk(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
        for (int i = 0; i < 6; i++)
        {
            Vector3 CheckVoxel = thisVoxel + VoxelData.FaceChecks[i];
            if (CheckVoxel.x < 0 || CheckVoxel.x > VoxelData.ChunkWidth - 1 ||
                CheckVoxel.y < 0 || CheckVoxel.y > VoxelData.ChunkHeight - 1 ||
                CheckVoxel.z < 0 || CheckVoxel.z > VoxelData.ChunkDepth - 1)
                _terrain.Vector3ToChunk(CheckVoxel + _chunkObject.transform.position).UpdateChunk();
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
        UpdateChunk();
    }

    public void Init()
    {
        _chunkObject = new GameObject();
        _meshFilter = _chunkObject.AddComponent<MeshFilter>();
        _renderer = _chunkObject.AddComponent<MeshRenderer>();
        _meshColider = _chunkObject.AddComponent<MeshCollider>();
        
        _renderer.material = BlockData._material;
        _chunkObject.transform.SetParent(_terrain.transform);
        _chunkObject.transform.position = new Vector3(_coord.X_int * VoxelData.ChunkWidth, 0f, _coord.Z_int * VoxelData.ChunkDepth);
        
        ChunkTypeSetting();
        UpdateChunk();
        
    }

    public void ClearChunk()
    {
        _vertexIndex = 0;
        _vertices.Clear();
        _indices.Clear();
        _uvs.Clear();
        _meshColider.sharedMesh = null;
    }
    public bool isActive
    {
        get
        {
            return _isActive;
        }
        set
        {
            _isActive = value;
            if (_chunkObject != null)
                _chunkObject.SetActive(value);
        }
    }
    void ChunkTypeSetting()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkDepth; z++)
                {
                    //청크의 내부 좌표가 아닌 월드 좌표를 구하기 위해서 더해준다.
                        _blockNames[x, y, z] = _terrain.TerrainCondition(new Vector3(x, y, z) + _chunkObject.transform.position);
                }
            }
        }
        isVoxelMapPopulated = true;
    }
    
    
    
    /// <summary>
    /// Voxel이 생성되어야할지 말아야할지 판단하는 함수
    /// </summary>
    /// <param name="pos">위치</param>
    /// <returns></returns>
    bool IsCheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || x > VoxelData.ChunkWidth - 1 ||
            y < 0 || y > VoxelData.ChunkHeight - 1 ||
            z < 0 || z > VoxelData.ChunkDepth - 1)
            return _terrain.CheckVoxel(pos + _chunkObject.transform.position);
        
        return BlockData.BlockTypeDictionary[_blockNames[x, y, z]].isSolid;
    }
    
    void UpdateChunk()
    {
        ClearChunk();
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkDepth; z++)
                {
                    if (BlockData.BlockTypeDictionary[_blockNames[x,y,z]].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }
        CreateMesh();
        _meshColider.sharedMesh = _meshFilter.mesh;
    }

    public BlockTypeEnum GetVoxelFromVector(Vector3 vector)
    {
        int xCheck = Mathf.FloorToInt(vector.x);
        int yCheck = Mathf.FloorToInt(vector.y);
        int zCheck = Mathf.FloorToInt(vector.z);
        
        xCheck -= Mathf.FloorToInt(_chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);
        return _blockNames[xCheck, yCheck, zCheck];
    }
    
    /// <summary>
    /// Voxel의 Vertex 정보 및 index정보 및 uv 정보를 넣는다.
    /// </summary>
    /// <param name="pos"></param>
    void UpdateMeshData(Vector3 pos)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!IsCheckVoxel(pos + VoxelData.FaceChecks[i]))
            {
                BlockTypeEnum blockKey = _blockNames[(int)pos.x, (int)pos.y, (int)pos.z];
                
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 0]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 1]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 2]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 3]]);

                AddTexture(BlockData.BlockTypeDictionary[blockKey].GetTextureID(i));
                
                _indices.Add(_vertexIndex);
                _indices.Add(_vertexIndex + 1);
                _indices.Add(_vertexIndex + 2);
                _indices.Add(_vertexIndex + 2);
                _indices.Add(_vertexIndex + 1);
                _indices.Add(_vertexIndex + 3);
                _vertexIndex += 4;
            }
        }
    }
    /// <summary>
    /// Vertex, uv, Index 정보를 Mesh에 넣는다.
    /// </summary>
    void CreateMesh()
    {
        Mesh mesh = new()
        {
            vertices = _vertices.ToArray(),
            triangles = _indices.ToArray(),
            uv = _uvs.ToArray()
        };
        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
    }

    /// <summary>
    /// Atlas 이미지를 uv 좌표에 맞게 매핑하는 로직을 담은 함수
    /// </summary>
    /// <param name="textureID"></param>
    void AddTexture(int textureID)
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
}
