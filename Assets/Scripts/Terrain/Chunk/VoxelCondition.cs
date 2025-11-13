using UnityEngine;

public class VoxelCondition
{
    public Vector3 Position;
    public BlockTypeEnum BlockType;

    public VoxelCondition(Vector3 position, BlockTypeEnum blockType)
    {
        Position = position;
        BlockType = blockType;
    }
}
