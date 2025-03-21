using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Collections;
using System.Collections.Generic;
using TMPro;

public class InventorySlot : MonoBehaviour,IDropHandler
{
    private InventoryManager inventory;
    public int i;
    public TextMeshProUGUI amountText;
    public int amount;
    
    
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            //InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            //inventoryItem.parentAfterDrag = transform;
        }
    }

    void Start()
    {
        inventory = FindFirstObjectByType<InventoryManager>();
    }

    private void Update()
    {
        if (transform.childCount == 2)
        {
            //inventory.isFull[i] = false;
        }
    }
}
