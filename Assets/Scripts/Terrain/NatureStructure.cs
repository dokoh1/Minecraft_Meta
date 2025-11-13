using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class NatureStructure
{
    /// <summary>
    /// 나무 한 그루를 구성하는 복셀 변경 작업들을 큐에 담아 반환하는 빌더
    /// </summary>
    /// <param name="position"></param>
    /// <param name="biometype"></param>
    /// <param name="noise"></param>
    /// <returns></returns>
    public Queue<VoxelCondition> MakeTree(Vector3 position, BiomeTypeData biometype, CustomNoise noise)
    {
        Queue<VoxelCondition> queue = new();
        int height = (int)(biometype.maxTrunkHeight *
                           noise.Get2DPerlin(new Vector2(position.x, position.z), biometype.trunkOffest, biometype.trunkScale));
        if (height < biometype.minTrunkHeight)
            height = biometype.minTrunkHeight;

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelCondition(new Vector3(position.x, position.y + i, position.z), biometype.wood));
        }
        
        if (biometype.treeChoice == BiomeTreeChoice.Cacti)
            return queue;
        
        for (int x = -2; x < 3; x++)
        {
            for (int z = -2; z < 3; z++)
            {
                int xAbs = Mathf.Abs(x);
                int zAbs = Mathf.Abs(z);
                if (xAbs == 2 || zAbs == 2)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        queue.Enqueue(new VoxelCondition(new Vector3(position.x + x, position.y + height + y, position.z + z),
                            BlockTypeEnum.Leave));
                    }
                }
                else
                {
                    for (int y = 0; y < 4; y++)
                    {
                        queue.Enqueue(new VoxelCondition(new Vector3(position.x + x, position.y + height + y, position.z + z),
                            BlockTypeEnum.Leave));
                    }
                }
            }
        }
        return queue;
    }
}
 
