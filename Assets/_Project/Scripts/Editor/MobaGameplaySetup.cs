using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace MobaGameplay.Editor
{
    /// <summary>
    /// Script de automatización para configurar el proyecto MobaGameplay.
    /// Ejecutar desde: MobaGameplay/Setup/Run Full Setup
    /// </summary>
    public static class MobaGameplaySetup
    {
        private const string SOURCE_ITEMS_PATH = "Assets/_Project/ScriptableObjects/Items";
        private const string TARGET_ITEMS_PATH = "Assets/Resources/ScriptableObjects/Items";
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/Items/GroundItem.prefab";
        private const string SORTING_LAYER = "World";

        [MenuItem("MobaGameplay/Setup/Run Full Setup")]
        public static void RunFullSetup()
        {
            Debug.Log("[MobaGameplaySetup] Iniciando configuración completa...");
            
            bool success = true;
            
            // TAREA 1: Crear prefab GroundItem
            success &= Task1_CreateGroundItemPrefab();
            
            // TAREA 2: Crear estructura de carpetas
            success &= Task2_CreateFolderStructure();
            
            // TAREA 3: Copiar items existentes
            success &= Task3_CopyItems();
            
            // TAREA 4: Configurar HeroEntity
            success &= Task4_ConfigureHeroEntity();
            
            // TAREA 5: Crear tag "GroundItem"
            success &= Task5_CreateTag();
            
            // Guardar assets
            AssetDatabase.SaveAssets();
            
            if (success)
            {
                Debug.Log("[MobaGameplaySetup] ✅ Configuración completada exitosamente!");
                EditorUtility.DisplayDialog("Setup Complete", 
                    "Todas las tareas se han completado exitosamente:\n\n" +
                    "✅ TAREA 1: Prefab GroundItem creado\n" +
                    "✅ TAREA 2: Estructura de carpetas creada\n" +
                    "✅ TAREA 3: Items copiados a Resources\n" +
                    "✅ TAREA 4: HeroEntity configurado con ItemPickupDetector\n" +
                    "✅ TAREA 5: Tag 'GroundItem' creado\n\n" +
                    "El proyecto está listo para usar el sistema de items!", 
                    "OK");
            }
            else
            {
                Debug.LogWarning("[MobaGameplaySetup] ⚠️ Algunas tareas fallaron. Revisa los logs.");
                EditorUtility.DisplayDialog("Setup Warning", 
                    "Algunas tareas no se completaron correctamente.\n\n" +
                    "Revisa la consola de Unity para más detalles.", 
                    "OK");
            }
        }

        #region TAREA 1: Crear prefab GroundItem
        private static bool Task1_CreateGroundItemPrefab()
        {
            Debug.Log("[MobaGameplaySetup] TAREA 1: Creando prefab GroundItem...");
            
            try
            {
                // Verificar si ya existe
                if (File.Exists(PREFAB_PATH))
                {
                    Debug.Log("[MobaGameplaySetup] Prefab GroundItem ya existe. Saltando...");
                    return true;
                }

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
                    Debug.LogWarning($"[MobaGameplaySetup] Sorting layer '{SORTING_LAYER}' not found. Using Default.");
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
                string directory = Path.GetDirectoryName(PREFAB_PATH);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Guardar prefab
                bool prefabSuccess;
                PrefabUtility.SaveAsPrefabAsset(groundItem, PREFAB_PATH, out prefabSuccess);

                // Limpiar objeto temporal
                Object.DestroyImmediate(groundItem);

                if (prefabSuccess)
                {
                    Debug.Log($"[MobaGameplaySetup] ✅ TAREA 1: Prefab GroundItem creado exitosamente en {PREFAB_PATH}");
                    return true;
                }
                else
                {
                    Debug.LogError("[MobaGameplaySetup] ❌ TAREA 1: Falló al crear prefab GroundItem");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MobaGameplaySetup] ❌ TAREA 1: Error - {ex.Message}");
                return false;
            }
        }
        #endregion

        #region TAREA 2: Crear estructura de carpetas
        private static bool Task2_CreateFolderStructure()
        {
            Debug.Log("[MobaGameplaySetup] TAREA 2: Creando estructura de carpetas...");
            
            try
            {
                bool created = false;
                
                if (!Directory.Exists("Assets/Resources"))
                {
                    Directory.CreateDirectory("Assets/Resources");
                    created = true;
                }
                
                if (!Directory.Exists("Assets/Resources/ScriptableObjects"))
                {
                    Directory.CreateDirectory("Assets/Resources/ScriptableObjects");
                    created = true;
                }
                
                if (!Directory.Exists(TARGET_ITEMS_PATH))
                {
                    Directory.CreateDirectory(TARGET_ITEMS_PATH);
                    created = true;
                }

                if (created)
                {
                    AssetDatabase.Refresh();
                    Debug.Log($"[MobaGameplaySetup] ✅ TAREA 2: Carpeta creada: {TARGET_ITEMS_PATH}");
                }
                else
                {
                    Debug.Log($"[MobaGameplaySetup] ✅ TAREA 2: La carpeta ya existe: {TARGET_ITEMS_PATH}");
                }
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MobaGameplaySetup] ❌ TAREA 2: Error - {ex.Message}");
                return false;
            }
        }
        #endregion

        #region TAREA 3: Copiar items existentes
        private static bool Task3_CopyItems()
        {
            Debug.Log("[MobaGameplaySetup] TAREA 3: Copiando items existentes...");
            
            try
            {
                if (!Directory.Exists(SOURCE_ITEMS_PATH))
                {
                    Debug.LogWarning($"[MobaGameplaySetup] ⚠️ TAREA 3: No existe la carpeta fuente: {SOURCE_ITEMS_PATH}");
                    return true; // No es un error crítico
                }

                // Buscar todos los assets .asset en la carpeta fuente (excluyendo .meta)
                string[] sourceFiles = Directory.GetFiles(SOURCE_ITEMS_PATH, "*.asset")
                    .Where(f => !f.EndsWith(".meta"))
                    .ToArray();

                if (sourceFiles.Length == 0)
                {
                    Debug.Log("[MobaGameplaySetup] ⚠️ TAREA 3: No se encontraron items para copiar");
                    return true;
                }

                int copiedCount = 0;
                foreach (string sourceFile in sourceFiles)
                {
                    string fileName = Path.GetFileName(sourceFile);
                    string targetFile = Path.Combine(TARGET_ITEMS_PATH, fileName);
                    
                    // Copiar el archivo
                    File.Copy(sourceFile, targetFile, true);
                    copiedCount++;
                    Debug.Log($"[MobaGameplaySetup] Copiado: {fileName}");
                }

                AssetDatabase.Refresh();
                Debug.Log($"[MobaGameplaySetup] ✅ TAREA 3: {copiedCount} items copiados exitosamente");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MobaGameplaySetup] ❌ TAREA 3: Error - {ex.Message}");
                return false;
            }
        }
        #endregion

        #region TAREA 4: Configurar HeroEntity
        private static bool Task4_ConfigureHeroEntity()
        {
            Debug.Log("[MobaGameplaySetup] TAREA 4: Configurando HeroEntity...");
            
            try
            {
                // Buscar HeroEntity en la escena activa
                HeroEntity hero = Object.FindObjectOfType<HeroEntity>();
                
                if (hero == null)
                {
                    Debug.LogError("[MobaGameplaySetup] ❌ TAREA 4: No se encontró HeroEntity en la escena");
                    return false;
                }

                GameObject heroObject = hero.gameObject;
                bool modified = false;

                // Verificar InventoryComponent
                var inventory = heroObject.GetComponent<MMORPG.Inventory.InventoryComponent>();
                if (inventory == null)
                {
                    inventory = heroObject.AddComponent<MMORPG.Inventory.InventoryComponent>();
                    Debug.Log("[MobaGameplaySetup] Agregado InventoryComponent a HeroEntity");
                    modified = true;
                }
                else
                {
                    Debug.Log("[MobaGameplaySetup] InventoryComponent ya existe en HeroEntity");
                }

                // Agregar ItemPickupDetector
                var pickupDetector = heroObject.GetComponent<ItemPickupDetector>();
                if (pickupDetector == null)
                {
                    pickupDetector = heroObject.AddComponent<ItemPickupDetector>();
                    Debug.Log("[MobaGameplaySetup] Agregado ItemPickupDetector a HeroEntity");
                    modified = true;
                }
                else
                {
                    Debug.Log("[MobaGameplaySetup] ItemPickupDetector ya existe en HeroEntity");
                }

                if (modified)
                {
                    EditorUtility.SetDirty(heroObject);
                    Debug.Log($"[MobaGameplaySetup] ✅ TAREA 4: HeroEntity configurado exitosamente");
                }
                else
                {
                    Debug.Log($"[MobaGameplaySetup] ✅ TAREA 4: HeroEntity ya estaba configurado");
                }
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MobaGameplaySetup] ❌ TAREA 4: Error - {ex.Message}");
                return false;
            }
        }
        #endregion

        #region TAREA 5: Crear tag "GroundItem"
        private static bool Task5_CreateTag()
        {
            Debug.Log("[MobaGameplaySetup] TAREA 5: Creando tag 'GroundItem'...");
            
            try
            {
                // Verificar si el tag ya existe
                if (TagExists("GroundItem"))
                {
                    Debug.Log("[MobaGameplaySetup] ✅ TAREA 5: Tag 'GroundItem' ya existe");
                    return true;
                }

                // Abrir ProjectSettings/TagManager.asset
                SerializedObject tagManager = new SerializedObject(
                    AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                
                SerializedProperty tagsProp = tagManager.FindProperty("tags");

                // Encontrar el primer índice vacío
                int emptyIndex = -1;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    if (string.IsNullOrEmpty(tagsProp.GetArrayElementAtIndex(i).stringValue))
                    {
                        emptyIndex = i;
                        break;
                    }
                }

                // Si no hay espacio vacío, agregar al final
                if (emptyIndex == -1)
                {
                    emptyIndex = tagsProp.arraySize;
                    tagsProp.InsertArrayElementAtIndex(emptyIndex);
                }

                // Asignar el tag
                tagsProp.GetArrayElementAtIndex(emptyIndex).stringValue = "GroundItem";
                tagManager.ApplyModifiedProperties();

                Debug.Log("[MobaGameplaySetup] ✅ TAREA 5: Tag 'GroundItem' creado exitosamente");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MobaGameplaySetup] ❌ TAREA 5: Error - {ex.Message}");
                Debug.Log("[MobaGameplaySetup] Puedes crear el tag manualmente en: Edit > Project Settings > Tags and Layers");
                return false;
            }
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
        #endregion

        #region Menús individuales para cada tarea
        [MenuItem("MobaGameplay/Setup/Task 1 - Create GroundItem Prefab")]
        public static void MenuTask1() => Task1_CreateGroundItemPrefab();

        [MenuItem("MobaGameplay/Setup/Task 2 - Create Folder Structure")]
        public static void MenuTask2() => Task2_CreateFolderStructure();

        [MenuItem("MobaGameplay/Setup/Task 3 - Copy Items")]
        public static void MenuTask3() => Task3_CopyItems();

        [MenuItem("MobaGameplay/Setup/Task 4 - Configure HeroEntity")]
        public static void MenuTask4() => Task4_ConfigureHeroEntity();

        [MenuItem("MobaGameplay/Setup/Task 5 - Create GroundItem Tag")]
        public static void MenuTask5() => Task5_CreateTag();
        #endregion
    }
}
