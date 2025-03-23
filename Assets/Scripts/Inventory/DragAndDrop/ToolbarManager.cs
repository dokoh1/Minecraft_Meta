using UnityEngine;
using UnityEngine.UI;

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

[System.Serializable]
public class ItemSlot
{
   public byte itemID;
   public Image icon;
}