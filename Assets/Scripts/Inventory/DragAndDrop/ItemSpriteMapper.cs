using UnityEngine;
using System.Collections.Generic;

public class ItemSpriteMapper : MonoBehaviour
{
    public static ItemSpriteMapper Instance;

    [System.Serializable]
    public class SpriteMapping
    {
        public Sprite sprite;
        public BlockTypeEnum blockType;
    }

    [Header("Sprite to BlockType Mapping")]
    public List<SpriteMapping> mappings = new();

    private Dictionary<Sprite, BlockTypeEnum> spriteToEnum = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;

        foreach (var map in mappings)
        {
            if (map.sprite != null && !spriteToEnum.ContainsKey(map.sprite))
                spriteToEnum.Add(map.sprite, map.blockType);
        }
    }

    public BlockTypeEnum GetBlockTypeFromSprite(Sprite sprite)
    {
        if (spriteToEnum.TryGetValue(sprite, out var result))
        {
            return result;
        }

        Debug.LogWarning($"Sprite 매핑 없음: {sprite?.name}");
        return BlockTypeEnum.Air; // 기본값
    }
}

