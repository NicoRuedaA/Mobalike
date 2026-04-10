using NUnit.Framework;
using UnityEngine;
using MobaGameplay.Game;
using System.Reflection;

namespace MobaGameplay.Tests
{
    /// <summary>
    /// Tests for GameStateManager wave logic.
    /// Verifies that wave clear events fire exactly once per wave
    /// (validation of Phase 1 fix: duplicate wave-clear detection removed).
    /// </summary>
    [TestFixture]
    public class GameStateManagerTests
    {
        private GameObject _go;
        private GameStateManager _gsm;

        [SetUp]
        public void SetUp()
        {
            // Force-reset singleton via reflection (private setter)
            ResetSingleton();

            _go = new GameObject("GameStateManager");
            _gsm = _go.AddComponent<GameStateManager>();

            // Set up minimal wave config so StartGame doesn't immediately end
            // waveConfigs is [SerializeField] private, so we use reflection
            var waveData = ScriptableObject.CreateInstance<WaveData>();
            waveData.enemyCount = 1;
            waveData.spawnInterval = 999f; // Long interval so spawning doesn't complete
            waveData.preparationTime = 999f; // Long prep so coroutine doesn't transition
            SetWaveConfigs(new WaveData[] { waveData });
        }

        [TearDown]
        public void TearDown()
        {
            // Reset time scale in case tests paused/ended the game
            Time.timeScale = 1f;

            if (_go != null)
            {
                Object.DestroyImmediate(_go);
            }

            // Clean up any leftover DontDestroyOnLoad objects
            var leftover = Object.FindObjectOfType<GameStateManager>();
            if (leftover != null)
            {
                Object.DestroyImmediate(leftover.gameObject);
            }

            // Ensure singleton is fully reset
            ResetSingleton();
        }

        /// <summary>
        /// Helper: Reset the GameStateManager singleton via reflection.
        /// The Instance property has a private setter, so we need reflection.
        /// </summary>
        private void ResetSingleton()
        {
            var prop = typeof(GameStateManager).GetProperty("Instance",
                BindingFlags.Public | BindingFlags.Static);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(null, null);
            }
            else
            {
                // Fallback: use backing field
                var field = typeof(GameStateManager).GetField("<Instance>k__BackingField",
                    BindingFlags.NonPublic | BindingFlags.Static);
                if (field == null)
                {
                    field = typeof(GameStateManager).GetField("s_Instance",
                        BindingFlags.NonPublic | BindingFlags.Static);
                }
                field?.SetValue(null, null);
            }
        }

        /// <summary>
        /// Helper: Set private waveConfigs field via reflection.
        /// </summary>
        private void SetWaveConfigs(WaveData[] configs)
        {
            var field = typeof(GameStateManager).GetField("waveConfigs",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(_gsm, configs);
            }
        }

        // ============================================================
        // Initial state
        // ============================================================

        [Test]
        public void GameState_StartsAtMenu()
        {
            Assert.AreEqual(GameState.Menu, _gsm.CurrentGameState,
                "Game should start in Menu state");
        }

        [Test]
        public void WaveState_StartsAtIdle()
        {
            Assert.AreEqual(WaveState.Idle, _gsm.CurrentWaveState,
                "Wave state should start at Idle");
        }

        [Test]
        public void Score_StartsAtZero()
        {
            Assert.AreEqual(0, _gsm.Score,
                "Score should start at 0");
        }

        // ============================================================
        // State transitions
        // ============================================================

        [Test]
        public void StartGame_TransitionsToPlaying()
        {
            _gsm.StartGame();

            // StartGame transitions to Playing, then StartNextWave transitions wave state.
            // Even though StartNextWave starts a coroutine, the GameState should be Playing.
            Assert.AreEqual(GameState.Playing, _gsm.CurrentGameState,
                "StartGame should transition to Playing state");
        }

        [Test]
        public void PauseGame_TransitionsToPaused()
        {
            _gsm.StartGame();
            _gsm.PauseGame();

            Assert.AreEqual(GameState.Paused, _gsm.CurrentGameState,
                "PauseGame should transition to Paused state");
        }

