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

            // Actualizar barras de recursos suavemente
            if (healthBar != null)
                healthBar.UpdateValue(playerEntity.CurrentHealth, playerEntity.MaxHealth);

            if (manaBar != null)
                manaBar.UpdateValue(playerEntity.CurrentMana, playerEntity.MaxMana);
        }

        private void TryBindPlayer()
        {
            // Intentar por nombre (compatibilidad con escena actual)
            GameObject playerGo = GameObject.Find("Player");

            // Fallback por tag para no depender de un nombre fijo
            if (playerGo == null)
            {
                playerGo = GameObject.FindGameObjectWithTag("Player");
            }

            if (playerGo != null)
            {
                playerEntity = playerGo.GetComponent<BaseEntity>();
                playerAbilities = playerGo.GetComponent<AbilityController>();

                // Vincular habilidades a los slots
                if (playerAbilities != null)
                {
                    if (slot1 != null) slot1.AssignAbility(playerAbilities.Ability1);
                    if (slot2 != null) slot2.AssignAbility(playerAbilities.Ability2);
                    if (slot3 != null) slot3.AssignAbility(playerAbilities.Ability3);
                }
                else
                {
                    Debug.LogWarning("[PlayerHUD] No AbilityController found on Player.");
                }
            }
        }
    }
}
