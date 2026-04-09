# Memoria del Proyecto - MobaGameplay

## Convenciones Generales

### Namespace
- **Principal**: `MobaGameplay.*` para todo el proyecto
- **Excepción conocida (PENDIENTE FIX)**: 13 archivos usan `MMORPG.Inventory` y `MMORPG.UI` — falta migrar a `MobaGameplay.Inventory` y `MobaGameplay.UI.Inventory`

### Versión del Motor
- **Real**: Unity 6 (6000.3.11f1) con URP 17.3.0
- **Nota**: La documentación dice Unity 2022.3 LTS — está desactualizada

### Estructura de Carpetas
```
Assets/_Project/
  Scripts/
    Core/         # Entidades, combate, movimiento base
    Abilities/    # Habilidades, proyectiles, efectos de área
    AI/           # Controlador de IA enemiga (6 estados)
    Animation/    # CharacterAnimator, AnimationEventReceiver
    Camera/       # CameraController
    Combat/       # RangedCombat, MeleeCombat, DamageInfo
    Controllers/  # PlayerInputController (input principal)
    Editor/       # Scripts de editor (26 total, ~15 redundantes)
    Game/         # GameStateManager, WaveData, ItemDropInitializer
    Inventory/    # Sistema de inventario (MMORPG.Inventory)
    Movement/     # XZPlaneMovement
    Testing/      # EquipmentTester
    UI/           # Interfaces, barras, targeting, HUD, inventario
    VFX/          # SimpleVFX
    Visuals/      # LaserSight
  Prefabs/        # Player, Enemy, Abilities, Items, UI
  Scenes/         # SampleScene
  Art/            # Animations, Icons
  Materials/      # Ground, TrailIndicator, TrailZone
  Shaders/        # Outline, UIHealthBarTick
  ScriptableObjects/  # Items (42 equipables), Waves (3)
```

## Entidades Principales

### BaseEntity (318 líneas)
- Clase abstracta base para todos los personajes
- Maneja vida, maná, stats de combate, muerte/revive
- Eventos: `OnTakeDamage`, `OnDeath`, `OnManaChanged`
- **BUG CONOCIDO**: `TakeDamage()` aplica daño crítico 2 veces (doble resta)
- **BUG CONOCIDO**: `Update()` regenera mana sin pasar por setter (no dispara evento)

### HeroEntity (73 líneas)
- Extiende `BaseEntity`
- Sistema de progresión: nivel, experiencia, oro
- Métodos: `AddGold()`, `AddExp()`, `LevelUp()`
- **BUG CONOCIDO**: `Awake()` acopla a `TargetingManager.Instance`
- Campos públicos sin `[SerializeField]` ni propiedades

### EnemyEntity (162 líneas)
- Extiende `BaseEntity` con recompensas y AI automática
- `NotifyKillReward()` — ARREGLADO, otorga oro/XP y spawnea GoldDrop
- **BUG CONOCIDO**: `Die()` llama `Destroy(1f)` pero base llama `Destroy(3f)` → double destroy
- **BUG CONOCIDO**: Usa `new void Start()` en vez de `override`, oculta base

## Sistemas Core

### Combate
- `RangedCombat.cs` (182 líneas) — Charged attacks, cooldowns ✅
- `MeleeCombat.cs` (28 líneas) — Básico, código muerto comentado ⚠️
- `DamageInfo.cs` (29 líneas) — Struct con tipos y críticos ✅

### IA Enemiga (639 líneas)
- `EnemyAIController` — State machine: Idle→Patrol→Chase→Attack→Retreat→Dead
- `Physics.OverlapSphere` por frame para detectar jugador — rendimiento ⚠️
- `HandleDamageDealt()` vacío, nunca llamado — código muerto

### GameStateManager (630 líneas)
- Singleton con DontDestroyOnLoad
- Sistema de oleadas, respawn, puntuación
- **BUG CONOCIDO**: `OnWaveCleared` se disporta 2 veces (detección duplicada)

### GoldDrop (90 líneas)
- Se mueve hacia el jugador cuando está en rango (5u)
- Se recoge automáticamente al estar cerca (1.5u)
- **BUG CONOCIDO**: `OnCollected()` no llama `hero.AddGold()` — NO SUMA ORO
- Usa `FindObjectOfType<HeroEntity>()` en Start — rendimiento ⚠️

