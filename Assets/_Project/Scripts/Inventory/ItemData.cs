using UnityEngine;

namespace MMORPG.Inventory
{
    public enum ItemType 
    { 
        Consumable, 
        Equipment 
    }

    public enum EquipSlot 
    { 
        None, 
        Head, 
        Chest, 
        Weapon, 
        Boots,
        Pants
    }

    [CreateAssetMenu(fileName = "NewItemData", menuName = "MMORPG/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public ItemType itemType;
        public EquipSlot equipSlot;
        public Sprite icon;

        [Header("Stats Bonus")]
        public int hpBonus;
        public int strBonus;
        public int agiBonus;
    }
}