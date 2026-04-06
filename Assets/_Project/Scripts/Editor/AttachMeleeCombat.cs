using UnityEngine;
using UnityEditor;
using MobaGameplay.Combat;

public class AttachMeleeCombat {
    [MenuItem("Tools/Attach Melee Combat to Player")]
    public static void Attach() {
        GameObject player = GameObject.Find("Player");
        if (player != null) {
            if (player.GetComponent<MeleeCombat>() == null) {
                player.AddComponent<MeleeCombat>();
                Debug.Log("MeleeCombat added to Player.");
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            } else {
                Debug.Log("MeleeCombat already exists on Player.");
            }
        } else {
            Debug.LogError("Player not found.");
        }
    }
}
