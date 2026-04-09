# Plan: Sistema de Inventario con Loot

## Estado Actual Confirmado

**Lo que EXISTE:**
- `EnemyEntity` tiene `goldReward` (10) y `experienceReward` (25) como valores numéricos
- Método `NotifyKillReward()` preparado pero vacío
- Sistema de inventario MMORPG básico (20 slots, drag & drop)

**Lo que NO existe:**
- Prefab físico de exp/oro
- Spawn de objetos en el mundo
- Sistema de pickups físicos
- Sistema de rareza visual

---

## Objetivo

Transformar las recompensas numéricas actuales en **items físicos dropeables** con sistema de rareza visual.

---

## Fases de Implementación

### Fase 1: Sistema de Rareza

**Archivos a crear:**
- `Assets/_Project/Scripts/Inventory/ItemRarity.cs` - Enum de rarezas
- `Assets/_Project/Scripts/Inventory/RarityColorConfig.cs` - Configuración de colores

**Colores por rareza:**
| Rareza | Color |
|--------|-------|
| Common | `#9E9E9E` (Gris) |
| Uncommon | `#4CAF50` (Verde) |
| Rare | `#2196F3` (Azul) |
| Epic | `#9C27B0` (Púrpura) |
| Legendary | `#FF9800` (Naranja/Dorado) |

### Fase 2: Items de Currency

**ScriptableObjects a crear:**
- "Gold Coin" - ItemData con rareza Common
- "Experience Orb" - ItemData con rareza Uncommon

**Modificación:**
- `ItemData.cs` - Agregar campo `rarity`

### Fase 3: Modificar EnemyEntity

**Archivo a modificar:**
- `Assets/_Project/Scripts/Core/EnemyEntity.cs`

**Cambios en método `Die()`:**
1. Mantener valores `goldReward` y `experienceReward` existentes
2. Spawnear monedas físicas según `goldReward`
3. Spawnear orbe de XP según `experienceReward`
4. Distribuir en área alrededor del enemigo muerto

### Fase 4: WorldItem (Pickup Físico)

**Archivos a crear:**
- `Assets/_Project/Scripts/Inventory/WorldItem.cs` - Lógica del item en el mundo
- `Assets/_Project/Prefabs/WorldItem.prefab` - Prefab base

**Funcionalidades:**
- Flotación y rotación visual
- Trigger collider para detección
- Auto-recolección al acercarse el jugador
- Aplicar efecto (oro al inventario, XP directa)
- Color/visual según rareza del item

### Fase 5: Visualización de Rareza

**Archivo a modificar:**
- `Assets/_Project/Scripts/UI/InventorySlotUI.cs`

**Cambios:**
- Mostrar borde de color según rareza del item
- Usar `RarityColorConfig` para obtener color

---

## Diagrama de Flujo

```
Enemy muere (Die())
        ↓
Spawn coins (goldReward / 10)
Spawn orb (experienceReward)
        ↓
WorldItem flota/rotación
        ↓
Jugador se acerca (trigger)
        ↓
Auto-recolección
        ↓
Aplicar efecto:
  - Oro → InventoryComponent
  - XP → HeroEntity.AddExperience()
```

---

## Archivos Afectados

### Nuevos (5):
1. `Assets/_Project/Scripts/Inventory/ItemRarity.cs`
2. `Assets/_Project/Scripts/Inventory/RarityColorConfig.cs`
3. `Assets/_Project/Scripts/Inventory/WorldItem.cs`
4. `Assets/_Project/Prefabs/WorldItem.prefab`
5. Assets de ScriptableObjects (GoldCoin, ExperienceOrb, RarityColorConfig)

### Modificados (3):
1. `Assets/_Project/Scripts/Inventory/ItemData.cs` - Campo rarity
2. `Assets/_Project/Scripts/Core/EnemyEntity.cs` - Spawn de drops
3. `Assets/_Project/Scripts/UI/InventorySlotUI.cs` - Colores de rareza

---

## Ventajas de este Enfoque

✅ **Reutiliza configuración existente** - Usa `goldReward` y `experienceReward` ya configurados  
✅ **Integración gradual** - El sistema de inventario ya existe  
✅ **Visual inmediata** - Los drops físicos mejoran la experiencia  
✅ **Extensible** - Fácil agregar nuevos tipos de items después  
