using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Animation
{
    [RequireComponent(typeof(BaseEntity))]
    public class CharacterAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("Referencia al Animator. Si está vacío, se buscará automáticamente.")]
        [SerializeField] private Animator animator;
        
        private BaseEntity entity;
        private int animIDSpeed;
        private int animIDMotionSpeed;
        private float animationBlend;

        private void Awake()
        {
            entity = GetComponent<BaseEntity>();
            
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            animIDSpeed = Animator.StringToHash("Speed");
            animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void Update()
        {
            // El componente sobrevive y no da error aunque la entidad no tenga un script de movimiento
            if (animator == null || entity.Movement == null) return;

            // Lee la velocidad actual del componente de movimiento
            float targetSpeed = entity.Movement.CurrentVelocity;

            // Suavizado de la transición (Blend Tree)
            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * 10f);
            if (animationBlend < 0.01f) animationBlend = 0f;

            // Envía la velocidad al Animator
            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, 1f); // Velocidad de reproducción normal
        }

        // --- Manejo de Eventos de Animación (Starter Assets) ---
        
        public void OnFootstep(AnimationEvent animationEvent)
        {
            // Este evento es disparado automáticamente por los clips de animación Walk/Run
            // Aquí podríamos reproducir sonidos de pasos en el futuro.
            // Por ahora, tener este método vacío silencia los errores de la consola.
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                // Ejemplo: Play footstep audio
            }
        }

        public void OnLand(AnimationEvent animationEvent)
        {
            // Evento disparado al aterrizar. Silenciamos el error.
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                // Ejemplo: Play landing audio
            }
        }
    }
}
