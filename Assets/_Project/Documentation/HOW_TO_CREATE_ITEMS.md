# Cómo Crear Items en Mobalike

> **Nota:** Este documento describe el sistema de items del proyecto Mobalike (MOBA, no MMORPG).

---

## 1. Crear un ItemData

### Paso 1: Crear el archivo

1. En Project window, navegar a `Assets/Resources/ScriptableObjects/Items/Equipment/`
2. Right-click → **Create > MobaGameplay > Item Data**
3. Nombrar el archivo (ej: `MyNewItem.asset`)

### Paso 2: Configurar el ItemData

Seleccionar el asset y configurar en el Inspector:

| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| **itemName** | Nombre del item | "Espada de Fuego" |
| **itemType** | `Consumable` o `Equipment` | `Equipment` |
| **equipSlot** | Slot de equipo (solo si es Equipment) | `Weapon` |
| **icon** | Sprite del ícono | Arrastrar Sprite |
| **hpBonus** | Bonus de HP | 100 |
| **strBonus** | Bonus de Strength | 25 |
| **agiBonus** | Bonus de Agility | 10 |

### Slots de Equipamiento Disponibles

```csharp
public enum EquipSlot 
{ 
    None, 
    Head, 
    Chest, 
    Weapon, 
    Boots,
    Pants
}
```

---

## 2. Sistema de Equipamiento

Los items de tipo `Equipment` pueden equiparse al jugador a través del `EquipmentComponent`.

### Cómo funciona

1. `EquipmentComponent` mantiene un dictionary de items equipados por `EquipSlot`
2. Cuando se equipa un item, los stats se aplican al dueño via `ApplyStatsToOwner()`
3. Los stats aplicados son: `hpBonus`, `strBonus`, `agiBonus`

### Equipar via código

```csharp
// Obtener referencia al EquipmentComponent del jugador
var equipment = player.GetComponent<EquipmentComponent>();

// Crear y configurar el item
var itemData = Resources.Load<ItemData>("ScriptableObjects/Items/Equipment/MyNewItem");

// Equipar
equipment.EquipItem(itemData);

// Desequipar
equipment.UnequipItem(EquipSlot.Weapon);
```

---

## 3. Agregar Items al Inventory (UI)

Para agregar un item físico a un slot de inventario en la UI:

1. Localizar el HUD/Canvas con el Inventory Grid en la escena o prefab
2. Expandir el Inventory Grid en la Hierarchy para ver los slots (`Slot_0`, `Slot_1`, etc.)
3. Seleccionar un slot y expandir para encontrar el objeto `Icon` o `ItemImage`
4. En el componente `Image`, asignar el Sprite del item
5. En el componente `DraggableItemUI` del slot, asignar el `ItemData` en el campo **Item Data**

---

## 4. Agregar un Nuevo EquipSlot

Si necesitás agregar un nuevo slot de equipamiento:

1. Editar `Assets/_Project/Scripts/Inventory/ItemData.cs`
2. Agregar al enum `EquipSlot`:

```csharp
public enum EquipSlot 
{ 
    None, 
    Head, 
    Chest, 
    Weapon, 
    Boots,
    Pants,
    // Agregar nuevo slot aquí
    Ring  // ejemplo
}
```

3. Opcional: Crear el slot visual en el prefab `PlayerHUD.prefab`

---

## 5. Items Existentes (referencia)

```
Assets/Resources/ScriptableObjects/Items/Equipment/
├── BubbleFrog.asset
├── BurningPalm.asset
├── ChronoGauge.asset
├── CorruptedPummeler.asset
├── Duality.asset
├── FeytechClaw.asset
├── ... (80+ items más)
```

---

## Notas

- Los items se cargan desde `Resources/` via `Resources.Load<ItemData>(path)`
- Los stats de equipment se aplican automáticamente cuando se equipan
- No hay sistema de rareza implementado todavía (ver `plan.md` para loot system)
