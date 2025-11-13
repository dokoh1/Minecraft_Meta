using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 텍스처의 알파로 구름 마스크를 읽어 타일로 만들어 플레이어 위로 반복 배치하는 시스템
/// </summary>
public class Clouds : MonoBehaviour
{
    public int cloudHeight = 70;

    [SerializeField] private Texture2D cloudTexture;
    [SerializeField] private Material cloudMaterial;
    
    private bool[,] _cloudData;
    
    private int _cloudTextureWidth;
    private int _cloudTileSize;
    
    private Vector3Int _offset;
    private Dictionary<Vector2Int, GameObject> _clouds = new();
    
    /// <summary>
    /// 텍스쳐 크기/타일 크기/오프셋 계산
    /// 구름 레이어 중심을 월드 중앙 (TerrainMiddle, cloudHeight, TerrainMiddle)에 둠.
    /// LoadCloudData()로 마스크 만들고, CreateCloud()로 최초 타일 생성.
    /// </summary>
    private void Start()
    {
        _cloudTextureWidth =  cloudTexture.width;
        _cloudTileSize = VoxelData.ChunkWidth;
        _offset = new Vector3Int( - (_cloudTextureWidth / 2), 0, - (_cloudTextureWidth / 2));
        
        transform.position = new Vector3(VoxelData.TerrainMiddle, cloudHeight, VoxelData.TerrainMiddle);
        
        LoadCloudData();
        CreateCloud();
    }
    /// <summary>
    /// cloudMaterial.color.a = MinecraftTerrain.Instance.globalLight
    /// → 낮일수록 알파↑, 밤일수록 알파↓ (페이드 효과)
    /// </summary>
    private void Update()
    {
        Color color = cloudMaterial.color;
        color.a = MinecraftTerrain.Instance.globalLight;
        cloudMaterial.color = color;
    }

    
    /// <summary>
    /// _cloudData[x,z] = (cloudTexture(x,z).a > 0)로 불투명 픽셀 마스크 생성.
    /// </summary>
    private void LoadCloudData()
    {
        _cloudData = new bool[_cloudTextureWidth, _cloudTextureWidth];
        Color[] cloudTex = cloudTexture.GetPixels();

        for (int x = 0; x < _cloudTextureWidth; x++)
        {
            for (int z = 0; z < _cloudTextureWidth; z++)
                _cloudData[x, z] = (cloudTex[z * _cloudTextureWidth + x].a > 0);
        }
    }
    /// <summary>
    /// 텍스처 영역을 타일 크기(cloudTileSize)로 스캔하며, 각 타일에 대응하는 메쉬를 만들어 생성
    /// </summary>
    private void CreateCloud()
    {
        for (int x = 0; x < _cloudTextureWidth; x += _cloudTileSize)
        {
            for (int z = 0; z < _cloudTextureWidth; z += _cloudTileSize)
            {
                Vector3 position = new Vector3(x, cloudHeight, z);
                _clouds.Add(PosFromV3(position), CreateCloudTile(AddCloudMeshData(x, z), position));
            }
        }
    }

    /// <summary>
    /// 빈 GameObject에 meshFilter/meshRenderer 붙이고 메터리얼/메쉬 세팅, 부모를 구름 레이어로 설정.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private GameObject CreateCloudTile(Mesh mesh, Vector3 position)
    {
        GameObject cloudTile = new GameObject();
        cloudTile.transform.position = position;
        cloudTile.transform.parent = transform;
        MeshFilter mF = cloudTile.AddComponent<MeshFilter>();
        MeshRenderer mR = cloudTile.AddComponent<MeshRenderer>();
        
        mR.sharedMaterial = cloudMaterial;
        mF.mesh = mesh;
        
        return cloudTile;
    }

    /// <summary>
    /// 플레이어 주변으로 타일들을 스냅 이동.
    /// position = player.pos + (x,0,z) + _offset
    /// position.x/z를 타일 크기 배수로 FloorToMultiple
    /// cloudPosition = PosFromV3(position)로 키 계산 → 해당 타일의 transform.position을 이동
    /// </summary>
    public void UpdateCloud()
    {
        for (int x = 0; x < _cloudTextureWidth; x += _cloudTileSize)
        {
            for (int z = 0; z < _cloudTextureWidth; z += _cloudTileSize)
            {
                Vector3 position = MinecraftTerrain.Instance.player.position + new Vector3(x, 0, z) + _offset;
                position = new Vector3(FloorToMultiple(position.x, _cloudTileSize), cloudHeight, FloorToMultiple(position.z, _cloudTileSize));
                Vector2Int cloudPosition = PosFromV3(position);
                
                _clouds[cloudPosition].transform.position = position;
            }
        }
    }
    // 주어진 값보다 작거나 같은 가장 가까운 배수로 내림(타일 그리드 스냅)
    private int FloorToMultiple(float value, int multiple) 
    {
        return Mathf.FloorToInt(value / (float)multiple) * multiple;
    }
    // x, z를 _cloudeTextureWidth로 래핑 -> 0..width-1 범위 키로 변환.
    private Vector2Int PosFromV3(Vector3 pos)
    {
        return new Vector2Int(CoordFromFloat(pos.x), CoordFromFloat(pos.z));
    }
    
    //음수/큰 값도 안전하게 모듈 좌표로 전환
    private int CoordFromFloat(float value)
    {
        return Mathf.FloorToInt(Mathf.Repeat(value, _cloudTextureWidth));
    }
    /// <summary>
    /// 한 타일(예: 16×16) 내부에서 _cloudData[x+innerX, z+innerZ]가 true인 픽셀마다 수평 쿼드 1개 생성
    /// 정점 4개, 삼각형 2개, 노멀 4개(Vector3.down) 추가.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private Mesh AddCloudMeshData(int x, int z)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;
        for (int innerX = 0; innerX < _cloudTileSize; innerX++)
        {
            for (int innerZ = 0; innerZ < _cloudTileSize; innerZ++)
            {
                int xVal = x + innerX;
                int zVal = z + innerZ;

                if (_cloudData[xVal, zVal])
                {
                    vertices.Add(new Vector3(innerX, 0, innerZ));
                    vertices.Add(new Vector3(innerX, 0, innerZ + 1));
                    vertices.Add(new Vector3(innerX + 1, 0, innerZ + 1));
                    vertices.Add(new Vector3(innerX + 1, 0, innerZ));
                
                    for (int i = 0; i < 4; i++)
                        normals.Add(Vector3.down);
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
