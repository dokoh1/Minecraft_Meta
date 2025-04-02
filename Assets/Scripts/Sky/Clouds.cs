using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevelPhysics;

public class Clouds : MonoBehaviour
{
    public int cloudHeight = 70;

    [SerializeField] private Texture2D cloudTexture = null;
    [SerializeField] private Material cloudMaterial = null;
    [SerializeField] private MinecraftTerrain _terrain = null;
    
    private bool[,] cloudData;
    
    private int cloudTextureWidth;
    private int cloudTileSize;
    private Vector3Int offset;
    private Dictionary<Vector2Int, GameObject> _clouds = new Dictionary<Vector2Int, GameObject>();
    
    private void Start()
    {
        cloudTextureWidth =  cloudTexture.width;
        cloudTileSize = VoxelData.ChunkWidth;
        offset = new Vector3Int( - (cloudTextureWidth / 2), 0, - (cloudTextureWidth / 2));
        
        transform.position = new Vector3(VoxelData.TerrainMiddle, cloudHeight, VoxelData.TerrainMiddle);
        
        LoadCloudData();
        CreateCloud();
    }

    private void Update()
    {
        Color color = cloudMaterial.color;
        color.a = _terrain.globalLight;
        cloudMaterial.color = color;
    }

    // 불투명 인 곳을 cloudData에 담음
    private void LoadCloudData()
    {
        cloudData = new bool[cloudTextureWidth, cloudTextureWidth];
        Color[] cloudTex = cloudTexture.GetPixels();

        for (int x = 0; x < cloudTextureWidth; x++)
        {
            for (int z = 0; z < cloudTextureWidth; z++)
            {
                cloudData[x, z] = (cloudTex[z * cloudTextureWidth + x].a > 0);
            }
        }
    }

    private void CreateCloud()
    {
        for (int x = 0; x < cloudTextureWidth; x += cloudTileSize)
        {
            for (int z = 0; z < cloudTextureWidth; z += cloudTileSize)
            {
                Vector3 position = new Vector3(x, cloudHeight, z);
                _clouds.Add(PosFromV3(position), CreateCloudTile(AddCloudMeshData(x, z), position));
            }
        }
    }

    private GameObject CreateCloudTile(Mesh mesh, Vector3 position)
    {
        GameObject cloudTile = new GameObject();
        cloudTile.transform.position = position;
        cloudTile.transform.parent = transform;
        MeshFilter mF = cloudTile.AddComponent<MeshFilter>();
        MeshRenderer mR = cloudTile.AddComponent<MeshRenderer>();
        
        mR.material = cloudMaterial;
        mF.mesh = mesh;
        
        return cloudTile;
    }

    public void UpdateCloud()
    {
        for (int x = 0; x < cloudTextureWidth; x += cloudTileSize)
        {
            for (int z = 0; z < cloudTextureWidth; z += cloudTileSize)
            {
                Vector3 position = _terrain.player.position + new Vector3(x, 0, z) + offset;
                position = new Vector3(FloorToMultiple(position.x, cloudTileSize), cloudHeight, FloorToMultiple(position.z, cloudTileSize));
                Vector2Int cloudPosition = PosFromV3(position);
                
                _clouds[cloudPosition].transform.position = position;
            }
        }
    }
    private int FloorToMultiple(float value, int multiple) {
        return Mathf.FloorToInt(value / (float)multiple) * multiple;
    }

    private Vector2Int PosFromV3(Vector3 pos)
    {
        return new Vector2Int(CoordFromFloat(pos.x), CoordFromFloat(pos.z));
    }
    
    private int CoordFromFloat(float value)
    {
        return Mathf.FloorToInt(Mathf.Repeat(value, cloudTextureWidth));
    }

    private Mesh AddCloudMeshData(int x, int z)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;
        for (int InnerX = 0; InnerX < cloudTileSize; InnerX++)
        {
            for (int InnerZ = 0; InnerZ < cloudTileSize; InnerZ++)
            {
                int xVal = x + InnerX;
                int zVal = z + InnerZ;

                if (cloudData[xVal, zVal])
                {
                    vertices.Add(new Vector3(InnerX, 0, InnerZ));
                    vertices.Add(new Vector3(InnerX, 0, InnerZ + 1));
                    vertices.Add(new Vector3(InnerX + 1, 0, InnerZ + 1));
                    vertices.Add(new Vector3(InnerX + 1, 0, InnerZ));
                
                    for (int i = 0; i < 4; i++)
                    {
                        normals.Add(Vector3.down);
                    }
                    indices.Add(vertCount + 1);
                    indices.Add(vertCount);
                    indices.Add(vertCount + 2);
                    indices.Add(vertCount + 2);
                    indices.Add(vertCount);
                    indices.Add(vertCount + 3);
                
                    vertCount += 4;
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }

}
