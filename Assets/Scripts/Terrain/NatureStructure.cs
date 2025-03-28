using System.Collections.Generic;
using UnityEngine;

public class NatureStructure
{
    public void MakeTree(Vector3 position, Queue<VoxelCondition> queue, BiomeTypeData biometype)
    {
        int height = (int)(biometype.maxTrunkHeight *
                           CustomNoise.Get2DPerlin(new Vector2(position.x, position.z), biometype.trunkOffest, biometype.trunkScale));
        if (height < biometype.minTrunkHeight)
            height = biometype.minTrunkHeight;

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelCondition(new Vector3(position.x, position.y + i, position.z), BlockTypeEnum.Wood));
        }

        for (int x = -3; x < 4; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                for (int z = -3; z < 4; z++)
                {
                    queue.Enqueue(new VoxelCondition(new Vector3(position.x + x, position.y + height + y, position.z + z),
                        BlockTypeEnum.Leave));
                }
            }
        }
        
        
    }
}
 
