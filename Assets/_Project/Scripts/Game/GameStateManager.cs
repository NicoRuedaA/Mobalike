using UnityEngine;
using System.Collections.Generic;
using MobaGameplay.Core;
using MobaGameplay.AI;
using MobaGameplay.Combat;

namespace MobaGameplay.Game
{
    /// <summary>
    /// Estados del juego principal.
    /// </summary>
    public enum GameState
    {
        /// <summary>Menú principal.</summary>
        Menu,
        
        /// <summary>Jugando activamente.</summary>
        Playing,
        
        /// <summary>Pausado.</summary>
        Paused,
        
        /// <summary>Juego terminado (victoria o derrota).</summary>
        Ended
    }

    /// <summary>
    /// Estados de la oleada.
    /// </summary>
    public enum WaveState
    {
        /// <summary>Sin oleadas activas.</summary>
        Idle,
        
        /// <summary>Preparando siguiente oleada.</summary>
        Preparing,
        
        /// <summary>Spawning enemigos.</summary>
        Spawning,
        
        /// <summary>Combate activo.</summary>
        Active,
        
        /// <summary>Oleada completada.</summary>
        Cleared,
        
        /// <summary>Todas las oleadas completadas.</summary>
        Victory
    }

    /// <summary>
    /// Administrador principal del estado del juego.
    /// Singleton que gestiona: estados del juego, oleadas, respawn del jugador, y puntuación.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        // ==================== SINGLETON ====================
        
        /// <summary>Instancia singleton.</summary>
        public static GameStateManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            Initialize();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        // ==================== CONFIGURACIÓN ====================
        
        [Header("Jugador")]
        [Tooltip("Referencia al jugador en la escena.")]
        [SerializeField] private BaseEntity playerEntity;
        
        [Tooltip("Punto de spawn del jugador.")]
        [SerializeField] private Transform playerSpawnPoint;
        
        [Tooltip("Vidas iniciales del jugador.")]
        [SerializeField] private int startingLives = 3;
        
        [Tooltip("Tiempo de respawn en segundos.")]
        [SerializeField] private float respawnTime = 5f;
        
        [Header("Oleadas")]
        [Tooltip("Configuración de oleadas.")]
        [SerializeField] private WaveData[] waveConfigs;
        
        [Tooltip("Tiempo entre oleadas en segundos.")]
        [SerializeField] private float timeBetweenWaves = 10f;
        
        [Tooltip("Puntos de spawn para enemigos.")]
        [SerializeField] private Transform[] enemySpawnPoints;
        
        [Header("Debug")]
        [Tooltip("Si es true, inicia el juego automáticamente al entrar en Play Mode. Útil para pruebas rápidas.")]
        [SerializeField] private bool autoStartGame = false;
        
        [Header("Puntuación")]
        [Tooltip("Puntos awarded por muerte de enemigo.")]
        [SerializeField] private int scorePerKill = 10;
        
        [Tooltip("Puntos bonus por completar oleada.")]
        [SerializeField] private int scorePerWaveComplete = 100;
        
        // ==================== ESTADO PÚBLICO ====================
        
        /// <summary>Estado actual del juego.</summary>
        public GameState CurrentGameState { get; private set; } = GameState.Menu;
        
        /// <summary>Estado actual de oleadas.</summary>
        public WaveState CurrentWaveState { get; private set; } = WaveState.Idle;
        
        /// <summary>Oleada actual (1-indexed).</summary>
        public int CurrentWaveNumber { get; private set; }
        
        /// <summary>Puntuación total.</summary>
        public int Score { get; private set; }
        
        /// <summary>Vidas restantes del jugador.</summary>
        public int LivesRemaining { get; private set; }
        
        /// <summary>Enemigos restantes en la oleada actual.</summary>
        public int EnemiesRemaining { get; private set; }
        
        /// <summary>Si el juego terminó en victoria.</summary>
        public bool IsVictory { get; private set; }
        
        // ==================== ESTADO PRIVADO ====================
        
        /// <summary>Enemigos vivos activos.</summary>
        private List<EnemyAIController> _activeEnemies = new List<EnemyAIController>();
        
        /// <summary>Enemigos spawning actualmente.</summary>
        private int _enemiesToSpawn;
        
        /// <summary>Temporizador de spawn.</summary>
        private float _spawnTimer;
        
        /// <summary>Temporizador de respawn.</summary>
        private float _respawnTimer;
        
        /// <summary>Si está en countdown de respawn.</summary>
        private bool _isRespawning;
        
