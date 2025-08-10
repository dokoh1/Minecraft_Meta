using System;
using UI.Inventory.Repectoring.Items;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//인벤토리의 각 슬롯을 관리하는 스크립트.
//슬롯이 어떤 아이템(ItemData)를 몇 개나 가지고 있는지, 그리고 슬롯의 UI(아이콘, 수량 텍스트)를 어떻게 보여줄지 관리
//개별 슬롯을 관리하는 스크립트이므로 인벤토리 전체를 관리하지는 않음
public class InventorySlot : MonoBehaviour
{
    //현재 슬롯이 어떤 아이템을 담고 있는지 정보를 담을 변수
    public ItemData currentItem;
    //현재 슬롯에 아이템이 몇 개나 쌓여있는지를 나타내는 숫자
    [HideInInspector] 
    public int ItemQuantity;

    [Header("UI Elements")] 
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    
    //다른 스크립트가 이 슬롯에 아이템을 지정할 때 호출할 함수
    public void SetItem(ItemData newItem, int quantity)
    {
        currentItem = newItem;
        ItemQuantity = quantity;
        UpdateSlotUI();
    }
    
    //다른 스크립트가 이 슬롯을 비울 때 호출할 함수
    public void ClearSlot()
    {
        currentItem = null;
        ItemQuantity = 0;
        UpdateSlotUI();
    }
    
    //인벤토리 슬롯의 모습을 새로고침하는 기능
    private void UpdateSlotUI(){
        
        if (currentItem == null)
        {
            itemIcon.enabled = false;
            quantityText.text = "";
        }
        else
        {
            itemIcon.enabled = true;
            itemIcon.sprite = currentItem.itemIcon;

            if (ItemQuantity > 1)
            {
                quantityText.text = ItemQuantity.ToString();
            }
            else
            {
                quantityText.text = "";
            }

        }
    }
}

