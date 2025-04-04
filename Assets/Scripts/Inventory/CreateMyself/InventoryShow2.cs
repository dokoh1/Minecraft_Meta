using System;
using Unity.VisualScripting;
using UnityEngine;

// i 키를 눌렀을 때 MainInventoryGroup 이 활성화되도록 하는 클래스
public class InventoryShow2 : MonoBehaviour
{
   //target이라는 변수를 통해 Canvas에 상속되어 있는 자식의 모든 컴포넌트에 접근할 수 있도록 한다. 
   public GameObject target;
   private bool _mouseLockHide = true;
   public PlayerMove player;
   private void Awake()
   {
      target.SetActive(false);
   }
   private void Update()
   {
      LookInventory();
   }
   
   //인벤토리 창이 켜지도록 하는 메소드
   private void LookInventory()
   {
      if (Input.GetKeyDown(KeyCode.I))
      {
         if (target.activeSelf)
         {
            if (!player._mouseLockHide)
               player._mouseLockHide = true;
            target.SetActive(false);
            player.PasueLock = false;
         }
         else if (target.activeSelf == false && player._inventoryLock)
         {
            if (player._mouseLockHide)
                  player._mouseLockHide = false;
            target.SetActive(true);
            player.PasueLock = true;
         }
      }
   }
}
