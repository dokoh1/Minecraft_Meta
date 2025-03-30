using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//클릭->드래그용 아이템 복제생성
public class InventorySlotManager : MonoBehaviour, IPointerDownHandler
{
    //현재 슬롯에 들어있는 블록의 종류
    public BlockTypeEnum blockID;
    //아이템 이미지
    public Sprite icon;
    //드래그할 때 생성될 프리팹
    public GameObject draggableItemPrefab;
    public Transform draggableItemParent;

    //인벤토리 내의 슬롯에 마우스 클릭 이벤트가 들어왔을 때 실행되는 메소드
    public void OnPointerDown(PointerEventData eventData)
    {
        //마우스 왼쪽 버튼이 클릭되었을 때 
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //드래그용 프리팹(복사된 것)을 생성하여 캔버스 위에 올림
            GameObject draggaed = Instantiate(draggableItemPrefab,draggableItemParent);
            //새로 만든 드래그 아이템을 마우스 커서 위에 위치시킴
            draggaed.transform.position = Input.mousePosition;
            
            //슬롯이 가지고 있던 정보를 꺼내서 새로 만든 DraggableItem 프리팹에 넣어주는 작업
            // 프리팹 안의 DraggableItem 스크립트를 가져온다.
            DraggableItem item = draggaed.GetComponent<DraggableItem>();
            
            //드래그된 아이템에게 현재 슬롯의 정보를 복사해서 넘겨주기
            item._blockID = blockID;
            item.icon = icon;
            
            //프리팹의 Image에 아이템 이미지를 적용해서 화면에 보이게 한다.
            draggaed.GetComponent<Image>().sprite = icon;
        }
    }
    
}
