using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Animation
{
    // Asegura que este componente siempre tenga un BaseEntity adjunto en el mismo GameObject
    [RequireComponent(typeof(BaseEntity))]
    public class CharacterAnimator : MonoBehaviour
    {
        private Animator animator;

        // ──────────────────────────────────────────────
        //  CLIPS DE ANIMACIÓN (ASIGNADOS EN INSPECTOR)
        // ──────────────────────────────────────────────
        [Header("Movement Clips")]
        [SerializeField] private AnimationClip clipIdle;
        [SerializeField] private AnimationClip clipWalk;
        [SerializeField] private AnimationClip clipRun;

        [Header("Jump Clips")]
        [SerializeField] private AnimationClip clipInAir;
        [SerializeField] private AnimationClip clipJumpStart;
        [SerializeField] private AnimationClip clipJumpLand;

        [Header("Combat Clips")]
        [SerializeField] private AnimationClip clipDash;
        [SerializeField] private AnimationClip clipHit;
        [SerializeField] private AnimationClip clipCast;
        [SerializeField] private AnimationClip clipDeath;

        [Header("Upper Body Clips")]
        [SerializeField] private AnimationClip clipBasicAttack;

        [Header("Blend Tree Thresholds")]
        [Tooltip("Valores que el BlendTree espera para caminar y correr. (Por defecto: Walk=2.0, Run=5.335 en Unity)")]
        [SerializeField] private float blendWalkSpeed   = 2.0f;
        [SerializeField] private float blendSprintSpeed = 5.335f;

        // NOTA: placeholderName es el nombre exacto del clip en el AnimatorController base.
        // Si el nombre del placeholder cambia en el controller, actualizar aquí también.
        [Header("Placeholder Names")]
        [Tooltip("Nombre exacto del clip placeholder en el AnimatorController para cada estado")]
        [SerializeField] private string placeholderIdle         = "Idle";
        [SerializeField] private string placeholderWalk         = "Walk_N";
        [SerializeField] private string placeholderRun          = "Run_N";
        [SerializeField] private string placeholderInAir        = "InAir";
        [SerializeField] private string placeholderJumpStart    = "JumpStart";
        [SerializeField] private string placeholderJumpLand     = "JumpLand";
        [SerializeField] private string placeholderDash         = "Roll";
        [SerializeField] private string placeholderHit          = "Hit";
        [SerializeField] private string placeholderCast         = "Cast";
        [SerializeField] private string placeholderDeath        = "Death";
        [SerializeField] private string placeholderBasicAttack  = "punch";

        // ---- Referencias internas ----
        private BaseEntity entity;

        // Variables para cachear los IDs de los parámetros del Animator.
        // Usar enteros (hashes) es mucho más eficiente en rendimiento que usar strings constantemente.
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
        private int animIDCast1;
        private int animIDCast2;
        private int animIDCast3;
        private int animIDIsDead;
        private int animIDIsAiming;
        private int animIDIsCharging;

        // Variable para suavizar (interpolar) la transición de velocidad en el Blend Tree
        private float animationBlend;


        // ──────────────────────────────────────────────
        //  CICLO DE VIDA
        // ──────────────────────────────────────────────

        private void Awake()
        {
            // Obtenemos la referencia principal de la entidad
            entity = GetComponent<BaseEntity>();

            // Buscamos el Animator en este objeto o en sus hijos (útil si el modelo 3D es un hijo)
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogError($"[CharacterAnimator] {gameObject.name}: no se encontró ningún Animator.", this);
                return;
            }

            // Configuramos el AnimatorOverrideController y cacheamos los Hashes
            ApplyAnimationOverrides();
            AssignAnimationIDs();
        }

        private void Start()
        {
            // Nos suscribimos a los eventos de los diferentes módulos de la entidad.
            // Esto desacopla la animación de la lógica: el animador solo escucha cuando algo sucede.
            if (entity.Combat != null)        entity.Combat.OnBasicAttack            += TriggerAttackAnimation;
            if (entity.Movement != null)      entity.Movement.OnDashStart            += TriggerDashAnimation;
            if (entity.AbilitySystem != null) entity.AbilitySystem.OnAbilityExecuted += TriggerCastAnimation;

            entity.OnTakeDamage += TriggerHitAnimation;
            entity.OnDeath      += TriggerDeathAnimation;
        }

        private void OnDestroy()
        {
            // CRÍTICO: Siempre desuscribirse de los eventos al destruir el objeto
            // para evitar memory leaks (fugas de memoria) y errores de referencia nula.
            if (entity == null) return;

            if (entity.Combat != null)        entity.Combat.OnBasicAttack            -= TriggerAttackAnimation;
            if (entity.Movement != null)      entity.Movement.OnDashStart            -= TriggerDashAnimation;
            if (entity.AbilitySystem != null) entity.AbilitySystem.OnAbilityExecuted -= TriggerCastAnimation;

            entity.OnTakeDamage -= TriggerHitAnimation;
            entity.OnDeath      -= TriggerDeathAnimation;
        }

        // ──────────────────────────────────────────────
        //  OVERRIDE CONTROLLER
        // ──────────────────────────────────────────────

        /// <summary>
        /// Crea un AnimatorOverrideController dinámico en tiempo de ejecución.
        /// Reemplaza SOLO los clips que tengan un clip asignado en el Inspector de Unity.
        /// Los slots vacíos conservan la animación original del AnimatorController base.
        /// Esto permite reutilizar un solo Animator State Machine para múltiples personajes.
        /// </summary>
        private void ApplyAnimationOverrides()
        {
            var baseController     = animator.runtimeAnimatorController;
            var overrideController = new AnimatorOverrideController(baseController);

            // Obtenemos la lista de todas las animaciones actuales en el controlador base
            var overrides = new System.Collections.Generic.List<
                System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);

            // Recorremos la lista y buscamos si tenemos un reemplazo configurado
            for (int i = 0; i < overrides.Count; i++)
            {
                AnimationClip original    = overrides[i].Key;
                AnimationClip replacement = FindOverrideFor(original);

                // Si encontramos un reemplazo válido, lo asignamos en el par clave-valor
                if (replacement != null)
                {
                    overrides[i] = new System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>(
                        original, replacement
                    );
                }
            }

            // Aplicamos los cambios al override controller y se lo inyectamos al Animator
            overrideController.ApplyOverrides(overrides);
            animator.runtimeAnimatorController = overrideController;
        }

        /// <summary>
        /// Compara el nombre del clip original (placeholder) con las variables configuradas
        /// en el Inspector. Retorna el nuevo Clip si hace match, o null si no hay reemplazo.
        /// </summary>
        private AnimationClip FindOverrideFor(AnimationClip original)
        {
            if (original == null) return null;

            string name = original.name;

            // Verificamos el nombre y que el usuario realmente haya asignado un clip en el inspector
            if (name == placeholderIdle         && clipIdle         != null) return clipIdle;
            if (name == placeholderWalk         && clipWalk         != null) return clipWalk;
            if (name == placeholderRun          && clipRun          != null) return clipRun;
            if (name == placeholderInAir        && clipInAir        != null) return clipInAir;
            if (name == placeholderJumpStart    && clipJumpStart    != null) return clipJumpStart;
            if (name == placeholderJumpLand     && clipJumpLand     != null) return clipJumpLand;
            if (name == placeholderDash         && clipDash         != null) return clipDash;
            if (name == placeholderHit          && clipHit          != null) return clipHit;
            if (name == placeholderCast         && clipCast         != null) return clipCast;
            if (name == placeholderDeath        && clipDeath        != null) return clipDeath;
            if (name == placeholderBasicAttack  && clipBasicAttack  != null) return clipBasicAttack;

            return null; // Se conserva el original si no entra en ningún 'if'
        }

        // ──────────────────────────────────────────────
        //  IDs DE PARÁMETROS
        // ──────────────────────────────────────────────

        /// <summary>
        /// Convierte los strings de los nombres de parámetros del Animator a sus respectivos Hashes (int).
        /// </summary>
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
            animIDCast1       = Animator.StringToHash("Cast1");
            animIDCast2       = Animator.StringToHash("Cast2");
            animIDCast3       = Animator.StringToHash("Cast3");
            animIDIsDead      = Animator.StringToHash("IsDead");
            animIDIsAiming    = Animator.StringToHash("IsAiming");
            animIDIsCharging  = Animator.StringToHash("IsCharging");
        }

        // ──────────────────────────────────────────────
        //  TRIGGERS (Llamados por Eventos)
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
            // Configuramos un bool para detener otra lógica de Update y lanzamos el trigger visual
            animator.SetBool(animIDIsDead, true);
            animator.SetTrigger(animIDDeath);
        }

        /// <summary>
        /// Dispara el trigger de cast (hechizo/habilidad) correspondiente al slot de la habilidad.
        /// Los triggers deben existir en el AnimatorController (Cast, Cast1, Cast2...).
        /// </summary>
        private void TriggerCastAnimation(int slot)
        {
            if (animator == null) return;

            // Determina qué Trigger enviar basándose en el slot de la habilidad ejecutada
            int triggerHash = slot switch
            {
                0 => animIDCast,
                1 => animIDCast1,
                2 => animIDCast2,
                3 => animIDCast3,
                _ => animIDCast // Fallback por defecto
            };

            // Validamos que el parámetro realmente exista en el Animator para no lanzar errores de consola
            if (HasAnimatorParameter(triggerHash, AnimatorControllerParameterType.Trigger))
                animator.SetTrigger(triggerHash);
        }

        // ──────────────────────────────────────────────
        //  UPDATE (Sincronización Continua)
        // ──────────────────────────────────────────────

        private void Update()
        {
            if (animator == null || entity.Movement == null) return;

            // Si está muerto, bloqueamos el procesamiento de animaciones de movimiento
            if (animator.GetBool(animIDIsDead)) return;

            // 1. Sincronización de Velocidad (Blend Tree)
            float targetSpeed = 0f;
            if (entity.Movement.CurrentVelocity > 0.01f)
                targetSpeed = entity.Movement.IsSprinting ? blendSprintSpeed : blendWalkSpeed;

            // Interpolación lineal (Lerp) para que la transición entre caminar y correr sea suave
            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * 10f);
            if (animationBlend < 0.01f && targetSpeed == 0f) animationBlend = 0f;

            animator.SetFloat(animIDSpeed,       animationBlend);
            animator.SetFloat(animIDMotionSpeed, 1f); // Multiplicador base de velocidad de animación

            // 2. Estados de Aire y Salto
            animator.SetBool(animIDGrounded, entity.Movement.IsGrounded);
            // FreeFall es cierto si NO toca el suelo y tampoco está en la fase inicial del salto
            animator.SetBool(animIDFreeFall, !entity.Movement.IsGrounded && !entity.Movement.IsJumping);

            // Jump es un Bool en StarterAssetsThirdPerson — se sincroniza como estado continuo,
            // igual que Grounded. SetTrigger no funciona sobre parámetros Bool.
            animator.SetBool(animIDJump, entity.Movement.IsJumping);

            // 3. Estados de Apuntado y Carga (Combate/Movimiento Especial)
            bool isAiming   = false;
            bool isCharging = false;

            // Verificamos si usa movimiento en plano XZ y está en modo direccional (ej. strafe)
            if (entity.Movement is MobaGameplay.Movement.XZPlaneMovement xzMovement)
                isAiming = xzMovement.CurrentMovementMode == MobaGameplay.Movement.XZPlaneMovement.MovementMode.Directional;

            // Verificamos si es un personaje a distancia que está cargando un ataque
            if (entity.Combat is MobaGameplay.Combat.RangedCombat rangedCombat)
            {
                isCharging = rangedCombat.IsCharging;
                if (isCharging) isAiming = true; // Forzar apuntado si está cargando
            }

            animator.SetBool(animIDIsAiming,   isAiming);
            animator.SetBool(animIDIsCharging, isCharging);
        }

        // ──────────────────────────────────────────────
        //  HELPERS
        // ──────────────────────────────────────────────

        /// <summary>
        /// Comprueba de forma segura si un parámetro específico existe en el Animator
        /// para evitar advertencias de Unity en consola.
        /// </summary>
        private bool HasAnimatorParameter(int hash, AnimatorControllerParameterType type)
        {
            foreach (var param in animator.parameters)
                if (param.nameHash == hash && param.type == type)
                    return true;
            return false;
        }

        // ──────────────────────────────────────────────
        //  ANIMATION EVENTS (Llamados desde los Clips)
        // ──────────────────────────────────────────────

        // Métodos vacíos preparados para recibir eventos de animación configurados directamente en los clips (ej. para reproducir sonido de pasos)
        public void OnFootstep(AnimationEvent animationEvent) { }
        public void OnLand(AnimationEvent animationEvent)     { }

