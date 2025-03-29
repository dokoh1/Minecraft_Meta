using UnityEngine;

//각 slot의 정보를 불러오는 클래스
public class ToolbarSlotManager : MonoBehaviour
{
    private ItemTypeData2 _itemTypeData2;
    public BlockTypeEnum slotEnum;
    public bool hasBlock;
    private Sprite _itemSprite;
    
    void Update()
    {
        //_itemSprite = _itemTypeData2.ItemSprite;
    }
}
