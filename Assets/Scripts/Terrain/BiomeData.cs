using UnityEngine;
using System.Collections.Generic;

public enum BiomeTypeEnum
{
    SnowField,
    Hills,
    Forest,
    Desert,
}
public class BiomeData : MonoBehaviour
{
   public readonly Dictionary<BiomeTypeEnum, BiomeTypeData> BiomeTypeDictionary = new();
    private void Awake()
    {
        LoadBiomeTypes();
    }

    // Update is called once per frame
    private void LoadBiomeTypes()
    {
        BiomeTypeData[] biomeTypes = Resources.LoadAll<BiomeTypeData>("BiomeTypes");
        foreach (var biometype in biomeTypes)
        {
            BiomeTypeEnum biomeEnum;
            if (System.Enum.TryParse(biometype.biomeName, out biomeEnum))
            {
                BiomeTypeDictionary.Add(biomeEnum, biometype);
            }
        }
    }
}
