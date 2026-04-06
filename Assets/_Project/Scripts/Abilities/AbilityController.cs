using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Abilities
{
    [RequireComponent(typeof(BaseEntity))]
    public class AbilityController : MonoBehaviour
    {
        [Header("Equipped Abilities")]
        [SerializeField] private BaseAbility ability1;
        [SerializeField] private BaseAbility ability2;
        [SerializeField] private BaseAbility ability3;

        private BaseEntity entity;
        
        public BaseAbility ActiveTargetingAbility { get; private set; }

        private void Awake()
        {
            entity = GetComponent<BaseEntity>();
        }

        private void Start()
        {
            InitializeAbilities();
        }

        private void InitializeAbilities()
        {
            if (ability1 != null) ability1.Initialize(entity);
            if (ability2 != null) ability2.Initialize(entity);
            if (ability3 != null) ability3.Initialize(entity);
        }

        public void TryStartTargetingAbility1() { StartTargeting(ability1); }
        public void TryStartTargetingAbility2() { StartTargeting(ability2); }
        public void TryStartTargetingAbility3() { StartTargeting(ability3); }

        private void StartTargeting(BaseAbility ability)
        {
            if (ability == null) return;
            if (!ability.CanCast())
            {
                Debug.Log($"[Ability] {ability.abilityName} is on cooldown!");
                return;
            }

            // Cancel existing targeting if any
            if (ActiveTargetingAbility != null && ActiveTargetingAbility != ability)
            {
                ActiveTargetingAbility.CancelTargeting();
            }

            ActiveTargetingAbility = ability;
            ActiveTargetingAbility.BeginTargeting();
        }

        public void CancelTargeting()
        {
            if (ActiveTargetingAbility != null)
            {
                ActiveTargetingAbility.CancelTargeting();
                ActiveTargetingAbility = null;
            }
        }

        public void ExecuteTargeting(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (ActiveTargetingAbility != null)
            {
                ActiveTargetingAbility.ExecuteCast(targetPosition, targetEntity);
                ActiveTargetingAbility = null; // Clear state after casting
            }
        }
    }
}