### Abilities
- `AbilityController` (277 líneas) — 4 slots, targeting, cooldowns
- `BaseAbility` (169 líneas) — Clase abstracta con cooldown, mana, targeting
- 4 habilidades implementadas: Fireball, Dash, GroundSmash, GroundTrail
- `Projectile.cs` base tiene daño COMENTADO — no aplica daño por sí misma
- `LinearProjectile` y `BasicAttackProjectile` sí aplican daño correctamente

### EquipmentComponent (210 líneas)
- Calcula stats de items equipados y aplica al héroe
- **BUG CONOCIDO**: `ApplyStatsToOwner()` usa `+=` en vez de `=` → acumula al re-equipar
- **BUG CONOCIDO**: `_baseMaxHealth` se captura en `Awake()` — no se actualiza con level-ups
- AGI bonus comentado (línea 121): `// _owner.AttackSpeed += agi * 0.1f;`

## Bugs Críticos — ARREGLADOS (2026-04-09, Fase 1)

1. ✅ **Daño crítico duplicado** — multiplicador ANTES de restar vida, una sola resta
2. ✅ **GoldDrop no suma oro** — agregado `goldAmount` y `_hero.AddGold()`
3. ✅ **Double Destroy** — eliminado `Destroy(1f)` de EnemyEntity, solo base `Destroy(3f)`
4. ✅ **Wave clear duplicado** — lógica duplicada eliminada de `UpdateActiveWave()`
5. ✅ **Equipment acumula stats** — `+=` → `=` en `ApplyStatsToOwner()`
6. ✅ **Mana regen sin evento** — `currentMana` → `CurrentMana` (usa setter)
7. ✅ **EnemyEntity.Start() oculta base** — `new void Start()` → `protected override`

## Deuda Técnica — ARREGLADA (2026-04-09, Fases 2-3)

8. ✅ **Equipment + Level-up** — `EquipmentComponent` se suscribe a `OnLevelUp`, `RefreshBaseStats()` calcula valores reales
9. ✅ **Namespace MMORPG** — 20 archivos migrados a `MobaGameplay.*`
10. ✅ **Projectile.cs daño comentado** — descomentado y corregido con `DamageInfo`
11. ✅ **Reflexión en AbilityController** — reemplazado con propiedades públicas
12. ✅ **5 editor scripts eliminados** — FixAbilityIcons, AttachMeleeCombat, FixEventSystem, GroundItemPrefabCreator, CleanupPlayerAbilities
13. ✅ **Código muerto limpiado** — MeleeCombat, EnemyAIController.HandleDamageDealt, EquipmentComponent AGI TODO
14. ✅ **8 archivos basura eliminados** — cookies.txt, mcp files, scripts Python, Django template
15. ✅ **README.md actualizado** — Unity 6 (6000.3), URP 17.3

## Bugs Arreglados (2026-04-08)

1. ✅ `EnemyEntity.NotifyKillReward()` — implementado, da oro/XP y spawnea drop
2. ✅ `AoEZone.cs` — daño descomentado y funcional
3. ✅ `EquipmentComponent` — stats aplicados al héroe (con bug de acumulación)
4. ✅ `GoldDrop` — recolección funciona por distancia (sin depender de collider del jugador)
5. ✅ `GroundTrailAbility` — TrailZone con BoxCollider trigger y OverlapBox de respaldo
6. ✅ FloatingStatusBar — chip damage verde fijo, tick marks shader
7. ✅ Ability icons — asignados por AutoFixAbilityIcons (usa reflexión)
8. ✅ GroundTrailAbility — asignada al slot 4 del prefab del jugador

## Notas Técnicas
- El jugador no tiene Collider en su prefab base — GoldDrop usa detección por distancia
- TrailZone requiere BoxCollider con isTrigger=true
- El proyecto usa Unity 6 (6000.3.11f1), NO Unity 2022.3 LTS
- Namespace unificado: todo usa `MobaGameplay.*` (antes `MMORPG.*` migrado)
- 21 editor scripts restantes (5 eliminados en limpieza)
- 0 tests unitarios
- MCP para Unity configurado pero con problemas de sesión HTTP (solo SSE continuo)
- Projectile.cs (clase base) ahora aplica daño correctamente
- AbilityController ya no usa reflexión — usa propiedades públicas

## Última actualización
2026-04-09 — Fases 1-3 completadas: 7 bugs críticos + deuda técnica resuelta