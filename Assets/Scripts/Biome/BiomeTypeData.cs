using UnityEngine;
using UnityEngine.Serialization;

public enum biomeTreeChoice
{
    tree,
    Cacti,
}
[CreateAssetMenu(fileName = "BiomeTypeData", menuName = "Minecraft/BiomeTypeData")]
public class BiomeTypeData : ScriptableObject
{
    //biome 이름
    public string biomeName;
    
    //PerlinNoise
    public int offset;
    public float scale;
    
    //기온
    public float temperature;
    
    //지형 높이
    public int terrainHeight;
    //지형 scale
    public float terrainScale;

    public BlockTypeEnum surfaceBlock;
    public BlockTypeEnum subSurfaceBlock;

    [FormerlySerializedAs("TreeChoice")] [Header("Trees")] 
    public biomeTreeChoice treeChoice;
    public BlockTypeEnum wood;
    
    public float treeZoneScale;

    [Range(0.1f, 1f)] 
    public float treeZoneThreshold;
    public float treePlaceScale;
    
    [Range(0.1f, 1f)]
    public float treePlaceThreshold;

    public bool placeMajor = true;

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