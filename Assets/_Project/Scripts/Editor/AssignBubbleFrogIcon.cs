using UnityEngine;
using UnityEditor;
using MMORPG.Inventory;

public class AssignBubbleFrogIcon
{
    [MenuItem("Tools/MMORPG/Assign Bubble Frog Icon")]
    public static void AssignIcon()
    {
        string iconPath = "Assets/_Project/Art/Icons/BubbleFrog.png";
        string itemPath = "Assets/_Project/ScriptableObjects/Items/BubbleFrog.asset";

        // Import the asset and force update
        AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceUpdate);

        // Change texture type to Sprite
        TextureImporter importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
        else
        {
            Debug.LogError("Failed to get TextureImporter for " + iconPath);
            return;
        }

        // Load the sprite
        Sprite sprite = null;
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(iconPath);
        foreach (var asset in assets)
        {
            if (asset is Sprite s)
            {
                sprite = s;
                break;
            }
        }

        if (sprite == null)
        {
            Debug.LogError("Failed to load Sprite at " + iconPath);
            return;
        }

        // Load the ItemData ScriptableObject
        ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(itemPath);
        if (itemData == null)
        {
            Debug.LogError("Failed to load ItemData at " + itemPath);
            return;
        }

        // Assign the sprite
        itemData.icon = sprite;

        // Save the asset
        EditorUtility.SetDirty(itemData);
        AssetDatabase.SaveAssets();

        Debug.Log("Successfully assigned Bubble Frog icon to BubbleFrog item data.");
    }
}
