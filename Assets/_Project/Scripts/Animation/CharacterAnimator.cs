using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Animation
{
    [RequireComponent(typeof(BaseEntity))]
    public class CharacterAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Animator animator;
        
        private BaseEntity entity;
        private int animIDSpeed;
        private int animIDMotionSpeed;
        private int animIDGrounded;
        private int animIDJump;
        private int animIDFreeFall;
        private float animationBlend;

        private void Awake()
        {
            entity = GetComponent<BaseEntity>();
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError($"CharacterAnimator en {gameObject.name} NO encontró un Animator en sus hijos. ¡Las animaciones no funcionarán!", this);
            }
            AssignAnimationIDs();
        }

        private void AssignAnimationIDs()
        {
            animIDSpeed = Animator.StringToHash("Speed");
            animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDJump = Animator.StringToHash("Jump");
            animIDFreeFall = Animator.StringToHash("FreeFall");
        }

        private void Start()
        {
            if (entity.Combat != null) entity.Combat.OnBasicAttack += TriggerAttackAnimation;
        }

        private void OnDestroy()
        {
            if (entity != null && entity.Combat != null) entity.Combat.OnBasicAttack -= TriggerAttackAnimation;
        }

        private void TriggerAttackAnimation()
        {
            if (animator != null) animator.SetTrigger("Attack");
        }

        private void Update()
        {
            if (animator == null || entity.Movement == null) return;

            float targetSpeed = entity.Movement.CurrentVelocity;
            
            // Siempre mantener el Blend positivo (0 a SprintSpeed) para no distorsionar el rig
            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * 10f);
            if (animationBlend < 0.01f) animationBlend = 0f;

            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, 1f);

            // Sincronizar parámetros de salto y caída
            bool isGrounded = entity.Movement.IsGrounded;
            bool isJumping = entity.Movement.IsJumping;

            animator.SetBool(animIDGrounded, isGrounded);
            animator.SetBool(animIDJump, isJumping);
            animator.SetBool(animIDFreeFall, !isGrounded && !isJumping);
        }

        public void OnFootstep(AnimationEvent animationEvent) { }
        public void OnLand(AnimationEvent animationEvent) { }
    }
}
