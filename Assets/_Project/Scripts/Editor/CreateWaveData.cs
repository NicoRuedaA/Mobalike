using UnityEngine;
using UnityEditor;
using MobaGameplay.Game;

namespace MobaGameplay.Editor
{
    /// <summary>
    /// Editor helper para crear WaveData assets.
    /// Menú: Assets → Create → MobaGameplay → Wave Data
    /// </summary>
    public class CreateWaveData
    {
        [MenuItem("Assets/Create/MobaGameplay/Wave Data", false, 1)]
        public static void CreateWaveDataAsset()
        {
            WaveData asset = ScriptableObject.CreateInstance<WaveData>();
            
            // Configuración por defecto
            asset.waveNumber = 1;
            asset.enemyCount = 5;
            asset.spawnInterval = 1f;
            asset.preparationTime = 5f;
            asset.difficultyMultiplier = 1f;
            asset.isBossWave = false;
            
            // Guardar
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets/_Project/ScriptableObjects/Game/Waves";
            }
            else if (!AssetDatabase.IsValidFolder(path))
            {
                path = System.IO.Path.GetDirectoryName(path);
            }
            
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/NewWaveData.asset");
            
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
