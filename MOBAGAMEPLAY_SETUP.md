# Configuración del Proyecto MobaGameplay

## Resumen de Tareas

He creado un script de automatización que ejecuta todas las tareas solicitadas. Como las herramientas MCP de Unity no están disponibles en este entorno, debes ejecutar el script manualmente en Unity.

## Archivo Creado

**`Assets/_Project/Scripts/Editor/MobaGameplaySetup.cs`**

Este script contiene todas las tareas automatizadas y añade menús al editor de Unity.

---

## Instrucciones de Ejecución

### Opción 1: Ejecutar Setup Completo (Recomendado)

1. Abre el proyecto en Unity Editor
2. Espera a que termine la compilación
3. Ve al menú: **`MobaGameplay > Setup > Run Full Setup`**
4. El script ejecutará todas las tareas automáticamente
5. Revisa la consola para ver el estado de cada tarea

### Opción 2: Ejecutar Tareas Individuales

Si prefieres ejecutar una tarea específica:

- **TAREA 1**: `MobaGameplay > Setup > Task 1 - Create GroundItem Prefab`
- **TAREA 2**: `MobaGameplay > Setup > Task 2 - Create Folder Structure`
- **TAREA 3**: `MobaGameplay > Setup > Task 3 - Copy Items`
- **TAREA 4**: `MobaGameplay > Setup > Task 4 - Configure HeroEntity`
- **TAREA 5**: `MobaGameplay > Setup > Task 5 - Create GroundItem Tag`

---

## Detalle de Cada Tarea

### ✅ TAREA 1: Crear Prefab GroundItem
- **Path del prefab**: `Assets/_Project/Prefabs/Items/GroundItem.prefab`
- **Componentes**:
  - `GroundItem` (script)
  - `SphereCollider` (isTrigger = true, radius = 0.5)
  - Hijo `SpriteVisual` con `SpriteRenderer`
- **Tag**: Configurado como "GroundItem"

### ✅ TAREA 2: Crear Estructura de Carpetas
- **Carpeta creada**: `Assets/Resources/ScriptableObjects/Items/`
- Esta carpeta es necesaria para cargar items automáticamente en runtime

### ✅ TAREA 3: Copiar Items Existentes
- **Origen**: `Assets/_Project/ScriptableObjects/Items/`
- **Destino**: `Assets/Resources/ScriptableObjects/Items/`
- Se copian todos los archivos `.asset` (excluyendo metadatos)

### ✅ TAREA 4: Configurar HeroEntity
1. Busca el GameObject con componente `HeroEntity` en la escena
2. Agrega `InventoryComponent` si no existe
3. Agrega `ItemPickupDetector` si no existe
- **Nota**: Asegúrate de tener una escena abierta con el HeroEntity

### ✅ TAREA 5: Crear Tag "GroundItem"
- Crea el tag "GroundItem" en el TagManager de Unity
- Este tag es usado por `ItemPickupDetector` para identificar items recogibles

---

## Verificación Manual

Después de ejecutar el setup, verifica lo siguiente:

### 1. Verificar Prefab
- Ve a `Assets/_Project/Prefabs/Items/`
- Confirma que existe `GroundItem.prefab`
- Haz doble clic para verificar la estructura

### 2. Verificar Carpetas
- Ve a `Assets/Resources/ScriptableObjects/Items/`
- Confirma que los items fueron copiados

### 3. Verificar HeroEntity
- Selecciona el GameObject del jugador en la escena
- En el Inspector, verifica:
  - ✅ Componente `InventoryComponent` presente
  - ✅ Componente `ItemPickupDetector` presente

### 4. Verificar Tag
- Ve a `Edit > Project Settings > Tags and Layers`
- Confirma que existe el tag "GroundItem" en la lista

---

## Posibles Problemas y Soluciones

### "No se encontró HeroEntity"
- Asegúrate de tener una escena abierta
- Verifica que existe un GameObject con el script `HeroEntity`
- La escena debe estar en modo Play o Edit

### "Tag 'GroundItem' no se pudo crear"
- Unity tiene un límite de tags
- Verifica que no hayas alcanzado el máximo en `Edit > Project Settings > Tags and Layers`

### "Items no se copiaron"
- Verifica que existan items en `Assets/_Project/ScriptableObjects/Items/`
- Los archivos deben tener extensión `.asset`

---

## Estado de la Configuración

| Tarea | Descripción | Estado |
|-------|-------------|--------|
| 1 | Crear prefab GroundItem | ⏳ Pendiente de ejecución en Unity |
| 2 | Crear estructura de carpetas | ⏳ Pendiente de ejecución en Unity |
| 3 | Copiar items existentes | ⏳ Pendiente de ejecución en Unity |
| 4 | Configurar HeroEntity | ⏳ Pendiente de ejecución en Unity |
| 5 | Crear tag "GroundItem" | ⏳ Pendiente de ejecución en Unity |

---

## Notas Adicionales

- El script es idempotente: puede ejecutarse múltiples veces sin problemas
- Si algo falla, revisa la consola de Unity para mensajes de error detallados
- El script detecta automáticamente si los elementos ya existen y los omite

---

*Documento generado automáticamente para la configuración del proyecto MobaGameplay*