        /// <summary>Temporizador entre oleadas.</summary>
        private float _waveTimer;
        
        // ==================== EVENTOS ====================
        
        /// <summary>Cuando cambia el estado del juego.</summary>
        public System.Action<GameState, GameState> OnGameStateChanged;
        
        /// <summary>Cuando cambia el estado de oleada.</summary>
        public System.Action<WaveState> OnWaveStateChanged;
        
        /// <summary>Cuando inicia una oleada.</summary>
        public System.Action<int> OnWaveStarted;
        
        /// <summary>Cuando se completa una oleada.</summary>
        public System.Action<int> OnWaveCleared;
        
        /// <summary>Cuando muere el jugador.</summary>
        public System.Action OnPlayerDeath;
        
        /// <summary>Cuando reaparece el jugador.</summary>
        public System.Action OnPlayerRespawn;
        
        /// <summary>Cuando termina el juego.</summary>
        public System.Action<bool> OnGameEnded;
        
        /// <summary>Cuando cambia la puntuación.</summary>
        public System.Action<int> OnScoreChanged;
        
        /// <summary>Cuando cambia el número de enemigos restantes.</summary>
        public System.Action<int> OnEnemiesRemainingChanged;
        
        // ==================== INICIALIZACIÓN ====================
        
        /// <summary>Inicializa el estado del juego.</summary>
        private void Initialize()
        {
            LivesRemaining = startingLives;
            CurrentWaveNumber = 0;
            Score = 0;
            
            // Si no hay spawn point, crear uno
            if (playerSpawnPoint == null)
            {
                GameObject spawnObj = new GameObject("PlayerSpawnPoint");
                spawnObj.transform.position = Vector3.zero;
                playerSpawnPoint = spawnObj.transform;
            }
        }
        
        private void Update()
        {
            // Solo actualizar si está jugando
            if (CurrentGameState != GameState.Playing)
                return;
            
            // Actualizar respawn
            UpdateRespawn();
            
            // Actualizar oleadas
            UpdateWaveState();
        }
        
        private void Start()
        {
            if (autoStartGame)
                StartGame();
        }
        
        // ==================== CONTROL DE ESTADO ====================
        
        /// <summary>Inicia el juego.</summary>
        public void StartGame()
        {
            if (CurrentGameState == GameState.Playing)
                return;
            
            TransitionToGameState(GameState.Playing);
            StartNextWave();
        }
        
        /// <summary>Pausa el juego.</summary>
        public void PauseGame()
        {
            if (CurrentGameState != GameState.Playing)
                return;
            
            TransitionToGameState(GameState.Paused);
            Time.timeScale = 0f;
        }
        
        /// <summary>Reanuda el juego.</summary>
        public void ResumeGame()
        {
            if (CurrentGameState != GameState.Paused)
                return;
            
            Time.timeScale = 1f;
            TransitionToGameState(GameState.Playing);
        }
        
        /// <summary>Termina el juego.</summary>
        /// <param name="victory">Si es victoria (true) o derrota (false).</param>
        public void EndGame(bool victory)
        {
            IsVictory = victory;
            TransitionToGameState(GameState.Ended);
            Time.timeScale = 0f; // Pausar tiempo
            
            OnGameEnded?.Invoke(victory);
        }
        
        /// <summary>Reinicia el juego a estado inicial.</summary>
        public void RestartGame()
        {
            Time.timeScale = 1f;
            
            // Limpiar enemigos
            ClearAllEnemies();
            
            // Reiniciar estado
            LivesRemaining = startingLives;
            CurrentWaveNumber = 0;
            Score = 0;
            CurrentWaveState = WaveState.Idle;
            _isRespawning = false;
            _respawnTimer = 0f;
            
            // Respawn jugador si está muerto
            if (playerEntity != null && playerEntity.IsDead)
            {
                RespawnPlayer();
            }
            
            // Notificar cambios
            OnScoreChanged?.Invoke(Score);
            OnEnemiesRemainingChanged?.Invoke(0);
            
            TransitionToGameState(GameState.Playing);
            StartNextWave();
        }
        
        /// <summary>Transición a nuevo estado de juego.</summary>
        private void TransitionToGameState(GameState newState)
        {
            if (CurrentGameState == newState)
                return;
            
            GameState oldState = CurrentGameState;
            CurrentGameState = newState;
            
            OnGameStateChanged?.Invoke(oldState, newState);
        }
        
