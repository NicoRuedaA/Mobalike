using UnityEditor;
using UnityEngine;

public class FixAnimationImportSettings : EditorWindow
{
    [MenuItem("Tools/Fix Animations (Bake Y)")]
    public static void FixAnimations()
    {
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/StarterAssets/ThirdPersonController/Character/Animations" });
        
        if (guids.Length == 0)
        {
            Debug.LogWarning("No animations found in Assets/StarterAssets/ThirdPersonController/Character/Animations");
            // Intentar buscar en todo el proyecto por si las movió
            guids = AssetDatabase.FindAssets("t:Model Locomotion");
        }

        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            
            if (importer != null && importer.defaultClipAnimations != null && importer.defaultClipAnimations.Length > 0)
            {
                ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
                bool changed = false;

                for (int i = 0; i < clips.Length; i++)
                {
                    // Bake Into Pose Y position prevents the animation from pulling the visual mesh into the ground
                    if (!clips[i].lockRootHeightY)
                    {
                        clips[i].lockRootHeightY = true;
                        changed = true;
                    }
                    if (!clips[i].lockRootPositionXZ)
                    {
                        clips[i].lockRootPositionXZ = true;
                        changed = true;
                    }
                    if (!clips[i].lockRootRotation)
                    {
                        clips[i].lockRootRotation = true;
                        changed = true;
                    }
                }

                if (changed)
                {
                    importer.clipAnimations = clips;
                    importer.SaveAndReimport();
                    fixedCount++;
                    Debug.Log($"Fixed Root Motion Baking for: {path}");
                }
            }
        }

        Debug.Log($"Animation fix complete. {fixedCount} files updated.");
    }
}