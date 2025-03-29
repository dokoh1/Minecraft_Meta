using UnityEngine;

//각 slot의 정보를 불러오는 클래스
public class ToolbarSlotManager : MonoBehaviour
{
    //처음에는 아무것도 들어있지 않음
    private ItemTypeData2 _itemTypeData2;
    public BlockTypeEnum slotEnum;
    public bool hasBlock = false;
    private Sprite _choosedItemSprite;
    
    public void BlockImageMatch(Sprite sprite, BlockTypeEnum id)
    {
        if (_choosedItemSprite != null)
        {
            _choosedItemSprite = _itemTypeData2.ItemSprite;
        }
        hasBlock = true;
        slotEnum = id;
    }
}
