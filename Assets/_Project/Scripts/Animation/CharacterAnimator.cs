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
            if (animator == null) animator = GetComponentInChildren<Animator>();
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
            if (targetSpeed > 0.01f) {
                Vector3 localVel = transform.InverseTransformDirection(entity.Movement.VelocityVector);
                if (localVel.z < -0.1f) {
                    targetSpeed = -targetSpeed; // Negativo para caminar hacia atrás
                }
            }
            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * 10f);
            if (Mathf.Abs(animationBlend) < 0.01f) animationBlend = 0f;

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
