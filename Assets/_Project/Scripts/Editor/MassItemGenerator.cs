using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using MobaGameplay.Inventory;

public class MassItemGenerator : EditorWindow
{
    [System.Serializable]
    public class ItemJsonData
    {
        public string name;
        public string slot;
        public string filename;
    }

    [MenuItem("Tools/MobaGameplay/Generate Mass Wiki Items")]
    public static void GenerateMassWikiItems()
    {
        string jsonPath = "Assets/_Project/Art/Icons/Equipment/items_data.json";
        if (!File.Exists(jsonPath))
        {
            Debug.LogError("items_data.json not found. Run the python scraper first.");
            return;
        }

        string jsonContent = File.ReadAllText(jsonPath);
        string wrappedJson = "{\"items\": " + jsonContent + "}";
        ItemDataList parsedData = JsonUtility.FromJson<ItemDataList>(wrappedJson);

        if (parsedData == null || parsedData.items == null)
        {
            Debug.LogError("Failed to parse JSON.");
            return;
        }

        string soPath = "Assets/_Project/ScriptableObjects/Items/Equipment";
        if (!Directory.Exists(soPath))
        {
            Directory.CreateDirectory(soPath);
            AssetDatabase.Refresh();
        }

        int count = 0;
        foreach (var itemInfo in parsedData.items)
        {
            string imgPath = "Assets/_Project/Art/Icons/Equipment/" + itemInfo.filename;
            
            // Update texture settings to Sprite (2D and UI)
            TextureImporter importer = AssetImporter.GetAtPath(imgPath) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(imgPath);
            Sprite itemSprite = null;
            foreach (var asset in assets)
            {
                if (asset is Sprite)
                {
                    itemSprite = (Sprite)asset;
                    break;
                }
            }

            // Map string slot to Enum
            EquipSlot parsedSlot = EquipSlot.None;
            if (itemInfo.slot.Contains("Chest")) parsedSlot = EquipSlot.Chest;
            else if (itemInfo.slot.Contains("Weapon")) parsedSlot = EquipSlot.Weapon;
            else if (itemInfo.slot.Contains("Boots")) parsedSlot = EquipSlot.Boots;

            // Generate SO
            string safeFilename = string.Join("", itemInfo.name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "");
            string assetPath = $"{soPath}/{safeFilename}.asset";

            ItemData existingData = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            if (existingData == null)
            {
                existingData = ScriptableObject.CreateInstance<ItemData>();
                AssetDatabase.CreateAsset(existingData, assetPath);
            }

            existingData.itemName = itemInfo.name;
            existingData.itemType = ItemType.Equipment;
            existingData.equipSlot = parsedSlot;
            existingData.icon = itemSprite;

            // Random stats for testing
            existingData.hpBonus = Random.Range(10, 100);
            existingData.strBonus = Random.Range(1, 20);
            existingData.agiBonus = Random.Range(1, 20);

            EditorUtility.SetDirty(existingData);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Successfully generated {count} Mass Wiki Items! Check {soPath}.");
    }

    [System.Serializable]
    private class ItemDataList
    {
        public List<ItemJsonData> items;
    }
}
