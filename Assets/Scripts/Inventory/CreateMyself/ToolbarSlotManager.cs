using UnityEngine;

public class ToolbarSlotManager : MonoBehaviour
{
    private ItemTypeData2 _itemTypeData2;
    public BlockTypeEnum slotEnum;
    public bool hasBlock;
    private Sprite _itemSprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _itemSprite = _itemTypeData2.ItemSprite;
    }
}
