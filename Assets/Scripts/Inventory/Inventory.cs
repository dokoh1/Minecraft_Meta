using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

//[CreateAssetMenu(fileName = "Inventory", menuName = "Inventory")]

public class Inventory : MonoBehaviour
{
   //45개의 인벤토리 slot을 InventoryManager에서 관리하기 위해 Inventory 배열을 public으로 선언
   //public Inventory[] inventorySlots;
   
   private bool ActiveFlag = false;
   /*
   public void AddItem(Item item)
   {
      for (int i = 0; i < inventorySlots.Length; i++)
      {
         //InventorySlots slot = inventorySlots[i];
      }
   }
*/
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
}


