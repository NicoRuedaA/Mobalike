using UnityEngine;
using MobaGameplay.AI;
using MobaGameplay.Game;

namespace MobaGameplay.Core
{
    /// <summary>
    /// Entidad de enemigo. Extiende BaseEntity con comportamiento específico de enemigo:
    /// - Hover outline automático
    /// - AI Controller automático
    /// - Integración con GameStateManager
    /// </summary>
    public class EnemyEntity : BaseEntity
    {
        [Header("Enemy Settings")]
        [Tooltip("Cantidad de oro que suelta al morir.")]
        [SerializeField] private int goldReward = 10;
        
        [Tooltip("Experiencia que otorga al morir.")]
        [SerializeField] private int experienceReward = 25;
        
        // Referencia al AI Controller
        private EnemyAIController _aiController;
        
        // Referencia al Game State Manager
        private GameStateManager _gameStateManager;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Agregar Hover Outline automáticamente
            SetupHoverOutline();
            
            // Agregar AI Controller automáticamente si no existe
            SetupAIController();
        }
        
        private new void Start()
        {
            // Buscar GameStateManager
            _gameStateManager = GameStateManager.Instance;
        }
        
        /// <summary>
        /// Configura el hover outline para este enemigo.
        /// </summary>
        private void SetupHoverOutline()
        {
            var hoverOutline = GetComponent<MobaGameplay.UI.Targeting.HoverOutline>();
            if (hoverOutline == null)
            {
                hoverOutline = gameObject.AddComponent<MobaGameplay.UI.Targeting.HoverOutline>();
                hoverOutline.outlineColor = Color.red;
                hoverOutline.outlineWidth = 0.02f;
            }
        }
        
        /// <summary>
        /// Configura el AI Controller si no existe. Si hay duplicados los elimina.
        /// </summary>
        private void SetupAIController()
        {
            var controllers = GetComponents<EnemyAIController>();

            if (controllers.Length > 1)
            {
                // Keep only the first one and destroy duplicates added at runtime
                for (int i = 1; i < controllers.Length; i++)
                {
                    Destroy(controllers[i]);
                }
            }

            _aiController = controllers.Length > 0 ? controllers[0] : gameObject.AddComponent<EnemyAIController>();
        }
        
        protected override void Die()
        {
            base.Die();
            
            Debug.Log($"[EnemyEntity] {gameObject.name} has died. Reward: {goldReward} gold, {experienceReward} XP");
            
            // Notificar al GameStateManager
            if (_gameStateManager != null)
            {
                // El EnemyAIController maneja su propia limpieza y notifica al GSM
                // Aquí solo manejamos recompensas
                NotifyKillReward();
            }
            
            // Deshabilitar AI
            if (_aiController != null)
            {
                _aiController.enabled = false;
            }
            
            // Destruir después de un delay para que los eventos disparen
            Destroy(gameObject, 1f);
        }
        
        /// <summary>
        /// Notifica las recompensas al jugador/GSM.
        /// </summary>
        private void NotifyKillReward()
        {
            // Aquí se podría:
            // 1. Award gold al jugador
            // 2. Award XP al jugador
            // 3. Notificar a sistemas de quest/logros
            
            // Por ahora solo log
            // En el futuro, esto se conectará al HeroEntity y al inventory system
        }
        
        /// <summary>
        /// Obtiene la referencia al AI Controller.
        /// </summary>
        public EnemyAIController GetAIController()
        {
            return _aiController;
        }
        
        /// <summary>
        /// Establece el objetivo del AI.
        /// </summary>
        public void SetTarget(BaseEntity target)
        {
            if (_aiController != null)
            {
                _aiController.SetTarget(target);
            }
        }
        
        /// <summary>
        /// Obtiene la cantidad de oro que suelta.
        /// </summary>
        public int GetGoldReward()
        {
            return goldReward;
        }
        
        /// <summary>
        /// Obtiene la experiencia que otorga.
        /// </summary>
        public int GetExperienceReward()
        {
            return experienceReward;
        }
    }
}
