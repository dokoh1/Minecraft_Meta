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
    //생성된 프리팹을 캔버스 UI의 최상단에 표시하기 위함.
    public Transform draggableItemParent;
    
    //복사본을 따로 저장할 변수
    //static으로 선언하는 이유는 Drop 이벤트를 가진 슬롯 스크립트에서 접근하기 위함
    public static GameObject dragClone;
    
    //슬롯이 아닌 다른 오브젝트에 _dragClone을 드랍할 경우 원복할 위치 백업용
    private Vector3 _startPosition;
    
    //슬롯이 아닌 다른 오브젝트에 Icon을 드랍할 경우 원복할 부모 백업용
    [HideInInspector] public Transform startParent;
    
    //인벤토리 내의 슬롯에 마우스 클릭 이벤트가 들어왔을 때 실행되는 메소드
    public void OnBeginDrag(PointerEventData eventData)
    {
        //드래그용 프리팹 생성하여 마우스 커서를 따라감
        dragClone = Instantiate(draggableItemPrefab, draggableItemParent);
        dragClone.transform.position = Input.mousePosition; 
        
        //정보 복사
        DraggableItem draggableItem = dragClone.GetComponent<DraggableItem>();
        draggableItem.blockID = blockID;
        draggableItem.icon = icon;
        
        //이미지 연결
        Image image = dragClone.GetComponent<Image>();
        image.sprite = icon;
        image.raycastTarget = false;
        
        //생성된 프리팹 이미지의 사이즈 조절
        RectTransform rt = dragClone.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 40);
        
        //백업용 포지션과 부모 위치 백업
        _startPosition = transform.position;
        startParent = transform.parent;
    }
    
    //드래그 중일 때 프리팹이 마우스 커서를 계속 따라다님
    public void OnDrag(PointerEventData eventData)
    {
        if (dragClone != null)
        {
            dragClone.transform.position = Input.mousePosition;
        }
    }

    //드래그가 끝났을 때 
    public void OnEndDrag(PointerEventData eventData)
    {
        //드롭이 끝난 지점 확인
        GameObject dropTarget = eventData.pointerEnter;
        
        //드래그 성공한 경우 ToolbarSlotManager.OnDrop이 처리해줌
        if (dropTarget != null && dropTarget.GetComponent<ToolbarSlotManager>() != null)
        {
            
        }
        //만약 드래그 실패한 경우에 프리팹을 삭제한다.
        else
        {
            //드롭 실패 시 dragClone 삭제
            if (dragClone != null)
            {
                Destroy(dragClone);
            }
        }
        dragClone = null;
        Debug.Log("드래그 끝남");
    }
}
