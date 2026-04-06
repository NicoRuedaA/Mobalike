using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixAnimatorRootMotion : EditorWindow
{
    [MenuItem("Tools/Fix Animator Root Motion")]
    public static void FixRootMotion()
    {
        // Find the Player GameObject in the active scene
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Player GameObject not found.");
            return;
        }

        Animator currentAnimator = player.GetComponentInChildren<Animator>();
        if (currentAnimator == null)
        {
            Debug.LogError("No Animator found anywhere in the Player.");
            return;
        }

        if (currentAnimator.applyRootMotion)
        {
            currentAnimator.applyRootMotion = false;
            Debug.Log("Disabled 'Apply Root Motion' on the Animator. This was causing the visual sinking.");
            
            // Save scene changes
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
        }
        else
        {
            Debug.Log("Root motion is already disabled. Checking child transform positions...");
            
            Transform visuals = player.transform.Find("Visuals");
            if (visuals != null)
            {
                if (visuals.localPosition != Vector3.zero)
                {
                    Debug.Log($"Visuals localPosition was {visuals.localPosition}, resetting to zero.");
                    visuals.localPosition = Vector3.zero;
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    EditorSceneManager.SaveOpenScenes();
                }
                else
                {
                    Debug.Log("Local position is zero. No obvious issue found.");
                }
            }
        }
    }
}