using UnityEngine;

namespace MobaGameplay.Game
{
    /// <summary>
    /// Configuración de una oleada de enemigos.
    /// Crear como ScriptableObject: Right-click → Create → MobaGameplay → Wave Data
    /// </summary>
    [CreateAssetMenu(fileName = "Wave", menuName = "MobaGameplay/Wave Data", order = 1)]
    public class WaveData : ScriptableObject
    {
        [Header("Configuración de Oleada")]
        [Tooltip("Número de la oleada (para display).")]
        public int waveNumber;
        
        [Tooltip("Cantidad de enemigos en esta oleada.")]
        public int enemyCount = 5;
        
        [Tooltip("Intervalo entre spawns de enemigos.")]
        public float spawnInterval = 1f;
        
        [Tooltip("Tiempo de preparación antes de spawnear (countdown).")]
        public float preparationTime = 5f;
        
        [Header("Enemigos")]
        [Tooltip("Prefabs de enemigos disponibles para esta oleada.")]
        public GameObject[] enemyPrefabs;
        
        [Header("Dificultad")]
        [Tooltip("Multiplicador de dificultad para stats de enemigos.")]
        public float difficultyMultiplier = 1f;
        
        [Header("Oleada Especial")]
        [Tooltip("Si esta es una oleada de jefe.")]
        public bool isBossWave;
        
        [Tooltip("Prefab del jefe (si isBossWave es true).")]
        public GameObject bossPrefab;
    }
}
