using UnityEngine;
using UnityEditor;

namespace MobaGameplay.UI.Targeting.Editor
{
    public static class TargetingPrefabBuilder
    {
        [MenuItem("Mobalike/Tools/Build Targeting Prefabs")]
        public static void BuildPrefabs()
        {
            // Ensure folders exist
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/UI/Targeting"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs/UI", "Targeting");
            }

            // Get a default unlit transparent material for the lines
            Material lineMat = new Material(Shader.Find("Sprites/Default"));

            // 1. Circle Indicator
            GameObject circleObj = new GameObject("CircleIndicator", typeof(LineRenderer), typeof(CircleIndicator));
            var circleLr = circleObj.GetComponent<LineRenderer>();
            circleLr.material = lineMat;
            circleLr.useWorldSpace = false;
            circleLr.loop = true;
            circleLr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            circleLr.receiveShadows = false;
            circleLr.numCapVertices = 5;
            circleLr.numCornerVertices = 5;

            string circlePath = "Assets/_Project/Prefabs/UI/Targeting/CircleIndicator.prefab";
            PrefabUtility.SaveAsPrefabAsset(circleObj, circlePath);
            Object.DestroyImmediate(circleObj);

            // 2. Line Indicator
            GameObject lineObj = new GameObject("LineIndicator", typeof(LineRenderer), typeof(LineIndicator));
            var lineLr = lineObj.GetComponent<LineRenderer>();
            lineLr.material = lineMat;
            lineLr.useWorldSpace = false;
            lineLr.loop = true;
            lineLr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineLr.receiveShadows = false;
            lineLr.numCapVertices = 5;
            lineLr.numCornerVertices = 5;

            string linePath = "Assets/_Project/Prefabs/UI/Targeting/LineIndicator.prefab";
            PrefabUtility.SaveAsPrefabAsset(lineObj, linePath);
            Object.DestroyImmediate(lineObj);

            // 3. Targeting Manager
            GameObject managerObj = new GameObject("TargetingManager", typeof(TargetingManager));
            var manager = managerObj.GetComponent<TargetingManager>();
            
            var so = new SerializedObject(manager);
            so.FindProperty("circleIndicatorPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(circlePath);
            so.FindProperty("lineIndicatorPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(linePath);
            so.ApplyModifiedProperties();

            string managerPath = "Assets/_Project/Prefabs/UI/Targeting/TargetingManager.prefab";
            PrefabUtility.SaveAsPrefabAsset(managerObj, managerPath);
            Object.DestroyImmediate(managerObj);

            Debug.Log("Successfully built Targeting Prefabs!");
        }
    }
}