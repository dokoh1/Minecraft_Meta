using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.EventSystems;

//[CreateAssetMenu(fileName = "Inventory", menuName = "Inventory")]

public class InventoryShow : MonoBehaviour
{
   private bool ActiveFlag = false;
   private PointerEventData pointerEventData;
   public GameObject[] inventorySlot;
   public GameObject[] toolBar;
   private void Start()
   {
      // 부모는 활성화 유지, 모든 자식 비활성화
      gameObject.SetActive(true);
      for (int i = 0; i < transform.childCount; i++)
      {
         transform.GetChild(i).gameObject.SetActive(false);
      }
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.I))
      {
         ActiveFlag = !ActiveFlag;

         // 모든 자식을 켜거나 끄기
         for (int i = 0; i < transform.childCount; i++)
         {
            transform.GetChild(i).gameObject.SetActive(ActiveFlag);
            ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.beginDragHandler);
         }
      }
   }

   public void AddItemToToolbar(Sprite blockIcon)
   {
      foreach (GameObject slotObj in toolBar)
      {
         InventorySlot slot = slotObj.GetComponent<InventorySlot>();
         /*
         if (!slot.hasBlock)
         {
            //slot.SetBlock(blockIcon);
            return;
         }
         */
      }
   }
}


/*
public class InventoryShow : MonoBehaviour
{
   private bool ActiveFlag = false;
   private PointerEventData pointerEventData;
   public GameObject[] inventorySlot;
   public GameObject[] toolBar;
   private void Start()
   {
      // 부모는 활성화 유지, 모든 자식 비활성화
      gameObject.SetActive(true);
      for (int i = 0; i < transform.childCount; i++)
      {
         transform.GetChild(i).gameObject.SetActive(false);
      }
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.I))
      {
         ActiveFlag = !ActiveFlag;

         // 모든 자식을 켜거나 끄기
         for (int i = 0; i < transform.childCount; i++)
         {
            transform.GetChild(i).gameObject.SetActive(ActiveFlag);
            ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.beginDragHandler);
         }
      }
   }
}
*/



//45개의 인벤토리 slot을 InventoryManager에서 관리하기 위해 Inventory 배열을 public으로 선언
//public Inventory[] inventorySlots;
   
//private bool ActiveFlag = false;
/*
public void AddItem(Item item)
{
   for (int i = 0; i < inventorySlots.Length; i++)
   {
      //InventorySlots slot = inventorySlots[i];
   }
}
*/
/*
private void Start()
{
   gameObject.SetActive(ActiveFlag);
}

private void Update()
{
   if (Input.GetKeyDown(KeyCode.I))
   {
      if (ActiveFlag == false)
      {
         gameObject.SetActive(true);
         ActiveFlag = true;
      }
      else if (ActiveFlag == true)
      {
         gameObject.SetActive(false);
         ActiveFlag = false;
      }
   }
}
*/

