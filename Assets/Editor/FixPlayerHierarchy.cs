using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixPlayerHierarchy : EditorWindow
{
    [MenuItem("Tools/Fix Player Hierarchy")]
    public static void FixHierarchy()
    {
        // Find the Player GameObject in the active scene
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject not found in the active scene.");
            return;
        }

        // Get the CharacterController
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogError("No CharacterController found on the Player.");
            return;
        }

        // Check if there is an Animator on the root. If so, move it.
        Animator rootAnimator = player.GetComponent<Animator>();
        Transform visualChild = GetVisualChild(player);
        
        if (rootAnimator != null)
        {
            // Move it to the child
            Animator newAnimator = visualChild.gameObject.GetComponent<Animator>();
            if (newAnimator == null) newAnimator = visualChild.gameObject.AddComponent<Animator>();
            EditorUtility.CopySerialized(rootAnimator, newAnimator);
            DestroyImmediate(rootAnimator);
            Debug.Log("Animator moved to child object: " + visualChild.name);
        }

        // Now, ensure the child object with the Animator has the AnimationEventReceiver
        Animator childAnimator = player.GetComponentInChildren<Animator>();
        if (childAnimator != null)
        {
            // Check for AnimationEventReceiver
            var eventReceiver = childAnimator.gameObject.GetComponent<MobaGameplay.Animation.AnimationEventReceiver>();
            if (eventReceiver == null)
            {
                childAnimator.gameObject.AddComponent<MobaGameplay.Animation.AnimationEventReceiver>();
                Debug.Log("Added AnimationEventReceiver to " + childAnimator.gameObject.name + " to fix 'OnLand' errors.");
            }
        }
        else
        {
            Debug.LogError("Could not find any Animator in the Player's children.");
        }

        // Save scene changes
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    private static Transform GetVisualChild(GameObject player)
    {
        Transform visualChild = player.transform.Find("Geometry"); 
        if (visualChild == null) visualChild = player.transform.Find("Skeleton");
        if (visualChild == null) visualChild = player.transform.Find("Armature");
        if (visualChild == null) visualChild = player.transform.Find("Visuals");

        if (visualChild == null)
        {
            GameObject visualGo = new GameObject("Visuals");
            visualGo.transform.SetParent(player.transform);
            visualGo.transform.localPosition = Vector3.zero;
            visualGo.transform.localRotation = Quaternion.identity;
            visualChild = visualGo.transform;
        }
        return visualChild;
    }
}