using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Text;

public class DebugText : MonoBehaviour
{
    public GameObject terrainGameObject;
    
    private MinecraftTerrain _terrain;
    private TextMeshProUGUI _text;
    private float _frame;
    private float _timer;

    private int halfVoxels;

    private int halfChunks;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _terrain = terrainGameObject.GetComponent<MinecraftTerrain>();
        halfVoxels = VoxelData.TerrainInVoxelSize / 2;
        halfChunks = VoxelData.TerrainSize / 2;
    }

    // Update is called once per frame
    void Update()
    {
        Transform player = _terrain.player.transform;
        Coord coord = _terrain._playerCoord;
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine("CopyCraft");
        sb.AppendLine($"{_frame} fps");
        sb.AppendLine($"XYZ : {Mathf.FloorToInt(player.position.x) - halfVoxels} / {Mathf.FloorToInt(player.position.y)} / {Mathf.FloorToInt(player.position.z) - halfVoxels}");
        sb.AppendLine($"Chunk : {coord.X_int - halfChunks} / {coord.Z_int - halfChunks}");
        sb.AppendLine("");
        _text.text = sb.ToString();
        // 1초마다 프레임을 출력
        if (_timer > 1f)
        {
            _frame = (int)(1f / Time.unscaledDeltaTime);
            _timer = 0;
        }
        else
        {
            _timer += Time.deltaTime;
        }
    }
}

