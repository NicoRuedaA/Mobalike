using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.UI.Targeting;

namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Base class for all abilities.
    /// Handles cooldown, mana cost, targeting, and casting.
    /// </summary>
    public abstract class BaseAbility : MonoBehaviour
    {
        // Constants
        protected const float MIN_COOLDOWN = 0f;
        protected const float MIN_CAST_TIME = 0f;
        protected const float MIN_MANA_COST = 0f;

        // Serializable Fields (backing fields with underscores)
        [Header("Ability Settings")]
        [SerializeField] private string _abilityName = "New Ability";
        [SerializeField] private float _cooldown = 5f;
        [SerializeField] private float _castTime = 0f;
        [SerializeField] private float _manaCost = 0f;

        [Header("Targeting Settings")]
        [SerializeField] private IndicatorType _targetingType = IndicatorType.Circle;
        [SerializeField] private float _castRange = 10f;
        [SerializeField] private float _range = 2f;
        [SerializeField] private float _width = 1f;

        // State
        private float currentCooldown = 0f;
        protected BaseEntity ownerEntity;

        // Public Properties with setters (for editor scripts)
        public string abilityName { get => _abilityName; set => _abilityName = value; }
        public float cooldown { get => _cooldown; set => _cooldown = Mathf.Max(0f, value); }
        public float castTime { get => _castTime; set => _castTime = Mathf.Max(0f, value); }
        public float manaCost { get => _manaCost; set => _manaCost = Mathf.Max(0f, value); }
        public IndicatorType TargetingType { get => _targetingType; set => _targetingType = value; }
        public float CastRange { get => _castRange; set => _castRange = Mathf.Max(0f, value); }
        public float Range { get => _range; set => _range = Mathf.Max(0f, value); }
        public float Width { get => _width; set => _width = Mathf.Max(0f, value); }

        // Read-only Properties
        public bool IsOnCooldown => currentCooldown > 0f;
        public float CurrentCooldown => currentCooldown;
        public float MaxCooldown => _cooldown;
        public float CooldownPercent => _cooldown > 0f ? currentCooldown / _cooldown : 0f;
        public bool HasEnoughMana => ownerEntity != null && ownerEntity.CurrentMana >= _manaCost;
        public BaseEntity Owner => ownerEntity;

        // Events
        public event System.Action<BaseAbility> OnAbilityExecuted;
        public event System.Action<BaseAbility> OnCooldownStarted;

        public virtual void Initialize(BaseEntity owner)
        {
            ownerEntity = owner;
        }

        private void Update()
        {
            // Tick down cooldown
            if (currentCooldown > 0f)
            {
                currentCooldown -= Time.deltaTime;
                currentCooldown = Mathf.Max(0f, currentCooldown);
            }
        }

        /// <summary>
        /// Check if ability can be cast right now.
        /// </summary>
        public virtual bool CanCast()
        {
            if (ownerEntity == null) return false;
            return !IsOnCooldown && HasEnoughMana && !ownerEntity.IsDead;
        }

        /// <summary>
        /// Begin targeting phase. Shows range indicator etc.
        /// </summary>
        public virtual void BeginTargeting()
        {
            if (TargetingManager.Instance != null)
            {
                TargetingManager.Instance.StartTargeting(this);
            }
        }

        /// <summary>
        /// Cancel targeting phase.
        /// </summary>
        public virtual void CancelTargeting()
        {
            if (TargetingManager.Instance != null)
            {
                TargetingManager.Instance.CancelTargeting();
            }
        }

        /// <summary>
        /// Execute the ability with given target.
        /// Override OnExecute for custom ability behavior, or override this method entirely.
        /// </summary>
        public virtual void ExecuteCast(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (!CanCast()) return;

            // Consume mana
            ownerEntity.CurrentMana -= _manaCost;
            
            // Start cooldown
            currentCooldown = _cooldown;
            OnCooldownStarted?.Invoke(this);

            // Cleanup targeting UI
            CancelTargeting();

            // Trigger event
            OnAbilityExecuted?.Invoke(this);

            // Execute ability-specific logic
            OnExecute(targetPosition, targetEntity);
        }

        /// <summary>
        /// Override this method to implement ability-specific logic.
        /// Called automatically by ExecuteCast after mana/cooldown handling.
        /// </summary>
        protected virtual void OnExecute(Vector3 targetPosition, BaseEntity targetEntity)
        {
            // Default implementation does nothing
            // Override in derived classes to add ability-specific behavior
        }

        /// <summary>
        /// Get the world position for ability range indicator.
        /// </summary>
        public Vector3 GetIndicatorPosition()
        {
            return ownerEntity != null ? ownerEntity.transform.position : transform.position;
        }

        /// <summary>
        /// Get remaining cooldown as percentage (1 = full cooldown, 0 = ready).
        /// </summary>
        public float GetCooldownRemainingPercent()
        {
            return Mathf.Clamp01(currentCooldown / Mathf.Max(0.001f, _cooldown));
        }

        /// <summary>
        /// Check if target is within cast range.
        /// </summary>
        public bool IsInRange(Vector3 targetPosition)
        {
            if (ownerEntity == null) return false;
            
            float distance = Vector3.Distance(ownerEntity.transform.position, targetPosition);
            return distance <= _castRange;
        }
    }
}
