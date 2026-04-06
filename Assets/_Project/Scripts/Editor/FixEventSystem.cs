using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class FixEventSystem {
    [MenuItem("Tools/Fix UI Event System")]
    public static void Fix() {
        EventSystem es = Object.FindObjectOfType<EventSystem>();
        if (es == null) {
            GameObject esObj = new GameObject("EventSystem");
            es = esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();
            Debug.Log("<color=green>Created EventSystem with New Input System module.</color>");
        } else {
            if (es.GetComponent<InputSystemUIInputModule>() == null) {
                var oldModule = es.GetComponent<StandaloneInputModule>();
                if (oldModule != null) Object.DestroyImmediate(oldModule);
                es.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("<color=green>Updated EventSystem to use New Input System module.</color>");
            } else {
                Debug.Log("EventSystem is already correctly configured.");
            }
        }
    }
}
