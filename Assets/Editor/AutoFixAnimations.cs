using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class AutoFixAnimations
{
    static AutoFixAnimations()
    {
        EditorApplication.delayCall += RunFix;
    }

    static void RunFix()
    {
        // Ejecutar solo una vez por sesión para no ciclar
        if (SessionState.GetBool("AutoBakeApplied", false)) return;
        SessionState.SetBool("AutoBakeApplied", true);

        int fixedCount = 0;
        string[] guids = AssetDatabase.FindAssets("t:Model");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            
            if (importer != null)
            {
                ModelImporterClipAnimation[] clips = importer.clipAnimations;
                if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;

                if (clips != null && clips.Length > 0)
                {
                    bool changed = false;
                    for (int i = 0; i < clips.Length; i++)
                    {
                        // Forzar "Bake Into Pose" para evitar que la animación hunda la malla
                        if (!clips[i].lockRootHeightY) { clips[i].lockRootHeightY = true; changed = true; }
                        if (!clips[i].lockRootPositionXZ) { clips[i].lockRootPositionXZ = true; changed = true; }
                        if (!clips[i].lockRootRotation) { clips[i].lockRootRotation = true; changed = true; }
                        
                        // Forzar "Based Upon" a Original para estabilidad
                        if (!clips[i].keepOriginalPositionY) { clips[i].keepOriginalPositionY = true; changed = true; }
                        if (!clips[i].keepOriginalPositionXZ) { clips[i].keepOriginalPositionXZ = true; changed = true; }
                        if (!clips[i].keepOriginalOrientation) { clips[i].keepOriginalOrientation = true; changed = true; }
                    }

                    if (changed)
                    {
                        importer.clipAnimations = clips;
                        importer.SaveAndReimport();
                        fixedCount++;
                    }
                }
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"[AutoFix] Auto-Baked Root Transform en {fixedCount} animaciones para arreglar el hundimiento del WASD.");
        }

        // Asegurarnos de que el Animator en la escena tiene Apply Root Motion desactivado
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Animator anim = player.GetComponentInChildren<Animator>();
            if (anim != null && anim.applyRootMotion)
            {
                anim.applyRootMotion = false;
                Debug.Log("[AutoFix] Desactivado Apply Root Motion en el Animator del Player.");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
}