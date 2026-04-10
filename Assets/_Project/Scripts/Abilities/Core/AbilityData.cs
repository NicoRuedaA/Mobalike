using UnityEngine;
using MobaGameplay.Combat;
using MobaGameplay.UI.Targeting;

namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Data-driven ability definition. All tuning lives here — no code changes needed
    /// to adjust cooldowns, damage, prefabs, etc.
    /// Create via: Right-click → Create → MobaGameplay → Ability Data
    /// 
    /// Design principle: ONE ScriptableObject per ability variant.
    /// "Fireball Rank 1" and "Fireball Rank 2" are different AbilityData assets.
    /// The behavior type determines WHICH code runs; the fields provide the numbers.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbilityData", menuName = "MobaGameplay/Ability Data")]
    public class AbilityData : ScriptableObject
    {
        // ============================================================
        // Identity
        // ============================================================

        [Header("Identity")]
        [Tooltip("Display name shown in UI")]
        public string abilityName = "New Ability";

        [Tooltip("Icon shown in HUD ability slots")]
        public Sprite icon;

        // ============================================================
        // Stats (tunable from Inspector)
        // ============================================================

        [Header("Stats")]
        [Tooltip("Cooldown in seconds after casting")]
        public float cooldown = 5f;

        [Tooltip("Mana cost to cast")]
        public float manaCost = 0f;

        [Tooltip("Cast time (0 = instant)")]
        public float castTime = 0f;

        // ============================================================
        // Targeting (determines indicator type and ranges)
        // ============================================================

        [Header("Targeting")]
        [Tooltip("Type of targeting indicator to show")]
        public IndicatorType targetingType = IndicatorType.Circle;

        [Tooltip("Maximum distance from caster to target position")]
        public float castRange = 10f;

        [Tooltip("Ability effective range (AoE radius, trail length, etc.)")]
        public float range = 2f;

        [Tooltip("Width of the ability (line indicator width, trail width)")]
        public float width = 1f;

        // ============================================================
        // Behavior (determines what code runs)
        // ============================================================

        [Header("Behavior")]
        [Tooltip("What this ability DOES when executed")]
        public AbilityBehaviorType behaviorType = AbilityBehaviorType.Projectile;

        // ============================================================
        // Damage (used by Projectile, AoE, Trail behaviors)
        // ============================================================

        [Header("Damage")]
        [Tooltip("Base damage dealt on hit")]
        public float baseDamage = 50f;

        [Tooltip("Damage type (Physical, Magical, True)")]
        public DamageType damageType = DamageType.Magical;

        [Tooltip("Ratio of Ability Power added to base damage (0 = no scaling)")]
        public float apRatio = 0f;

        [Tooltip("Ratio of Attack Damage added to base damage (0 = no scaling)")]
        public float adRatio = 0f;

        // ============================================================
        // Projectile (used when behaviorType = Projectile)
        // ============================================================

        [Header("Projectile Settings")]
        [Tooltip("Projectile prefab (must have LinearProjectile or similar component)")]
        public GameObject projectilePrefab;

        [Tooltip("Spawn height offset above caster")]
        public float projectileSpawnHeight = 1f;

        [Tooltip("Forward offset from caster position")]
        public float projectileSpawnForward = 1f;

        // ============================================================
        // Dash (used when behaviorType = Dash)
        // ============================================================

        [Header("Dash Settings")]
        [Tooltip("Dash movement speed")]
        public float dashSpeed = 20f;

        [Tooltip("Dash duration in seconds")]
        public float dashDuration = 0.2f;

        // ============================================================
        // AoE (used when behaviorType = AreaOfEffect)
        // ============================================================

        [Header("AoE Settings")]
        [Tooltip("AoE zone prefab (must have AoEZone component)")]
        public GameObject aoePrefab;

        [Tooltip("Delay before AoE explodes (seconds)")]
        public float aoeDelay = 1f;

        [Tooltip("AoE explosion radius")]
        public float aoeRadius = 3f;

        [Tooltip("VFX prefab spawned at AoE location")]
        public GameObject vfxPrefab;

        // ============================================================
        // Trail (used when behaviorType = Trail)
        // ============================================================

        [Header("Trail Settings")]
        [Tooltip("Trail zone prefab (must have TrailZone component)")]
        public GameObject trailZonePrefab;

        [Tooltip("Damage per second applied by each zone")]
        public float trailDamagePerSecond = 40f;

        [Tooltip("Duration of each zone in seconds")]
        public float trailZoneDuration = 3f;

        [Tooltip("Number of zones along the trail path")]
        public int trailZoneCount = 6;

        [Tooltip("Delay between spawning each zone")]
        public float trailSpawnDelay = 0.12f;

        [Tooltip("Width of each trail zone")]
        public float trailWidth = 3f;

        // ============================================================
        // Heal/Buff (used when behaviorType = HealBuff)
        // ============================================================

        [Header("Heal/Buff Settings")]
        [Tooltip("Amount of health restored (0 if not a heal)")]
        public float healAmount = 0f;

        [Tooltip("Amount of mana restored (0 if not a mana restore)")]
        public float manaRestoreAmount = 0f;

        [Tooltip("Buff duration in seconds (0 = instant)")]
        public float buffDuration = 0f;

        [Tooltip("Buff to apply (none, attackSpeed, moveSpeed, etc.)")]
        public BuffType buffType = BuffType.None;

        [Tooltip("Buff multiplier (e.g. 1.3 = 30% increase)")]
        public float buffMultiplier = 1f;
    }

    /// <summary>
    /// Buff types available for the HealBuff behavior.
    /// Expand as new buff types are needed.
    /// </summary>
    public enum BuffType
    {
        None,
        AttackSpeed,
        MoveSpeed,
        AttackDamage,
        Defense,
    }
}