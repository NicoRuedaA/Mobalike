using MMORPG.UI;
using MMORPG.Inventory;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class AddBubbleFrogToInventory
{
    [MenuItem("Tools/MMORPG/Add Bubble Frog To First Slot")]
    public static void AddItem()
    {
        GameObject dummyItem = GameObject.Find("DummyItem_0");
        if (dummyItem == null)
        {
            Debug.LogError("Could not find DummyItem_0 in the scene.");
            return;
        }

        DraggableItemUI draggableItem = dummyItem.GetComponent<DraggableItemUI>();
        if (draggableItem == null)
        {
            Debug.LogError("DummyItem_0 does not have a DraggableItemUI component.");
            return;
        }

        ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/_Project/ScriptableObjects/Items/BubbleFrog.asset");
        if (itemData == null)
        {
            Debug.LogError("Could not load BubbleFrog ItemData.");
            return;
        }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Icons/BubbleFrog.png");
        Sprite sprite = null;
        if (assets != null)
        {
            foreach (Object asset in assets)
            {
                if (asset is Sprite)
                {
                    sprite = (Sprite)asset;
                    break;
                }
            }
        }

        if (sprite == null)
        {
            Debug.LogError("Could not load BubbleFrog Sprite.");
            return;
        }

        draggableItem.itemData = itemData;

        Image image = dummyItem.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            EditorUtility.SetDirty(image);
        }
        else
        {
            Debug.LogError("DummyItem_0 does not have an Image component.");
            return;
        }

        EditorUtility.SetDirty(draggableItem);
        EditorSceneManager.MarkSceneDirty(dummyItem.scene);

        Debug.Log("Successfully added Bubble Frog to DummyItem_0.");
    }
}
