using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemClickMove : MonoBehaviour
{
   public Sprite item;
   List<GameObject> items = new List<GameObject>();

   public void OnMouseDown()
   {
      //왼쪽 마우스가 눌렸을 때 
      if (Input.GetMouseButtonDown(0))
      {
         //아이템 정보를 가져와서 
         
         //해당 아이템이 PlayerToolbar의 슬롯으로 이동한다. 
         
      }
      items.Add(this.gameObject);
   }
   
}
