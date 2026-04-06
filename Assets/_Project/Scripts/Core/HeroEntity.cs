using UnityEngine;
using System;
using MobaGameplay.UI.Targeting;

namespace MobaGameplay.Core
{
    public class HeroEntity : BaseEntity
    {
        [Header("Hero Progression")]
        public int CurrentLevel = 1;
        public int MaxLevel = 18;
        public float CurrentExp = 0f;
        public float ExpToNextLevel = 100f;
        public float CurrentGold = 0f;

        [Header("Stat Scaling (Per Level)")]
        public float HealthPerLevel = 85f;
        public float ManaPerLevel = 30f;
        public float ADPerLevel = 3.5f;
        public float APPerLevel = 0f;
        public float ArmorPerLevel = 4f;
        public float MRPerLevel = 1.25f;

        public event Action<int> OnLevelUp;
        public event Action<float, float> OnExpGained;
        public event Action<float> OnGoldGained;

        protected override void Awake()
        {
            base.Awake();
            if (TargetingManager.Instance != null)
                TargetingManager.Instance.Initialize(transform);
        }

        public void AddExp(float amount)
        {
            if (CurrentLevel >= MaxLevel) return;
            
            CurrentExp += amount;
            OnExpGained?.Invoke(CurrentExp, ExpToNextLevel);

            while (CurrentExp >= ExpToNextLevel && CurrentLevel < MaxLevel)
            {
                LevelUp();
            }
        }

        public void AddGold(float amount)
        {
            CurrentGold += amount;
            OnGoldGained?.Invoke(CurrentGold);
        }

        private void LevelUp()
        {
            CurrentExp -= ExpToNextLevel;
            CurrentLevel++;
            ExpToNextLevel *= 1.2f;

            MaxHealth += HealthPerLevel;
            CurrentHealth += HealthPerLevel;
            MaxMana += ManaPerLevel;
            CurrentMana += ManaPerLevel;
            AttackDamage += ADPerLevel;
            AbilityPower += APPerLevel;
            PhysicalArmor += ArmorPerLevel;
            MagicResistance += MRPerLevel;

            OnLevelUp?.Invoke(CurrentLevel);
            Debug.Log($"[HeroEntity] Leveled up to {CurrentLevel}!");
        }
    }
}
