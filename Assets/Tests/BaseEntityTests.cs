using NUnit.Framework;
using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace MobaGameplay.Tests
{
    /// <summary>
    /// Tests for BaseEntity.TakeDamage(), critical hits, armor, death, and mana.
    /// These tests verify the fixes from Phase 1: critical multiplier applied once
    /// (not twice), mana regen fires events, and damage reduction works correctly.
    /// </summary>
    [TestFixture]
    public class BaseEntityTests
    {
        private GameObject _gameObject;
        private TestEntity _entity;

        /// <summary>
        /// Concrete test subclass since BaseEntity is abstract.
        /// Uses only public properties to access and modify state.
        /// </summary>
        private class TestEntity : BaseEntity
        {
            protected override void Start()
            {
                // Call base Start to initialize health/mana
                base.Start();
            }

            /// <summary>
            /// Force-initialize health and mana for EditMode tests
            /// where Unity lifecycle (Start) may not run.
            /// Uses public property setters which fire events.
            /// </summary>
            public void ForceInitialize()
            {
                CurrentHealth = MaxHealth;
                CurrentMana = MaxMana;
            }
        }

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("TestEntity");
            _gameObject.AddComponent<BoxCollider>(); // Needed by Die()
            _entity = _gameObject.AddComponent<TestEntity>();

            // Force initialize since Start() may not run in EditMode tests
            _entity.ForceInitialize();
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
        // TakeDamage — Normal damage
        // ============================================================

        [Test]
        public void TakeDamage_NormalDamage_ReducesHealth()
        {
            float healthBefore = _entity.CurrentHealth;
            float damageAmount = 100f;

            _entity.TakeDamage(new DamageInfo(damageAmount, DamageType.TrueDamage, null));

            Assert.AreEqual(healthBefore - damageAmount, _entity.CurrentHealth, 0.01f);
        }

        [Test]
        public void TakeDamage_TrueDamage_FullAmount()
        {
            float healthBefore = _entity.CurrentHealth;

            _entity.TakeDamage(new DamageInfo(200f, DamageType.TrueDamage, null));

            Assert.AreEqual(healthBefore - 200f, _entity.CurrentHealth, 0.01f);
        }

        [Test]
        public void TakeDamage_PhysicalDamage_ReducedByArmor()
        {
            // With 30 armor (default): reduction = 100 / (100 + 30) ≈ 0.769
            float healthBefore = _entity.CurrentHealth;
            float expectedDamage = 100f * (100f / (100f + _entity.PhysicalArmor));

            _entity.TakeDamage(new DamageInfo(100f, DamageType.Physical, null));

            Assert.AreEqual(healthBefore - expectedDamage, _entity.CurrentHealth, 0.1f);
        }

        [Test]
        public void TakeDamage_MagicalDamage_ReducedByMR()
        {
            // With 30 MR (default): reduction = 100 / (100 + 30) ≈ 0.769
            float healthBefore = _entity.CurrentHealth;
            float expectedDamage = 100f * (100f / (100f + _entity.MagicResistance));

            _entity.TakeDamage(new DamageInfo(100f, DamageType.Magical, null));

            Assert.AreEqual(healthBefore - expectedDamage, _entity.CurrentHealth, 0.1f);
        }

        // ============================================================
        // TakeDamage — Critical hit (Bug #1 fix verification)
        // ============================================================

        [Test]
        public void TakeDamage_CriticalHit_AppliesMultiplierOnce()
        {
            // This test verifies the Phase 1 fix: critical multiplier applied ONCE.
            // Before: damage subtracted THEN bonus subtracted THEN multiplied = ~2.5x
            // After: multiplier applied BEFORE subtracting health = 1.5x
            float healthBefore = _entity.CurrentHealth;
            float damageAmount = 100f;

            var critDamage = new DamageInfo(damageAmount, DamageType.TrueDamage, null, isCritical: true);
            _entity.TakeDamage(critDamage);

            // 100 * 1.5 = 150 total damage. Health = 1000 - 150 = 850.
            float expectedHealth = healthBefore - (damageAmount * _entity.CriticalMultiplier);
            Assert.AreEqual(expectedHealth, _entity.CurrentHealth, 0.01f,
                $"Critical hit should do {damageAmount * _entity.CriticalMultiplier:F1} total damage, not more");
        }

        [Test]
        public void TakeDamage_CriticalHit_ExactMultiplier()
        {
            float healthBefore = _entity.CurrentHealth;

            _entity.TakeDamage(new DamageInfo(200f, DamageType.TrueDamage, null, isCritical: true));

            // 200 * 1.5 = 300 true damage
            Assert.AreEqual(healthBefore - 300f, _entity.CurrentHealth, 0.01f);
        }

        [Test]
        public void TakeDamage_NonCritical_NoMultiplier()
        {
            float healthBefore = _entity.CurrentHealth;

            _entity.TakeDamage(new DamageInfo(200f, DamageType.TrueDamage, null, isCritical: false));

            // No multiplier — 200 damage exactly
            Assert.AreEqual(healthBefore - 200f, _entity.CurrentHealth, 0.01f);
        }

        // ============================================================
        // TakeDamage — Death
        // ============================================================

        [Test]
        public void TakeDamage_LethalDamage_SetsIsDead()
        {
            Assert.IsFalse(_entity.IsDead, "Entity should start alive");

            _entity.TakeDamage(new DamageInfo(9999f, DamageType.TrueDamage, null));

            Assert.IsTrue(_entity.IsDead, "Entity should be dead after lethal damage");
        }

        [Test]
        public void TakeDamage_DoesNotDamageDeadEntity()
        {
            _entity.TakeDamage(new DamageInfo(9999f, DamageType.TrueDamage, null));
            Assert.IsTrue(_entity.IsDead);

            float healthAfterDeath = _entity.CurrentHealth;

            _entity.TakeDamage(new DamageInfo(100f, DamageType.TrueDamage, null));

            Assert.AreEqual(healthAfterDeath, _entity.CurrentHealth, 0.01f,
                "Dead entity should not take more damage");
        }

        // ============================================================
        // TakeDamage — Events
        // ============================================================

        [Test]
        public void TakeDamage_FiresOnTakeDamageEvent()
        {
            int invokeCount = 0;
            _entity.OnTakeDamage += (_) => invokeCount++;

            _entity.TakeDamage(new DamageInfo(50f, DamageType.TrueDamage, null));

            Assert.AreEqual(1, invokeCount, "OnTakeDamage should fire once per hit");
        }

        [Test]
        public void TakeDamage_LethalDamage_FiresOnDeathEvent()
        {
            int deathCount = 0;
            _entity.OnDeath += (_, _) => deathCount++;

            _entity.TakeDamage(new DamageInfo(9999f, DamageType.TrueDamage, null));

            Assert.AreEqual(1, deathCount, "OnDeath should fire exactly once");
        }

        // ============================================================
        // Mana — Property setter fires event (Bug #6 fix verification)
        // ============================================================

        [Test]
        public void CurrentMana_Setter_FiresOnManaChanged()
        {
            float? oldMana = null;
            float? newMana = null;
            _entity.OnManaChanged += (oldVal, newVal) => { oldMana = oldVal; newMana = newVal; };

            _entity.CurrentMana = 400f;

            Assert.IsNotNull(oldMana, "OnManaChanged should have fired");
            Assert.AreEqual(400f, newMana.Value, 0.01f, "New mana value should be 400");
        }

        // ============================================================
        // Heal
        // ============================================================

        [Test]
        public void Heal_IncreasesHealth()
        {
            _entity.TakeDamage(new DamageInfo(200f, DamageType.TrueDamage, null));
            float healthAfterDamage = _entity.CurrentHealth;

            _entity.Heal(100f);

            Assert.AreEqual(healthAfterDamage + 100f, _entity.CurrentHealth, 0.01f);
        }

        [Test]
        public void Heal_DoesNotExceedMaxHealth()
        {
            float maxHealth = _entity.MaxHealth;

            _entity.Heal(5000f); // Way over max

            Assert.AreEqual(maxHealth, _entity.CurrentHealth, 0.01f,
                "Heal should not exceed MaxHealth");
        }

        [Test]
        public void Heal_DoesNotAffectDeadEntity()
        {
            _entity.TakeDamage(new DamageInfo(9999f, DamageType.TrueDamage, null));
            Assert.IsTrue(_entity.IsDead);

            float healthBefore = _entity.CurrentHealth;
            _entity.Heal(500f);

            Assert.AreEqual(healthBefore, _entity.CurrentHealth, 0.01f,
                "Heal should not affect dead entities");
        }

        // ============================================================
        // DamageInfo struct
        // ============================================================

        [Test]
        public void DamageInfo_DefaultIsCriticalFalse()
        {
            var dmg = new DamageInfo(50f, DamageType.Physical, null);

            Assert.IsFalse(dmg.IsCritical, "Default DamageInfo should not be critical");
        }

        [Test]
        public void DamageInfo_ExplicitCriticalFlag()
        {
            var dmg = new DamageInfo(50f, DamageType.Physical, null, isCritical: true);

            Assert.IsTrue(dmg.IsCritical, "Explicitly critical DamageInfo should be critical");
        }

        // ============================================================
        // Initial state
        // ============================================================

        [Test]
        public void Entity_StartsAtMaxHealth()
        {
            Assert.AreEqual(_entity.MaxHealth, _entity.CurrentHealth, 0.01f,
                "Entity should start at full health after initialization");
        }

        [Test]
        public void Entity_StartsAtMaxMana()
        {
            Assert.AreEqual(_entity.MaxMana, _entity.CurrentMana, 0.01f,
                "Entity should start at full mana after initialization");
        }

        [Test]
        public void Entity_StartsAlive()
        {
            Assert.IsFalse(_entity.IsDead, "Entity should start alive");
        }
    }
}