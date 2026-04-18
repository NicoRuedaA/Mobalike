using UnityEngine;

namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Tipo de combate del héroe. Define cómo ataca cuerpo a cuerpo.
    /// </summary>
    public enum CombatType
    {
        Ranged,    // Usa proyectiles (arquero, mago)
        Melee      // Usa ataques cercanos (guerrero, assassin)
    }

    /// <summary>
    /// Define una clase de héroe para el MOBA. Cada clase tiene:
    /// - Stats base únicos
    /// - 4 habilidades específicas
    /// - Modelo visual (skin)
    /// - Tipo de combate (ranged/melee)
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

        [Tooltip("Tipo de combate: Ranged (proyectiles) o Melee (cuerpo a cuerpo)")]
        public CombatType combatType = CombatType.Ranged;

        [Tooltip("Mostrar líneas de aim (LaserSight, LineRenderer) - solo para ranged")]
        public bool showAimLines = true;

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

        [Header("Ranged Combat (if combatType == Ranged)")]
        [Tooltip("Prefab del proyectil para ataque básico")]
        public GameObject basicAttackProjectilePrefab;

        [Tooltip("Velocidad del proyectil")]
        public float projectileSpeed = 25f;

        [Tooltip("Distancia máxima del proyectil")]
        public float projectileMaxDistance = 20f;

        [Tooltip("Multiplicador de daño cuando está cargado")]
        public float chargedDamageMultiplier = 1.5f;

        [Tooltip("Multiplicador de velocidad cuando está cargado")]
        public float chargedSpeedMultiplier = 1.3f;

        [Tooltip("Multiplicador de tamaño cuando está cargado")]
        public float chargedSizeMultiplier = 1.5f;
        
        [Header("Ammo System (Optional)")]
        [Tooltip("Si es true, este héroe usa sistema de munición. Si es false, munición infinita.")]
        public bool hasAmmoSystem = false;
        
        [Tooltip("Munición máxima (solo si hasAmmoSystem = true)")]
        public int maxAmmo = 6;
        
        [Tooltip("Tiempo de recarga en segundos (solo si hasAmmoSystem = true)")]
        public float reloadTime = 2f;

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
            return !string.IsNullOrEmpty(className);
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