# Sistema de Drops - Instrucciones de Configuración

## Archivos Creados

1. ✅ **GroundItem.cs** - Componente para items en el mundo 3D
2. ✅ **ItemDropSystem.cs** - Sistema estático para generar drops
3. ✅ **ItemPickupDetector.cs** - Componente para recoger items
4. ✅ **EnemyEntity.cs** - Modificado para llamar ItemDropSystem.DropRandomItem()
5. ✅ **GroundItemPrefabCreator.cs** - Utilidad de Editor para crear el prefab

## Configuración Manual del Prefab GroundItem

Dado que los prefabs de Unity son archivos binarios/YAML complejos, sigue estos pasos para crear el prefab manualmente:

### Paso 1: Crear el GameObject
1. En Unity, ve a **GameObject > Create Empty**
2. Nómbralo **"GroundItem"**
3. Asigna el tag **"GroundItem"** (si no existe, créalo en Edit > Project Settings > Tags and Layers)

### Paso 2: Agregar Componentes al Root
1. Agrega el componente **GroundItem** (script)
2. Agrega un **Sphere Collider**:
   - Centrado en (0, 0, 0)
   - Radio: 0.5
   - ✅ Is Trigger: ON

### Paso 3: Crear el SpriteVisual (Hijo)
1. Crea un objeto hijo: **GameObject > Create Empty Child**
2. Nómbralo **"SpriteVisual"**
3. Resetea su Transform (Position: 0,0,0)
4. Agrega **Sprite Renderer**:
   - Scale: 0.5, 0.5, 0.5
   - Sorting Layer: "World" (o "Default" si no existe)

### Paso 4: Conectar Referencias
1. En el componente GroundItem del root, asigna:
   - **Sprite Renderer**: arrastra el SpriteVisual aquí

### Paso 5: Guardar como Prefab
1. Arrastra el GroundItem a la carpeta `Assets/_Project/Prefabs/Items/`
2. Elimina el objeto de la escena

## Alternativa: Usar el Script de Editor

1. En Unity, ve al menú **MobaGameplay > Create GroundItem Prefab**
2. Esto creará automáticamente el prefab en la ruta correcta

## Configuración del Jugador

1. Selecciona tu **HeroEntity** en la escena
2. Agrega el componente **ItemPickupDetector**
3. Ajusta los valores:
   - **Detection Radius**: 2.0 (radio para recoger items)
   - **Ground Item Layer**: Default (o el layer que uses)
   - **Pickup Vfx Prefab**: (Opcional) prefab de partículas

## Configuración de Items para Drops

### Opción A: Usar Resources (Recomendado)
1. Crea la carpeta: `Assets/Resources/ScriptableObjects/Items/`
2. Copia o mueve todos los ItemData (.asset) a esa carpeta
3. El sistema los cargará automáticamente

### Opción B: Asignación Manual
1. Crea un GameObject vacío en la escena llamado "ItemDropManager"
2. Agrega este script temporal:

```csharp
using UnityEngine;
using MobaGameplay.Inventory;
using MMORPG.Inventory;

public class ItemDropInitializer : MonoBehaviour
{
    [SerializeField] private ItemData[] availableItems;

    void Awake()
    {
        ItemDropSystem.SetAvailableItems(availableItems);
    }
}
```

3. Asigna todos los ItemData al array en el Inspector

## Verificación del GroundSmashAbility

✅ **GroundSmashAbility.cs ya está funcionando correctamente**:
- Usa `Physics.OverlapSphere` para detectar enemigos
- Aplica daño físico a todos los enemigos en el área
- Excluye al owner del daño
- Mantiene la dirección hacia el objetivo

## Flujo de Funcionamiento

1. **Hero** usa GroundSmash → hace daño a enemigos
2. **Enemigo** muere → EnemyEntity.Die() se ejecuta
3. **NotifyKillReward()** → llama ItemDropSystem.DropRandomItem()
4. **ItemDropSystem** → 30% chance de spawnear item
5. **GroundItem** aparece en posición del enemigo + 0.5f Y
6. **GroundItem** flota y rota (animación visual)
7. **Hero** se acerca (2.0f radio) → ItemPickupDetector detecta
8. **ItemPickupDetector** intenta agregar al inventario
9. Si hay espacio → item se agrega, GroundItem se destruye
10. Si no hay espacio → item permanece en el suelo

## Notas Importantes

- El sistema usa el tag "GroundItem" para detección
- El inventario tiene 20 slots (definido en InventoryComponent.InventorySize)
- Los items se instancian con offset Y = 0.5f para evitar clipping
- La animación flotante usa movimiento senoidal
- El pickup es automático al acercarse (no requiere input)

## Troubleshooting

**Los items no aparecen:**
- Verifica que los ItemData estén en Resources/ScriptableObjects/Items/
- O usa ItemDropSystem.SetAvailableItems() para asignar manualmente

**El jugador no recoge items:**
- Asegúrate de que HeroEntity tenga ItemPickupDetector
- Verifica que el tag "GroundItem" exista
- Revisa el Detection Radius (default: 2.0f)

**El prefab no se encuentra:**
- Crea el prefab manualmente o usa el menú MobaGameplay > Create GroundItem Prefab
- El prefab debe estar en Assets/_Project/Prefabs/Items/GroundItem.prefab

**Errores de compilación:**
- Verifica que todos los scripts estén en los namespaces correctos
- GroundItem e ItemDropSystem están en MobaGameplay.Inventory
- ItemData y InventoryComponent están en MMORPG.Inventory
