using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

//각 slot의 정보를 불러오는 클래스
public class ToolbarSlotManager : MonoBehaviour, IDropHandler
{
    //처음에는 아무것도 들어있지 않음
    private ItemTypeData2 _itemTypeData2;
    public BlockTypeEnum slotEnum;
    public bool hasBlock = false;
    private Sprite _choosedItemSprite;

    public GameObject itemDisplayPrefab;
    public void OnDrop(PointerEventData eventData)
    {
        //드래그된 오브젝트에 DraggableItem 스크립트가 붙어있는지 확인
        DraggableItem dragged = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (dragged != null)
        {
            //슬롯 이미지 변경
            GetComponent<Image>().sprite = dragged.icon;
            
            //ToolbarSlot의 상태를 업데이트
            slotEnum = dragged.blockID;
            hasBlock = true;
            
            //드래그 복사본 제거
            Destroy(dragged.gameObject);
        }
    }
    
    
    public void SetSlotItem(Sprite sprite, BlockTypeEnum id)
    {
        //기존에 있던 자식 이미지 제거(중복 방지)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        
        /*
        if (_choosedItemSprite != null)
        {
            _choosedItemSprite = _itemTypeData2.ItemSprite;
            GetComponent<Image>().sprite = _choosedItemSprite;
        }
        hasBlock = true;
        slotEnum = id;
        */
    }
}
