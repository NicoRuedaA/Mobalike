using UnityEngine;
using UnityEditor;
using MobaGameplay.Abilities;
using MobaGameplay.UI.Targeting;
using MobaGameplay.Combat;

namespace MobaGameplay.Editor
{
    /// <summary>
    /// Creates AbilityData ScriptableObject assets from the old MonoBehaviour-based ability values.
    /// Also adds and configures the AbilitySystem component on the Player prefab.
    /// 
    /// Usage: Mobalike > Tools > Migrate to Data-Driven Abilities
    /// </summary>
    public static class AbilityDataMigrator
    {
        private const string ASSET_FOLDER = "Assets/_Project/Data/Abilities";
        private const string PLAYER_PREFAB_PATH = "Assets/_Project/Prefabs/Characters/Player.prefab";

        [MenuItem("Mobalike/Tools/Migrate to Data-Driven Abilities")]
        public static void Migrate()
        {
            // Ensure output folder exists
            if (!AssetDatabase.IsValidFolder(ASSET_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Abilities");
            }

            // Load prefabs
            GameObject fireballPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Abilities/FireballProjectile.prefab");
            GameObject groundSmashVfx = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Abilities/GroundSmashVFX.prefab");
            GameObject trailZonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Abilities/TrailZone.prefab");

            // Load icons
            Sprite icon1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Abilities/1.png");
            Sprite icon2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Abilities/2.png");
            Sprite icon3 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Abilities/3.png");
            Sprite icon4 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Icons/Abilities/4.png");

            int created = 0;

            // ========================================
            // 1. Fireball (Projectile)
            // ========================================
            var fireball = ScriptableObject.CreateInstance<AbilityData>();
            fireball.abilityName = "Fireball";
            fireball.icon = icon1;
            fireball.cooldown = 5f;
            fireball.manaCost = 0f;
            fireball.castTime = 0f;
            fireball.targetingType = IndicatorType.Circle;
            fireball.castRange = 10f;
            fireball.range = 2f;
            fireball.width = 1f;
            fireball.behaviorType = AbilityBehaviorType.Projectile;
            fireball.baseDamage = 80f;
            fireball.damageType = DamageType.Magical;
            fireball.apRatio = 0.6f;
            fireball.adRatio = 0f;
            fireball.projectilePrefab = fireballPrefab;
            fireball.projectileSpawnHeight = 1f;
            fireball.projectileSpawnForward = 1f;
            SaveAsset(fireball, $"{ASSET_FOLDER}/FireballAbilityData.asset", ref created);

            // ========================================
            // 2. Ground Smash (AreaOfEffect)
            // ========================================
            var groundSmash = ScriptableObject.CreateInstance<AbilityData>();
            groundSmash.abilityName = "Ground Smash";
            groundSmash.icon = icon2;
            groundSmash.cooldown = 5f;
            groundSmash.manaCost = 0f;
            groundSmash.castTime = 0f;
            groundSmash.targetingType = IndicatorType.Circle;
            groundSmash.castRange = 10f;
            groundSmash.range = 2f;     // Range is used for OverlapSphere radius
            groundSmash.width = 1f;
            groundSmash.behaviorType = AbilityBehaviorType.AreaOfEffect;
            groundSmash.baseDamage = 100f;
            groundSmash.damageType = DamageType.Physical;
            groundSmash.apRatio = 0f;
            groundSmash.adRatio = 0.8f;
            groundSmash.aoePrefab = null; // GroundSmashAbility uses instant OverlapSphere, not AoEZone
            groundSmash.aoeRadius = 2f;   // Maps to old Range field
            groundSmash.aoeDelay = 0f;    // Instant, no delay
            groundSmash.vfxPrefab = groundSmashVfx;
            SaveAsset(groundSmash, $"{ASSET_FOLDER}/GroundSmashAbilityData.asset", ref created);

            // ========================================
            // 3. Dash
            // ========================================
            var dash = ScriptableObject.CreateInstance<AbilityData>();
            dash.abilityName = "Dash";
            dash.icon = icon3;
            dash.cooldown = 5f;
            dash.manaCost = 0f;
            dash.castTime = 0f;
            dash.targetingType = IndicatorType.Circle;
            dash.castRange = 10f;
            dash.range = 2f;
            dash.width = 1f;
            dash.behaviorType = AbilityBehaviorType.Dash;
            dash.baseDamage = 0f;
            dash.damageType = DamageType.TrueDamage;
            dash.apRatio = 0f;
            dash.adRatio = 0f;
            dash.dashSpeed = 20f;
            dash.dashDuration = 0.2f;
            SaveAsset(dash, $"{ASSET_FOLDER}/DashAbilityData.asset", ref created);

            // ========================================
            // 4. Ground Trail (Trail)
            // ========================================
            var groundTrail = ScriptableObject.CreateInstance<AbilityData>();
            groundTrail.abilityName = "Ground Trail";
            groundTrail.icon = icon4;
            groundTrail.cooldown = 12f;
            groundTrail.manaCost = 80f;
            groundTrail.castTime = 0f;
            groundTrail.targetingType = IndicatorType.Trail;
            groundTrail.castRange = 25f;
            groundTrail.range = 20f;     // Trail length
            groundTrail.width = 3f;
            groundTrail.behaviorType = AbilityBehaviorType.Trail;
            groundTrail.baseDamage = 0f;
            groundTrail.damageType = DamageType.Magical;
            groundTrail.apRatio = 0f;
            groundTrail.adRatio = 0f;
            groundTrail.trailZonePrefab = trailZonePrefab;
            groundTrail.trailDamagePerSecond = 40f;
            groundTrail.trailZoneDuration = 3f;
            groundTrail.trailZoneCount = 6;
            groundTrail.trailSpawnDelay = 0.12f;
            groundTrail.trailWidth = 3f;
            SaveAsset(groundTrail, $"{ASSET_FOLDER}/GroundTrailAbilityData.asset", ref created);

            AssetDatabase.Refresh();

            // ========================================
            // 5. Add AbilitySystem to Player prefab
            // ========================================
            SetupPlayerAbilitySystem(fireball, groundSmash, dash, groundTrail);

            Debug.Log($"[AbilityDataMigrator] Migration complete! Created {created} AbilityData assets.");
            EditorUtility.DisplayDialog("Migration Complete",
                $"Created {created} AbilityData assets in {ASSET_FOLDER}\n\n" +
                "AbilitySystem component added to Player prefab.\n\n" +
                "Enter Play mode to test the new system!",
                "OK");
        }

        private static void SaveAsset(AbilityData asset, string path, ref int count)
        {
            // Don't overwrite existing assets
            var existing = AssetDatabase.LoadAssetAtPath<AbilityData>(path);
            if (existing != null)
            {
                Debug.LogWarning($"[AbilityDataMigrator] Asset already exists at {path}. Skipping. Delete it first if you want to recreate.");
                return;
            }

            AssetDatabase.CreateAsset(asset, path);
            count++;
            Debug.Log($"[AbilityDataMigrator] Created: {path}");
        }

        private static void SetupPlayerAbilitySystem(
            AbilityData fireball, AbilityData groundSmash,
            AbilityData dash, AbilityData groundTrail)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PLAYER_PREFAB_PATH);
            if (prefabRoot == null)
            {
                Debug.LogError($"[AbilityDataMigrator] Failed to load Player prefab at {PLAYER_PREFAB_PATH}");
                return;
            }

