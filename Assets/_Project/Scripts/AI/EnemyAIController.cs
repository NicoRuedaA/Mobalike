using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;
using System.Collections.Generic;

namespace MobaGameplay.AI
{
    /// <summary>
    /// Controlador de IA para enemigos.
    /// Maneja el estado machine: Idle → Patrol → Chase → Attack → Retreat → Dead.
    /// </summary>
    [RequireComponent(typeof(BaseEntity))]
    public class EnemyAIController : MonoBehaviour
    {
        // ==================== CONFIGURACIÓN ====================
        
        [Header("Detección")]
        [Tooltip("Radio de detección del jugador.")]
        [SerializeField] private float detectionRadius = 15f;
        
        [Tooltip("Tag del jugador para detección.")]
        [SerializeField] private string playerTag = "Player";
        
        [Header("Combate")]
        [Tooltip("Rango de ataque melee.")]
        [SerializeField] private float attackRange = 2f;
        
        [Tooltip("Tiempo entre ataques.")]
        [SerializeField] private float attackCooldown = 1.5f;
        
        [Tooltip("Daño base del ataque.")]
        [SerializeField] private float attackDamage = 20f;
        
        [Header("Movimiento")]
        [Tooltip("Velocidad de movimiento al perseguir.")]
        [SerializeField] private float chaseSpeed = 5f;
        
        [Tooltip("Velocidad de movimiento normal.")]
        [SerializeField] private float patrolSpeed = 2f;
        
        [Header("Patrulla")]
        [Tooltip("Tiempo de espera en Idle antes de patrullar (segundos).")]
        [SerializeField] private float idleWaitTime = 2f;
        
        [Header("Patrulla")]
        [Tooltip("Puntos de patrulla. Si está vacío, no patrulla.")]
        [SerializeField] private Transform[] patrolPoints;
        
        [Tooltip("Tiempo de espera en punto de patrulla.")]
        [SerializeField] private float waitAtPatrolPoint = 1f;
        
        [Header("Persecución")]
        [Tooltip("Tiempo máximo persiguiendo antes de volver a patrulla.")]
        [SerializeField] private float maxChaseDuration = 8f;
        
        [Header("Retirada")]
        [Tooltip("Porcentaje de vida para retirarse (0-1).")]
        [SerializeField] private float retreatThreshold = 0.25f;
        
        [Tooltip("Porcentaje de vida para recuperarse de la retirada (0-1).")]
        [SerializeField] private float retreatRecoveryThreshold = 0.5f;
        
        [Tooltip("Distancia a la que se retira.")]
        [SerializeField] private float retreatDistance = 8f;
        
        // ==================== ESTADO ====================
        
        /// <summary>Estado actual del AI.</summary>
        public EnemyState CurrentState { get; private set; } = EnemyState.Idle;
        
        /// <summary>Jugador objetivo actual.</summary>
        public BaseEntity TargetPlayer { get; private set; }
        
        /// <summary>Referencia a la entidad.</summary>
        private BaseEntity _entity;
        
        /// <summary>Tiempo en el estado actual.</summary>
        private float _timeInCurrentState;
        
        /// <summary>Índice del punto de patrulla actual.</summary>
        private int _currentPatrolIndex;
        
        /// <summary>Temporizador de ataque.</summary>
        private float _attackTimer;
        
        /// <summary>Punto de retirada.</summary>
        private Vector3 _retreatPoint;
        
        /// <summary>Si el enemigo está mirando al jugador.</summary>
        private bool _isFacingTarget;
        
        /// <summary>Referencia al CharacterController para movimiento.</summary>
        private CharacterController _characterController;
        
        // ==================== CONSTANTES ====================
        
        private const float ROTATION_SPEED = 10f;
        private const float ARRIVAL_THRESHOLD = 0.5f;
        
// ==================== EVENTOS ====================
        
        /// <summary>Cuando cambia el estado.</summary>
        public System.Action<EnemyState, EnemyState> OnStateChanged;
        
        /// <summary>Cuando el enemigo muere.</summary>
        public System.Action<EnemyAIController> OnDeath;

        // ==================== UNITY LIFECYCLE ====================
        
        private void Awake()
        {
            _entity = GetComponent<BaseEntity>();
            _characterController = GetComponent<CharacterController>();
            
            // Suscribirse a eventos de muerte
            _entity.OnDeath += HandleDeath;
        }
        
