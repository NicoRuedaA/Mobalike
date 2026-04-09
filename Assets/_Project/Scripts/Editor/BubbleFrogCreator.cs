using UnityEditor;
using UnityEngine;
using MobaGameplay.Inventory;

namespace MobaGameplay.Editor
{
    public static class BubbleFrogCreator
    {
        [MenuItem("Tools/MobaGameplay/Create Bubble Frog Item")]
        public static void CreateBubbleFrog()
        {
            string path = "Assets/_Project/ScriptableObjects/Items/BubbleFrog.asset";
            
            // Check if already exists
            ItemData existingItem = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (existingItem != null)
            {
                Debug.Log($"Bubble Frog already exists at {path}");
                Selection.activeObject = existingItem;
                return;
            }

            // Create new instance
            ItemData newItem = ScriptableObject.CreateInstance<ItemData>();
            
            // Assign fields based on Wiki (Healer/Protector focus)
            newItem.itemName = "Bubble Frog";
            newItem.itemType = ItemType.Consumable; // As requested, though it's technically a Grip in Supervive
            newItem.equipSlot = EquipSlot.None;
            
            // logical stats based on "Your Heals apply a shield equal to 25% of the heal... +12% Outbound Healing"
            newItem.hpBonus = 120; // 120 HP for the 12% outbound healing flavor
            newItem.agiBonus = 25; // 25 Agi for the 25% shield flavor
            newItem.strBonus = 0;

            // Save the asset
            AssetDatabase.CreateAsset(newItem, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Successfully created Bubble Frog at {path}");
            
            // Highlight it in the editor
            Selection.activeObject = newItem;
        }
    }
}
