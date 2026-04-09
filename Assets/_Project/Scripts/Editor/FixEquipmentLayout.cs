using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

namespace MobaGameplay.Editor
{
    public class FixEquipmentLayout
    {
        [MenuItem("Tools/MobaGameplay/Fix Equipment Layout")]
        public static void FixLayout()
        {
            GameObject equipPanelObj = GameObject.Find("EquipmentPanel");
            if (equipPanelObj == null)
            {
                Debug.LogWarning("EquipmentPanel not found in the scene.");
                return;
            }

            // Remove VerticalLayoutGroup if exists
            VerticalLayoutGroup vlg = equipPanelObj.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                Undo.DestroyObjectImmediate(vlg);
            }

            // Remove Gloves
            Transform gloves = equipPanelObj.transform.Find("EquipSlot_Gloves");
            if (gloves != null)
            {
                Undo.DestroyObjectImmediate(gloves.gameObject);
            }

            // Set positions
            SetSlotPos(equipPanelObj, "EquipSlot_Head", new Vector2(0, 180));
            SetSlotPos(equipPanelObj, "EquipSlot_Chest", new Vector2(-80, 50));
            SetSlotPos(equipPanelObj, "EquipSlot_Weapon", new Vector2(80, 50));
            SetSlotPos(equipPanelObj, "EquipSlot_Pants", new Vector2(0, -80));
            SetSlotPos(equipPanelObj, "EquipSlot_Boots", new Vector2(0, -210));

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Equipment Layout Fixed");
        }

        private static void SetSlotPos(GameObject panel, string slotName, Vector2 pos)
        {
            Transform slot = panel.transform.Find(slotName);
            if (slot != null)
            {
                RectTransform rect = slot.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Undo.RecordObject(rect, "Fix Layout");
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = pos;
                }
            }
        }
    }
}