        private void OnDestroy()
        {
            _entity.OnDeath -= HandleDeath;
        }
        
        private void Update()
        {
            // Si está muerto, no hacer nada
            if (CurrentState == EnemyState.Dead)
                return;
            
            // Actualizar timer de estado
            _timeInCurrentState += Time.deltaTime;
            
            // Ejecutar comportamiento según estado
            switch (CurrentState)
            {
                case EnemyState.Idle:
                    UpdateIdle();
                    break;
                case EnemyState.Patrol:
                    UpdatePatrol();
                    break;
                case EnemyState.Chase:
                    UpdateChase();
                    break;
                case EnemyState.Attack:
                    UpdateAttack();
                    break;
                case EnemyState.Retreat:
                    UpdateRetreat();
                    break;
            }
            
            // Siempre intentar detectar jugador (excepto en Dead)
            TryDetectPlayer();
        }
        
        // ==================== MÁQUINA DE ESTADOS ====================
        
        /// <summary>
        /// Transición a un nuevo estado.
        /// </summary>
        private void TransitionTo(EnemyState newState)
        {
            if (CurrentState == newState)
                return;
            
            EnemyState oldState = CurrentState;
            CurrentState = newState;
            _timeInCurrentState = 0f;
            
            // Ejecutar OnEnter del estado
            OnEnterState(newState);
            
            OnStateChanged?.Invoke(oldState, newState);
        }
        
        /// <summary>
        /// Ejecuta lógica de entrada al estado.
        /// </summary>
        private void OnEnterState(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Idle:
                    // Detener movimiento
                    StopMovement();
                    break;
                    
                case EnemyState.Patrol:
                    // Iniciar hacia el primer punto
                    if (patrolPoints != null && patrolPoints.Length > 0)
                    {
                        _currentPatrolIndex = 0;
                    }
                    break;
                    
                case EnemyState.Chase:
                    // El target ya debería estar establecido
                    break;
                    
                case EnemyState.Attack:
                    // Detener movimiento para atacar
                    StopMovement();
                    _attackTimer = 0f;
                    break;
                    
                case EnemyState.Retreat:
                    // Calcular punto de retirada
                    CalculateRetreatPoint();
                    break;
                    
                case EnemyState.Dead:
                    StopMovement();
                    OnDeath?.Invoke(this);
                    break;
            }
        }
        
        // ==================== DETECCIÓN ====================
        
        /// <summary>
        /// Intenta detectar al jugador dentro del radio de detección.
        /// También verifica si el target actual sigue en rango.
        /// </summary>
        private void TryDetectPlayer()
        {
            // Si ya tiene target vivo, verificar si sigue en rango de detección
            if (TargetPlayer != null)
            {
                if (TargetPlayer.IsDead)
                {
                    // Target murió, limpiar
                    TargetPlayer = null;
                }
                else
                {
                    // Verificar distancia al target
                    float distanceToTarget = Vector3.Distance(transform.position, TargetPlayer.transform.position);
                    if (distanceToTarget > detectionRadius)
                    {
                        // Target salió del rango, perderlo
                        TargetPlayer = null;
                    }
                    else
                    {
                        // Target sigue en rango, no buscar
                        return;
                    }
                }
            }
            
            // Buscar jugadores por tag usando OverlapSphere
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                detectionRadius
            );
            
            foreach (var hit in hits)
            {
                // Verificar si tiene el tag del jugador
                if (!hit.CompareTag(playerTag))
                    continue;
                    
                var entity = hit.GetComponentInParent<BaseEntity>();
                if (entity != null && !entity.IsDead && entity is HeroEntity)
                {
                    SetTarget(entity);
                    return;
                }
            }
            
