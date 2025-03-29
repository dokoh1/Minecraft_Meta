using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-2000)]
public class BlockData : MonoBehaviour
{
    public readonly Dictionary<BlockTypeEnum, BlockTypeData> BlockTypeDictionary = new();
    public Material material;
    public Material transparentMaterial;
    public Material leaveMaterial;
    // public Material leavesMaterial;
    
    private void Awake()
    {
        LoadBlockTypes();
    }

    /// <summary>
    /// enum과 blockType의 blockname과 비교하여
    /// BlockType ScriptableObject 인스턴스 파일을 Dictionary에 저장
    /// </summary>
    private void LoadBlockTypes()
    {
        BlockTypeData[] blockTypes = Resources.LoadAll<BlockTypeData>("BlockTypes");
        foreach (var blocktype in blockTypes)
        {
            BlockTypeEnum blockEnum;
            if (System.Enum.TryParse(blocktype.blockName, out blockEnum))
            {
                BlockTypeDictionary.Add(blockEnum, blocktype);
            }
        }
    }
}
