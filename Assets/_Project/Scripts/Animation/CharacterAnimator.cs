using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Animation
{
    [RequireComponent(typeof(BaseEntity))]
    public class CharacterAnimator : MonoBehaviour
    {
        private Animator animator;

        [Header("Movement Clips")]
        [Tooltip("Si se deja vacío, se usa el clip original del AnimatorController")]
        [SerializeField] private AnimationClip clipIdleAir;
        [SerializeField] private AnimationClip clipIdle;
        [SerializeField] private AnimationClip clipWalk;
        [SerializeField] private AnimationClip clipRun;

        [Header("Jump Clips")]
        [SerializeField] private AnimationClip clipInAir;
        [SerializeField] private AnimationClip clipJumpStart;
        [SerializeField] private AnimationClip clipJumpLand;
        [SerializeField] private AnimationClip clipJumpLandWalk;
        [SerializeField] private AnimationClip clipJumpLandRun;

        [Header("Combat Clips")]
        [SerializeField] private AnimationClip clipAttack;
        [SerializeField] private AnimationClip clipDash;
        [SerializeField] private AnimationClip clipHit;
        [SerializeField] private AnimationClip clipCast;
        [SerializeField] private AnimationClip clipDeath;

        [Header("Upper Body Clips")]
        [SerializeField] private AnimationClip clipHookPunch;

        // ---- Referencias internas ----
        private BaseEntity entity;

        private int animIDSpeed;
        private int animIDMotionSpeed;
        private int animIDGrounded;
        private int animIDJump;
        private int animIDFreeFall;
        private int animIDAttack;
        private int animIDHit;
        private int animIDDeath;
        private int animIDDash;
        private int animIDCast;
        private int animIDIsDead;
        private int animIDIsAiming;
        private int animIDIsCharging;

        private float animationBlend;

        // Índices fijos del Blend Tree para mapear posición → SerializeField.
        // Estos corresponden al orden en que aparecen en el AnimatorController base.
        private const int BLEND_IDLE_AIR       = 0;
        private const int BLEND_IDLE           = 1;
        private const int BLEND_WALK           = 2;
        private const int BLEND_RUN            = 3;

        private const int BLEND_JUMP_LAND      = 0;
        private const int BLEND_JUMP_LAND_WALK = 1;
        private const int BLEND_JUMP_LAND_RUN  = 2;

        // ──────────────────────────────────────────────
        //  CICLO DE VIDA
        // ──────────────────────────────────────────────

        private void Awake()
        {
            entity = GetComponent<BaseEntity>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogError($"[CharacterAnimator] {gameObject.name}: no se encontró ningún Animator.", this);
                return;
            }

            ApplyAnimationOverrides();
            AssignAnimationIDs();
        }

        private void Start()
        {
            if (entity.Combat != null)        entity.Combat.OnBasicAttack              += TriggerAttackAnimation;
            if (entity.Movement != null)      entity.Movement.OnDashStart               += TriggerDashAnimation;
            if (entity.AbilitySystem != null) entity.AbilitySystem.OnAbilityExecuted   += TriggerCastAnimation;

            entity.OnTakeDamage += TriggerHitAnimation;
            entity.OnDeath      += TriggerDeathAnimation;
        }

        private void OnDestroy()
        {
            if (entity == null) return;

            if (entity.Combat != null)        entity.Combat.OnBasicAttack             -= TriggerAttackAnimation;
            if (entity.Movement != null)      entity.Movement.OnDashStart              -= TriggerDashAnimation;
            if (entity.AbilitySystem != null) entity.AbilitySystem.OnAbilityExecuted  -= TriggerCastAnimation;

            entity.OnTakeDamage -= TriggerHitAnimation;
            entity.OnDeath      -= TriggerDeathAnimation;
        }

        // ──────────────────────────────────────────────
        //  OVERRIDE CONTROLLER
        // ──────────────────────────────────────────────

        /// <summary>
        /// Crea un AnimatorOverrideController y reemplaza SOLO los clips
        /// que tengan un clip asignado en el Inspector. Los slots vacíos
        /// conservan la animación original del AnimatorController base.
        /// </summary>
        private void ApplyAnimationOverrides()
        {
            var baseController = animator.runtimeAnimatorController;
            var overrideController = new AnimatorOverrideController(baseController);

            var overrides = new System.Collections.Generic.List<
                System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);

            // Recorrer todos los clips del controller base
            for (int i = 0; i < overrides.Count; i++)
            {
                AnimationClip original = overrides[i].Key;
                AnimationClip replacement = FindOverrideFor(original);

                // Solo override si el usuario asignó un clip. Si no, dejar el original intacto.
                if (replacement != null)
                {
                    overrides[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(
                        original, replacement
                    );
                }
            }

            overrideController.ApplyOverrides(overrides);
            animator.runtimeAnimatorController = overrideController;
        }

        /// <summary>
        /// Dado un clip original del controller, determina si tenemos un
        /// reemplazo asignado en el Inspector. Usa el nombre del clip para
        /// identificarlo. Retorna null si no hay reemplazo → se conserva el original.
        /// </summary>
        private AnimationClip FindOverrideFor(AnimationClip original)
        {
            if (original == null) return null;

            string name = original.name;

            // Clips simples con nombre único
            switch (name)
            {
                case "Idle":          return clipIdle;
                case "Walk_N":        return clipWalk;
                case "Run_N":         return clipRun;
                case "InAir":         return clipInAir;
                case "JumpStart":     return clipJumpStart;
                case "JumpLand":      return clipJumpLand;
                case "Walk_N_Land":   return clipJumpLandWalk;
                case "Run_N_Land":    return clipJumpLandRun;
            }

            // Para clips que se llaman "mixamo.com" (nombre duplicado de Mixamo),
            // necesitamos usar heurísticas por longitud de clip para diferenciar.
            // Esto es una limitación de que Mixamo no renombra sus exports.
            // NOTA: estos clips se matchean si el usuario los definió como fallback
            // en el controller original. Se recomienda renombrar los FBX imports.

            return null;
        }

        // ──────────────────────────────────────────────
        //  IDs DE PARÁMETROS
        // ──────────────────────────────────────────────

        private void AssignAnimationIDs()
        {
            animIDSpeed       = Animator.StringToHash("Speed");
            animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            animIDGrounded    = Animator.StringToHash("Grounded");
            animIDJump        = Animator.StringToHash("Jump");
            animIDFreeFall    = Animator.StringToHash("FreeFall");
            animIDAttack      = Animator.StringToHash("Attack");
            animIDHit         = Animator.StringToHash("Hit");
            animIDDeath       = Animator.StringToHash("Death");
            animIDDash        = Animator.StringToHash("Dash");
            animIDCast        = Animator.StringToHash("Cast");
            animIDIsDead      = Animator.StringToHash("IsDead");
            animIDIsAiming    = Animator.StringToHash("IsAiming");
            animIDIsCharging  = Animator.StringToHash("IsCharging");
        }

        // ──────────────────────────────────────────────
        //  TRIGGERS
        // ──────────────────────────────────────────────

        private void TriggerAttackAnimation() =>
            animator?.SetTrigger(animIDAttack);

        private void TriggerDashAnimation() =>
            animator?.SetTrigger(animIDDash);

        private void TriggerHitAnimation(MobaGameplay.Combat.DamageInfo info) =>
            animator?.SetTrigger(animIDHit);

        private void TriggerDeathAnimation(BaseEntity ent, MobaGameplay.Combat.DamageInfo info)
        {
            if (animator == null) return;
            animator.SetBool(animIDIsDead, true);
            animator.SetTrigger(animIDDeath);
        }

        private void TriggerCastAnimation(int slot) =>
            animator?.SetTrigger(animIDCast);

        // ──────────────────────────────────────────────
        //  UPDATE
        // ──────────────────────────────────────────────

        private void Update()
        {
            if (animator == null || entity.Movement == null) return;

            // Velocidad → Blend Tree de movimiento
            float targetSpeed = entity.Movement.CurrentVelocity;
            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * 10f);
            if (animationBlend < 0.01f) animationBlend = 0f;

            animator.SetFloat(animIDSpeed,       animationBlend);
            animator.SetFloat(animIDMotionSpeed, 1f);

            // Grounding y salto
            animator.SetBool(animIDGrounded, entity.Movement.IsGrounded);
            animator.SetBool(animIDJump,     entity.Movement.IsJumping);
            animator.SetBool(animIDFreeFall, !entity.Movement.IsGrounded && !entity.Movement.IsJumping);

            // Estados de combate
            bool isAiming   = false;
            bool isCharging = false;

            if (entity.Movement is MobaGameplay.Movement.XZPlaneMovement xzMovement)
                isAiming = xzMovement.CurrentMovementMode == MobaGameplay.Movement.XZPlaneMovement.MovementMode.Directional;

            if (entity.Combat is MobaGameplay.Combat.RangedCombat rangedCombat)
            {
                isCharging = rangedCombat.IsCharging;
                if (isCharging) isAiming = true;
            }

            animator.SetBool(animIDIsAiming,   isAiming);
            animator.SetBool(animIDIsCharging, isCharging);
        }

        // ──────────────────────────────────────────────
        //  ANIMATION EVENTS
        // ──────────────────────────────────────────────

        public void OnFootstep(AnimationEvent animationEvent) { }
        public void OnLand(AnimationEvent animationEvent)     { }
    }
}
