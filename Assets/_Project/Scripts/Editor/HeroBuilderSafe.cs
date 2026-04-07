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
            rb.isKinematic = true; // Use raycasts/SphereCast in Update instead of physics engine forces
            
            var trail = fireballObj.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 0.5f;
            trail.endWidth = 0f;
            trail.material = orangeMat;
            
            var proj = fireballObj.AddComponent<LinearProjectile>();
            proj.speed = 20f;
            proj.maxDistance = 25f;
            proj.collisionRadius = 0.5f;
            proj.hitLayers = ~0; // Hit everything for now
            
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
            
            var baProj = basicAttackObj.AddComponent<BasicAttackProjectile>();
            
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
            vfx.duration = 0.3f;
            vfx.maxScale = 3f;
            
            string smashPath = "Assets/_Project/Prefabs/Abilities/GroundSmashVFX.prefab";
            GameObject savedSmash = PrefabUtility.SaveAsPrefabAsset(smashObj, smashPath);
            Object.DestroyImmediate(smashObj);

            // 3. Build Player Template (ONLY AS PREFAB, NO SCENE CHANGES)
            GameObject heroObj = new GameObject("PlayerHero_Template");
            
            // Note: We deliberately do NOT add visual meshes, rigidbodies, colliders, or movement
            // scripts so you can easily copy-paste these components into your existing Player object
            // or just use this as a reference without it crashing due to missing components.
            
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
            fbAbility.projectilePrefab = savedFireball;
            fbAbility.baseDamage = 80f;
            
            var smashAbility = heroObj.AddComponent<GroundSmashAbility>();
            smashAbility.abilityName = "Ground Smash";
            smashAbility.cooldown = 8f;
            smashAbility.manaCost = 80f;
            smashAbility.TargetingType = UI.Targeting.IndicatorType.Circle;
            smashAbility.CastRange = 10f; // Can cast up to 10m away
            smashAbility.Range = 3f; // AoE radius is 3m
            smashAbility.vfxPrefab = savedSmash;
            smashAbility.baseDamage = 120f;
            
            var dashAbility = heroObj.AddComponent<DashAbility>();
            dashAbility.abilityName = "Dash";
            dashAbility.cooldown = 6f;
            dashAbility.manaCost = 30f;
            dashAbility.TargetingType = UI.Targeting.IndicatorType.None; // Instant cast
            dashAbility.dashSpeed = 30f;
            
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
    }
}