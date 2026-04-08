using UnityEngine;
using UnityEditor;
using TMPro;
using MobaGameplay.UI;

namespace MobaGameplay.Editor
{
    /// <summary>
    /// Genera automáticamente el UI de inicio del juego.
    /// </summary>
    public class CreateGameStartUI
    {
        [MenuItem("GameObject/MobaGameplay/Create Game Start UI", false, 20)]
        public static void CreateUI()
        {
            // Verificar si ya existe
            GameObject existingUI = GameObject.Find("GameStartUI");
            if (existingUI != null)
            {
                Selection.activeGameObject = existingUI;
                EditorUtility.DisplayDialog("Game Start UI", "Ya existe un GameStartUI en la escena.", "OK");
                return;
            }
            
            // Crear Canvas principal
            GameObject canvasObj = new GameObject("GameStartUI");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Panel de inicio
            GameObject panelObj = new GameObject("StartPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;
            
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f); // Fondo semi-transparente
            
            // Título
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.7f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(400, 60);
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "MOBA GAMEPLAY";
            titleText.fontSize = 48;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            
            // Info de Wave
            GameObject waveInfoObj = new GameObject("WaveInfo");
            waveInfoObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform waveRect = waveInfoObj.AddComponent<RectTransform>();
            waveRect.anchorMin = new Vector2(0.5f, 0.5f);
            waveRect.anchorMax = new Vector2(0.5f, 0.5f);
            waveRect.anchoredPosition = new Vector2(0, 20);
            waveRect.sizeDelta = new Vector2(400, 40);
            
            TextMeshProUGUI waveInfo = waveInfoObj.AddComponent<TextMeshProUGUI>();
            waveInfo.text = "Presiona START para comenzar";
            waveInfo.fontSize = 24;
            waveInfo.alignment = TextAlignmentOptions.Center;
            waveInfo.color = Color.yellow;
            
            // Info de Stats
            GameObject statsObj = new GameObject("StatsInfo");
            statsObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.5f, 0.45f);
            statsRect.anchorMax = new Vector2(0.5f, 0.45f);
            statsRect.anchoredPosition = Vector2.zero;
            statsRect.sizeDelta = new Vector2(400, 30);
            
            TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
            statsText.text = "Vidas: ❤ | Score: 0";
            statsText.fontSize = 20;
            statsText.alignment = TextAlignmentOptions.Center;
            statsText.color = Color.cyan;
            
            // Botón START
            GameObject buttonObj = new GameObject("StartButton");
            buttonObj.transform.SetParent(panelObj.transform, false);
            
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.25f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.25f);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.sizeDelta = new Vector2(200, 60);
            
            Button button = buttonObj.AddComponent<Button>();
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f); // Azul
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock()
            {
                normalColor = new Color(0.2f, 0.6f, 1f),
                highlightedColor = new Color(0.3f, 0.8f, 1f),
                pressedColor = new Color(0.1f, 0.4f, 0.8f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f
            };
            
            // Texto del botón
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "START";
            buttonText.fontSize = 28;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            
            // Agregar componente GameStartUI
            GameStartUI gameStartUI = canvasObj.AddComponent<GameStartUI>();
            
            // Asignar referencias por nombre (más seguro que por índice)
            SerializedObject serialized = new SerializedObject(gameStartUI);
            serialized.FindProperty("startPanel").objectReferenceValue = panelObj;
            serialized.FindProperty("startButton").objectReferenceValue = button;
            serialized.FindProperty("waveInfoText").objectReferenceValue = waveInfo;
            serialized.FindProperty("livesText").objectReferenceValue = statsText;
            serialized.FindProperty("scoreText").objectReferenceValue = statsText;
            serialized.ApplyModifiedProperties();
            
            Selection.activeGameObject = canvasObj;
            
            Debug.Log("[CreateGameStartUI] Game Start UI creada!");
            
            EditorUtility.DisplayDialog(
                "Game Start UI Creada",
                "UI de inicio creada:\n" +
                "- Panel semi-transparente\n" +
                "- Título 'MOBA GAMEPLAY'\n" +
                "- Info de Wave\n" +
                "- Info de Vidas y Score\n" +
                "- Botón START\n\n" +
                "El botón ya está conectado al GameStateManager.",
                "OK"
            );
        }
    }
}
