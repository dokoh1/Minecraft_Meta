using System;
using Unity.VisualScripting;
using UnityEngine;

// i 키를 눌렀을 때 MainInventoryGroup 이 활성화되도록 하는 클래스
public class InventoryShow2 : MonoBehaviour
{
   //target이라는 변수를 통해 Canvas에 상속되어 있는 자식의 모든 컴포넌트에 접근할 수 있도록 한다. 
   public GameObject target;
   
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
         target.SetActive(!target.activeSelf);
      }
   }
}
