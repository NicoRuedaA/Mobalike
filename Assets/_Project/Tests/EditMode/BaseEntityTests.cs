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
        /// Adds a BoxCollider so Die() can find and disable it.
        /// </summary>
        private class TestEntity : BaseEntity
        {
            // Expose for test assertions
            public float PublicCurrentHealth => currentHealth;
            public float PublicMaxHealth => maxHealth;
            public float PublicCurrentMana => currentMana;
            public float PublicMaxMana => maxMana;
            public float PublicManaRegen => manaRegen;
            public bool PublicManaInitialized => manaInitialized;

            // Test helpers
            public int OnTakeDamageCallCount { get; private set; }
            public int OnDeathCallCount { get; private set; }

            protected override void Start()
            {
                // Call base Start to initialize health/mana
                base.Start();
            }

            public void ForceInitialize()
            {
                // Manually initialize for tests that can't rely on Unity lifecycle
                currentHealth = maxHealth;
                currentMana = maxMana;
                manaInitialized = true;
            }

            public void SetOnTakeDamageCallback(System.Action<DamageInfo> callback)
            {
                OnTakeDamage += callback;
            }

            // Override Awake to set up test defaults before base.Awake
            protected override void Awake()
            {
                base.Awake();
            }
        }

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("TestEntity");
            _gameObject.AddComponent<BoxCollider>(); // Needed by Die()
            _entity = _gameObject.AddComponent<TestEntity>();

            // BaseEntity.Start() sets currentHealth/maxHealth, but in EditMode tests
            // Unity lifecycle may not run Start. Force initialize.
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
            // Arrange: entity starts at MaxHealth (1000 by default)
            float healthBefore = _entity.CurrentHealth;
            float damageAmount = 100f;

            // Act
            _entity.TakeDamage(new DamageInfo(damageAmount, DamageType.TrueDamage, null));

            // Assert: health reduced by damage amount
            Assert.AreEqual(healthBefore - damageAmount, _entity.CurrentHealth, 0.01f);
        }

        [Test]
        public void TakeDamage_TrueDamage_FullAmount()
        {
            // True damage bypasses all reduction
            float healthBefore = _entity.CurrentHealth;

            _entity.TakeDamage(new DamageInfo(200f, DamageType.TrueDamage, null));

            Assert.AreEqual(healthBefore - 200f, _entity.CurrentHealth, 0.01f);
        }

        [Test]
        public void TakeDamage_PhysicalDamage_ReducedByArmor()
        {
            // With 30 armor: reduction = 100 / (100 + 30) = 0.769...
            // Damage = 100 * 0.769 = 76.9
            float healthBefore = _entity.CurrentHealth;
            float expectedDamage = 100f * (100f / (100f + 30f));

            _entity.TakeDamage(new DamageInfo(100f, DamageType.Physical, null));

            Assert.AreEqual(healthBefore - expectedDamage, _entity.CurrentHealth, 0.1f);
        }

        [Test]
        public void TakeDamage_MagicalDamage_ReducedByMR()
        {
            // With 30 MR: reduction = 100 / (100 + 30) = 0.769...
            float healthBefore = _entity.CurrentHealth;
            float expectedDamage = 100f * (100f / (100f + 30f));

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
            // Before the fix, damage was applied twice (subtract + bonus subtract + multiply).
            // After: multiplier applied BEFORE subtracting health.
            float healthBefore = _entity.CurrentHealth;
            float damageAmount = 100f;

            // Use IsCritical = true with 1.5x multiplier (default)
            var critDamage = new DamageInfo(damageAmount, DamageType.TrueDamage, null, isCritical: true);

            _entity.TakeDamage(critDamage);

            // Expected: 100 * 1.5 = 150 damage. Health = 1000 - 150 = 850.
            // Bug would have been: 100 + (100*0.5) = 150 subtracted, then actualDamage *= 1.5
            // making it look like 150 total but actually doing ~250 damage.
            float expectedHealth = healthBefore - (damageAmount * _entity.CriticalMultiplier);
            Assert.AreEqual(expectedHealth, _entity.CurrentHealth, 0.01f,
                $"Critical hit should do {damageAmount * _entity.CriticalMultiplier:F1} total damage, not more");
        }

        [Test]
        public void TakeDamage_CriticalHit_ExactMultiplier()
        {
            // Verify the exact multiplier value (1.5x default)
            float healthBefore = _entity.CurrentHealth;

            _entity.TakeDamage(new DamageInfo(200f, DamageType.TrueDamage, null, isCritical: true));

            // 200 * 1.5 = 300 true damage
            Assert.AreEqual(healthBefore - 300f, _entity.CurrentHealth, 0.01f);
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
            // Kill the entity
            _entity.TakeDamage(new DamageInfo(9999f, DamageType.TrueDamage, null));
            Assert.IsTrue(_entity.IsDead);

            float healthAfterDeath = _entity.CurrentHealth;

            // Try to damage again
            _entity.TakeDamage(new DamageInfo(100f, DamageType.TrueDamage, null));

            // Health should not change (early return in TakeDamage when IsDead)
            Assert.AreEqual(healthAfterDeath, _entity.CurrentHealth, 0.01f);
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
        // Mana — Regeneration (Bug #6 fix verification)
        // ============================================================

        [Test]
        public void CurrentMana_Setter_FiresOnManaChanged()
        {
            // Verify that the property setter fires the event (Phase 1 fix)
            float? oldMana = null;
            float? newMana = null;
            _entity.OnManaChanged += (oldVal, newVal) => { oldMana = oldVal; newMana = newVal; };

            // Reduce mana manually to trigger a change
            _entity.CurrentMana = 400f;

            Assert.IsNotNull(oldMana, "OnManaChanged should have fired");
            Assert.IsNotNull(newMana, "OnManaChanged should have fired");
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
    }
}