        /// <summary>Transición a nuevo estado de oleada.</summary>
        private void TransitionToWaveState(WaveState newState)
        {
            if (CurrentWaveState == newState)
                return;
            
            CurrentWaveState = newState;
            OnWaveStateChanged?.Invoke(newState);
        }
        
        // ==================== SISTEMA DE OLEADAS ====================
        
        /// <summary>Inicia la siguiente oleada.</summary>
        public void StartNextWave()
        {
            // Verificar si hay más oleadas
            if (waveConfigs == null || CurrentWaveNumber >= waveConfigs.Length)
            {
                // Todas las oleadas completadas - Victoria
                TransitionToWaveState(WaveState.Victory);
                EndGame(true);
                return;
            }
            
            CurrentWaveNumber++;
            TransitionToWaveState(WaveState.Preparing);
            OnWaveStarted?.Invoke(CurrentWaveNumber);
            
            // Configurar oleada
            WaveData config = waveConfigs[CurrentWaveNumber - 1];
            _enemiesToSpawn = config.enemyCount;
            EnemiesRemaining = config.enemyCount;
            // Start timer at spawnInterval so the first enemy spawns after one interval, not immediately.
            _spawnTimer = config.spawnInterval;
            
            // Empezar spawning después del countdown de preparación
            StartCoroutine(PrepareWaveCoroutine(config.preparationTime));
        }
        
        /// <summary>Coroutine para countdown de preparación antes de empezar a spawnear.</summary>
        private System.Collections.IEnumerator PrepareWaveCoroutine(float preparationTime)
        {
            yield return new WaitForSeconds(preparationTime);
            
            if (CurrentGameState != GameState.Playing)
                yield break;
            
            TransitionToWaveState(WaveState.Spawning);
        }
        
        /// <summary>Actualiza el estado de oleadas.</summary>
        private void UpdateWaveState()
        {
            switch (CurrentWaveState)
            {
                case WaveState.Spawning:
                    UpdateSpawning();
                    break;
                    
                case WaveState.Active:
                    UpdateActiveWave();
                    break;
                    
                case WaveState.Cleared:
                    // Esperar para siguiente oleada
                    _waveTimer -= Time.deltaTime;
                    if (_waveTimer <= 0f)
                    {
                        StartNextWave();
                    }
                    break;
            }
        }
        
        /// <summary>Actualiza el spawning de enemigos.</summary>
        private void UpdateSpawning()
        {
            if (_enemiesToSpawn <= 0)
            {
                TransitionToWaveState(WaveState.Active);
                return;
            }
            
            if (waveConfigs == null || CurrentWaveNumber < 1 || CurrentWaveNumber > waveConfigs.Length)
                return;
            
            WaveData config = waveConfigs[CurrentWaveNumber - 1];
            _spawnTimer -= Time.deltaTime;
            
            if (_spawnTimer <= 0f)
            {
                SpawnEnemy(config);
                _spawnTimer = config.spawnInterval;
                _enemiesToSpawn--;
            }
        }
        
        /// <summary>Spawn un enemigo de la oleada actual.</summary>
        private void SpawnEnemy(WaveData config)
        {
            if (config.enemyPrefabs == null || config.enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("[GameStateManager] No enemy prefabs configured for wave " + CurrentWaveNumber);
                return;
            }
            
            // Seleccionar prefab aleatorio
            GameObject prefab = config.enemyPrefabs[Random.Range(0, config.enemyPrefabs.Length)];
            
            // Seleccionar punto de spawn
            Vector3 spawnPos;
            if (enemySpawnPoints != null && enemySpawnPoints.Length > 0)
            {
                Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
                spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            }
            else
            {
                spawnPos = Vector3.zero;
            }
            
            // Instanciar
            GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);
            EnemyAIController aiController = enemyObj.GetComponent<EnemyAIController>();
            
            if (aiController != null)
            {
                aiController.SetTarget(playerEntity);
                aiController.OnDeath += HandleEnemyDeath;
                _activeEnemies.Add(aiController);
            }
            else
            {
                Debug.LogWarning("[GameStateManager] Spawned enemy without EnemyAIController: " + prefab.name);
            }
            
