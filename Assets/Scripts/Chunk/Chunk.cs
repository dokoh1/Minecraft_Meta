using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Animations;

public class Chunk
{
    // public Queue<VoxelCondition> Modifications = new();
    public readonly List<VoxelCondition> Modifications = new(100);
    public Vector3 Position;
    Queue<Vector3Int> litBlocks = new Queue<Vector3Int>();
    
    public Coord _coord;
    private BlockData _blockData;
    private MinecraftTerrain _terrain;
    private GameObject _chunkObject;
    
    private MeshRenderer _renderer;
    private MeshFilter _meshFilter;
    private MeshCollider _meshColider;
    
    
    private readonly BlockState[,,] _blockNames = new BlockState[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkDepth];
    private readonly List<Vector3> _vertices = new(10000);
    private readonly List<int> _indices = new(10000);
    private readonly List<Vector2> _uvs = new(10000);
    private readonly List<int> _transparentIndices = new(10000);
    private readonly List<int> _leaveIndices = new(10000);
    private readonly List<Color> _colors = new(10000);
    private readonly List<Vector3> _normals = new(10000);
    private Material[] _materials = new Material[3];

    private int _vertexIndex = 0;
    
    //Chunk Acitve Bool
    private bool _isActive;
    
    //청크가 아직 초기화 중이거나, 다른 연산이 진행 중인지 파악하는 bool
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
            if (!_isBlockNamePopulated)
                return false;
            else
                return true;
        }
    }
    
    public Chunk(Coord coord, BlockData blockData, MinecraftTerrain terrain)
    {
        _coord = coord;
        _blockData = blockData;
        _terrain = terrain;
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
        _chunkObject.transform.position = new Vector3(_coord.X * VoxelData.ChunkWidth, 0f, _coord.Z * VoxelData.ChunkDepth);
        
        Position = _chunkObject.transform.position;
        ThreadChunkTypeSetting();
    }
    
    private void ThreadChunkTypeSetting()
    {
        Vector3 pos = Vector3.zero;
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkDepth; z++)
                {
                    //청크의 내부 좌표가 아닌 월드 좌표를 구하기 위해서 더해준다.
                    pos.Set(x, y, z);
                    _blockNames[x, y, z] = new BlockState(_terrain.TerrainCondition(pos + Position));
                }
            }
        }

        _isBlockNamePopulated = true;
        lock (_terrain.ChunkUpdateLock)
        {
            _terrain._chunksToUpdate.Add(this);
        }
    }

    public void UpdateChunk()
    {
        while (Modifications.Count > 0)
        {
            VoxelCondition voxelCondition = Modifications[0];
            Modifications.RemoveAt(0);
            
            Vector3 pos = voxelCondition.Position -= Position;
            _blockNames[(int)pos.x, (int)pos.y, (int)pos.z].BlockType = voxelCondition.BlockType;
        }
        ClearChunk();
        CalculateLight();
        Vector3 updatePos = new Vector3(0, 0, 0);
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkDepth; z++)
                {
                    if (_blockData.BlockTypeDictionary[_blockNames[x, y, z].BlockType].isSolid)
                    {
                        updatePos.Set(x, y, z);
                        UpdateMeshData(updatePos);
                    }
                }
            }
        }
        _terrain.ChunksQueue.Enqueue(this);
    }
    private void UpdateMeshData(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        
        BlockTypeEnum blockKey = _blockNames[x, y, z].BlockType;
        bool isTransparent = _terrain.blockData.BlockTypeDictionary[blockKey].isDrawing;
        bool isLeave = _terrain.blockData.BlockTypeDictionary[blockKey].isLeave;
        
        for (int i = 0; i < 6; i++)
        {
            BlockState neighbor = IsCheckVoxel(pos + VoxelData.FaceChecks[i]);
            if (neighbor != null && _blockData.BlockTypeDictionary[neighbor.BlockType].isDrawing)
            {
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 0]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 1]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 2]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 3]]);
                for (int n = 0; n < 4; n++)
                {
                    _normals.Add(VoxelData.FaceChecks[i]);
                }
                AddTexture(_blockData.BlockTypeDictionary[blockKey].GetTextureID(i), isLeave);
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

    
    private void UpdateAroundChunk(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
        for (int i = 0; i < 6; i++)
        {
            Vector3 checkVoxel = thisVoxel + VoxelData.FaceChecks[i];

            if (!IsVoxelInChunk((int)checkVoxel.x, (int)checkVoxel.y, (int)checkVoxel.z))
                _terrain._chunksToUpdate.Insert(0, _terrain.Vector3ToChunk(checkVoxel + Position));
        }
    }
    
    public void EditBlockInChunk(Vector3 pos, BlockTypeEnum blockType)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(_chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);
        
        _blockNames[xCheck, yCheck, zCheck].BlockType = blockType;

        lock (_terrain.ChunkUpdateLock)
        {
            _terrain._chunksToUpdate.Insert(0, this);
            UpdateAroundChunk(xCheck, yCheck, zCheck);
            
        }
    }




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
    /// Voxel이 생성되어야할지 말아야할지 판단하는 함수
    /// </summary>
    /// <param name="pos">위치</param>
    /// <returns></returns>
    private BlockState IsCheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
            return _terrain.GetBlockState(pos + Position);
        
        return _blockNames[x, y, z];
    }


    public BlockState GetVoxelFromVector(Vector3 vector)
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

    void CalculateLight()
    {
        litBlocks.Clear();
        Vector3Int lightPos = new Vector3Int();
        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.ChunkDepth; z++)
            {
                float lightRay = 1f;
                for (int y = VoxelData.ChunkHeight - 1; y >= 0; y--)
                {
                    BlockState thisBlock = _blockNames[x, y, z];

                    if (thisBlock.BlockType != BlockTypeEnum.Air && _blockData.BlockTypeDictionary[thisBlock.BlockType].transparency < lightRay)
                    {
                        lightRay = _blockData.BlockTypeDictionary[thisBlock.BlockType].transparency;
                    }
                    thisBlock.GlobalLightPercent = lightRay;
                    _blockNames[x, y, z] = thisBlock;
                    if (lightRay > VoxelData.lightFalloff)
                    {
                        lightPos.Set(x, y ,z);
                        litBlocks.Enqueue(lightPos);
                    }
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
                    if (_blockNames[neighbor.x, neighbor.y, neighbor.z].GlobalLightPercent <
                        _blockNames[block.x, block.y, block.z].GlobalLightPercent - VoxelData.lightFalloff)
                    {
                        _blockNames[neighbor.x, neighbor.y, neighbor.z].GlobalLightPercent =
                            _blockNames[block.x, block.y, block.z].GlobalLightPercent - VoxelData.lightFalloff;
                        if (_blockNames[neighbor.x, neighbor.y, neighbor.z].GlobalLightPercent > VoxelData.lightFalloff)
                            litBlocks.Enqueue(neighbor);
                    }
                }
            }
        }
    
    }

    /// <summary>
    /// Vertex, uv, Index 정보를 Mesh에 넣는다.
    /// </summary>
    public void CreateMesh()
    {
        Mesh mesh = new();
        mesh.SetVertices(_vertices);
        mesh.subMeshCount = 3;
        
        mesh.SetTriangles(_indices.ToArray(), 0);
        mesh.SetTriangles(_transparentIndices.ToArray(), 1);
        mesh.SetTriangles(_leaveIndices.ToArray(), 2);
        
        mesh.SetUVs(0, _uvs);
        mesh.SetColors(_colors);
        mesh.SetNormals(_normals);
        _meshFilter.mesh = mesh;    
        _meshColider.sharedMesh = _meshFilter.mesh;
        
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
        else if (isLeave)
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
    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidth - 1 ||
            y < 0 || y > VoxelData.ChunkHeight - 1 ||
            z < 0 || z > VoxelData.ChunkDepth - 1)
            return false;
        return true;
    }
}

public class BlockState
{
    public BlockTypeEnum BlockType;
    public float GlobalLightPercent;

    public BlockState()
    {
        BlockType = BlockTypeEnum.Air;
        GlobalLightPercent = 0f;
    }

    public BlockState(BlockTypeEnum blocktype)
    {
        BlockType = blocktype;
        GlobalLightPercent = 0f;
    }
}

public class Coord
{
    public int X;
    public int Z;
    
    public Coord(int x, int z)
    {
        X = x;
        Z = z;
    }

    public Coord()
    {
        X = 0;
        Z = 0;
    }

    public Coord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);
        X = Mathf.FloorToInt(xCheck) /  VoxelData.ChunkWidth;
        Z = Mathf.FloorToInt(zCheck) / VoxelData.ChunkDepth;
    }

    public bool Equals(Coord other)
    {
        if (other == null)
            return false;
        else if (other.X == X && other.Z == Z)
            return true;
        else
            return false;
    }
}