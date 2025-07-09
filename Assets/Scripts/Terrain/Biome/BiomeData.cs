using UnityEngine;
using System.Collections.Generic;

public class BiomeData : MonoBehaviour
{
   public readonly Dictionary<BiomeTypeEnum, BiomeTypeData> BiomeTypeDictionary = new();
    private void Awake()
    {
        LoadBiomeTypes();
    }
    
    private void LoadBiomeTypes()
    {
        BiomeTypeData[] biomeTypes = Resources.LoadAll<BiomeTypeData>("BiomeTypes");
        foreach (var biomeType in biomeTypes)
        {
            if (System.Enum.TryParse(biomeType.biomeName, out BiomeTypeEnum biomeEnum))
            {
                BiomeTypeDictionary.Add(biomeEnum, biomeType);
            }
        }
    }
}
