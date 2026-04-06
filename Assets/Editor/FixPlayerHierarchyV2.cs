using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixPlayerHierarchyV2 : EditorWindow
{
    [MenuItem("Tools/Fix Player Hierarchy V2 (Definitive)")]
    public static void FixHierarchy()
    {
        // Find the Player GameObject in the active scene
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject not found in the active scene.");
            return;
        }

        // 1. Create a "Visuals" container if it doesn't exist
        Transform visualsRoot = player.transform.Find("Visuals");
        if (visualsRoot == null)
        {
            GameObject visualsGo = new GameObject("Visuals");
            visualsGo.transform.SetParent(player.transform);
            visualsGo.transform.localPosition = Vector3.zero;
            visualsGo.transform.localRotation = Quaternion.identity;
            visualsRoot = visualsGo.transform;
            Debug.Log("Created 'Visuals' root object.");
        }

        // 2. Find the Animator (wherever it ended up in the previous step)
        Animator currentAnimator = player.GetComponentInChildren<Animator>();
        if (currentAnimator == null)
        {
            Debug.LogError("No Animator found anywhere in the Player or its children. Please add one to 'Visuals'.");
            return;
        }

        // 3. Move Animator to Visuals if it's not already there
        if (currentAnimator.gameObject != visualsRoot.gameObject)
        {
            Animator newAnimator = visualsRoot.gameObject.GetComponent<Animator>();
            if (newAnimator == null) newAnimator = visualsRoot.gameObject.AddComponent<Animator>();
            EditorUtility.CopySerialized(currentAnimator, newAnimator);

            // Also move the AnimationEventReceiver if it's there
            var oldReceiver = currentAnimator.gameObject.GetComponent<MobaGameplay.Animation.AnimationEventReceiver>();
            if (oldReceiver != null)
            {
                var newReceiver = visualsRoot.gameObject.GetComponent<MobaGameplay.Animation.AnimationEventReceiver>();
                if (newReceiver == null) newReceiver = visualsRoot.gameObject.AddComponent<MobaGameplay.Animation.AnimationEventReceiver>();
                EditorUtility.CopySerialized(oldReceiver, newReceiver);
                DestroyImmediate(oldReceiver);
            }

            DestroyImmediate(currentAnimator);
            Debug.Log("Moved Animator to the 'Visuals' root object.");
        }
        else
        {
            // Ensure receiver exists on Visuals
            var receiver = visualsRoot.gameObject.GetComponent<MobaGameplay.Animation.AnimationEventReceiver>();
            if (receiver == null) visualsRoot.gameObject.AddComponent<MobaGameplay.Animation.AnimationEventReceiver>();
        }

        // 4. Move all visual parts (Mesh and Bones) inside "Visuals"
        // Common names in StarterAssets/Mixamo
        string[] visualNames = { "Geometry", "Skeleton", "Armature", "Mesh", "Armature_Mesh" };
        
        // We collect them first to avoid modifying the hierarchy while iterating
        System.Collections.Generic.List<Transform> toMove = new System.Collections.Generic.List<Transform>();

        foreach (Transform child in player.transform)
        {
            if (child == visualsRoot) continue; // Skip the visuals root itself

            bool isVisualPart = false;
            foreach (string name in visualNames)
            {
                if (child.name.Contains(name) || (child.GetComponent<SkinnedMeshRenderer>() != null))
                {
                    isVisualPart = true;
                    break;
                }
            }

            if (isVisualPart)
            {
                toMove.Add(child);
            }
        }

        foreach (Transform t in toMove)
        {
            t.SetParent(visualsRoot);
            Debug.Log($"Moved '{t.name}' inside 'Visuals' so the Animator can control it.");
        }

        // 5. Save scene changes
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("SUCCESS: Hierarchy fixed! The Animator and Bones are now together under 'Visuals'.");
    }
}