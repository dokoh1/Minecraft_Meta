using UnityEngine;

//PlayerToolbarSlot 1~9에 위치
//플레이어가 선택한 슬롯을 추적하고 해당 아이템 반환
public class ToolBarManager2 : MonoBehaviour

{
    //highlightFrame은 플레이어가 선택한 슬롯을 알려주고 강조해주는 용도이다.
    public RectTransform highlightFrame;
    //9개의 슬롯 배열을 선언하여 각각의 슬롯을 대입해준다. 
    public ToolbarSlotManager[] toolbarSlots = new ToolbarSlotManager[9];
    //현재 선택된 슬롯의 인덱스(highlightFrame이 게임 시작할 때 맨 앞 슬롯에 위치하므로 0으로 초기화)
    private int currentSlotIndex = 0; 
    //시작할 때 highlightFrame위치 조정할 값
    private float _initSetting = 50;
    
    private BlockTypeEnum[] _itemName = new BlockTypeEnum[9];
    
    //게임이 시작할 때의 초기 highlightFrame의 위치를 설정을 해준다.(보통은 맨 앞의 슬롯에 위치해줌)
    private void Start()
    {
        //만약에 highlightFrame이미지가 존재하고, toolbarSlots의 길이가 0보다 클때 
        //highlightFrame의 위치를 그에 맞게 조정을 해준다. 
        if (highlightFrame != null && toolbarSlots.Length > 0)
        {
            //toolbarSlot의 각 위치를 SlotPosition이라는 변수에 저장하여 highlightFrame의 위치에 새로 할당해준다.
            //highlightFrame의 위치 업데이트
            Vector3 worldPos = toolbarSlots[currentSlotIndex].transform.position;
            float x = worldPos.x + _initSetting;
            float y = worldPos.y + _initSetting;
            highlightFrame.position = new Vector3(x,y,highlightFrame.position.z); //new Vector3(SlotPosition.x,SlotPosition.y,SlotPosition.z);
        }
    }

    //플레이어가 1~9 번호키를 누를 때마다 자동으로 highlightFrame의 위치를 조정해준다.
    private void Update()
    {
        for (int i = 0; i < toolbarSlots.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectedSlot(i);
            }
        }
    }
    
    //highlightFrame의 위치를 조정한 이후에 해당 슬롯의 아이템 정보를 넘겨준다.
    public void SelectedSlot(int index)
    {
        //만약 들어온 인덱스가 0보다 작거나 toolbarSlots의 길이보다 크거나 같으면 다시 자신이 있던 자리로 돌아감.
        if (index < 0 || index >= toolbarSlots.Length) return;
        //이전의 슬롯 인덱스 번호에 새로 들어온 인덱스 번호를 대입하여 갱신해줌
        currentSlotIndex = index;
        //현재 선택되어있는 슬롯의 위치를 가져와서 변수에 저장
        Vector3 changedPos = toolbarSlots[currentSlotIndex].transform.position;
        highlightFrame.position = changedPos;
    }

    public BlockTypeEnum GetItemID()
    {
        return toolbarSlots[currentSlotIndex].slotEnum;
    } 
    
    public ToolbarSlotManager GetActiveSlot()
    {
        return toolbarSlots[currentSlotIndex];
    }
}

