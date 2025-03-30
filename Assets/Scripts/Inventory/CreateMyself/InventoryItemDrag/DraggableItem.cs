using UnityEngine;
using UnityEngine.EventSystems;

//드래그 앤 드롭 인터렉션
//드래그 중 아이템 정보를 들고 다니면서 UI 이미지가 마우스를 따라다니고,
//드롭될 때 정보를 넘겨주는 역할
//InventorySlotManager가 전달해준 정보를 들고 마우스를 따라다닐 수 있음.
//슬롯이 알고 있는 정보를 복사해서 가져다가 세팅함.
public class DraggableItem : MonoBehaviour,IBeginDragHandler,IDragHandler, IEndDragHandler
{
    //드래그 시작 전 
    //드래그되고 있는 블록의 종류
    public BlockTypeEnum _blockID;
    //블록 아이템의 이미지
    public Sprite icon;
    
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 시 호출됨!
    }
    
    //드래그 중일 때 마우스입력의 포지션을 계속 불러와줌.
    public void OnDrag(PointerEventData eventData)
    {
        //드래그되고 있는 오브젝트를 마우스 커서 위에 위치시킴
        transform.position = Input.mousePosition;
    }
    
    //드래그가 끝났을 때 드롭 위치에 정보 전달
    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag("ToolbarSlot"))
        {
            //드롭한 대상에서 ToolbarSlotManager스크립트를 불러옴
            ToolbarSlotManager slot = eventData.pointerEnter.GetComponent<ToolbarSlotManager>();
            if (slot != null)
            {
                //툴바 슬롯에 내가 들고 있던 아이템 정보를 넘겨줌.
                slot.SetSlotItem(icon, _blockID);
            }
        }
        //드래그가 끝났기 때문에 드래그용 복사본 오브젝트 제거
        Destroy(gameObject);
    }
    
}
