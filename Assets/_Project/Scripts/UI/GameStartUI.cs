using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MobaGameplay.Game;

namespace MobaGameplay.UI
{
    /// <summary>
    /// Panel de inicio del juego.
    /// Muestra botón para comenzar y wave info.
    /// </summary>
    public class GameStartUI : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private GameObject startPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private TextMeshProUGUI waveInfoText;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private TextMeshProUGUI scoreText;
        
        [Header("Configuración")]
        [SerializeField] private bool showStartPanelOnStart = true;
        
        private void Start()
        {
            // Suscribirse a eventos del GameStateManager
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameStateChanged += HandleGameStateChanged;
                GameStateManager.Instance.OnWaveStarted += HandleWaveStarted;
                GameStateManager.Instance.OnScoreChanged += HandleScoreChanged;
            }
            
            // Configurar botón
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }
            
            // Mostrar panel inicial si está configurado
            if (startPanel != null)
            {
                startPanel.SetActive(showStartPanelOnStart);
            }
            
            UpdateUI();
        }
        
        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
                GameStateManager.Instance.OnWaveStarted -= HandleWaveStarted;
                GameStateManager.Instance.OnScoreChanged -= HandleScoreChanged;
            }
        }
        
        private void Update()
        {
            // Actualizar UI mientras juega
            if (GameStateManager.Instance != null)
            {
                UpdatePlayingUI();
            }
        }
        
        private void OnStartButtonClicked()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StartGame();
            }
        }
        
        private void HandleGameStateChanged(GameState oldState, GameState newState)
        {
            UpdateUI();
            
            // Ocultar panel de inicio cuando empieza a jugar
            if (newState == GameState.Playing)
            {
                if (startPanel != null)
                {
                    startPanel.SetActive(false);
                }
            }
            
            // Mostrar panel si termina el juego
            if (newState == GameState.Ended)
            {
                if (startPanel != null)
                {
                    startPanel.SetActive(true);
                    if (waveInfoText != null)
                    {
                        bool isVictory = GameStateManager.Instance.IsVictory;
                        waveInfoText.text = isVictory ? "¡VICTORIA!" : "GAME OVER";
                    }
                }
            }
        }
        
        private void HandleWaveStarted(int waveNumber)
        {
            UpdateUI();
        }
        
        private void HandleScoreChanged(int newScore)
        {
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (GameStateManager.Instance == null)
                return;
            
            // Wave info
            if (waveInfoText != null)
            {
                if (GameStateManager.Instance.CurrentGameState == GameState.Menu)
                {
                    waveInfoText.text = "Presiona START para comenzar";
                }
                else if (GameStateManager.Instance.CurrentGameState == GameState.Playing)
                {
                    waveInfoText.text = $"Wave {GameStateManager.Instance.CurrentWaveNumber} / {GameStateManager.Instance.GetTotalWaves()}";
                }
                else if (GameStateManager.Instance.CurrentGameState == GameState.Ended)
                {
                    waveInfoText.text = GameStateManager.Instance.IsVictory ? "¡VICTORIA!" : "GAME OVER";
                }
            }
            
            // Lives
            if (livesText != null)
            {
                livesText.text = $"❤ {GameStateManager.Instance.LivesRemaining}";
            }
            
            // Score
            if (scoreText != null)
            {
                scoreText.text = $"Score: {GameStateManager.Instance.Score}";
            }
        }
        
        private void UpdatePlayingUI()
        {
            // Actualización en tiempo real durante gameplay
            if (GameStateManager.Instance?.CurrentGameState == GameState.Playing)
            {
                if (livesText != null)
                {
                    livesText.text = $"❤ {GameStateManager.Instance.LivesRemaining}";
                }
                
                if (scoreText != null)
                {
                    scoreText.text = $"Score: {GameStateManager.Instance.Score}";
                }
                
                if (waveInfoText != null)
                {
                    int current = GameStateManager.Instance.CurrentWaveNumber;
                    int total = GameStateManager.Instance.GetTotalWaves();
                    int enemies = GameStateManager.Instance.EnemiesRemaining;
                    waveInfoText.text = $"Wave {current}/{total} | Enemigos: {enemies}";
                }
            }
        }
        
        /// <summary>
        /// Muestra el panel de inicio.
        /// </summary>
        public void ShowStartPanel()
        {
            if (startPanel != null)
            {
                startPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// Oculta el panel de inicio.
        /// </summary>
        public void HideStartPanel()
        {
            if (startPanel != null)
            {
                startPanel.SetActive(false);
            }
        }
    }
}
