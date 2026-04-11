# Memoria del Proyecto - MobaGameplay

> Estado actual del proyecto — Actualizado 2026-04-11

---

## 📦 Estado del Proyecto

| Aspecto | Estado |
|----------|--------|
| **Versión Unity** | 6 (6000.3.11f1) con URP 17.3.0 |
| **Namespace** | Unificado: `MobaGameplay.*` |
| **Tests** | 62 tests pasando ✅ |
| **Assembly Definitions** | 3 asmdef: `MobaGameplay.Runtime`, `MobaGameplay.Editor`, `MobaGameplay.Tests` |

---

## 🏗️ Estructura de Carpetas

```
Assets/_Project/
├── Scripts/
│   ├── Core/           # BaseEntity, HeroEntity, EnemyEntity
│   ├── Abilities/      # Habilidades (legacy + data-driven)
│   │   ├── Core/       # AbilitySystem, AbilityData, AbilityInstance
│   │   ├── Behaviors/  # AbilityBehaviorFactory
│   │   └── Types/      # Projectile, AoE, Trail abilities
│   ├── AI/             # EnemyAIController (6 estados)
│   ├── Animation/      # CharacterAnimator
│   ├── Camera/        # CameraController
│   ├── Combat/        # RangedCombat, MeleeCombat, DamageInfo
│   ├── Controllers/   # PlayerInputController
│   ├── Editor/        # Herramientas editor
│   ├── Game/          # GameStateManager, ItemDropInitializer
│   ├── Inventory/     # EquipmentComponent
│   ├── Movement/      # XZPlaneMovement
│   ├── UI/            # HUD, AbilitySlotUI, ResourceBarUI, FloatingText
│   └── VFX/           # SimpleVFX
├── Prefabs/           # Player, Enemies, Abilities
├── Scenes/           # SampleScene
├── Art/               # Animations, Icons
├── Materials/         # Ground, TrailIndicator
├── Shaders/           # Outline, UIHealthBarTick
└── Data/              # AbilityData ScriptableObjects
```

---

## 🎮 Entidades Principales

### BaseEntity (318 líneas)
- **Responsabilidades**: Vida, maná, stats, muerte/revive
- **Eventos**: `OnTakeDamage`, `OnDeath`, `OnManaChanged`, `OnHealed`
- **Properties**: `CurrentHealth`, `CurrentMana`, `IsDead`

### HeroEntity (73 líneas)
- Extiende `BaseEntity`
- **Sistema de progresión**: nivel, experiencia, oro
- **Métodos**: `AddGold()`, `AddExp()`, `LevelUp()`

### EnemyEntity (162 líneas)
- Extiende `BaseEntity` con recompensas
- **IA automática**: `EnemyAIController` attached
- **Recompensas**: `goldReward`, `experienceReward`

---

## ⚔️ Sistemas Core

### Combate
| Sistema | Archivo | Estado |
|---------|---------|---------|
| Charged attacks | `RangedCombat.cs` | ✅ Completo |
| Melee attacks | `MeleeCombat.cs` | ✅ Completo |
| Damage info | `DamageInfo.cs` | ✅ Completo |

### Habilidades (Dual System)
| Sistema | Tipo | Estado |
|--------|------|--------|
| `AbilityController` | Legacy (MonoBehaviour) | ✅ Mantenido por compatibilidad |
| `AbilitySystem` | Nuevo (data-driven) | ✅ ACTIVO — usa AbilityData SOs |

**Habilidades implementadas (ambas versiones)**:
- Fireball (Projectile)
- Ground Smash (AoE)
- Dash (Movement)
- Ground Trail (Trail Zone)

### IA Enemiga
- **Estado**: 6 estados → Idle → Patrol → Chase → Attack → Retreat → Dead
- **Archivo**: `EnemyAIController.cs` (639 líneas)

### Game Loop
- **Archivo**: `GameStateManager.cs` (630 líneas)
- **Features**: Oleadas, respawn, puntuación, wave configs

---

## 🐛 Bugs Críticos — ARREGLADOS

### Fase 1 (2026-04-09)
| # | Bug | Fix |
|---|-----|------|
| 1 | Daño crítico duplicado (2.5x vs 1.5x) | Multiplicador ANTES de restar, una sola resta |
| 2 | GoldDrop no suma oro | Agregado `goldAmount` + `AddGold()` |
| 3 | Double Destroy (1s + 3s) | Eliminado `Destroy(1f)`, solo `Destroy(3f)` |
| 4 | Wave clear dispara 2 veces | Lógica duplicada eliminada |
| 5 | Equipment stats acumulan | `+=` → `=` en `ApplyStatsToOwner()` |
| 6 | Mana regen sin evento | `currentMana` → `CurrentMana` (setter) |
| 7 | EnemyEntity.Start() oculta base | `new void Start()` → `protected override` |

