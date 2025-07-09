using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Minecraft/PlayerData")]
public class PlayerData : ScriptableObject
{
    public float reach;
    public float walkSpeed;
    public float runSpeed;
    public float mouseSpeed;
    public float jumpForce;
    public float flyForce;
    public float playerHeight;
}
