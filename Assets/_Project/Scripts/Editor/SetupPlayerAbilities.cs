using UnityEngine;
using UnityEditor;
using MobaGameplay.Abilities;

namespace MobaGameplay.Editor
{
    public static class AutoSetupAbilityIcons
    {
        [MenuItem("Mobalike/Tools/Auto-Setup Player Ability Icons")]
        public static void SetupIcons()
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
                    Debug.LogError($"[AutoSetupAbilityIcons] Icon {i + 1}.png not found!");
                    return;
                }
            }

            // Abrir el prefab del Player
            string prefabPath = "Assets/_Project/Prefabs/Characters/Player.prefab";
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            
            if (prefabRoot == null)
            {
                Debug.LogError("[AutoSetupAbilityIcons] Failed to load Player.prefab");
                return;
            }

            try
            {
                // Encontrar el AbilityController
                var abilityController = prefabRoot.GetComponent<AbilityController>();
                if (abilityController == null)
                {
                    Debug.LogError("[AutoSetupAbilityIcons] AbilityController not found!");
                    return;
                }

                // Obtener las abilities por reflection o propiedades
                var abilities = new BaseAbility[4];
                abilities[0] = abilityController.Ability1;
                abilities[1] = abilityController.Ability2;
                abilities[2] = abilityController.Ability3;
                abilities[3] = abilityController.Ability4;

                string[] names = { "Fireball", "Ground Smash", "Dash", "Ground Trail" };
                int configured = 0;

                // Marcar el prefab como modificado
                Undo.RecordObject(prefabRoot, "Setup Player Ability Icons");
                
                for (int i = 0; i < 4; i++)
                {
                    if (abilities[i] == null)
                    {
                        Debug.LogWarning($"[AutoSetupAbilityIcons] Ability {i + 1} is null!");
                        continue;
                    }

                    // Marcar como modificado
                    Undo.RecordObject(abilities[i], $"Setup Ability {i + 1}");

                    // Asignar nombre e icono usando reflection en los campos privados
                    var nameField = typeof(BaseAbility).GetField("_abilityName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var iconField = typeof(BaseAbility).GetField("_abilityIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (nameField != null)
                    {
                        string currentName = (string)nameField.GetValue(abilities[i]);
                        if (string.IsNullOrEmpty(currentName) || currentName == "New Ability")
                        {
                            nameField.SetValue(abilities[i], names[i]);
                        }
                    }

                    if (iconField != null)
                    {
                        iconField.SetValue(abilities[i], icons[i]);
                        configured++;
                        Debug.Log($"[AutoSetupAbilityIcons] Set ability {i + 1} ({names[i]}) icon to: {icons[i].name}");
                    }

                    // Marcar el objeto como modificado
                    EditorUtility.SetDirty(abilities[i]);
                }

                // Marcar el AbilityController como modificado también
                EditorUtility.SetDirty(abilityController);
                
                // Guardar el prefab
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                AssetDatabase.Refresh();
                
                Debug.Log($"[AutoSetupAbilityIcons] Successfully configured {configured} ability icons!");
                EditorUtility.DisplayDialog("Success", 
                    $"Configured {configured} abilities with icons!\n\n" +
                    "Please re-enter Play mode to see the changes.", "OK");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}
