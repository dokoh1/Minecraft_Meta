using UnityEngine;

[CreateAssetMenu(fileName = "InGameSetting", menuName = "Minecraft/InGameSettings")]
public class InGame : ScriptableObject
{
    [Header("View Settings")]
    public int viewDistance;
    public int loadDistance;

    [Header("Controls")] [Range(0.1f, 10f)]
    public float mouseSensitivity;

    [Header("World Gen")]
    public int seed = 0;
}
