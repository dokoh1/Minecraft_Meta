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
    public Load[] loads;
}

[System.Serializable]
public class Load
{
    public string blockName;
    public BlockTypeEnum blockType;
    public int MinHeight;
    public int MaxHeight;
    public float Scale;
    public float threshold;
    public float noiseOffset;

}