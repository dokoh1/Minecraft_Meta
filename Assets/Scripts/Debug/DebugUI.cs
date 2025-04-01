using TMPro;
using UnityEngine;
using System.Text;

public class DebugText : MonoBehaviour
{
    public GameObject terrainGameObject;
    
    private MinecraftTerrain _terrain;
    private TextMeshProUGUI _text;
    
    private float _frame;
    private float _timer;
    
    private int _halfVoxels;
    private int _halfChunks;

    private void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _terrain = terrainGameObject.GetComponent<MinecraftTerrain>();
        _halfVoxels = VoxelData.TerrainInVoxelSize / 2;
        _halfChunks = VoxelData.TerrainSize / 2;
    }
    
    private void Update()
    {
        Transform player = _terrain.player.transform;
        Coord coord = _terrain.PlayerCoord;
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine("CopyCraft");
        sb.AppendLine($"{_frame} fps");
        sb.AppendLine($"XYZ : {Mathf.FloorToInt(player.position.x) - _halfVoxels} / {Mathf.FloorToInt(player.position.y)} / {Mathf.FloorToInt(player.position.z) - _halfVoxels}");
        if (coord == null)
            return;
        sb.AppendLine($"Chunk : {coord.X - _halfChunks} / {coord.Z - _halfChunks}");
        sb.AppendLine("");
        
        _text.text = sb.ToString();
        if (_timer > 1f)
        {
            _frame = (int)(1f / Time.unscaledDeltaTime);
            _timer = 0;
        }
        else
            _timer += Time.deltaTime;
    }
}

