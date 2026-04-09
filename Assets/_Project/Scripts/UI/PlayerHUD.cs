using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Abilities;

namespace MobaGameplay.UI
{
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
        private AbilityController playerAbilities;

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

            // Reintentar asignar abilities si aún no están asignadas (timing issue)
            if (playerAbilities != null && slot1 != null && slot1.GetAbility() == null)
            {
                TryAssignAbilities();
            }

            // Actualizar barras de recursos suavemente
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
                playerAbilities = playerGo.GetComponent<AbilityController>();

                TryAssignAbilities();
            }
        }

        private void TryAssignAbilities()
        {
            if (playerAbilities == null) return;

            // Solo asignar si el slot existe y aún no tiene ability asignada
            if (slot1 != null && slot1.GetAbility() == null) 
                slot1.AssignAbility(playerAbilities.Ability1);
            if (slot2 != null && slot2.GetAbility() == null) 
                slot2.AssignAbility(playerAbilities.Ability2);
            if (slot3 != null && slot3.GetAbility() == null) 
                slot3.AssignAbility(playerAbilities.Ability3);
            if (slot4 != null && slot4.GetAbility() == null) 
                slot4.AssignAbility(playerAbilities.Ability4);

            Debug.Log($"[PlayerHUD] Bound abilities. A1:{playerAbilities.Ability1?.abilityName} A2:{playerAbilities.Ability2?.abilityName} A3:{playerAbilities.Ability3?.abilityName} A4:{playerAbilities.Ability4?.abilityName}");
        }
    }
}
