using NUnit.Framework;
using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Tests
{
    /// <summary>
    /// Tests for HeroEntity progression: leveling, gold, experience, and stat scaling.
    /// Verifies that level-up correctly increases stats and that gold accumulates properly.
    /// </summary>
    [TestFixture]
    public class HeroEntityTests
    {
        private GameObject _gameObject;
        private HeroEntity _hero;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("TestHero");
            _gameObject.AddComponent<BoxCollider>();
            _hero = _gameObject.AddComponent<HeroEntity>();

            // Force initialize like BaseEntity.Start() would
            // HeroEntity.Start() -> base.Start() -> currentHealth = maxHealth
            _hero.CurrentHealth = _hero.MaxHealth;
            _hero.CurrentMana = _hero.MaxMana;
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
            }
        }

        // ============================================================
        // AddGold
        // ============================================================

        [Test]
        public void AddGold_IncreasesGold()
        {
            _hero.AddGold(50f);

            Assert.AreEqual(50f, _hero.CurrentGold, 0.01f);
        }

        [Test]
        public void AddGold_Accumulates()
        {
            _hero.AddGold(30f);
            _hero.AddGold(70f);

            Assert.AreEqual(100f, _hero.CurrentGold, 0.01f);
        }

        [Test]
        public void AddGold_FiresOnGoldGainedEvent()
        {
            float? receivedGold = null;
            _hero.OnGoldGained += (gold) => receivedGold = gold;

            _hero.AddGold(75f);

            Assert.IsNotNull(receivedGold, "OnGoldGained should have fired");
            Assert.AreEqual(75f, receivedGold.Value, 0.01f);
        }

        // ============================================================
        // AddExp / LevelUp
        // ============================================================

        [Test]
        public void AddExp_IncreasesExperience()
        {
            _hero.AddExp(50f);

            Assert.AreEqual(50f, _hero.CurrentExp, 0.01f);
        }

        [Test]
        public void AddExp_EnoughForLevelUp_IncreasesLevel()
        {
            // Default ExpToNextLevel = 100
            _hero.AddExp(150f); // 150 > 100, should level up

            Assert.AreEqual(2, _hero.CurrentLevel, "Hero should be level 2 after gaining enough XP");
        }

        [Test]
        public void AddExp_EnoughForLevelUp_CarriesOverExcess()
        {
            _hero.AddExp(150f); // 150 - 100 = 50 carry-over

            Assert.AreEqual(50f, _hero.CurrentExp, 0.01f, "Excess XP should carry over");
        }

        [Test]
        public void AddExp_LevelUp_IncreasesMaxHealth()
        {
            float healthBefore = _hero.MaxHealth;
            float healthPerLevel = _hero.HealthPerLevel;

            _hero.AddExp(_hero.ExpToNextLevel); // Exactly enough for 1 level

            Assert.AreEqual(healthBefore + healthPerLevel, _hero.MaxHealth, 0.01f,
                "MaxHealth should increase by HealthPerLevel on level up");
        }

        [Test]
        public void AddExp_LevelUp_IncreasesCurrentHealth()
        {
            float healthBefore = _hero.CurrentHealth;
            float healthPerLevel = _hero.HealthPerLevel;

            _hero.AddExp(_hero.ExpToNextLevel);

            Assert.AreEqual(healthBefore + healthPerLevel, _hero.CurrentHealth, 0.01f,
                "CurrentHealth should also increase by HealthPerLevel on level up");
        }

        [Test]
        public void AddExp_LevelUp_IncreasesAttackDamage()
        {
            float adBefore = _hero.AttackDamage;
            float adPerLevel = _hero.ADPerLevel;

            _hero.AddExp(_hero.ExpToNextLevel);

            Assert.AreEqual(adBefore + adPerLevel, _hero.AttackDamage, 0.01f,
                "AttackDamage should increase by ADPerLevel on level up");
        }

        [Test]
        public void AddExp_LevelUp_IncreasesArmor()
        {
            float armorBefore = _hero.PhysicalArmor;
            float armorPerLevel = _hero.ArmorPerLevel;

            _hero.AddExp(_hero.ExpToNextLevel);

            Assert.AreEqual(armorBefore + armorPerLevel, _hero.PhysicalArmor, 0.01f);
        }

        [Test]
        public void AddExp_MultipleLevels()
        {
            _hero.AddExp(300f); // 100 + 120 (next level) = 220, leaves 80 leftover = level 3

            Assert.AreEqual(3, _hero.CurrentLevel, "Should reach level 3 with 300 XP");
        }

        [Test]
        public void AddExp_LevelUp_FiresOnLevelUpEvent()
        {
            int newLevel = -1;
            _hero.OnLevelUp += (level) => newLevel = level;

            _hero.AddExp(_hero.ExpToNextLevel);

            Assert.AreEqual(2, newLevel, "OnLevelUp should fire with the new level");
        }

        [Test]
        public void AddExp_DoesNotExceedMaxLevel()
        {
            // Rapidly level to max
            for (int i = 0; i < 20; i++)
            {
                _hero.AddExp(_hero.ExpToNextLevel);
            }

            Assert.AreEqual(_hero.MaxLevel, _hero.CurrentLevel,
                "Should not exceed MaxLevel");
        }

        [Test]
        public void AddExp_AtMaxLevel_DoesNotGainMoreExp()
        {
            // Level to max
            for (int i = 0; i < 20; i++)
            {
                _hero.AddExp(_hero.ExpToNextLevel);
            }

            float expBefore = _hero.CurrentExp;
            _hero.AddExp(50f); // Should be ignored

            Assert.AreEqual(expBefore, _hero.CurrentExp, 0.01f,
                "Should not gain XP at max level");
        }

        // ============================================================
        // ExpToNextLevel scaling
        // ============================================================

        [Test]
        public void AddExp_LevelUp_ScalesExpRequirement()
        {
            float expFirst = _hero.ExpToNextLevel; // 100
            _hero.AddExp(expFirst); // Level 2
            float expSecond = _hero.ExpToNextLevel; // 120

            Assert.AreEqual(expFirst * 1.2f, expSecond, 0.01f,
                "ExpToNextLevel should scale by 1.2x per level");
        }

        // ============================================================
        // Starting state
        // ============================================================

        [Test]
        public void HeroEntity_StartsAtLevelOne()
        {
            Assert.AreEqual(1, _hero.CurrentLevel);
        }

        [Test]
        public void HeroEntity_StartsWithZeroGold()
        {
            Assert.AreEqual(0f, _hero.CurrentGold, 0.01f);
        }

        [Test]
        public void HeroEntity_StartsWithZeroExp()
        {
            Assert.AreEqual(0f, _hero.CurrentExp, 0.01f);
        }
    }
}