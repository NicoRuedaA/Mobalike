# ⚔️ Cómo Crear Items (MVC Architecture)

> **Actualizado:** 2026-04-13

**IMPORTANTE:** Todos los archivos deben estar dentro de `Assets/_Project/` o `Assets/Resources/` según el tipo.

---

## 🏗️ 1. Arquitectura (MVC)

El sistema de inventario usa el patrón **Model-View-Controller**:

*   **Model (Backend - `ItemData`):** ScriptableObject con los datos del item. Solo contiene stats y propiedades. No sabe nada de la UI.
*   **View/Controller (Frontend - `DraggableItemUI`):** Representación visual en el HUD y lógica de interacción. Lee del `ItemData` para mostrar el ícono.

---

## 🛠️ Step 1: Crear el ItemData

### Ubicación

Los items de equipo van en:
```
Assets/Resources/ScriptableObjects/Items/Equipment/
```

### Crear el asset

1. Navegar a `Assets/Resources/ScriptableObjects/Items/Equipment/`
2. Right-click → **Create > MobaGameplay > Item Data**
3. Nombrar el archivo (ej: `MyAwesomeSword.asset`)
4. Configurar en el Inspector:

| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| **itemName** | Nombre del item | "Espada de Fuego" |
| **itemType** | `Consumable` o `Equipment` | `Equipment` |
| **equipSlot** | Slot de equipo | `Weapon` |
| **icon** | Sprite del ícono | Arrastrar PNG |
| **hpBonus** | Bonus de HP | 100 |
| **strBonus** | Bonus de Strength | 25 |
| **agiBonus** | Bonus de Agility | 10 |

---

## 🖼️ Step 2: El Ícono (Sprite)

1. **Importar imagen:** PNG a `Assets/_Project/Art/Icons/Items/`
2. **Texture Type (CRÍTICO):** Seleccionar la imagen → Inspector → `Texture Type: Sprite (2D and UI)` → Apply

### Errores comunes

- **Evento de raycast bloqueado:** Al arrastrar un item, el item mismo puede bloquear el raycast. El `DraggableItemUI` debe disable `raycastTarget` en `OnBeginDrag`.

---

## 🖱️ Step 3: Crear la UI del Item

Para agregar un item a un slot de inventario en el HUD:

1. En el UI Canvas, crear un `GameObject > UI > Image` o usar un Item Prefab
2. Agregar el componente `DraggableItemUI` al GameObject
3. Asignar el `ItemData` al campo **Item Data** en el Inspector

### Bugs a evitar

- **EventSystem:** Si no funcionan los clicks/drags, verificar que existe un `EventSystem` con `InputSystemUIInputModule` (no el legacy `StandaloneInputModule`)

---

## ⚙️ Editor Scripting Gotchas

Si escribís herramientas de Editor para importar items:

1. **WebP Bug:** Al hacer scraping de wikis, no usar el `src` directo de `<img>`. Frecuentemente sirven `.webp` con extensión `.png`.
   - **Fix:** Usar la API del sitio (ej: `api.php?action=query&prop=imageinfo`)

2. **AssetDatabase Sprite Cache:** `LoadAssetAtPath<Sprite>()` puede retornar `null` si se llama en el mismo frame que la importación.
   - **Fix:** Usar `AssetDatabase.LoadAllAssetsAtPath()` y filtrar por tipo

---

## 📁 Estructura de Items

```
Assets/
├── _Project/
│   ├── Art/Icons/Items/           # Sprites de items
│   └── Scripts/Inventory/         # ItemData.cs, EquipmentComponent.cs
└── Resources/
    └── ScriptableObjects/
        └── Items/
            └── Equipment/         # Los .asset de items
```
