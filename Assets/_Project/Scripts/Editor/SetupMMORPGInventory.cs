using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using MMORPG.Inventory;
using MMORPG.UI;

namespace MMORPG.EditorScripts
{
    public class SetupMMORPGInventory
    {
        [MenuItem("Tools/Create MMORPG HUD")]
        public static void CreateHUD()
        {
            // Setup Player Object
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                player = new GameObject("Player");
                Undo.RegisterCreatedObjectUndo(player, "Create Player");
            }

            if (player.GetComponent<InventoryComponent>() == null)
                Undo.AddComponent<InventoryComponent>(player);

            if (player.GetComponent<EquipmentComponent>() == null)
                Undo.AddComponent<EquipmentComponent>(player);

            // Create Canvas Hierarchy
            GameObject canvasObj = new GameObject("HUD Canvas");
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create HUD Canvas");
            
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();

            // Setup HUD Manager
            HUDManager hudManager = Undo.AddComponent<HUDManager>(canvasObj);

            // Create Inventory Panel
            GameObject panelObj = new GameObject("InventoryPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(900, 600);
            
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            
            hudManager.inventoryPanel = panelObj;

            // Create Stats Placeholder Text
            GameObject statsObj = new GameObject("StatsText");
            statsObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0, 0);
            statsRect.anchorMax = new Vector2(0.2f, 1);
            statsRect.pivot = new Vector2(0, 0.5f);
            statsRect.offsetMin = new Vector2(20, 20);
            statsRect.offsetMax = new Vector2(0, -20);
            
            Text statsText = statsObj.AddComponent<Text>();
            statsText.text = "Character Stats\n\nHP: 0\nSTR: 0\nAGI: 0";
            statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statsText.fontSize = 24;
            statsText.color = Color.white;

            // Create Equipment Panel
            GameObject equipPanelObj = new GameObject("EquipmentPanel");
            equipPanelObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform equipRect = equipPanelObj.AddComponent<RectTransform>();
            equipRect.anchorMin = new Vector2(0.2f, 0);
            equipRect.anchorMax = new Vector2(0.4f, 1);
            equipRect.offsetMin = new Vector2(0, 20);
            equipRect.offsetMax = new Vector2(-20, -20);

            // Generate Equipment Slots
            string[] equipNames = { "Head", "Chest", "Weapon", "Pants", "Boots" };
            EquipmentSlotUI.EquipmentType[] equipTypes = { 
                EquipmentSlotUI.EquipmentType.Head, 
                EquipmentSlotUI.EquipmentType.Chest, 
                EquipmentSlotUI.EquipmentType.Weapon, 
                EquipmentSlotUI.EquipmentType.Pants,
                EquipmentSlotUI.EquipmentType.Boots
            };
            Vector2[] equipPositions = {
                new Vector2(0, 180),   // Head
                new Vector2(-80, 50),  // Chest
                new Vector2(80, 50),   // Weapon
                new Vector2(0, -80),   // Pants
                new Vector2(0, -210)   // Boots
            };

            for (int i = 0; i < equipNames.Length; i++)
            {
                GameObject slot = new GameObject($"EquipSlot_{equipNames[i]}");
                slot.transform.SetParent(equipPanelObj.transform, false);
                
                RectTransform slotRect = slot.AddComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(0.5f, 0.5f);
                slotRect.anchorMax = new Vector2(0.5f, 0.5f);
                slotRect.pivot = new Vector2(0.5f, 0.5f);
                slotRect.anchoredPosition = equipPositions[i];
                slotRect.sizeDelta = new Vector2(100, 100);
                
                Image slotImage = slot.AddComponent<Image>();
                slotImage.color = new Color(0.4f, 0.3f, 0.3f, 1f);

                EquipmentSlotUI equipSlotUI = slot.AddComponent<EquipmentSlotUI>();
                equipSlotUI.slotType = equipTypes[i];

                // Add a text label to identify the equipment slot
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(slot.transform, false);
                Text labelText = labelObj.AddComponent<Text>();
                labelText.text = equipNames[i];
                labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                labelText.alignment = TextAnchor.MiddleCenter;
                labelText.color = new Color(1f, 1f, 1f, 0.5f);
                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.sizeDelta = Vector2.zero;
            }

            // Create Grid Layout for Slots
            GameObject gridObj = new GameObject("InventoryGrid");
            gridObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.4f, 0);
            gridRect.anchorMax = new Vector2(1, 1);
            gridRect.offsetMin = new Vector2(0, 20);
            gridRect.offsetMax = new Vector2(-20, -20);
            
            GridLayoutGroup gridLayout = gridObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(80, 80);
            gridLayout.spacing = new Vector2(15, 15);
            gridLayout.childAlignment = TextAnchor.UpperLeft;

            // Generate dummy slot UI images
            for (int i = 0; i < InventoryComponent.InventorySize; i++)
            {
                GameObject slot = new GameObject($"Slot_{i}");
                slot.transform.SetParent(gridObj.transform, false);
                Image slotImage = slot.AddComponent<Image>();
                slotImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                slot.AddComponent<InventorySlotUI>();

                // Add a dummy draggable item to the first few slots for testing
                if (i < 3)
                {
                    GameObject itemObj = new GameObject($"DummyItem_{i}");
                    itemObj.transform.SetParent(slot.transform, false);
                    Image itemImage = itemObj.AddComponent<Image>();
                    itemImage.color = i == 0 ? Color.red : (i == 1 ? Color.blue : Color.green);
                    RectTransform itemRect = itemObj.GetComponent<RectTransform>();
                    itemRect.anchorMin = Vector2.zero;
                    itemRect.anchorMax = Vector2.one;
                    itemRect.sizeDelta = Vector2.zero;
                    
                    itemObj.AddComponent<DraggableItemUI>();
                }
            }

            // Select Canvas in Editor
            Selection.activeGameObject = canvasObj;
            Debug.Log("Created MMORPG HUD, connected to HUDManager, and populated Inventory/Equipment components on 'Player'.");
        }
    }
}
