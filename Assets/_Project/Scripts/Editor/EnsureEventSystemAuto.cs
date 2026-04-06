using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class EnsureEventSystemAuto {
    static EnsureEventSystemAuto() {
        EditorApplication.delayCall += RunCheck;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode) {
        RunCheck();
    }

    static void RunCheck() {
        
        
        EventSystem es = Object.FindObjectOfType<EventSystem>();
        if (es == null) {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();
            Debug.Log("<color=green>EventSystem was missing. Created automatically to enable UI interaction.</color>");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        } else {
            if (es.GetComponent<InputSystemUIInputModule>() == null) {
                var oldModule = es.GetComponent<StandaloneInputModule>();
                if (oldModule != null) Object.DestroyImmediate(oldModule);
                es.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("<color=green>EventSystem updated to use New Input System.</color>");
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}
