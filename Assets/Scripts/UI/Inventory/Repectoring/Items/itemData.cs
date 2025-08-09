using UnityEngine;

namespace UI.Inventory.Repectoring.Items{
    
    [CreateAssetMenu(fileName = "New Item", menuName = "Minecraft/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("인벤토리 아이템")]
        public string itemName;
        public Sprite itemIcon;
        
        [Header("인벤토리 아이템에 대응되는 실제 블록타입")]
        public BlockTypeEnum blockType;
        public bool isStackable;
        
    }
}
