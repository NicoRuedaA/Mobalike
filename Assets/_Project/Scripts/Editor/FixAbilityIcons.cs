using UnityEngine;
using UnityEditor;
using MobaGameplay.Abilities;

namespace MobaGameplay.Editor
{
    public static class FixAbilityIcons
    {
        [MenuItem("Mobalike/Tools/Fix Ability Icons")]
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
                    Debug.LogError($"[FixAbilityIcons] Icon {i + 1}.png not found!");
                    return;
                }
            }

            string prefabPath = "Assets/_Project/Prefabs/Characters/Player.prefab";
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            
            if (prefabRoot == null)
            {
                Debug.LogError("[FixAbilityIcons] Failed to load Player.prefab");
                return;
            }

            try
            {
                var abilityController = prefabRoot.GetComponent<AbilityController>();
                if (abilityController == null)
                {
                    Debug.LogError("[FixAbilityIcons] AbilityController not found!");
                    return;
                }

                // Buscar abilities por tipo específico
                var fireball = prefabRoot.GetComponent<FireballAbility>();
                var groundSmash = prefabRoot.GetComponent<GroundSmashAbility>();
                var dash = prefabRoot.GetComponent<DashAbility>();
                var groundTrail = prefabRoot.GetComponent<GroundTrailAbility>();

                int fixedCount = 0;

                // Función para asignar ícono
                System.Action<BaseAbility, Sprite, string> assignIcon = (ability, icon, name) =>
                {
                    if (ability == null) return;
                    
                    Undo.RecordObject(ability, $"Fix Icon for {name}");
                    
                    // Usar reflection para asignar el campo privado
                    var iconField = typeof(BaseAbility).GetField("_abilityIcon", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var nameField = typeof(BaseAbility).GetField("_abilityName", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (iconField != null)
                    {
                        iconField.SetValue(ability, icon);
                        fixedCount++;
                        Debug.Log($"[FixAbilityIcons] Assigned icon '{icon.name}' to {name}");
                    }
                    
                    if (nameField != null && (string.IsNullOrEmpty(ability.abilityName) || ability.abilityName == "New Ability"))
                    {
                        nameField.SetValue(ability, name);
                    }
                    
                    EditorUtility.SetDirty(ability);
                };

                assignIcon(fireball, icons[0], "Fireball");
                assignIcon(groundSmash, icons[1], "Ground Smash");
                assignIcon(dash, icons[2], "Dash");
                assignIcon(groundTrail, icons[3], "Ground Trail");

                // Actualizar las referencias en AbilityController
                Undo.RecordObject(abilityController, "Update AbilityController references");
                var so = new SerializedObject(abilityController);
                so.FindProperty("ability1").objectReferenceValue = fireball;
                so.FindProperty("ability2").objectReferenceValue = groundSmash;
                so.FindProperty("ability3").objectReferenceValue = dash;
                so.FindProperty("ability4").objectReferenceValue = groundTrail;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(abilityController);

                // Guardar
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                AssetDatabase.Refresh();

                Debug.Log($"[FixAbilityIcons] Fixed {fixedCount} abilities!");
                EditorUtility.DisplayDialog("Success", 
                    $"Fixed {fixedCount} ability icons!\n\n" +
                    "Please re-enter Play mode to see the changes.", "OK");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}
