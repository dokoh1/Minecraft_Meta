using UnityEngine;

//인벤토리 데이터 관리
public class InventoryManager : MonoBehaviour
{
   //public bool[] isFull;
   //public GameObject[] slots;

   









   /*
   public InventorySlot[] inventorySlots;
   public GameObject inventoryItemPrefab;
   public void AddItem(Item item)
   {
      for (int i = 0; i < inventorySlots.Length; i++)
      {
         InventorySlot slot = inventorySlots[i];
         InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
         if (itemInSlot == null)
         {
            SpawnNewItem(item, slot);
            return;
         }
      }
   }
*/
   /*
   void SpawnNewItem(Item item, InventorySlot slot)
   {
      GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
      InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
      //inventoryItem.InitializeItem(item);
   }
   */
}