            // No encontró jugador
            TargetPlayer = null;
        }
        
        // ==================== ESTADOS ====================
        
        /// <summary>Lógica del estado Idle.</summary>
        private void UpdateIdle()
        {
            // Si tiene target, ir a chase
            if (TargetPlayer != null && !TargetPlayer.IsDead)
            {
                TransitionTo(EnemyState.Chase);
                return;
            }
            
            // Si hay puntos de patrulla, ir a patrol después de esperar
            if (HasPatrolPoints() && _timeInCurrentState > idleWaitTime)
            {
                TransitionTo(EnemyState.Patrol);
            }
        }
        
        /// <summary>Lógica del estado Patrol.</summary>
        private void UpdatePatrol()
        {
            // Si tiene target, ir a chase
            if (TargetPlayer != null && !TargetPlayer.IsDead)
            {
                TransitionTo(EnemyState.Chase);
                return;
            }
            
            // Moverse hacia punto de patrulla
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                TransitionTo(EnemyState.Idle);
                return;
            }
            
            Transform targetPoint = patrolPoints[_currentPatrolIndex];
            if (targetPoint == null)
            {
                TransitionTo(EnemyState.Idle);
                return;
            }
            
            // Moverse
            Vector3 direction = (targetPoint.transform.position - transform.position).normalized;
            direction.y = 0;
            
            // Detener si está muy cerca
            float distance = Vector3.Distance(transform.position, targetPoint.transform.position);
            if (distance < ARRIVAL_THRESHOLD)
            {
                // Esperar en el punto
                if (_timeInCurrentState > waitAtPatrolPoint)
                {
                    // Ir al siguiente punto
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Length;
                    _timeInCurrentState = 0f;
                }
            }
            else
            {
                MoveToDirection(direction, patrolSpeed);
                RotateTowards(direction);
            }
        }
        
        /// <summary>Lógica del estado Chase.</summary>
        private void UpdateChase()
        {
            // Verificar si perdió al target
            if (TargetPlayer == null || TargetPlayer.IsDead)
            {
                TargetPlayer = null;
                TransitionToPatrolOrIdle();
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, TargetPlayer.transform.position);
            
            // Si está en rango de ataque
            if (distanceToPlayer <= attackRange)
            {
                TransitionTo(EnemyState.Attack);
                return;
            }
            
            // Si se pasó del tiempo máximo de persecución
            if (_timeInCurrentState > maxChaseDuration)
            {
                TransitionToPatrolOrIdle();
                return;
            }
            
            // Perseguir
            Vector3 direction = (TargetPlayer.transform.position - transform.position).normalized;
            direction.y = 0;
            MoveToDirection(direction, chaseSpeed);
            RotateTowards(direction);
        }
        
        /// <summary>Lógica del estado Attack.</summary>
        private void UpdateAttack()
        {
            // Verificar target
            if (TargetPlayer == null || TargetPlayer.IsDead)
            {
                TargetPlayer = null;
                TransitionToPatrolOrIdle();
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, TargetPlayer.transform.position);
            
            // Si salió del rango de ataque
            if (distanceToPlayer > attackRange)
            {
                // Si sigue en rango de detección, perseguir
                if (distanceToPlayer <= detectionRadius)
                {
                    TransitionTo(EnemyState.Chase);
                }
                else
                {
                    TransitionToPatrolOrIdle();
                }
                return;
            }
            
            // Verificar vida para retirada
            float healthPercent = _entity.CurrentHealth / _entity.MaxHealth;
            if (healthPercent <= retreatThreshold)
            {
                TransitionTo(EnemyState.Retreat);
                return;
            }
            
            // Rotar hacia el objetivo
            Vector3 direction = (TargetPlayer.transform.position - transform.position).normalized;
            direction.y = 0;
            RotateTowards(direction);
            
            // Atacar si el cooldown está listo
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                PerformAttack();
                _attackTimer = attackCooldown;
            }
        }
        
        /// <summary>Lógica del estado Retreat.</summary>
        private void UpdateRetreat()
        {
            // Verificar vida
            float healthPercent = _entity.CurrentHealth / _entity.MaxHealth;
            
            // Si se recuperó lo suficiente y tiene target
            if (healthPercent >= retreatRecoveryThreshold && TargetPlayer != null && !TargetPlayer.IsDead)
            {
                TransitionTo(EnemyState.Chase);
                return;
            }
            
            // Si llegó al punto de retirada
            float distanceToRetreat = Vector3.Distance(transform.position, _retreatPoint);
            if (distanceToRetreat < ARRIVAL_THRESHOLD)
            {
                // Encontrar nuevo punto o quedarse quieto
                if (TargetPlayer != null && !TargetPlayer.IsDead)
                {
                    float currentHealthPercent = _entity.CurrentHealth / _entity.MaxHealth;
                    if (currentHealthPercent >= retreatRecoveryThreshold)
                    {
                        TransitionTo(EnemyState.Chase);
                    }
                    else
                    {
                        // Quedarse quieto y regenerar
                        StopMovement();
                    }
                }
                else
                {
                    TransitionTo(EnemyState.Idle);
                }
                return;
            }
            
            // Moverse hacia punto de retirada
            Vector3 direction = (_retreatPoint - transform.position).normalized;
            direction.y = 0;
            MoveToDirection(direction, chaseSpeed);
            RotateTowards(direction);
        }
        
        // ==================== ACCIONES ====================
        
        /// <summary>Establece el objetivo del enemigo.</summary>
        public void SetTarget(BaseEntity target)
        {
            TargetPlayer = target;
            
            // Si estaba en idle o patrol, ir a chase
            if (CurrentState == EnemyState.Idle || CurrentState == EnemyState.Patrol)
            {
                TransitionTo(EnemyState.Chase);
            }
        }
        
        /// <summary>Realiza el ataque.</summary>
        private void PerformAttack()
        {
            if (TargetPlayer == null || TargetPlayer.IsDead)
                return;
            
            // Verificar que sigue en rango
            float distance = Vector3.Distance(transform.position, TargetPlayer.transform.position);
            if (distance > attackRange)
                return;
            
            // Aplicar daño
            DamageInfo damageInfo = new DamageInfo(attackDamage, DamageType.Physical, _entity);
            TargetPlayer.TakeDamage(damageInfo);
            
            // Aquí se podría reproducir animación, VFX, etc.
            // Por ahora confiamos en el AnimationEventReceiver
        }
        
        /// <summary>Calcula el punto de retirada.</summary>
        private void CalculateRetreatPoint()
        {
            Vector3 retreatDir;
            
            if (TargetPlayer != null && !TargetPlayer.IsDead)
            {
                // alejarse del jugador
                retreatDir = (transform.position - TargetPlayer.transform.position).normalized;
            }
            else
            {
                // alejarse en dirección opuesta al último punto visto
                retreatDir = -transform.forward;
            }
            
            _retreatPoint = transform.position + retreatDir * retreatDistance;
            
            // Clamp a un área razonable (opcional, ajustar según el mapa)
            _retreatPoint.y = transform.position.y;
        }
        
        /// <summary>Transición a patrulla o idle según configuración.</summary>
        private void TransitionToPatrolOrIdle()
        {
            if (HasPatrolPoints())
            {
                TransitionTo(EnemyState.Patrol);
            }
            else
            {
                TransitionTo(EnemyState.Idle);
            }
        }
        
        /// <summary>Verifica si hay puntos de patrulla.</summary>
        private bool HasPatrolPoints()
        {
            return patrolPoints != null && patrolPoints.Length > 0;
        }
        
        // ==================== MOVIMIENTO ====================
        
        /// <summary>Mueve el CharacterController en una dirección.</summary>
        private void MoveToDirection(Vector3 direction, float speed)
        {
            if (_characterController != null)
            {
                _characterController.Move(direction * speed * Time.deltaTime);
            }
            else
            {
                // Fallback a Transform.Translate
                transform.Translate(direction * speed * Time.deltaTime, Space.World);
            }
        }
        
        /// <summary>Rota el transform hacia una dirección.</summary>
        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f)
                return;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, ROTATION_SPEED * Time.deltaTime);
        }
        
        /// <summary>Detiene el movimiento.</summary>
        private void StopMovement()
        {
            if (_characterController != null)
            {
                _characterController.Move(Vector3.zero);
            }
        }
        
        // ==================== EVENTOS ====================
        
        /// <summary>Maneja la muerte del enemigo.</summary>
        private void HandleDeath(BaseEntity entity, DamageInfo damageInfo)
        {
            TransitionTo(EnemyState.Dead);
        }
        
        // ==================== GIZMOS ====================
        
        private void OnDrawGizmosSelected()
        {
            // Radio de detección
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // Rango de ataque
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Punto de retirada (si está en estado retreat)
            if (CurrentState == EnemyState.Retreat)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_retreatPoint, 1f);
                Gizmos.DrawLine(transform.position, _retreatPoint);
            }
            
            // Puntos de patrulla
            if (patrolPoints != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    if (patrolPoints[i] == null)
                        continue;
                    
                    Gizmos.DrawWireSphere(patrolPoints[i].transform.position, 0.5f);
                    
                    // Línea al siguiente punto
                    int nextIndex = (i + 1) % patrolPoints.Length;
                    if (patrolPoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].transform.position, patrolPoints[nextIndex].transform.position);
                    }
                }
            }
            
            // Target actual
            if (TargetPlayer != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position + Vector3.up, TargetPlayer.transform.position + Vector3.up);
            }
        }
    }
}
