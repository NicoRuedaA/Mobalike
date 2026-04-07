using UnityEngine;

namespace MobaGameplay.Core
{
    /// <summary>
    /// Base class for all player controllers.
    /// Handles common setup and provides reference to the controlled entity.
    /// </summary>
    [RequireComponent(typeof(BaseEntity))]
    public abstract class BaseController : MonoBehaviour
    {
        [SerializeField, Tooltip("Enable debug logging for this controller")]
        protected bool debugMode = false;

        protected BaseEntity entity;

        protected virtual void Awake()
        {
            entity = GetComponent<BaseEntity>();
            
            #if UNITY_EDITOR
            if (debugMode)
            {
                Debug.Log($"[{GetType().Name}] Initialized for entity: {entity?.name}");
            }
            #endif
        }

        /// <summary>
        /// Check if the controlled entity is valid and alive.
        /// Use this before processing any input or actions.
        /// </summary>
        protected bool IsEntityValid => entity != null && !entity.IsDead;
    }
}
