using UnityEngine;
using System;
using MobaGameplay.UI.Targeting;

namespace MobaGameplay.Core
{
    public class HeroEntity : BaseEntity
    {
        /// <summary>
        /// Singleton instance of the hero. Set in Awake, cleared in OnDestroy.
        /// Use instead of FindObjectOfType&lt;HeroEntity&gt;() for performance.
        /// </summary>
        public static HeroEntity Instance { get; private set; }

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

        public event Action<int> OnLevelUp;
        public event Action<float, float> OnExpGained;
        public event Action<float> OnGoldGained;

        protected override void Awake()
        {
            // Singleton setup — prevent duplicates
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[HeroEntity] Duplicate hero detected: {gameObject.name}. Destroying.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            base.Awake();
            if (TargetingManager.Instance != null)
                TargetingManager.Instance.Initialize(transform);
        }

        protected override void OnDestroy()
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