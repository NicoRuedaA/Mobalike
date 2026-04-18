using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Abilities;

namespace MobaGameplay.UI
{
    /// <summary>
    /// HUD principal del jugador. Muestra barras de vida/maná y habilidades.
    /// Usa exclusivamente el sistema data-driven (AbilitySystem).
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Stats UI")]
        [SerializeField] private ResourceBarUI healthBar;
        [SerializeField] private ResourceBarUI manaBar;

        [Header("Abilities UI")]
        [SerializeField] private AbilitySlotUI slot1;
        [SerializeField] private AbilitySlotUI slot2;
        [SerializeField] private AbilitySlotUI slot3;
        [SerializeField] private AbilitySlotUI slot4;

        private BaseEntity playerEntity;
        private AbilitySystem abilitySystem;
        private bool abilitiesAssigned;

        private void Start()
        {
            TryBindPlayer();
        }

        private void Update()
        {
            // Si el jugador se crea después del HUD, reintentar hasta enlazarlo
            if (playerEntity == null)
            {
                TryBindPlayer();
                if (playerEntity == null) return;
            }

            // Solo intentar asignar una vez
            if (!abilitiesAssigned)
            {
                TryAssignAbilities();
                abilitiesAssigned = true;
            }

            // Actualizar barras de recursos
            if (healthBar != null)
                healthBar.UpdateValue(playerEntity.CurrentHealth, playerEntity.MaxHealth);

            if (manaBar != null)
                manaBar.UpdateValue(playerEntity.CurrentMana, playerEntity.MaxMana);
        }

        private void TryBindPlayer()
        {
            GameObject playerGo = GameObject.Find("Player");

            if (playerGo == null)
            {
                playerGo = GameObject.FindGameObjectWithTag("Player");
            }

            if (playerGo != null)
            {
                playerEntity = playerGo.GetComponent<BaseEntity>();
                abilitySystem = playerGo.GetComponent<AbilitySystem>();
            }
        }

        private void TryAssignAbilities()
        {
            if (abilitySystem == null) return;

            if (slot1 != null)
            {
                var data = abilitySystem.GetAbilityData(0);
                if (data != null) slot1.AssignAbility(data, abilitySystem, 0);
            }
            if (slot2 != null)
            {
                var data = abilitySystem.GetAbilityData(1);
                if (data != null) slot2.AssignAbility(data, abilitySystem, 1);
            }
            if (slot3 != null)
            {
                var data = abilitySystem.GetAbilityData(2);
                if (data != null) slot3.AssignAbility(data, abilitySystem, 2);
            }
            if (slot4 != null)
            {
                var data = abilitySystem.GetAbilityData(3);
                if (data != null) slot4.AssignAbility(data, abilitySystem, 3);
            }
        }
    }
}