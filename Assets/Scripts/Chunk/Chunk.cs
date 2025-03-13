
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private MeshRenderer renderer;
    private MeshFilter meshFilter;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    int vertexIndex = 0;
    
    List<Vector3> vertices = new List<Vector3>();
    List<int> indices = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    
    bool [,,] voxelCheck = new bool[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkDepth];
    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        
        InitVoxelCheck();
        CreateVoxelChunk();
        CreateMesh();
    }

    void InitVoxelCheck()
    {

            for (int y = 0; y < VoxelData.chunkHeight; y++)
            {
                for (int x = 0; x < VoxelData.chunkWidth; x++)
                {
                    for (int z = 0; z < VoxelData.chunkDepth; z++)
                    {
                        voxelCheck[x, y, z] = true;
                    }
                }
            }
    }

    bool IsCheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 ||
            z > VoxelData.chunkDepth - 1)
            return false;
        return voxelCheck[x, y, z];
    }
    void CreateVoxelChunk()
    {
        for (int y = 0; y < VoxelData.chunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.chunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.chunkDepth; z++)
                {
                    AddVoxelChunk(new Vector3(x, y, z));
                }
            }
        }
    }
    void AddVoxelChunk(Vector3 pos)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!IsCheckVoxel(pos + VoxelData.faceChecks[i]))
            {
                    // 처음에 vertex 정보를 Index정보를 바탕으로 먼저 그리고 vertexIndex를 넣는다.
                    vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 0]]);
                    vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 1]]);
                    vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 2]]);
                    vertices.Add(pos + VoxelData.VoxelVertes[VoxelData.VoxelIndex[i, 3]]);
                    uvs.Add(VoxelData.VoxelUv[0]);
                    uvs.Add(VoxelData.VoxelUv[1]);
                    uvs.Add(VoxelData.VoxelUv[2]);
                    uvs.Add(VoxelData.VoxelUv[3]);
                    indices.Add(vertexIndex);
                    indices.Add(vertexIndex + 1);
                    indices.Add(vertexIndex + 2);
                    indices.Add(vertexIndex + 2);
                    indices.Add(vertexIndex + 1);
                    indices.Add(vertexIndex + 3);
                    vertexIndex += 4;
            }
        }
        
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
    void Update()
    {
        
    }
}
