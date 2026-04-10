using UnityEngine;
using UnityEditor;
using MobaGameplay.Core;
using MobaGameplay.Abilities;
using MobaGameplay.Abilities.Projectiles;
using MobaGameplay.Controllers;
using MobaGameplay.Combat;
using MobaGameplay.Movement;
using MobaGameplay.VFX;

namespace MobaGameplay.Editor
{
    public static class HeroBuilderSafe
    {
        [MenuItem("Mobalike/Tools/Build Isolated Prefabs")]
        public static void BuildPrefabs()
        {
            // Ensure folders
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Abilities"))
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Abilities");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Characters"))
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Characters");

            Material orangeMat = new Material(Shader.Find("Standard"));
            orangeMat.color = new Color(1f, 0.5f, 0f);
            
            // 1. Build Fireball Projectile
            GameObject fireballObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fireballObj.name = "FireballProjectile";
            fireballObj.transform.localScale = Vector3.one * 0.5f;
            fireballObj.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;
            
            var col = fireballObj.GetComponent<SphereCollider>();
            col.isTrigger = true;
            
            var rb = fireballObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            
            var trail = fireballObj.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 0.5f;
            trail.endWidth = 0f;
            trail.material = orangeMat;
            
            var proj = fireballObj.AddComponent<LinearProjectile>();
            SetSerializedField(proj, "speed", 20f);
            SetSerializedField(proj, "maxDistance", 25f);
            SetSerializedField(proj, "collisionRadius", 0.5f);
            SetSerializedField(proj, "hitLayers", ~0);
            
            string fireballPath = "Assets/_Project/Prefabs/Abilities/FireballProjectile.prefab";
            GameObject savedFireball = PrefabUtility.SaveAsPrefabAsset(fireballObj, fireballPath);
            Object.DestroyImmediate(fireballObj);

            // 1.5 Build Basic Attack Projectile
            GameObject basicAttackObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            basicAttackObj.name = "BasicAttackProjectile";
            basicAttackObj.transform.localScale = Vector3.one * 0.2f;
            basicAttackObj.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;
            
            var baCol = basicAttackObj.GetComponent<SphereCollider>();
            baCol.isTrigger = true;
            
            var baRb = basicAttackObj.AddComponent<Rigidbody>();
            baRb.isKinematic = true; 
            
            basicAttackObj.AddComponent<BasicAttackProjectile>();
            
            string basicAttackPath = "Assets/_Project/Prefabs/Abilities/BasicAttackProjectile.prefab";
            GameObject savedBasicAttack = PrefabUtility.SaveAsPrefabAsset(basicAttackObj, basicAttackPath);
            Object.DestroyImmediate(basicAttackObj);

            // 2. Build Ground Smash VFX
            GameObject smashObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            smashObj.name = "GroundSmashVFX";
            smashObj.transform.localScale = new Vector3(1, 0.1f, 1);
            smashObj.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;
            Object.DestroyImmediate(smashObj.GetComponent<Collider>());
            
            var vfx = smashObj.AddComponent<SimpleVFX>();
            SetSerializedField(vfx, "duration", 0.3f);
            SetSerializedField(vfx, "maxScale", 3f);
            
            string smashPath = "Assets/_Project/Prefabs/Abilities/GroundSmashVFX.prefab";
            GameObject savedSmash = PrefabUtility.SaveAsPrefabAsset(smashObj, smashPath);
            Object.DestroyImmediate(smashObj);

            // 3. Build Player Template (ONLY AS PREFAB, NO SCENE CHANGES)
            GameObject heroObj = new GameObject("PlayerHero_Template");
            
            var heroEntity = heroObj.AddComponent<HeroEntity>();
            heroEntity.MaxHealth = 1500f;
            heroEntity.CurrentHealth = 1500f;
            heroEntity.AttackDamage = 60f;
            heroEntity.AbilityPower = 100f;
            
            var abilityController = heroObj.AddComponent<AbilityController>();
            
            // Abilities Setup
            var fbAbility = heroObj.AddComponent<FireballAbility>();
            fbAbility.abilityName = "Fireball";
            fbAbility.cooldown = 4f;
            fbAbility.manaCost = 50f;
            fbAbility.TargetingType = UI.Targeting.IndicatorType.Line;
            fbAbility.CastRange = 15f;
            fbAbility.Range = 15f;
            fbAbility.Width = 1f;
            SetSerializedField(fbAbility, "projectilePrefab", savedFireball);
            SetSerializedField(fbAbility, "baseDamage", 80f);
            
            var smashAbility = heroObj.AddComponent<GroundSmashAbility>();
            smashAbility.abilityName = "Ground Smash";
            smashAbility.cooldown = 8f;
            smashAbility.manaCost = 80f;
            smashAbility.TargetingType = UI.Targeting.IndicatorType.Circle;
            smashAbility.CastRange = 10f;
            smashAbility.Range = 3f;
            SetSerializedField(smashAbility, "vfxPrefab", savedSmash);
            SetSerializedField(smashAbility, "baseDamage", 120f);
            
            var dashAbility = heroObj.AddComponent<DashAbility>();
            dashAbility.abilityName = "Dash";
            dashAbility.cooldown = 6f;
            dashAbility.manaCost = 30f;
            dashAbility.TargetingType = UI.Targeting.IndicatorType.None;
            SetSerializedField(dashAbility, "dashSpeed", 30f);
            
            var so = new SerializedObject(abilityController);
            so.FindProperty("ability1").objectReferenceValue = fbAbility;
            so.FindProperty("ability2").objectReferenceValue = smashAbility;
            so.FindProperty("ability3").objectReferenceValue = dashAbility;
            so.ApplyModifiedProperties();
            
            string heroPath = "Assets/_Project/Prefabs/Characters/PlayerHero_Template.prefab";
            PrefabUtility.SaveAsPrefabAsset(heroObj, heroPath);
            Object.DestroyImmediate(heroObj);

            Debug.Log("Successfully built isolated Prefabs safely!");
        }

        /// <summary>
        /// Helper to set private serialized fields via SerializedObject.
        /// Used because fields are [SerializeField] private for encapsulation.
        /// </summary>
        public static void SetSerializedField(Object target, string fieldName, object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[HeroBuilder] Field '{fieldName}' not found on {target.GetType().Name}");
                return;
            }

            switch (value)
            {
                case float f:
                    prop.floatValue = f;
                    break;
                case int i:
                    prop.intValue = i;
                    break;
                case bool b:
                    prop.boolValue = b;
                    break;
                case LayerMask lm:
                    prop.intValue = lm;
                    break;
                case Object obj:
                    prop.objectReferenceValue = obj;
                    break;
                default:
                    Debug.LogWarning($"[HeroBuilder] Unsupported type for field '{fieldName}': {value.GetType().Name}");
                    return;
            }

            so.ApplyModifiedProperties();
        }
    }
}