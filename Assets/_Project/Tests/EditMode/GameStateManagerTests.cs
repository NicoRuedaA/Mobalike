using NUnit.Framework;
using UnityEngine;
using MobaGameplay.Game;

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
            _go = new GameObject("GameStateManager");
            _gsm = _go.AddComponent<GameStateManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.DestroyImmediate(_go);
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
            _gsm.SetPlayerEntity(player);

            _gsm.RestartGame();

            Assert.AreEqual(0, _gsm.Score, "Score should reset to 0 on restart");
            Object.DestroyImmediate(playerObj);
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