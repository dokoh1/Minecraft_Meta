using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotClick : MonoBehaviour, IPointerClickHandler
{
    private InventorySlot slot;

    private void Awake()
    {
        slot = GetComponent<InventorySlot>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slot.hasBlock)
        {
            FindFirstObjectByType<InventoryShow>().AddItemToToolbar(slot.icon.sprite);
        }
    }
}
