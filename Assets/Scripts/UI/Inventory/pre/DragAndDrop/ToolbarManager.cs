using UnityEngine;
using UnityEngine.UI;

public class ToolbarManager : MonoBehaviour
{
   public RectTransform highlight;                     // 하이라이트 프레임
   public InventorySlotPre[] toolbarSlots;                // 9개의 슬롯
   
   private int currentSlotIndex = 0;                   // 현재 선택된 슬롯 인덱스
   private float _initSetting = 50; 

   private void Start()
   {
      // 시작할 때 하이라이트 위치 설정
      if (highlight != null && toolbarSlots.Length > 0)
      {
         Vector3 worldPos = toolbarSlots[currentSlotIndex].transform.position;
         float x = worldPos.x + _initSetting;
         float y = worldPos.y + _initSetting;
         highlight.position = new Vector3(x,y,highlight.position.z);
      }
   }
   private void Update()
   {
      for (int i = 0; i < 9; i++)
      {
         if (Input.GetKeyDown(KeyCode.Alpha1 + i))
         {
            SelectSlot(i);
            
            /*
            currentSlotIndex = i;

            if (highlight != null && currentSlotIndex < toolbarSlots.Length)
            {
               highlight.position = toolbarSlots[currentSlotIndex].transform.position;
            }

            Debug.Log($"🔹 선택된 슬롯: {currentSlotIndex + 1}, 아이템 ID: {toolbarSlots[currentSlotIndex].itemID}");
            */
         }
      }
   }

   public void SelectSlot(int index)
   {
      
      if(index < 0 || index >= toolbarSlots.Length) return;
      currentSlotIndex = index;
      //슬롯의 월드 좌표 기준으로 위치 설정
      Vector3 worldPos = toolbarSlots[currentSlotIndex].transform.position;
      highlight.position = worldPos;
      //highlight.position = toolbarSlots[currentSlotIndex].transform.position;
      //항상 맨 위로 올리기 (렌더 순서상 최상단)
      highlight.SetAsLastSibling();
      Debug.Log($"[Toolbar] 선택된 슬롯: {currentSlotIndex + 1}, 아이템 ID: {toolbarSlots[currentSlotIndex].itemID}");
   }
   

   // 외부에서 현재 선택된 블럭 ID를 가져올 수 있도록
   public byte GetSelectedItemID()
   {
      return toolbarSlots[currentSlotIndex].itemID;
   }

   public InventorySlotPre GetActiveSlot()
   {
      return toolbarSlots[currentSlotIndex];
   }
}


/*
public class ToolbarManager : MonoBehaviour
{
   public RectTransform highlight;
   public ItemSlot[] itemSlots;

   public BlockData blockData;  // BlockData를 참조!
   private int slotIndex = 0;

   private void Start()
   {
      foreach (ItemSlot slot in itemSlots)
      {
         BlockTypeEnum blockType = (BlockTypeEnum)slot.itemID;
         if (blockData.BlockTypeDictionary.ContainsKey(blockType))
         {
            //slot.icon.sprite = blockData.BlockTypeDictionary[blockType].icon; // ⬅ 아래 참고!
         }
         else
         {
            Debug.LogWarning($"블럭 ID {slot.itemID} 가 BlockTypeDictionary에 없습니다.");
         }
      }

      highlight.position = itemSlots[slotIndex].icon.transform.position;
   }

   // 선택된 슬롯의 아이템 ID 반환
   public byte GetSelectedItemID()
   {
      return itemSlots[slotIndex].itemID;
   }
}
*/

/*
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ToolbarManager : MonoBehaviour
{
   BlockTypeData blockTypeData;
   public PlayerMove player;
   
   public RectTransform highlight;
   public ItemSlot[] itemSlots;
   int slotIndex = 0;

   private void Start()
   {
      blockTypeData = GameObject.Find("BlockTypeData").GetComponent<BlockTypeData>();

      foreach (ItemSlot slot in itemSlots)
      {
         slot.icon.sprite = BlockTypeData.blocktypes[slot.itemID].icon;
         
      }
   }
   
   public byte GetSelectedItemID()
   {
      return itemSlots[slotIndex].itemID;
   }

   
}
*/

/*
[System.Serializable]
public class ItemSlot
{
   public byte itemID;
   public Image icon;
}
*/