            // Notificar enemigos restantes
            OnEnemiesRemainingChanged?.Invoke(EnemiesRemaining);
        }
        
        /// <summary>Actualiza la oleada activa (combate).</summary>
        private void UpdateActiveWave()
        {
            // Wave clear is handled by HandleEnemyDeath() to avoid duplicate triggers.
            // Only check for player-related state here.
        }
        
        /// <summary>Maneja la muerte de un enemigo.</summary>
        private void HandleEnemyDeath(EnemyAIController enemy)
        {
            if (enemy == null)
                return;
            
            // Desuscribirse
            enemy.OnDeath -= HandleEnemyDeath;
            _activeEnemies.Remove(enemy);
            
            // Actualizar contadores
            EnemiesRemaining--;
            AddScore(scorePerKill);
            OnEnemiesRemainingChanged?.Invoke(EnemiesRemaining);
            
            // Verificar fin de oleada
            if (CurrentWaveState == WaveState.Active && EnemiesRemaining <= 0 && _enemiesToSpawn <= 0)
            {
                TransitionToWaveState(WaveState.Cleared);
                OnWaveCleared?.Invoke(CurrentWaveNumber);
                AddScore(scorePerWaveComplete);
                
                // Configurar timer para siguiente oleada
                _waveTimer = timeBetweenWaves;
            }
        }
        
        /// <summary>Limpia todos los enemigos activos.</summary>
        private void ClearAllEnemies()
        {
            foreach (var enemy in _activeEnemies)
            {
                if (enemy != null)
                {
                    enemy.OnDeath -= HandleEnemyDeath;
                    Destroy(enemy.gameObject);
                }
            }
            _activeEnemies.Clear();
            EnemiesRemaining = 0;
            OnEnemiesRemainingChanged?.Invoke(0);
        }
        
        // ==================== SISTEMA DE RESPAWN ====================
        
        /// <summary>Actualiza el timer de respawn.</summary>
        private void UpdateRespawn()
        {
            if (!_isRespawning)
                return;
            
            _respawnTimer -= Time.deltaTime;
            
            if (_respawnTimer <= 0f)
            {
                _isRespawning = false;
                RespawnPlayer();
            }
        }
        
        /// <summary>Hace reaparecer al jugador.</summary>
        private void RespawnPlayer()
        {
            if (playerEntity == null)
            {
                Debug.LogError("[GameStateManager] Cannot respawn: playerEntity is null");
                return;
            }
            
            // Mover a punto de spawn
            if (playerSpawnPoint != null)
            {
                playerEntity.transform.position = playerSpawnPoint.position;
                playerEntity.transform.rotation = playerSpawnPoint.rotation;
            }
            
            // Restaurar vida y mana
            playerEntity.Revive();
            
            // Notificar
            OnPlayerRespawn?.Invoke();
        }
        
        // ==================== GESTIÓN DE JUGADOR ====================
        
        /// <summary>Establece la referencia al jugador.</summary>
        public void SetPlayerEntity(BaseEntity entity)
        {
            // Desuscribir del anterior
            if (playerEntity != null)
            {
                playerEntity.OnDeath -= HandlePlayerDeath;
            }
            
            playerEntity = entity;
            
            // Suscribir al nuevo
            if (playerEntity != null)
            {
                playerEntity.OnDeath += HandlePlayerDeath;
            }
        }
        
        /// <summary>Maneja la muerte del jugador.</summary>
        private void HandlePlayerDeath(BaseEntity entity, DamageInfo damageInfo)
        {
            if (CurrentGameState != GameState.Playing)
                return;
            
            LivesRemaining--;
            OnPlayerDeath?.Invoke();
            
            // Verificar game over
            if (LivesRemaining <= 0)
            {
                EndGame(false);
                return;
            }
            
            // Iniciar countdown de respawn
            _isRespawning = true;
            _respawnTimer = respawnTime;
        }
        
        // ==================== PUNTUACIÓN ====================
        
        /// <summary>Añade puntos a la puntuación.</summary>
        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }
        
        // ==================== GETTERS ÚTILES ====================
        
        /// <summary>Obtiene el tiempo de respawn restante.</summary>
        public float GetRespawnTimeRemaining()
        {
            return _isRespawning ? _respawnTimer : 0f;
        }
        
        /// <summary>Obtiene el tiempo hasta la siguiente oleada.</summary>
        public float GetTimeToNextWave()
        {
            if (CurrentWaveState == WaveState.Cleared)
                return _waveTimer;
            return 0f;
        }
        
        /// <summary>Obtiene el número total de oleadas.</summary>
        public int GetTotalWaves()
        {
            return waveConfigs != null ? waveConfigs.Length : 0;
        }
        
        /// <summary>Obtiene la configuración de la oleada actual.</summary>
        public WaveData GetCurrentWaveConfig()
        {
            if (waveConfigs == null || CurrentWaveNumber < 1 || CurrentWaveNumber > waveConfigs.Length)
                return null;
            return waveConfigs[CurrentWaveNumber - 1];
        }
    }
}
