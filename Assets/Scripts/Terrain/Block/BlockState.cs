using UnityEngine;

public class BlockState
{
    public BlockTypeEnum BlockType;
    public float GlobalLightPercent;

    public BlockState()
    {
        BlockType = BlockTypeEnum.Air;
        GlobalLightPercent = 0f;
    }

    public BlockState(BlockTypeEnum blocktype)
    {
        BlockType = blocktype;
        GlobalLightPercent = 0f;
    }
}
