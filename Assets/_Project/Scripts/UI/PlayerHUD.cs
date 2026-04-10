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
        private AbilityController legacyAbilities;
        private AbilitySystem newAbilities;
        private bool usesNewSystem;
        private bool abilitiesAssigned;

        private void Start()
        {
            TryBindPlayer();
            
            #if UNITY_EDITOR
            Debug.Log($"[PlayerHUD] Start: playerEntity={playerEntity != null}, slot1={slot1}, slot2={slot2}, slot3={slot3}, slot4={slot4}");
            #endif
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
                #if UNITY_EDITOR
                Debug.Log("[PlayerHUD] About to call TryAssignAbilities");
                #endif
                TryAssignAbilities();
                abilitiesAssigned = true;
                
                #if UNITY_EDITOR
                Debug.Log("[PlayerHUD] TryAssignAbilities complete");
                #endif
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
                legacyAbilities = playerGo.GetComponent<AbilityController>();
                newAbilities = playerGo.GetComponent<AbilitySystem>();
                usesNewSystem = newAbilities != null;
            }
        }

        private void TryAssignAbilities()
        {
            #if UNITY_EDITOR
            Debug.Log($"[PlayerHUD] TryAssignAbilities: usesNewSystem={usesNewSystem}, newAbilities={newAbilities != null}");
            Debug.Log($"[PlayerHUD] Slots: s1={slot1 != null}, s2={slot2 != null}, s3={slot3 != null}, s4={slot4 != null}");
            #endif
            
            if (usesNewSystem)
            {
                // New data-driven system
                if (newAbilities == null) 
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[PlayerHUD] newAbilities is NULL!");
                    #endif
                    return;
                }

                if (slot1 != null)
                {
                    var data = newAbilities.GetAbilityData(0);
                    #if UNITY_EDITOR
                    Debug.Log($"[PlayerHUD] Slot1 data: {data?.abilityName}");
                    #endif
                    if (data != null) slot1.AssignAbility(data, newAbilities, 0);
                }
                if (slot2 != null)
                {
                    var data = newAbilities.GetAbilityData(1);
                    #if UNITY_EDITOR
                    Debug.Log($"[PlayerHUD] Slot2 data: {data?.abilityName}");
                    #endif
                    if (data != null) slot2.AssignAbility(data, newAbilities, 1);
                }
                if (slot3 != null)
                {
                    var data = newAbilities.GetAbilityData(2);
                    #if UNITY_EDITOR
                    Debug.Log($"[PlayerHUD] Slot3 data: {data?.abilityName}");
                    #endif
                    if (data != null) slot3.AssignAbility(data, newAbilities, 2);
                }
                if (slot4 != null)
                {
                    var data = newAbilities.GetAbilityData(3);
                    #if UNITY_EDITOR
                    Debug.Log($"[PlayerHUD] Slot4 data: {data?.abilityName}");
                    #endif
                    if (data != null) slot4.AssignAbility(data, newAbilities, 3);
                }
            }
            else
            {
                // Old MonoBehaviour system
                if (legacyAbilities == null) 
                {
                    #if UNITY_EDITOR
                    Debug.LogWarning("[PlayerHUD] legacyAbilities is NULL!");
                    #endif
                    return;
                }

                if (slot1 != null && slot1.GetAbility() == null) 
                    slot1.AssignAbility(legacyAbilities.Ability1);
                if (slot2 != null && slot2.GetAbility() == null) 
                    slot2.AssignAbility(legacyAbilities.Ability2);
                if (slot3 != null && slot3.GetAbility() == null) 
                    slot3.AssignAbility(legacyAbilities.Ability3);
                if (slot4 != null && slot4.GetAbility() == null) 
                    slot4.AssignAbility(legacyAbilities.Ability4);
            }
        }
    }
}