using UnityEngine;
using UnityEngine.UI;

public class ToolbarMirror : MonoBehaviour
{
    public static ToolbarMirror Instance;

    public InventorySlotPre[] playerToolbarSlots; // 실제 툴바 (화면 하단)

    private void Awake()
    {
        Instance = this;
    }

    public GameObject inventoryItemPrefab; //프리팹 연결
    public void SyncToolbarSlot(int index, Sprite sprite, byte id)
    {
        if (index < 0 || index >= playerToolbarSlots.Length) return;

        InventorySlotPre slotPre = playerToolbarSlots[index];
        
        slotPre.itemID = id;
        slotPre.hasBlock = true;

        // 기존 아이콘 제거
        foreach (Transform child in slotPre.transform)
        {
            Destroy(child.gameObject);
        }

        //복제
        GameObject newItem = Instantiate(inventoryItemPrefab, slotPre.transform);
        newItem.transform.localPosition = Vector3.zero;

        Destroy(newItem.GetComponent<InventoryItem>());
        // 아이콘 세팅
        Image img = newItem.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = sprite;
            img.raycastTarget = false;
        }
        /*
        slot.hasBlock = true;
        slot.itemID = id;

        if (slot.icon == null)
            slot.icon = slot.GetComponentInChildren<Image>();

        slot.icon.sprite = sprite;
        slot.icon.enabled = true;
        */
    }
}