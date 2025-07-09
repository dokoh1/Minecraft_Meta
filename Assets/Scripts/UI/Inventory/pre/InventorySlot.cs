using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public bool hasBlock = false;
    public byte itemID;
    
    public int slotIndex;
    public void SetBlock(Sprite sprite, byte id)
    {
        if (icon != null)
        {
            icon = GetComponentInChildren<Image>();
        }
        icon.sprite = sprite;
        icon.enabled = true;
        hasBlock = true;
        itemID = id;
        
        ToolbarMirror.Instance.SyncToolbarSlot(slotIndex, sprite, id);
    }
}

//이 클래스는 인벤토리 창에 있는 9개의 Toolbar와
//인벤토리 창을 닫아도 플레이 도중에 계속 하단에 표시가 되는 Hotbar 를 연동하는 클래스이다.
/*
public class InventorySlot : MonoBehaviour
{
    public GameObject Item;
    //public Item currentItem;
    public Image icon;
    public bool hasBlock = false;

    public void SetBlock(Sprite blockIcon)
    {
        icon.sprite = blockIcon;
        icon.enabled = true;
        hasBlock = true;
    }

    public void ClearBlock()
    {
        icon.sprite = null;
        icon.enabled = false;
        hasBlock = false;
    }
}
*/


/*
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
*/