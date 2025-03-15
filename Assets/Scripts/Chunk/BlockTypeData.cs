using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BlockTypeData", menuName = "Block/BlockTypeData")]
public class BlockTypeData : ScriptableObject
{
    
    [FormerlySerializedAs("IsSolid")] public bool isSolid;
    [FormerlySerializedAs("BlockName")] public string blockName;
    [FormerlySerializedAs("BackFaceTexture")] public int backFaceTexture;
    [FormerlySerializedAs("FrontFaceTexture")] public int frontFaceTexture;
    [FormerlySerializedAs("TopFaceTexture")] public int topFaceTexture;
    [FormerlySerializedAs("BottomFaceTexture")] public int bottomFaceTexture;
    [FormerlySerializedAs("LeftFaceTexture")] public int leftFaceTexture;
    [FormerlySerializedAs("RightFaceTexture")] public int rightFaceTexture;
    
    // public BlockTypeData(string name, bool solid, int side, int top, int bottom)
    // {
    //     BlockName = name;
    //     IsSolid = solid;
    //     BackFaceTexture = side;
    //     FrontFaceTexture = side;
    //     TopFaceTexture = top;
    //     BottomFaceTexture = bottom;
    //     LeftFaceTexture = side;
    //     RightFaceTexture = side;
    // }
    //
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