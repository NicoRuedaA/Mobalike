using UnityEngine;
using UnityEditor;
using MobaGameplay.Game;
using MobaGameplay.Core;

namespace MobaGameplay.Editor
{
    /// <summary>
    /// Setup automático del GameStateManager y sus dependencias.
    /// Ejecutar: GameObject → MobaGameplay → Setup Game State Manager
    /// </summary>
    public class AutoSetupGameManager
    {
        [MenuItem("GameObject/MobaGameplay/Setup Game State Manager", false, 10)]
        public static void SetupGameStateManager()
        {
            // 1. Crear o encontrar GameStateManager
            GameObject gsmObj = GameObject.Find("GameStateManager");
            if (gsmObj == null)
            {
                gsmObj = new GameObject("GameStateManager");
                gsmObj.AddComponent<GameStateManager>();
                Debug.Log("[AutoSetup] Created GameStateManager");
            }
            else
            {
                Debug.Log("[AutoSetup] GameStateManager already exists");
            }
            
            GameStateManager gsm = gsmObj.GetComponent<GameStateManager>();
            
            // 2. Crear Player Spawn Point
            GameObject playerSpawn = GameObject.Find("PlayerSpawnPoint");
            if (playerSpawn == null)
            {
                playerSpawn = new GameObject("PlayerSpawnPoint");
                playerSpawn.transform.position = Vector3.zero;
                Debug.Log("[AutoSetup] Created PlayerSpawnPoint at origin");
            }
            
            // Configurar en GSM
            SerializedObject gsmSerialized = new SerializedObject(gsm);
            gsmSerialized.FindProperty("playerSpawnPoint").objectReferenceValue = playerSpawn.transform;
            gsmSerialized.ApplyModifiedProperties();
            
            // 3. Encontrar Player
            HeroEntity player = Object.FindFirstObjectByType<HeroEntity>();
            if (player != null)
            {
                gsm.SetPlayerEntity(player);
                Debug.Log($"[AutoSetup] Connected Player: {player.name}");
            }
            else
            {
                Debug.LogWarning("[AutoSetup] No HeroEntity (Player) found in scene!");
            }
            
            // 4. Crear Enemy Spawn Points
            CreateEnemySpawnPoints(gsm, gsmSerialized);
            
            // 5. Crear WaveData básico si no existe
            CreateDefaultWaveData(gsm, gsmSerialized);
            
            // 6. Seleccionar el GSM
            Selection.activeGameObject = gsmObj;
            
            EditorUtility.DisplayDialog(
                "GameStateManager Setup Complete",
                "GameStateManager configurado:\n" +
                "- PlayerSpawnPoint creado\n" +
                "- 4 EnemySpawnPoints creados\n" +
                "- Player conectado\n" +
                "- WaveData básica creada\n\n" +
                "Para iniciar el juego, llama:\n" +
                "GameStateManager.Instance.StartGame()",
                "OK"
            );
        }
        
        private static void CreateEnemySpawnPoints(GameStateManager gsm, SerializedObject gsmSerialized)
        {
            string[] spawnNames = { "EnemySpawnNorth", "EnemySpawnSouth", "EnemySpawnEast", "EnemySpawnWest" };
            Vector3[] spawnPositions = {
                new Vector3(0, 0.5f, 25),
                new Vector3(0, 0.5f, -25),
                new Vector3(25, 0.5f, 0),
                new Vector3(-25, 0.5f, 0)
            };
            
            Transform[] spawns = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject spawn = GameObject.Find(spawnNames[i]);
                if (spawn == null)
                {
                    spawn = new GameObject(spawnNames[i]);
                    spawn.transform.position = spawnPositions[i];
                }
                spawns[i] = spawn.transform;
            }
            
            // Agregar gizmo visual
            foreach (var spawn in spawns)
            {
                if (spawn.GetComponent<EnemySpawnPoint>() == null)
                {
                    spawn.gameObject.AddComponent<EnemySpawnPoint>();
                }
            }
            
            gsmSerialized.FindProperty("enemySpawnPoints").arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                gsmSerialized.FindProperty($"enemySpawnPoints.Array.data[{i}]").objectReferenceValue = spawns[i];
            }
            gsmSerialized.ApplyModifiedProperties();
            
            Debug.Log("[AutoSetup] Created 4 Enemy Spawn Points");
        }
        
        private static void CreateDefaultWaveData(GameStateManager gsm, SerializedObject gsmSerialized)
        {
            // Buscar Enemy_Dummy como prefab de enemigo
            GameObject enemyPrefab = GameObject.Find("Enemy_Dummy");
            
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[AutoSetup] Enemy_Dummy not found, WaveData created without enemy prefab");
            }
            
            // Crear 3 WaveData
            WaveData[] waves = new WaveData[3];
            for (int i = 0; i < 3; i++)
            {
                WaveData wave = ScriptableObject.CreateInstance<WaveData>();
                wave.waveNumber = i + 1;
                wave.enemyCount = 3 + (i * 2); // Wave 1: 3, Wave 2: 5, Wave 3: 7
                wave.spawnInterval = 1.5f;
                wave.preparationTime = 5f;
                wave.difficultyMultiplier = 1f + (i * 0.25f); // Más difícil cada ola
                wave.isBossWave = false;
                
                if (enemyPrefab != null)
                {
                    wave.enemyPrefabs = new GameObject[] { enemyPrefab };
                }
                
                // Guardar como asset
                string path = "Assets/_Project/ScriptableObjects/Game/Waves";
                if (!AssetDatabase.IsValidFolder(path))
                {
                    AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects/Game", "Waves");
                }
                
                string assetPath = $"{path}/Wave_{i + 1}.asset";
                AssetDatabase.CreateAsset(wave, assetPath);
                waves[i] = wave;
            }
            
            AssetDatabase.SaveAssets();
            
            // Asignar al GSM
            gsmSerialized.FindProperty("waveConfigs").arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                gsmSerialized.FindProperty($"waveConfigs.Array.data[{i}]").objectReferenceValue = waves[i];
            }
            gsmSerialized.ApplyModifiedProperties();
            
            Debug.Log("[AutoSetup] Created 3 WaveData assets");
        }
        
        [MenuItem("GameObject/MobaGameplay/Start Game", false, 11)]
        public static void StartGame()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StartGame();
                Debug.Log("[AutoSetup] Game Started!");
            }
            else
            {
                Debug.LogError("[AutoSetup] GameStateManager not found! Run Setup first.");
            }
        }
    }
    
    /// <summary>
    /// Componente visual para puntos de spawn de enemigos.
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, 1.5f);
            
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
            Gizmos.DrawSphere(transform.position, 0.5f);
            
            // Flecha hacia abajo
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 2f);
        }
    }
}
