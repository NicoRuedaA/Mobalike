using UnityEngine;
using UnityEditor;
using MobaGameplay.Abilities;

namespace MobaGameplay.Editor
{
    public static class ForceFixAbilityIcons
    {
        [MenuItem("Mobalike/Tools/Force Fix Ability Icons")]
        public static void FixIcons()
        {
            // Cargar iconos
            Sprite[] icons = new Sprite[4];
            icons[0] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Abilities/1.png");
            icons[1] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Abilities/2.png");
            icons[2] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Abilities/3.png");
            icons[3] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Abilities/4.png");

            for (int i = 0; i < 4; i++)
            {
                if (icons[i] == null)
                {
                    Debug.LogError($"[ForceFixAbilityIcons] Icon {i + 1}.png not found!");
                    return;
                }
                Debug.Log($"[ForceFixAbilityIcons] Loaded icon {i + 1}: {icons[i].name}");
            }

            string prefabPath = "Assets/_Project/Prefabs/Characters/Player.prefab";
            
            // Cargar el prefab para edición
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            
            if (prefabRoot == null)
            {
                Debug.LogError("[ForceFixAbilityIcons] Failed to load Player.prefab");
                return;
            }

            try
            {
                // Buscar TODOS los abilities en el prefab
                var allAbilities = prefabRoot.GetComponents<BaseAbility>();
                Debug.Log($"[ForceFixAbilityIcons] Found {allAbilities.Length} abilities on prefab");
                
                int fixedCount = 0;
                
                foreach (var ability in allAbilities)
                {
                    if (ability == null) continue;
                    
                    string abilityName = ability.GetType().Name;
                    Sprite iconToAssign = null;
                    string displayName = ability.abilityName;
                    
                    // Asignar icono según el tipo
                    switch (abilityName)
                    {
                        case "FireballAbility":
                            iconToAssign = icons[0];
                            displayName = "Fireball";
                            break;
                        case "GroundSmashAbility":
                            iconToAssign = icons[1];
                            displayName = "Ground Smash";
                            break;
                        case "DashAbility":
                            iconToAssign = icons[2];
                            displayName = "Dash";
                            break;
                        case "GroundTrailAbility":
                            iconToAssign = icons[3];
                            displayName = "Ground Trail";
                            break;
                    }
                    
                    if (iconToAssign != null)
                    {
                        Undo.RecordObject(ability, $"Fix Icon for {abilityName}");
                        
                        // Usar reflection para asignar el campo privado
                        var iconField = typeof(BaseAbility).GetField("_abilityIcon", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var nameField = typeof(BaseAbility).GetField("_abilityName", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        if (iconField != null)
                        {
                            iconField.SetValue(ability, iconToAssign);
                            fixedCount++;
                            Debug.Log($"[ForceFixAbilityIcons] ASSIGNED icon '{iconToAssign.name}' to {abilityName}");
                        }
                        else
                        {
                            Debug.LogError($"[ForceFixAbilityIcons] Could not find _abilityIcon field on {abilityName}");
                        }
                        
                        if (nameField != null && (string.IsNullOrEmpty(ability.abilityName) || ability.abilityName == "New Ability"))
                        {
                            nameField.SetValue(ability, displayName);
                            Debug.Log($"[ForceFixAbilityIcons] Set ability name to: {displayName}");
                        }
                        
                        EditorUtility.SetDirty(ability);
                    }
                    else
                    {
                        Debug.LogWarning($"[ForceFixAbilityIcons] No icon mapping for {abilityName}");
                    }
                }

                // Guardar el prefab
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"[ForceFixAbilityIcons] SUCCESS! Fixed {fixedCount} abilities!");
                EditorUtility.DisplayDialog("Ability Icons Fixed", 
                    $"Successfully assigned icons to {fixedCount} abilities!\n\n" +
                    "Please re-enter Play mode to see the changes.", "OK");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}
