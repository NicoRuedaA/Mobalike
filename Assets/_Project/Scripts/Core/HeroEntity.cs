using UnityEngine;
using System;
using MobaGameplay.UI.Targeting;
using MobaGameplay.Abilities;
using MobaGameplay.Combat;
using MobaGameplay.Visuals;

namespace MobaGameplay.Core
{
    public class HeroEntity : BaseEntity
    {
        /// <summary>
        /// Singleton instance of the hero. Set in Awake, cleared in OnDestroy.
        /// Use instead of FindObjectOfType&lt;HeroEntity&gt;() for performance.
        /// </summary>
        public static HeroEntity Instance { get; private set; }

        [Header("Hero Class")]
        [Tooltip("Clase del héroe. Asigna esto para自动-configurar modelo, stats y habilidades.")]
        [SerializeField] private HeroClass heroClass;

        [Header("Hero Progression")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int maxLevel = 18;
        [SerializeField] private float currentExp;
        [SerializeField] private float expToNextLevel = 100f;
        [SerializeField] private float currentGold;
        [SerializeField] private float expScaleMultiplier = 1.2f;

        [Header("Stat Scaling (Per Level)")]
        [SerializeField] private float healthPerLevel = 85f;
        [SerializeField] private float manaPerLevel = 30f;
        [SerializeField] private float adPerLevel = 3.5f;
        [SerializeField] private float apPerLevel = 0f;
        [SerializeField] private float armorPerLevel = 4f;
        [SerializeField] private float mrPerLevel = 1.25f;

        // References
        private HeroEntityVisuals visuals;

        // Public read-only properties
        public int CurrentLevel => currentLevel;
        public int MaxLevel => maxLevel;
        public float CurrentExp => currentExp;
        public float ExpToNextLevel => expToNextLevel;
        public float CurrentGold => currentGold;
        public float HealthPerLevel => healthPerLevel;
        public float ManaPerLevel => manaPerLevel;
        public float ADPerLevel => adPerLevel;
        public float APPerLevel => apPerLevel;
        public float ArmorPerLevel => armorPerLevel;
        public float MRPerLevel => mrPerLevel;

        /// <summary>
        /// La clase asignada a este héroe (solo lectura).
        /// </summary>
        public HeroClass HeroClass => heroClass;

        public event Action<int> OnLevelUp;
        public event Action<float, float> OnExpGained;
        public event Action<float> OnGoldGained;

        protected override void Awake()
        {
            Debug.Log("[HeroEntity] >>>>>>>>>> Awake START <<<<<<<<<<");
            
            // Singleton setup — prevent duplicates
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[HeroEntity] Duplicate hero detected: {gameObject.name}. Destroying.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Fallback: try to load Mage class if heroClass is null
            if (heroClass == null)
            {
                heroClass = Resources.Load<HeroClass>("ScriptableObjects/Heroes/Mage");
                Debug.Log($"[HeroEntity] Loaded heroClass from Resources: {heroClass?.className ?? "NULL"}");
            }

            Debug.Log($"[HeroEntity] Awake: heroClass = {heroClass?.className ?? "NULL"}");

            // Apply class configuration BEFORE base.Awake()
            if (heroClass != null)
            {
                Debug.Log($"[HeroEntity] About to apply class: {heroClass.className}");
                ApplyClassConfiguration();
            }
            else
            {
                Debug.LogError("[HeroEntity] heroClass is NULL even after fallback!");
            }

            base.Awake();
            // Find TargetingManager directly since Instance might not be ready yet
            var targetingManager = FindObjectOfType<MobaGameplay.UI.Targeting.TargetingManager>();
            if (targetingManager != null)
            {
                targetingManager.Initialize(transform);
            }
            else
            {
                Debug.LogWarning("[HeroEntity] TargetingManager not found in Awake");
            }
        }

        private void Start()
        {
            // Find TargetingManager directly since Instance might not be set
            var targetingManager = FindObjectOfType<MobaGameplay.UI.Targeting.TargetingManager>();
            
            if (targetingManager != null)
            {
                var transformField = targetingManager.GetType().GetField("playerTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var existingTransform = transformField?.GetValue(targetingManager) as Transform;
                
                if (existingTransform == null)
                {
                    Debug.Log("[HeroEntity] Calling TargetingManager.Initialize from Start");
                    targetingManager.Initialize(transform);
                }
            }
            else
            {
                Debug.LogWarning("[HeroEntity] TargetingManager not found in scene");
            }
        }

        /// <summary>
        /// Aplica la configuración de la clase al héroe (modelo, stats, habilidades).
        /// </summary>
        private void ApplyClassConfiguration()
        {
            Debug.Log($"[HeroEntity] ApplyClassConfiguration ENTER, heroClass={heroClass?.className}");
            
            if (heroClass == null) {
                Debug.LogWarning("[HeroEntity] ApplyClassConfiguration: heroClass is null!");
                return;
            }
            
            bool isValid = heroClass.IsValid();
            Debug.Log($"[HeroEntity] ApplyClassConfiguration: IsValid={isValid}");
            
            if (!isValid) {
                Debug.LogWarning("[HeroEntity] ApplyClassConfiguration SKIPPED - class invalid");
                return;
            }

            Debug.Log("[HeroEntity] ApplyClassConfiguration: Proceeding with config...");

            // Apply base stats from class
            MaxHealth = heroClass.baseHealth;
            CurrentHealth = heroClass.baseHealth;
            MaxMana = heroClass.baseMana;
            CurrentMana = heroClass.baseMana;
            PhysicalArmor = heroClass.baseArmor;
            MagicResistance = heroClass.baseMagicResist;
            
            // Override attack damage and movement speed
            AttackDamage = heroClass.baseAttackDamage;
            MovementSpeed = heroClass.baseMoveSpeed;
            
            // Set health/mana regen
            HealthRegen = heroClass.healthRegen;
            ManaRegen = heroClass.manaRegen;

            // Setup combat component based on combat type
            Debug.Log("[HeroEntity] Calling SetupCombatComponent...");
            SetupCombatComponent();

            Debug.Log($"[HeroEntity] Applied class: {heroClass.className}");
        }

        /// <summary>
        /// Configura el componente de combate basado en el CombatType de la clase.
        /// </summary>
        private void SetupCombatComponent()
        {
            Debug.Log($"[HeroEntity] SetupCombatComponent START, combatType = {heroClass?.combatType}");
            
            if (heroClass == null) {
                Debug.LogWarning("[HeroEntity] SetupCombatComponent SKIPPED - heroClass is null");
                return;
            }
            
            if (heroClass.combatType == CombatType.Ranged)
            {
                // Add or configure RangedCombat
                RangedCombat rangedCombat = GetComponent<RangedCombat>();
                if (rangedCombat == null)
                {
                    Debug.Log("[HeroEntity] Adding RangedCombat component");
                    rangedCombat = gameObject.AddComponent<RangedCombat>();
                }
                else
                {
                    Debug.Log("[HeroEntity] RangedCombat already exists");
                }
                
                Debug.Log("[HeroEntity] Calling ConfigureFromHeroClass...");
                rangedCombat.ConfigureFromHeroClass(heroClass);
                Debug.Log("[HeroEntity] ConfigureFromHeroClass completed");
            }
            else if (heroClass.combatType == CombatType.Melee)
            {
                // TODO: Implement MeleeCombat when available
                Debug.Log($"[HeroEntity] Melee combat not yet implemented");
            }
        }

        /// <summary>
        /// Cambia la clase del héroe en runtime.
        /// </summary>
        public void SetClass(HeroClass newClass)
        {
            heroClass = newClass;
            if (heroClass != null)
            {
                ApplyClassConfiguration();
            }
        }

        protected void OnDestroy()
        {
            // Clear singleton reference when destroyed
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void AddExp(float amount)
        {
            if (currentLevel >= maxLevel) return;
            
            currentExp += amount;
            OnExpGained?.Invoke(currentExp, expToNextLevel);

            while (currentExp >= expToNextLevel && currentLevel < maxLevel)
            {
                LevelUp();
            }
        }

        public void AddGold(float amount)
        {
            currentGold += amount;
            OnGoldGained?.Invoke(currentGold);
        }

        private void LevelUp()
        {
            currentExp -= expToNextLevel;
            currentLevel++;
            expToNextLevel *= expScaleMultiplier;

            MaxHealth += healthPerLevel;
            CurrentHealth += healthPerLevel;
            MaxMana += manaPerLevel;
            CurrentMana += manaPerLevel;
            AttackDamage += adPerLevel;
            AbilityPower += apPerLevel;
            PhysicalArmor += armorPerLevel;
            MagicResistance += mrPerLevel;

            OnLevelUp?.Invoke(currentLevel);
            Debug.Log($"[HeroEntity] Leveled up to {currentLevel}!");
        }
    }
}