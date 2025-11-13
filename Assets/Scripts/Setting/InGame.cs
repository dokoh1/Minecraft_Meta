using UnityEngine;

[CreateAssetMenu(fileName = "InGameSetting", menuName = "Minecraft/InGameSettings")]
public class InGame : ScriptableObject
{
    [Header("Game Data")] public string version = "0.0.0.01";
    [Header("View Settings")]
    public int viewDistance;
    public int loadDistance;
    public bool enableThreading = true;
    public bool enableAnimatedChunks = false;
    
    [Header("Controls")] [Range(0.1f, 10f)]
    public float mouseSensitivity;

    [Header("World Gen")]
    public int seed = 0;
}
