using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public BlockData blockData;

    private MeshRenderer _renderer;
    private MeshFilter _meshFilter;
    
    private int _vertexIndex = 0;
    private readonly BlockTypeEnum[,,] _blockNames = new BlockTypeEnum[16, 256, 16];
    
    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _indices = new();
    private readonly List<Vector2> _uvs = new();


    private void Awake()
    {
        blockData = GameObject.Find("BlockData").GetComponent<BlockData>();
        _renderer = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();

        ChunkTypeSetting();
        CreateVoxelChunk();
        CreateMesh();
    }
    
    void ChunkTypeSetting()
    {

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkDepth; z++)
                {
                    // if (y < 200)
                    // {
                    //     blockNames[y, x, z] = "Dirt";
                    // }
                    // else
                    _blockNames[x, y, z] = BlockTypeEnum.Stone;

                }
            }
        }
    }

    /// <summary>
    /// 정육면체의 면이 내부에 있는 면인지 외부에 있는 면인지 확인하는 함수
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
            return false;
        
        return blockData.BlockTypeDictionary[_blockNames[x, y, z]].isSolid;
    }

    void CreateVoxelChunk()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkDepth; z++)
                {
                    AddVoxelChunk(new Vector3(x, y, z));
                }
            }
        }
    }
    /// <summary>
    /// Voxel의 Vertex 정보 및 index정보 및 uv 정보를 넣는다.
    /// </summary>
    /// <param name="pos"></param>
    void AddVoxelChunk(Vector3 pos)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!IsCheckVoxel(pos + VoxelData.FaceChecks[i]))
            {
                BlockTypeEnum blockKey = _blockNames[(int)pos.x, (int)pos.y, (int)pos.z];
                
                // 처음에 vertex 정보를 Index정보를 바탕으로 먼저 그리고 vertexIndex를 넣는다.
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 0]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 1]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 2]]);
                _vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 3]]);

                AddTexture(blockData.BlockTypeDictionary[blockKey].GetTextureID(i));
                
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
        // ReSharper disable once PossibleLossOfFraction
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