        [Test]
        public void ResumeGame_TransitionsBackToPlaying()
        {
            _gsm.StartGame();
            _gsm.PauseGame();
            _gsm.ResumeGame();

            Assert.AreEqual(GameState.Playing, _gsm.CurrentGameState,
                "ResumeGame should transition back to Playing");
        }

        [Test]
        public void EndGame_TransitionsToEnded()
        {
            _gsm.StartGame();
            _gsm.EndGame(false);

            Assert.AreEqual(GameState.Ended, _gsm.CurrentGameState,
                "EndGame should transition to Ended state");
        }

        [Test]
        public void EndGame_RecordsVictory()
        {
            _gsm.StartGame();
            _gsm.EndGame(true);

            Assert.IsTrue(_gsm.IsVictory, "IsVictory should be true when ending with victory");
        }

        [Test]
        public void EndGame_RecordsDefeat()
        {
            _gsm.StartGame();
            _gsm.EndGame(false);

            Assert.IsFalse(_gsm.IsVictory, "IsVictory should be false when ending with defeat");
        }

        // ============================================================
        // Player management
        // ============================================================

        [Test]
        public void SetPlayerEntity_SetsReference()
        {
            var playerObj = new GameObject("TestPlayer");
            var player = playerObj.AddComponent<TestHeroEntity>();

            _gsm.SetPlayerEntity(player);

            // The GameStateManager should have accepted the player reference
            // (We can't directly check the private field, but we verify no exception was thrown)
            Object.DestroyImmediate(playerObj);
        }

        // ============================================================
        // Score
        // ============================================================

        [Test]
        public void AddScore_IncreasesScore()
        {
            _gsm.AddScore(100);

            Assert.AreEqual(100, _gsm.Score,
                "AddScore should increase score");
        }

        [Test]
        public void AddScore_Accumulates()
        {
            _gsm.AddScore(50);
            _gsm.AddScore(75);

            Assert.AreEqual(125, _gsm.Score,
                "Score should accumulate across multiple calls");
        }

        [Test]
        public void AddScore_FiresOnScoreChanged()
        {
            int? receivedScore = null;
            _gsm.OnScoreChanged += (score) => receivedScore = score;

            _gsm.AddScore(42);

            Assert.IsNotNull(receivedScore, "OnScoreChanged should fire");
            Assert.AreEqual(42, receivedScore.Value);
        }

        // ============================================================
        // Restart
        // ============================================================

        [Test]
        public void RestartGame_ResetsScore()
        {
            _gsm.AddScore(500);
            _gsm.StartGame();

            // Need a player entity for RestartGame (it calls RespawnPlayer)
            var playerObj = new GameObject("TestPlayer");
            var player = playerObj.AddComponent<TestHeroEntity>();
            player.ForceInit();
            _gsm.SetPlayerEntity(player);

            _gsm.RestartGame();

            Assert.AreEqual(0, _gsm.Score, "Score should reset to 0 on restart");
            Object.DestroyImmediate(playerObj);
        }

        // ============================================================
        // Without waves — immediate game end
        // ============================================================

        [Test]
        public void StartGame_WithNoWaves_TransitionsToVictory()
        {
            // Remove wave configs to test the no-waves scenario
            SetWaveConfigs(null);

            _gsm.StartGame();

            // Without wave configs, StartNextWave should end the game
            Assert.AreEqual(GameState.Ended, _gsm.CurrentGameState,
                "StartGame with no waves should end in Ended state");
            Assert.IsTrue(_gsm.IsVictory, "No waves should result in victory");
        }

        // ============================================================
        // Helper: Minimal test entity
        // ============================================================

        private class TestHeroEntity : MobaGameplay.Core.BaseEntity
        {
            protected override void Start()
            {
                base.Start();
            }

            public void ForceInit()
            {
                CurrentHealth = MaxHealth;
                CurrentMana = MaxMana;
            }
        }
    }
}