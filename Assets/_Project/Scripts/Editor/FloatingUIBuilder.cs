using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using MobaGameplay.UI;
using MobaGameplay.Core;

namespace MobaGameplay.Editor
{
    public static class FloatingUIBuilder
    {
        [MenuItem("Mobalike/Tools/Build Floating UI")]
        public static void BuildFloatingUI()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/UI"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");
            }

            // 1. Create Canvas
            GameObject canvasObj = new GameObject("FloatingStatusBar");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<Billboard>();
            
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(2f, 0.4f); // 2 meters width, 0.4 height in world space

            // 2. Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            // 3. Health Bar
            GameObject hpObj = new GameObject("HealthFill");
            hpObj.transform.SetParent(bgObj.transform, false);
            Image hpImage = hpObj.AddComponent<Image>();
            hpImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // LoL Green
            hpImage.type = Image.Type.Filled;
            hpImage.fillMethod = Image.FillMethod.Horizontal;
            hpImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            
            RectTransform hpRect = hpObj.GetComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(0f, 0.3f); // Top 70%
            hpRect.anchorMax = new Vector2(1f, 1f);
            hpRect.sizeDelta = Vector2.zero;
            hpRect.anchoredPosition = Vector2.zero;

            // 4. Mana Bar
            GameObject manaObj = new GameObject("ManaFill");
            manaObj.transform.SetParent(bgObj.transform, false);
            Image manaImage = manaObj.AddComponent<Image>();
            manaImage.color = new Color(0.2f, 0.5f, 0.9f, 1f); // LoL Blue
            manaImage.type = Image.Type.Filled;
            manaImage.fillMethod = Image.FillMethod.Horizontal;
            manaImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            
            RectTransform manaRect = manaObj.GetComponent<RectTransform>();
            manaRect.anchorMin = new Vector2(0f, 0f); // Bottom 30%
            manaRect.anchorMax = new Vector2(1f, 0.3f);
            manaRect.sizeDelta = Vector2.zero;
            manaRect.anchoredPosition = Vector2.zero;

            // 5. Setup Script
            var statusBar = canvasObj.AddComponent<FloatingStatusBar>();
            var so = new SerializedObject(statusBar);
            so.FindProperty("healthFill").objectReferenceValue = hpImage;
            so.FindProperty("manaFill").objectReferenceValue = manaImage;
            so.ApplyModifiedProperties();

            // 6. Save Prefab
            string prefabPath = "Assets/_Project/Prefabs/UI/FloatingStatusBar.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(canvasObj, prefabPath);
            Object.DestroyImmediate(canvasObj);

            // 7. Attach to all BaseEntities in Scene
            BaseEntity[] entities = Object.FindObjectsOfType<BaseEntity>();
            int attachedCount = 0;
            
            foreach (var ent in entities)
            {
                if (ent.GetComponentInChildren<FloatingStatusBar>() == null)
                {
                    GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(savedPrefab);
                    inst.transform.SetParent(ent.transform, false);
                    inst.transform.localPosition = new Vector3(0, 2.5f, 0); // Position above character head
                    attachedCount++;
                }
            }

            Debug.Log($"FloatingStatusBar built successfully. Attached to {attachedCount} entities in scene.");
        }
    }
}