#if UNITY_EDITOR
        // ──────────────────────────────────────────────
        //  DEBUG UI (Solo visible en el Editor de Unity)
        // ──────────────────────────────────────────────
        
        /// <summary>
        /// Dibuja una pequeña ventana en la pantalla del juego para monitorear el estado actual de las animaciones.
        /// </summary>
        private void OnGUI()
        {
            if (animator == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 350, 110), GUI.skin.box);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14 };
            GUIStyle textStyle  = new GUIStyle(GUI.skin.label) { fontSize = 13 };

            string locoState = "Idle (Quieto)";
            float  midPoint  = (blendWalkSpeed + blendSprintSpeed) / 2f;

            // Calculamos visualmente en qué estado del BlendTree nos encontramos
            if      (animationBlend > 0.1f && animationBlend <= midPoint) locoState = "Walking";
            else if (animationBlend > midPoint)                           locoState = "Running";

            GUILayout.Label($"[Animator Debug] - {gameObject.name}", titleStyle);
            GUILayout.Label($"Sprint: {entity.Movement?.IsSprinting}", textStyle);
            GUILayout.Label($"Blend:  {animationBlend:F2}", textStyle);
            GUILayout.Label($"Estado: {locoState}", titleStyle);

            GUILayout.EndArea();
        }
#endif
    }
}