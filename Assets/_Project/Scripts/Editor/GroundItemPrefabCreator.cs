using UnityEngine;
using UnityEditor;
using MobaGameplay.Inventory;

namespace MobaGameplay.Editor
{
    /// <summary>
    /// Utilidad para crear el prefab GroundItem con toda la configuración necesaria.
    /// </summary>
    public static class GroundItemPrefabCreator
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Items/GroundItem.prefab";
        private const string SORTING_LAYER = "World";

        [MenuItem("MobaGameplay/Create GroundItem Prefab")]
        public static void CreateGroundItemPrefab()
        {
            // Crear el GameObject raíz
            GameObject groundItem = new GameObject("GroundItem");

            // Configurar tag
            groundItem.tag = "GroundItem";

            // Agregar componente GroundItem
            GroundItem groundItemComponent = groundItem.AddComponent<GroundItem>();

            // Agregar SphereCollider (trigger)
            SphereCollider collider = groundItem.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.5f;

            // Crear hijo para el SpriteRenderer
            GameObject spriteObject = new GameObject("SpriteVisual");
            spriteObject.transform.SetParent(groundItem.transform);
            spriteObject.transform.localPosition = Vector3.zero;
            spriteObject.transform.localRotation = Quaternion.identity;

            // Configurar SpriteRenderer
            SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.transform.localScale = Vector3.one * 0.5f;

            // Intentar asignar el sorting layer "World"
            int sortingLayerID = SortingLayer.NameToID(SORTING_LAYER);
            if (sortingLayerID != -1)
            {
                spriteRenderer.sortingLayerID = sortingLayerID;
            }
            else
            {
                Debug.LogWarning($"[GroundItemPrefabCreator] Sorting layer '{SORTING_LAYER}' not found. Using Default.");
            }

            // Asignar referencia al GroundItem component
            SerializedObject serializedObject = new SerializedObject(groundItemComponent);
            SerializedProperty spriteRendererProp = serializedObject.FindProperty("_spriteRenderer");
            if (spriteRendererProp != null)
            {
                spriteRendererProp.objectReferenceValue = spriteRenderer;
                serializedObject.ApplyModifiedProperties();
            }

            // Asegurar que existe la carpeta
            string directory = System.IO.Path.GetDirectoryName(PREFAB_PATH);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Guardar prefab
            bool prefabSuccess;
            PrefabUtility.SaveAsPrefabAsset(groundItem, PREFAB_PATH, out prefabSuccess);

            if (prefabSuccess)
            {
                Debug.Log($"[GroundItemPrefabCreator] Successfully created GroundItem prefab at {PREFAB_PATH}");
                EditorUtility.DisplayDialog("Success", $"GroundItem prefab created at:\n{PREFAB_PATH}", "OK");
            }
            else
            {
                Debug.LogError("[GroundItemPrefabCreator] Failed to create GroundItem prefab");
                EditorUtility.DisplayDialog("Error", "Failed to create GroundItem prefab. Check console for details.", "OK");
            }

            // Limpiar objeto temporal
            Object.DestroyImmediate(groundItem);
        }

        [MenuItem("MobaGameplay/Setup Item Drop System")]
        public static void SetupItemDropSystem()
        {
            // Verificar que existe el prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                bool createPrefab = EditorUtility.DisplayDialog(
                    "Prefab Not Found",
                    "GroundItem prefab not found. Would you like to create it?",
                    "Yes", "No");

                if (createPrefab)
                {
                    CreateGroundItemPrefab();
                }
                return;
            }

            // Verificar que existe el tag GroundItem
            if (!TagExists("GroundItem"))
            {
                EditorUtility.DisplayDialog(
                    "Tag Missing",
                    "The tag 'GroundItem' does not exist. Please add it in:\n" +
                    "Edit > Project Settings > Tags and Layers",
                    "OK");
                return;
            }

            EditorUtility.DisplayDialog(
                "Setup Complete",
                "Item Drop System is ready to use!\n\n" +
                "Don't forget to:\n" +
                "1. Add ItemPickupDetector to your HeroEntity\n" +
                "2. Ensure enemies have EnemyEntity component\n" +
                "3. Move items to Resources/ScriptableObjects/Items for automatic loading",
                "OK");
        }

        private static bool TagExists(string tagName)
        {
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
            {
                if (UnityEditorInternal.InternalEditorUtility.tags[i] == tagName)
                    return true;
            }
            return false;
        }
    }
}
