using UnityEngine;
using UnityEditor;
using MobaGameplay.Core;
using MobaGameplay.Abilities;
using MobaGameplay.Abilities.Projectiles;
using MobaGameplay.VFX;

namespace MobaGameplay.Editor
{
    public static class VisualsBuilder
    {
        [MenuItem("Mobalike/Tools/Build Ability Visuals")]
        public static void BuildVisuals()
        {
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
            HeroBuilderSafe.SetSerializedField(proj, "speed", 20f);
            HeroBuilderSafe.SetSerializedField(proj, "maxDistance", 25f);
            HeroBuilderSafe.SetSerializedField(proj, "collisionRadius", 0.5f);
            HeroBuilderSafe.SetSerializedField(proj, "hitLayers", (int)~0);
            
            string fireballPath = "Assets/_Project/Prefabs/Abilities/FireballProjectile.prefab";
            PrefabUtility.SaveAsPrefabAsset(fireballObj, fireballPath);
            Object.DestroyImmediate(fireballObj);

            // 2. Build Ground Smash VFX
            GameObject smashObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            smashObj.name = "GroundSmashVFX";
            smashObj.transform.localScale = new Vector3(1, 0.1f, 1);
            smashObj.GetComponent<MeshRenderer>().sharedMaterial = orangeMat;
            Object.DestroyImmediate(smashObj.GetComponent<Collider>());
            
            var vfx = smashObj.AddComponent<SimpleVFX>();
            HeroBuilderSafe.SetSerializedField(vfx, "duration", 0.3f);
            HeroBuilderSafe.SetSerializedField(vfx, "maxScale", 3f);
            
            string smashPath = "Assets/_Project/Prefabs/Abilities/GroundSmashVFX.prefab";
            PrefabUtility.SaveAsPrefabAsset(smashObj, smashPath);
            Object.DestroyImmediate(smashObj);

            Debug.Log("Successfully built isolated Ability Visuals!");
        }
    }
}