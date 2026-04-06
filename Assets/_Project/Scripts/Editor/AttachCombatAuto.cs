using UnityEngine;
using UnityEditor;
using MobaGameplay.Combat;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AttachCombatAuto {
    static AttachCombatAuto() {
        EditorApplication.delayCall += RunSetup;
    }

    static void RunSetup() {
        if (SessionState.GetBool("CombatAttachDone_V2", false)) return;
        SessionState.SetBool("CombatAttachDone_V2", true);

        // 1. Attach Component to Player
        var activeScene = EditorSceneManager.GetActiveScene();
        bool dirty = false;
        
        GameObject[] roots = activeScene.GetRootGameObjects();
        foreach (var root in roots) {
            if (root.name == "Player") {
                if (root.GetComponent<MeleeCombat>() == null) {
                    root.AddComponent<MeleeCombat>();
                    Debug.Log("<color=cyan>Attached MeleeCombat to Player automatically.</color>");
                    dirty = true;
                }
            } else {
                Transform pTransform = root.transform.Find("Player");
                if (pTransform != null) {
                    if (pTransform.gameObject.GetComponent<MeleeCombat>() == null) {
                        pTransform.gameObject.AddComponent<MeleeCombat>();
                        Debug.Log("<color=cyan>Attached MeleeCombat to Player automatically.</color>");
                        dirty = true;
                    }
                }
            }
        }

        // 2. Ensure FBX Bake Settings are correct
        string modelPath = "Assets/_Project/Hook Punch.fbx";
        ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer != null) {
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips != null && clips.Length > 0) {
                bool needsSave = false;
                for (int i = 0; i < clips.Length; i++) {
                    if (!clips[i].keepOriginalOrientation || !clips[i].keepOriginalPositionXZ || !clips[i].keepOriginalPositionY) {
                        clips[i].keepOriginalOrientation = true;
                        clips[i].keepOriginalPositionXZ = true;
                        clips[i].keepOriginalPositionY = true;
                        clips[i].lockRootRotation = true;
                        clips[i].lockRootPositionXZ = true;
                        clips[i].lockRootHeightY = true;
                        needsSave = true;
                    }
                }
                if (needsSave) {
                    importer.clipAnimations = clips;
                    importer.SaveAndReimport();
                    Debug.Log("<color=cyan>Fixed Hook Punch FBX Bake Settings automatically.</color>");
                }
            }
        }

        if (dirty) {
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
        }
    }
}
