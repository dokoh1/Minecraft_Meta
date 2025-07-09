using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BlockTypeData", menuName = "Minecraft/BlockTypeData")]
public class BlockTypeData : ScriptableObject
{
    public bool isSolid;
    public bool isDrawing;
    public bool isLeave;
    public float transparency;
    public string blockName;
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    
    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID");
                return 0;
        }
    }
}