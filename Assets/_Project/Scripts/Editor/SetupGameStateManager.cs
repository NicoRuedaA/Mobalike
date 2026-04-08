using UnityEngine;
using UnityEditor;
using MobaGameplay.Game;

namespace MobaGameplay.Editor
{
    /// <summary>
    /// Editor helper para setup rápido del GameStateManager.
    /// Menú: GameObject → MobaGameplay → Game State Manager
    /// </summary>
    public class SetupGameStateManager
    {
        [MenuItem("GameObject/MobaGameplay/Game State Manager", false, 10)]
        public static void CreateGameStateManager()
        {
            // Verificar si ya existe
            if (GameStateManager.Instance != null)
            {
                Selection.activeGameObject = GameStateManager.Instance.gameObject;
                EditorUtility.DisplayDialog(
                    "GameStateManager", 
                    "Ya existe un GameStateManager en la escena.", 
                    "OK"
                );
                return;
            }
            
            // Crear GameObject
            GameObject go = new GameObject("GameStateManager");
            go.AddComponent<GameStateManager>();
            
            // Hacerlo persistir entre escenas
            Object.DontDestroyOnLoad(go);
            
            Selection.activeGameObject = go;
            
            // Crear spawn points por defecto si no existen
            CreateDefaultSpawnPoints(go);
            
            Debug.Log("[SetupGameStateManager] GameStateManager creado. Configura las oleadas en el Inspector.");
        }
        
        [MenuItem("GameObject/MobaGameplay/Player Spawn Point", false, 11)]
        public static void CreatePlayerSpawnPoint()
        {
            GameObject go = new GameObject("PlayerSpawnPoint");
            go.transform.position = Vector3.zero;
            
            // Marcar con layer "Ground" o similar
            Selection.activeGameObject = go;
        }
        
        [MenuItem("GameObject/MobaGameplay/Enemy Spawn Point", false, 12)]
        public static void CreateEnemySpawnPoint()
        {
            GameObject go = new GameObject("EnemySpawnPoint");
            
            // Posicionar ligeramente elevado para mejor visibilidad
            go.transform.position = new Vector3(0, 0.5f, 20);
            
            // Agregar Gizmo visual
            var component = go.AddComponent<EnemySpawnPoint>();
            
            Selection.activeGameObject = go;
        }
        
        private static void CreateDefaultSpawnPoints(GameObject gsm)
        {
            // Crear punto de spawn del jugador
            GameObject playerSpawn = GameObject.Find("PlayerSpawnPoint");
            if (playerSpawn == null)
            {
                playerSpawn = new GameObject("PlayerSpawnPoint");
                playerSpawn.transform.position = Vector3.zero;
            }
            
            // Asignar al GameStateManager
            var serializedObject = new SerializedObject(gsm.GetComponent<GameStateManager>());
            serializedObject.FindProperty("playerSpawnPoint").objectReferenceValue = playerSpawn.transform;
            serializedObject.ApplyModifiedProperties();
            
            // Crear puntos de spawn de enemigos
            string[] spawnPointNames = { "EnemySpawnNorth", "EnemySpawnSouth", "EnemySpawnEast", "EnemySpawnWest" };
            Vector3[] spawnPositions = {
                new Vector3(0, 0.5f, 30),
                new Vector3(0, 0.5f, -30),
                new Vector3(30, 0.5f, 0),
                new Vector3(-30, 0.5f, 0)
            };
            
            Transform[] spawnPoints = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject spawn = GameObject.Find(spawnPointNames[i]);
                if (spawn == null)
                {
                    spawn = new GameObject(spawnPointNames[i]);
                    spawn.transform.position = spawnPositions[i];
                }
                spawnPoints[i] = spawn.transform;
            }
            
            // Asignar al GameStateManager
            serializedObject = new SerializedObject(gsm.GetComponent<GameStateManager>());
            serializedObject.FindProperty("enemySpawnPoints").arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                serializedObject.FindProperty($"enemySpawnPoints.Array.data[{i}]").objectReferenceValue = spawnPoints[i];
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    /// <summary>
    /// Componente visual para puntos de spawn de enemigos.
    /// Muestra un gizmo en el editor.
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
            
            // Flecha hacia abajo
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 3f);
        }
    }
}
