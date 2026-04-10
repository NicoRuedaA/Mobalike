using UnityEngine;

namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Define una clase de héros para el MOBA. Cada clase tiene:
    /// - Stats base únicos
    /// - 4 habilidades específicas
    /// - Modelo visual (skin)
    /// 
    /// Crear: Right-click → Create → MobaGameplay → Hero Class
    /// </summary>
    [CreateAssetMenu(fileName = "NewHeroClass", menuName = "MobaGameplay/Hero Class")]
    public class HeroClass : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Nombre de la clase (ej: Warrior, Mage, Assassin)")]
        public string className = "New Class";

        [Tooltip("Descripción breve de la clase")]
        public string description = "";

        [Tooltip("Rol principal (Tank, Fighter, Mage, Assassin, Ranger, Support)")]
        public string role = "Fighter";

        [Header("Visuals")]
        [Tooltip("Prefab del modelo visual. Debe tener Animator con el mismo rig que las demás clases.")]
        public GameObject modelPrefab;

        [Tooltip("Animator Controller compartido (todos los modelos deben tener el mismo esqueleto)")]
        public RuntimeAnimatorController animatorController;

        [Header("Base Stats")]
        [Tooltip("Salud base al nivel 1")]
        public float baseHealth = 500f;

        [Tooltip("Maná base al nivel 1")]
        public float baseMana = 300f;

        [Tooltip("Daño de ataque físico base")]
        public float baseAttackDamage = 50f;

        [Tooltip("Velocidad de movimiento base")]
        public float baseMoveSpeed = 5f;

        [Tooltip("Armadura física base")]
        public float baseArmor = 20f;

        [Tooltip("Resistencia mágica base")]
        public float baseMagicResist = 30f;

        [Tooltip("Regeneración de vida por segundo")]
        public float healthRegen = 1f;

        [Tooltip("Regeneración de maná por segundo")]
        public float manaRegen = 1f;

        [Header("Abilities")]
        [Tooltip("Las 4 habilidades de esta clase (orden: Q, W, E, R)")]
        public AbilityData[] abilities = new AbilityData[4];

        /// <summary>
        /// Obtiene la habilidad en el slot especificado (0-3).
        /// </summary>
        public AbilityData GetAbility(int slot)
        {
            if (slot < 0 || slot >= 4 || abilities == null || abilities[slot] == null)
                return null;
            return abilities[slot];
        }

        /// <summary>
        /// Valida que la clase esté correctamente configurada.
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(className) 
                && modelPrefab != null 
                && animatorController != null;
        }

        void OnValidate()
        {
            // Asegurar que el array tenga 4 elementos
            if (abilities == null || abilities.Length != 4)
            {
                AbilityData[] old = abilities ?? new AbilityData[0];
                abilities = new AbilityData[4];
                for (int i = 0; i < Mathf.Min(old.Length, 4); i++)
                {
                    abilities[i] = old[i];
                }
            }
        }
    }
}