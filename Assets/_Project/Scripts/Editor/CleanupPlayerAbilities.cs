using UnityEngine;
using UnityEditor;
using MobaGameplay.Abilities;

namespace MobaGameplay.Editor
{
    public static class CleanupPlayerAbilities
    {
        [MenuItem("Mobalike/Tools/Cleanup Player Abilities")]
        public static void Cleanup()
        {
            string prefabPath = "Assets/_Project/Prefabs/Characters/Player.prefab";
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            
            if (prefabRoot == null)
            {
                Debug.LogError("[CleanupPlayerAbilities] Failed to load Player.prefab");
                return;
            }

            try
            {
                // Obtener TODAS las abilities del Player
                var allAbilities = prefabRoot.GetComponents<BaseAbility>();
                Debug.Log($"[CleanupPlayerAbilities] Found {allAbilities.Length} abilities on Player");

                int removed = 0;
                foreach (var ability in allAbilities)
                {
                    // Si no tiene ícono asignado, es una ability "fantasma"
                    if (ability.AbilityIcon == null)
                    {
                        string abilityName = ability.abilityName;
                        string abilityType = ability.GetType().Name;
                        
                        Undo.DestroyObjectImmediate(ability);
                        removed++;
                        Debug.Log($"[CleanupPlayerAbilities] Removed '{abilityName}' ({abilityType}) - no icon assigned");
                    }
                    else
                    {
                        Debug.Log($"[CleanupPlayerAbilities] Keeping '{ability.abilityName}' ({ability.GetType().Name}) - has icon: {ability.AbilityIcon.name}");
                    }
                }

                if (removed > 0)
                {
                    // Guardar el prefab
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                    AssetDatabase.Refresh();
                    
                    Debug.Log($"[CleanupPlayerAbilities] Removed {removed} abilities without icons. Please re-enter Play mode.");
                    EditorUtility.DisplayDialog("Cleanup Complete", 
                        $"Removed {removed} abilities without icons.\n\n" +
                        "The Player prefab has been cleaned up.\n" +
                        "Please re-enter Play mode to see the changes.", "OK");
                }
                else
                {
                    Debug.Log("[CleanupPlayerAbilities] No abilities to remove - all have icons assigned");
                    EditorUtility.DisplayDialog("No Action Needed", 
                        "All abilities already have icons assigned.", "OK");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}
