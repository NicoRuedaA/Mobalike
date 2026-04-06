using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using MobaGameplay.UI;

namespace MobaGameplay.UI.Editor
{
    public static class HUDBuilder
    {
        [MenuItem("Mobalike/Tools/Build Player HUD Prefab")]
        public static void BuildPlayerHUD()
        {
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/UI"))
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");

            // Create Canvas
            GameObject canvasObj = new GameObject("PlayerHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add PlayerHUD script
            var playerHUD = canvasObj.AddComponent<PlayerHUD>();

            // --- Resource Bars (Bottom Center) ---
            GameObject barsContainer = new GameObject("ResourceBars", typeof(RectTransform));
            barsContainer.transform.SetParent(canvasObj.transform, false);
            RectTransform barsRect = barsContainer.GetComponent<RectTransform>();
            barsRect.anchorMin = new Vector2(0.5f, 0f);
            barsRect.anchorMax = new Vector2(0.5f, 0f);
            barsRect.pivot = new Vector2(0.5f, 0f);
            barsRect.anchoredPosition = new Vector2(0, 50);
            barsRect.sizeDelta = new Vector2(400, 80);

            var playerHUDSO = new SerializedObject(playerHUD);

            // Health Bar
            GameObject healthBarObj = CreateResourceBar("HealthBar", barsContainer.transform, new Color(0.2f, 0.8f, 0.2f));
            healthBarObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);
            playerHUDSO.FindProperty("healthBar").objectReferenceValue = healthBarObj.GetComponent<ResourceBarUI>();

            // Mana Bar
            GameObject manaBarObj = CreateResourceBar("ManaBar", barsContainer.transform, new Color(0.2f, 0.4f, 0.9f));
            manaBarObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
            manaBarObj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 20); // slightly thinner
            playerHUDSO.FindProperty("manaBar").objectReferenceValue = manaBarObj.GetComponent<ResourceBarUI>();

            // --- Ability Slots (Bottom Right) ---
            GameObject abilitiesContainer = new GameObject("Abilities", typeof(RectTransform));
            abilitiesContainer.transform.SetParent(canvasObj.transform, false);
            RectTransform abRect = abilitiesContainer.GetComponent<RectTransform>();
            abRect.anchorMin = new Vector2(1f, 0f);
            abRect.anchorMax = new Vector2(1f, 0f);
            abRect.pivot = new Vector2(1f, 0f);
            abRect.anchoredPosition = new Vector2(-50, 50);
            abRect.sizeDelta = new Vector2(300, 80);
            
            var hl = abilitiesContainer.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 20;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childControlWidth = false;
            hl.childControlHeight = false;

            GameObject slot1Obj = CreateAbilitySlot("AbilitySlot_0", abilitiesContainer.transform);
            playerHUDSO.FindProperty("slot1").objectReferenceValue = slot1Obj.GetComponent<AbilitySlotUI>();

            GameObject slot2Obj = CreateAbilitySlot("AbilitySlot_1", abilitiesContainer.transform);
            playerHUDSO.FindProperty("slot2").objectReferenceValue = slot2Obj.GetComponent<AbilitySlotUI>();

            GameObject slot3Obj = CreateAbilitySlot("AbilitySlot_2", abilitiesContainer.transform);
            playerHUDSO.FindProperty("slot3").objectReferenceValue = slot3Obj.GetComponent<AbilitySlotUI>();

            playerHUDSO.ApplyModifiedProperties();

            // Save Prefab
            string prefabPath = "Assets/_Project/Prefabs/UI/PlayerHUD.prefab";
            PrefabUtility.SaveAsPrefabAsset(canvasObj, prefabPath);
            Object.DestroyImmediate(canvasObj);
            
            Debug.Log($"Successfully built Player HUD prefab at {prefabPath}");
        }

        private static GameObject CreateResourceBar(string name, Transform parent, Color fillCol)
        {
            GameObject barObj = new GameObject(name, typeof(RectTransform), typeof(ResourceBarUI));
            barObj.transform.SetParent(parent, false);
            RectTransform rt = barObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 30);

            // Background
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(barObj.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = Vector2.zero;
            bgObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Fill
            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(barObj.transform, false);
            RectTransform fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one; fillRt.sizeDelta = Vector2.zero;
            Image fillImg = fillObj.GetComponent<Image>();
            fillImg.color = fillCol;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;

            // Text
            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(barObj.transform, false);
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one; textRt.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 18;
            tmp.color = Color.white;
            tmp.text = "100 / 100";
            
            // Assign references
            var rbUI = barObj.GetComponent<ResourceBarUI>();
            var so = new SerializedObject(rbUI);
            so.FindProperty("fillImage").objectReferenceValue = fillImg;
            so.FindProperty("valueText").objectReferenceValue = tmp;
            so.ApplyModifiedProperties();

            return barObj;
        }

        private static GameObject CreateAbilitySlot(string name, Transform parent)
        {
            GameObject slotObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(AbilitySlotUI));
            slotObj.transform.SetParent(parent, false);
            RectTransform rt = slotObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);
            slotObj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(slotObj.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero; iconRt.anchorMax = Vector2.one; iconRt.sizeDelta = Vector2.zero;
            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.color = new Color(0.5f, 0.5f, 0.5f, 1f);

            // Cooldown Overlay
            GameObject cdObj = new GameObject("CooldownOverlay", typeof(RectTransform), typeof(Image));
            cdObj.transform.SetParent(slotObj.transform, false);
            RectTransform cdRt = cdObj.GetComponent<RectTransform>();
            cdRt.anchorMin = Vector2.zero; cdRt.anchorMax = Vector2.one; cdRt.sizeDelta = Vector2.zero;
            Image cdImg = cdObj.GetComponent<Image>();
            cdImg.color = new Color(0, 0, 0, 0.7f);
            cdImg.type = Image.Type.Filled;
            cdImg.fillMethod = Image.FillMethod.Radial360;
            cdImg.fillOrigin = (int)Image.Origin360.Top;

            // Cooldown Text
            GameObject textObj = new GameObject("CooldownText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(slotObj.transform, false);
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one; textRt.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 24;
            tmp.color = Color.white;
            tmp.text = "";

            // Assign references
            var slotUI = slotObj.GetComponent<AbilitySlotUI>();
            var so = new SerializedObject(slotUI);
            so.FindProperty("iconImage").objectReferenceValue = iconImg;
            so.FindProperty("cooldownOverlay").objectReferenceValue = cdImg;
            so.FindProperty("cooldownText").objectReferenceValue = tmp;
            so.ApplyModifiedProperties();

            return slotObj;
        }
    }
}