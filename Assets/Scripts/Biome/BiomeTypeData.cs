using UnityEngine;

[CreateAssetMenu(fileName = "BiomeTypeData", menuName = "Minecraft/BiomeTypeData")]
public class BiomeTypeData : ScriptableObject
{
    //biome 이름
    public string biomeName;

    //기본 지형 높이
    public int solidGroundHeight; 
    //지형 높이
    public int terrainHeight;
    //지형 scale
    public float terrainScale;
    
    [Header("Trees")] 
    public float treeZoneScale;

    [Range(0.1f, 1f)] 
    public float treeZoneThreshold;
    public float treePlaceScale;
    
    [Range(0.1f, 1f)]
    public float treePlaceThreshold;

    public int maxTrunkHeight;
    public int minTrunkHeight;
    
    public float trunkScale;
    public float trunkOffest;
    public Load[] loads;
}

[System.Serializable]
public class Load
{
    public string blockName;
    public BlockTypeEnum blockType;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;

}