            try
            {
                // Check if AbilitySystem already exists
                var existingSystem = prefabRoot.GetComponent<AbilitySystem>();
                if (existingSystem != null)
                {
                    Debug.LogWarning("[AbilityDataMigrator] AbilitySystem already exists on Player prefab. Updating abilities.");
                    // Update the equipped abilities list
                    var serializedObj = new SerializedObject(existingSystem);
                    var equippedAbilities = serializedObj.FindProperty("equippedAbilities");
                    
                    if (equippedAbilities != null)
                    {
                        equippedAbilities.arraySize = 4;
                        equippedAbilities.GetArrayElementAtIndex(0).objectReferenceValue = fireball;
                        equippedAbilities.GetArrayElementAtIndex(1).objectReferenceValue = groundSmash;
                        equippedAbilities.GetArrayElementAtIndex(2).objectReferenceValue = dash;
                        equippedAbilities.GetArrayElementAtIndex(3).objectReferenceValue = groundTrail;
                        serializedObj.ApplyModifiedProperties();
                    }
                }
                else
                {
                    // Add AbilitySystem component
                    var abilitySystem = prefabRoot.AddComponent<AbilitySystem>();
                    
                    var serializedObj = new SerializedObject(abilitySystem);
                    var equippedAbilities = serializedObj.FindProperty("equippedAbilities");
                    
                    if (equippedAbilities != null)
                    {
                        equippedAbilities.arraySize = 4;
                        equippedAbilities.GetArrayElementAtIndex(0).objectReferenceValue = fireball;
                        equippedAbilities.GetArrayElementAtIndex(1).objectReferenceValue = groundSmash;
                        equippedAbilities.GetArrayElementAtIndex(2).objectReferenceValue = dash;
                        equippedAbilities.GetArrayElementAtIndex(3).objectReferenceValue = groundTrail;
                        serializedObj.ApplyModifiedProperties();
                    }
                }

                // Save the prefab
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PLAYER_PREFAB_PATH);
                Debug.Log("[AbilityDataMigrator] Player prefab updated with AbilitySystem component.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}