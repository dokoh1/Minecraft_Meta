using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//각 slot의 정보를 불러오는 클래스
public class ToolbarSlotManager : MonoBehaviour, IDropHandler
{
    //처음에는 아무것도 들어있지 않음
    private ItemTypeData2 _itemTypeData2;
    public BlockTypeEnum slotEnum;
    public bool hasBlock = false;
    private Sprite _choosedItemSprite;
    //슬롯의 슬롯 번호를 저장하는 변수
    public int slotIndex;
    //동기화 그룹 설정용으로 static배열 추가
    public static ToolbarSlotManager[] PlayerSlots = new ToolbarSlotManager[18];
    
    //드롭시 생성될 프리팹
    public GameObject itemDisplayPrefab;
    //슬롯에 위치할 아이템을 저장하는 변수
    private GameObject _itemDisplay;
    
    //Start()에서 자동으로 자기 등록
    private void Start()
    {
        PlayerSlots[slotIndex] = this;
    }
    
    //슬롯에 드롭이 끝났을 떄의 이벤트
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("드롭 성공");

        if (InventorySlotManager.dragClone == null)
        {
            Debug.Log("dragClone이 null입니다.");
            return;
        }
        //드래그된 오브젝트에 DraggableItem 스크립트가 붙어있는지 확인
        DraggableItem dragged = InventorySlotManager.dragClone.GetComponent<DraggableItem>();
        if (dragged == null)
        {
            Debug.Log("DraggableItem 스크립트 없음");
            return;
        }
        
        //기존에 있던 디스플레이 제거
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        //부모를 이 슬롯으로 설정
        //InventorySlotManager.dragClone.transform.SetParent(transform);
        //InventorySlotManager.dragClone.transform.localPosition = Vector3.zero;
        
        //디스플레이 프리팹 생성 및 부모를 슬롯으로 지정
        _itemDisplay = Instantiate(itemDisplayPrefab,transform);
        _itemDisplay.GetComponent<Image>().sprite = dragged.icon;
        //위치를 정확히 슬롯 중앙으로 올 수 있게끔
        _itemDisplay.transform.localPosition = Vector3.zero;
        
        //슬롯 상태 업데이트
        slotEnum = dragged.blockID;
        hasBlock = true;
        
        //아이템 스프라이트 저장
        _choosedItemSprite = dragged.icon;
        
        //드래그 프리팹 삭제
        Destroy(InventorySlotManager.dragClone);
        InventorySlotManager.dragClone = null;
        SetSlotItem(_choosedItemSprite, slotEnum);
    }
    
    public void SetSlotItem(Sprite sprite, BlockTypeEnum id)
    {
        //기존에 있던 자식 이미지 제거(중복 방지)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        _itemDisplay = Instantiate(itemDisplayPrefab,transform);
        _itemDisplay.GetComponent<Image>().sprite = sprite;
        _itemDisplay.transform.localPosition = Vector3.zero;
        _choosedItemSprite = sprite;
        
        // if (_choosedItemSprite != null)
        // {
        //     _choosedItemSprite = _itemTypeData2.ItemSprite;
        //     GetComponent<Image>().sprite = _choosedItemSprite;
        // }
        
        slotEnum = id;
        hasBlock = true;
        
        //아이템 추가 시 동일 슬롯 index에 있는 동기화 슬롯도 갱신
        for (int i = 0; i < PlayerSlots.Length; i++)
        {
            if(i == slotIndex) continue;

            if (PlayerSlots[i] != null && PlayerSlots[i].slotEnum != id)
            {
                //중복 호출 방지용 메소드
                PlayerSlots[i].SetSync(sprite, id);
            }
        }
    }
    
    public void SetSync(Sprite sprite, BlockTypeEnum id)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if (sprite != null)
        {
            _choosedItemSprite = sprite;
            GetComponent<Image>().sprite = _choosedItemSprite;
        }
        
        hasBlock = true;
        slotEnum = id;
    }
}
