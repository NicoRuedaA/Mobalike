using UnityEngine;
using UnityEditor;
using MMORPG.Inventory;
using MMORPG.UI;

public class AssignDummyItemData
{
    [MenuItem("Tools/Assign Dummy ItemData")]
    public static void CreateAndAssign()
    {
        string folderPath = "Assets/_Project/ScriptableObjects/Items";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            System.IO.Directory.CreateDirectory(Application.dataPath + folderPath.Substring(6));
            AssetDatabase.Refresh();
        }

        ItemData helmet = CreateItem("Helmet", ItemType.Equipment, EquipSlot.Head);
        ItemData chestplate = CreateItem("Chestplate", ItemType.Equipment, EquipSlot.Chest);
        ItemData sword = CreateItem("Sword", ItemType.Equipment, EquipSlot.Weapon);

        AssignToDummy("DummyItem_0", helmet);
        AssignToDummy("DummyItem_1", chestplate);
        AssignToDummy("DummyItem_2", sword);
        
        AssetDatabase.SaveAssets();
        Debug.Log("Dummy Items Created and Assigned successfully.");
    }

    private static ItemData CreateItem(string name, ItemType type, EquipSlot slot)
    {
        string path = $"Assets/_Project/ScriptableObjects/Items/{name}.asset";
        ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (item == null)
        {
            item = ScriptableObject.CreateInstance<ItemData>();
            item.itemName = name;
            item.itemType = type;
            item.equipSlot = slot;
            AssetDatabase.CreateAsset(item, path);
        }
        return item;
    }

    private static void AssignToDummy(string gameObjectName, ItemData data)
    {
        GameObject go = GameObject.Find(gameObjectName);
        if (go != null)
        {
            DraggableItemUI draggable = go.GetComponent<DraggableItemUI>();
            if (draggable != null)
            {
                draggable.itemData = data;
                EditorUtility.SetDirty(draggable);
            }
        }
    }
}