### Fase 2 (2026-04-09)
| # | Bug | Fix |
|---|-----|------|
| 8 | Equipment pierde stats al subir nivel | `RefreshBaseStats()` + suscripción a `OnLevelUp` |
| 9 | Namespace MMORPG inconsistente | `MMORPG.*` → `MobaGameplay.*` (20 archivos) |
| 10 | Projectile.cs daño comentado | Descomentado con check `!hitEntity.IsDead` |

### Fase 3 (2026-04-09)
| # | Bug | Fix |
|---|-----|------|
| 11 | Editor scripts redundantes | Eliminados 5 scripts one-time |
| 12 | AbilityController usa reflexión | Reemplazado por propiedades públicas |
| 13 | Código muerto | Removido métodos vacíos y comentarios |
| 14 | Archivos basura en raíz | Eliminados cookies, MCP logs, Python scripts |

### Post-Fase 3 (2026-04-10)
| # | Bug | Fix |
|---|-----|------|
| 15 | Íconos de abilities como gray boxes | `AutoFixAbilityIcons()` en Awake |
| 16 | HUD duplicate objects | Removidos duplicados de escena |
| 17 | PlayerHUD late binding null | `TryBindPlayer()` en Update con fallback por tag |
| 18 | Floating health bar no actualiza | Auto-wire en Awake + `RefreshBars()` en Update |
| 19 | Proyectiles pasan por enemigos | `hitLayers` inicializado correctamente |
| 20 | Quick Cast tap no funciona | `else if` → `if` independientes |

### Fase 4 - Bugfix Cooldown (2026-04-10)
| # | Bug | Fix |
|---|-----|------|
| 21 | Cooldown overlay no se muestra | `Awake()` auto-busca hijos + called a `UpdateNewAbility()` |

---

## 🧪 Testing (Fase 4)

**62 tests pasando** ✅

| Suite | Tests | Cobertura |
|-------|-------|-----------|
| BaseEntityTest | 22 | TakeDamage, críticos, death, heal, mana |
| HeroEntityTest | 16 | AddGold, AddExp, LevelUp, stat scaling |
| EquipmentComponentTest | 12 | equip/unequip, acumulación, level-up |
| GameStateManagerTest | 12 | estados, transiciones, score, restart |

### Lecciones de EditMode Testing
- `Start()` no ejecuta en EditMode → flags como `manaInitialized` quedan en `false`
- `Destroy()` no funciona → usar `LogAssert.Expect(LogType.Error, ...)`
- Singleton persiste entre tests → resetear via reflection en TearDown

---

## 📋 Documentación del Proyecto

| Archivo | Estado | Descripción |
|---------|--------|-------------|
| `roadmap.md` | ✅ Actualizado | Estado general y plan de fases |
| `memoria.md` | ✅ Actualizado | Convenciones y aprendizajes |
| `README.md` | ⚠️ Parcial | Input system (necesita actualización) |
| `MOBAGAMEPLAY_SETUP.md` | ✅ | Guía de setup |
| `HOW_TO_CREATE_ITEMS.md` | ✅ | Creación de items |
| `docs/DEPLOYMENT.md` | ⚠️ | Deployment (revisar) |

---

## 🔧 Configuración Técnica

### Assembly Definitions
| Archivo | Assembly | Propósito |
|---------|----------|-----------|
| `MobaGameplay.Runtime.asmdef` | MobaGameplay.Runtime | Todo el código del juego |
| `MobaGameplay.Editor.asmdef` | MobaGameplay.Editor | Herramientas Editor |
| `MobaGameplay.Tests.asmdef` | MobaGameplay.Tests | Tests (referencia Runtime + NUnit) |

### Paquetes Requeridos
- `Unity.InputSystem`
- `Unity.TextMeshPro`
- `com.unity.render-pipelines.universal` (URP 17.3.0)

---

## 📝 Notas Importantes

1. **Input System**: `else if (wasReleasedThisFrame)` rompe Quick Cast → usar `if` independientes
2. **LayerMask**: `hitLayers` por defecto es `0` (Nothing) → siempre inicializar con `~0`
3. **Projectiles**: Spawn a `Y = 1.0f`, dirección `dir.y = 0` para intersectar hurtboxes
4. **Equipment stats**: Usar `=` no `+=` para evitar acumulación
5. **Critical damage**: Multiplicador ANTES de restar vida, una sola resta
6. **Namespace ≠ Assembly**: `UnityEngine.InputSystem` ≠ `Unity.InputSystem`

---

## ���� Limpieza Realizada (2026-04-10)
- Eliminados objetos `HealthBackground` y `HealthRecentDamageFill` de la raíz de la escena
- Limpiados debug logs de `AbilitySlotUI.cs` y `PlayerHUD.cs`

---

## 📅 Último Update
**2026-04-11** — Documentación actualizada, 62 tests pasando ✅