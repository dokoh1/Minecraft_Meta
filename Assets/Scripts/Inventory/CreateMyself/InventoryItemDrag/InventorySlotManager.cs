using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//클릭->드래그용 아이템 프리팹 복제생성
public class InventorySlotManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //현재 슬롯에 들어있는 블록의 종류
    public BlockTypeEnum blockID;
    //아이템 이미지
    public Sprite icon;
    //드래그할 때 생성될 프리팹
    public GameObject draggableItemPrefab;
    public Transform draggableItemParent;
    
    //복사본을 따로 저장할 변수
    private GameObject _dragClone;
    
    //인벤토리 내의 슬롯에 마우스 클릭 이벤트가 들어왔을 때 실행되는 메소드
    public void OnBeginDrag(PointerEventData eventData)
    {
        //드래그용 프리팹 생성하여 마우스 커서를 따라감
        _dragClone = Instantiate(draggableItemPrefab, draggableItemParent);
        _dragClone.transform.position = Input.mousePosition;
        
        //정보 복사
        DraggableItem draggableItem = _dragClone.GetComponent<DraggableItem>();
        draggableItem.blockID = blockID;
        draggableItem.icon = icon;
        
        //이미지 연결
        Image image = _dragClone.GetComponent<Image>();
        image.sprite = icon;
        image.raycastTarget = false;
        
        //생성된 프리팹 이미지의 사이즈 조절
        RectTransform rt = _dragClone.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 40);
    }

    
    public void OnDrag(PointerEventData eventData)
    {
        if (_dragClone != null)
        {
            _dragClone.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_dragClone != null)
        {
            Destroy(_dragClone);
        }
    }
}
