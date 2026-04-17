# Roadmap - MobaGameplay

> Estado del proyecto y plan de trabajo — Actualizado 2026-04-11

---

## 📊 Estado Actual

### ✅ Sistemas Completamente Funcionales

| Sistema | Estado | Archivos Clave |
|---------|--------|----------------|
| **Combate** | ✅ Completo | `RangedCombat` con charged attack, `DamageInfo` con tipos y críticos |
| **Entidades** | ✅ Completo | `BaseEntity`, `HeroEntity`, `EnemyEntity` con muerte/revive/progresión |
| **Movimiento** | ✅ Completo | `XZPlaneMovement` (WASD, sprint, dash, gravedad) |
| **Input** | ✅ Completo | `PlayerInputController` completo (622 líneas) |
| **IA Enemiga** | ✅ Completo | `EnemyAIController` con 6 estados: Idle→Patrol→Chase→Attack→Retreat→Dead |
| **Oleadas** | ✅ Completo | `GameStateManager` con sistema de oleadas Idle→Victory, respawn, scoring |
| **UI Floating Text** | ✅ Completo | `FloatingDamageText` con física, pop, colores por tipo de daño |
| **UI Status Bar** | ✅ Completo | `FloatingStatusBar` con chip damage estilo LoL + tick marks shader |
| **Targeting** | ✅ Completo | Indicadores de círculo, línea, trail + `HoverOutline` |
| **Habilidades Base** | ✅ Completo | `AbilityController` con 4 slots, targeting y ejecución |
| **Habilidades Específicas** | ✅ Completo | Fireball, Dash, GroundSmash, GroundTrail — las 4 funcionan |
| **Recompensas** | ✅ Completo | `NotifyKillReward()` otorga oro/XP al héroe + spawnea `GoldDrop` |
| **AoEZone** | ✅ Completo | Daño activo con `OverlapSphere` y `DamageType.Magical` |
| **TrailZone** | ✅ Completo | DoT con `OverlapBox` de respaldo + `OnTriggerEnter/Exit` |
| **Equipo** | ✅ Conectado | `EquipmentComponent.ApplyStatsToOwner()` aplica HP y STR al héroe |
| **Shader Ticks** | ✅ Completo | `UIHealthBarTick.shader` funcional con intervalos configurables |
| **Colisión/Death** | ✅ Completo | Hurtboxes estandarizados, `Die()` desactiva colliders y movimiento |
| **Íconos Abilities** | ✅ Completo | `AutoFixAbilityIcons()` en `AbilityController` (runtime + ContextMenu) |

### 🔧 Sistemas con Bugs Menores Pendientes

| Sistema | Problema | Severidad |
|---------|----------|-----------|
| **GoldDrop rendimiento** | Cada `GoldDrop` llama `FindObjectOfType<HeroEntity>()` en Start | 🟡 Medio |

---

## 🔴 Bugs Críticos (DIAGNÓSTICO 2026-04-09)

### ✅ ARREGLADOS (Fase 1) — Completado 2026-04-09

| Bug | Archivo | Fix |
|-----|---------|-----|
| Daño crítico duplicado (~2.5x en vez de 1.5x) | `BaseEntity.cs` | Multiplicador antes de restar, una sola resta |
| GoldDrop no suma oro | `GoldDrop.cs` | Agregado `goldAmount` + `AddGold()` |
| Double Destroy (1s + 3s) | `EnemyEntity.cs` | Eliminado `Destroy(1f)`, solo base(3f) |
| Wave clear dispara 2 veces | `GameStateManager.cs` | Lógica duplicada eliminada de `UpdateActiveWave()` |
| Equipment stats acumulan | `EquipmentComponent.cs` | `+=` → `=` en `ApplyStatsToOwner()` |
| Mana regen sin evento | `BaseEntity.cs` | `currentMana` → `CurrentMana` (usa setter) |
| EnemyEntity.Start() oculta base | `EnemyEntity.cs` | `new void Start()` → `protected override void Start()` |

### ✅ ARREGLADOS (Fase 2) — Completado 2026-04-09

| Bug | Archivo | Fix |
|-----|---------|-----|
| Equipment pierde stats al subir nivel | `EquipmentComponent.cs` | `RefreshBaseStats()` + suscripción a `OnLevelUp` |
| Namespace MMORPG inconsistente (13→20 archivos) | Inventory + UI | `MMORPG.*` → `MobaGameplay.*` |
| Projectile.cs daño comentado | `Projectile.cs` | Descomentado `TakeDamage` + check `!hitEntity.IsDead` |

### ✅ ARREGLADOS (Fase 3) — Completado 2026-04-09

| Bug | Archivo | Fix |
|-----|---------|-----|
| Editor scripts redundantes (15) | `Editor/` | Eliminados 5 scripts one-time/duplicados |
| AbilityController usa reflexión | `AbilityController.cs` | Reemplazado por propiedades públicas |
| Código muerto | Varios | Removido código comentado, métodos vacíos |
| Archivos basura en raíz | Raíz | Eliminados cookies, MCP logs, scripts Python, Django template |
| README desactualizado | `README.md` | Unity 2022.3 → Unity 6 (6000.3), URP 14 → 17.3 |

### ✅ ARREGLADOS (Post-Fase 3, Bugfixes adicionales)

| Bug | Archivo | Fix |
|-----|---------|-----|
| Íconos de abilities como gray boxes | `AbilityController.cs` | `AutoFixAbilityIcons()` en Awake + ContextMenu |
| HUD duplicate objects en escena | Escena | Removidos `PlayerHUD` duplicados |
| PlayerHUD late binding null | `PlayerHUD.cs` | `TryBindPlayer()` en Update con fallback por tag |
| Floating health bar no actualiza | `FloatingStatusBar.cs` | Auto-wire en Awake + `RefreshBars()` en Update |
| Proyectiles pasan por enemigos | `RangedCombat.cs` | `hitLayers` inicializado correctamente |
| Quick Cast tap no funciona | `PlayerInputController.cs` | `else if` → `if` independiente para release |

### ✅ ARREGLADOS (Cooldown Bugfix - 2026-04-10)

| Bug | Archivo | Fix |
|----|---------|-----|
| Cooldown overlay y texto no se muestran | `AbilitySlotUI.cs` | `Awake()` auto-busca hijos por nombre + llamada inmediata a `UpdateNewAbility()` |
| Referencias UI nulas en runtime | `AbilitySlotUI.cs` | Auto-find de `cooldownOverlay` y `cooldownText` por nombre |

---

## 🎯 Roadmap de Trabajo

### Fase 1: Bugs Críticos ✅ COMPLETADO (2026-04-09)

### Fase 2: Bugs Medios y Deuda Técnica ✅ COMPLETADO (2026-04-09)

### Fase 3: Polish y Limpieza ✅ COMPLETADO (2026-04-09)

### Fase 4: Tests Unitarios ✅ COMPLETADO (2026-04-10)

**Tiempo real: ~4 horas**

- ✅ **Configurar assembly definitions** — 3 asmdef: `MobaGameplay.Runtime`, `MobaGameplay.Editor`, `MobaGameplay.Tests`
- ✅ **Referencias de paquetes** — `Unity.InputSystem` y `Unity.TextMeshPro` en Runtime y Editor asmdef
- ✅ **Eliminar asmdef huérfano** — removido de `Art/Icons/Abilities/Tests/`
- ✅ **Tests de BaseEntity** — 22 tests: TakeDamage, críticos, death, heal, mana, DamageInfo
- ✅ **Tests de HeroEntity** — 16 tests: AddGold, AddExp, LevelUp, stat scaling
- ✅ **Tests de EquipmentComponent** — 12 tests: equip/unequip, acumulación, level-up interaction
- ✅ **Tests de GameStateManager** — 12 tests: estados, transiciones, score, restart
- ✅ **Fix EditMode issues** — `manaInitialized` via reflection, `LogAssert.Expect` para `Destroy()`, `_owner` forzado via reflection en EquipmentComponent
- ✅ **Todos los 62 tests pasan** ✅

#### Lecciones aprendidas de EditMode Testing en Unity

| Problema | Causa | Solución |
|----------|-------|----------|
| `OnManaChanged` no dispara | `manaInitialized=false` (se setea en `Start()` que no corre) | Reflection para setear flag en SetUp |
| `Destroy()` error en EditMode | `Die()` llama `Destroy()` que no es válida en Edit Mode | `LogAssert.Expect(LogType.Error, ...)` |
| `_owner` null en EquipmentComponent | `Awake()` no inicializa correctamente en EditMode | Reflection para forzar `_owner` e invocar `OnEnable()` |
| `GameStateManager` singleton entre tests | `Instance` persiste entre SetUp/TearDown | Reflection para resetear singleton + cleanup en TearDown |
| Asmdef sin referencias a paquetes | Custom asmdef no incluye paquetes automáticamente | Agregar `Unity.InputSystem` y `Unity.TextMeshPro` a `references` |

### Bloque A: Encapsulación ✅ COMPLETADO (2026-04-10)

**Tiempo real: ~2 horas**

- ✅ **11 public fields → [SerializeField] private + properties** en HeroEntity
- ✅ **Habilidades encapsuladas** — FireballAbility, GroundSmashAbility, DashAbility
- ✅ **AoEZone, LinearProjectile** — encapsulados
- ✅ **HoverOutline, SimpleVFX, CircleIndicator, LineIndicator** — encapsulados
- ✅ **HeroBuilderSafe y VisualsBuilder** — actualizados para usar SetSerializedField helper
- ✅ **EnemyEntity** — usa property accessors de HoverOutline en vez de field directo
- ✅ **Todos los 62 tests pasan después del refactor** ✅

### Bloque B: Eliminar FindObjectOfType ✅ COMPLETADO (2026-04-17)

**Tiempo real: ~15 minutos**

- ✅ **GoldDrop.cs** — Ya usaba `HeroEntity.Instance` (migrado previamente)
- ✅ **EnemyEntity.cs** — Ya usaba `HeroEntity.Instance` (migrado previamente)
- ✅ **HeroEntity.cs** — Reemplazados 2x `FindObjectOfType<TargetingManager>()` por `TargetingManager.Instance`
- ✅ **Eliminado hack de reflection** en `Start()` — ya no se necesita verificar `playerTransform` por reflection
- ✅ **Eliminada duplicación** — `Awake()` ya no llama a TargetingManager (se hace una sola vez en `Start()`)
- ✅ **63 tests pasan** ✅

#### Cambios en HeroEntity.cs

| Antes | Después |
|-------|---------|
| `FindObjectOfType<TargetingManager>()` en Awake | Removido (Instance puede no existir en Awake) |
| `FindObjectOfType<TargetingManager>()` en Start | `TargetingManager.Instance` |
| Reflection hack para verificar `playerTransform` | Removido — `Initialize()` es idempotente |

### Bloque C: Extraer Magic Numbers ⏳ Pendiente

- [ ] Reemplazar magic numbers por `[SerializeField] private` con nombres descriptivos
- [ ] Priorizar: daño, velocidades, rangos, cooldowns

### Fase 5: Nuevas Habilidades

**Tiempo estimado: 2-4 horas**

- [ ] Crear 2-3 habilidades adicionales:
  - Heal/Shield (habilidad de soporte)
  - Multi-shot (ataque en cono)
  - Buff de velocidad/ataque

### Fase 6: Balance y Testing Final

**Tiempo estimado: 1-2 horas**

- [ ] Ajustar números de daño, velocidades, cooldowns
- [ ] Testing completo de inicio a fin (victoria/derrota)
- [ ] Verificar edge cases (muerte simultánea, level-up durante equipo, etc.)

### Fase 7: Sistema de Animaciones

**Tiempo estimado: 3-4 horas**

#### Estado Actual del Sistema de Animaciones

| Componente | Estado | Notas |
|------------|--------|-------|
| **CharacterAnimator.cs** | ⚠️ Básico | 80 líneas, solo Attack trigger conectado |
| **Controller** | ⚠️ Limitado | StarterAssetsThirdPerson con solo 1 animación (Hook Punch) |
| **Animaciones disponibles** | 🔴 Sin usar | warriorSlash, warriorCasting, warriorPowerUp, warriorBlock existen en proyecto |

#### Parámetros Actuales del Animator

```
├── Speed (Float) - velocidad de movimiento
├── MotionSpeed (Float) - siempre 1
├── Grounded (Bool) - si está en suelo
├── Jump (Bool) - si está saltando
├── FreeFall (Bool) - si está cayendo
└── Attack (Trigger) - ataque básico
```

#### Sprint 1: Conectar Existentes (30 min)

- [ ] Conectar `warriorSlash` al trigger **Attack** (Falta asignar state/clip en Unity)
- [x] Crear parámetro **IsCharging** (bool) para charged attack
- [x] Agregar **IsAiming** (bool) para cuando apunta con RMB
- [ ] Testear attack animation

#### Sprint 2: Dash y Muerte (1 hora)

- [x] Agregar evento **OnDashStart** en XZPlaneMovement
- [ ] Crear/animar animación de Dash (usar `warriorPowerUp` acelerado)
- [x] Conectar **OnDeath** event → Death animation (Trigger programado)
- [ ] Crear animación de muerte (buscar/crear)
- [ ] Testear ambos

#### Sprint 3: Habilidades (1 hora)

- [x] Agregar evento **OnAbilityCast** en AbilitySystem (ya existe OnAbilityExecuted y fue conectado)
- [ ] Conectar `warriorCasting` a habilidades (Q, W, E, R)
- [ ] Crear diferenciación visual entre habilidades si es necesario
- [ ] Testear casting animations

#### Sprint 4: Hit Reaction y Polish (30 min)

- [x] Agregar evento **OnTakeDamage** → HitReaction trigger (Trigger Hit programado)
- [ ] Crear animación de stagger/hit
- [ ] Ajustar blend times y transiciones
- [ ] Test completo de todas las animaciones

#### Animaciones a Implementar

| # | Animación | Trigger | Archivo Origen |
|---|-----------|---------|----------------|
| 1 | **Dash** | Space + Dash | warriorPowerUp (acelerado) |
| 2 | **Charged Attack** | LMB hold + RMB | warriorSlash (slower) |
| 3 | **Death** | OnDeath | Nueva (buscar/crear) |
| 4 | **Hit Reaction** | OnTakeDamage | Nueva (buscar/crear) |
| 5 | **Fireball Cast (Q)** | Ability Q | warriorCasting |
| 6 | **Ground Smash Cast (W)** | Ability W | warriorCasting |
| 7 | **Dash Cast (E)** | Ability E | warriorCasting |
| 8 | **Ground Trail Cast (R)** | Ability R | warriorCasting |

#### Cambios Requeridos en Código

**1. CharacterAnimator.cs - Nuevos eventos:**
```csharp
entity.Combat.OnAbilityCast += TriggerAbilityAnimation;
entity.Movement.OnDashStart += TriggerDashAnimation;
entity.OnTakeDamage += TriggerHitAnimation;
entity.OnDeath += TriggerDeathAnimation;
```

**2. Nuevos parámetros del Animator:**
```
├── IsDashing (bool)
├── IsCasting (bool)
├── IsDead (bool)
├── AbilityIndex (int) // 0-3 para Q,W,E,R
└── HitReaction (trigger)
```

---

## 🏗️ Plan Arquitectónico (Features adicionales)

Features pedidas por el usuario, planIFICadas pero no implementadas:

| Block | Feature | Estado |
|-------|---------|--------|
| Block 1 | Floating Health/Mana Bars (LoL style) | ✅ Implementado |
| Block 2 | Floating Damage Text (pop-ups de daño) | ✅ Implementado |
| Block 3 | Ammo/Reload para ataques básicos | ⏳ Pendiente |
| Block 4 | Dynamic Dash con colisiones + Impact VFX | ⏳ Pendiente |

**Nota:** Blocks 1 y 2 ya estaban implementados al momento del diagnóstico. Blocks 3 y 4 quedan como trabajo futuro.

---

## 📈 Métricas de Progreso

| Aspecto | Estado | Porcentaje |
|---------|--------|------------|
| **Sistemas Core** | Sólido, todos los bugs críticos arreglados | 95% |
| **UI/UX** | Completo, tick marks + floating text funcionales | 95% |
| **Combate** | Funcional, críticos y colisiones correctos | 95% |
| **Progresión** | GoldDrop suma oro, level-up + equipo correctos | 95% |
| **Inventario** | Stats se aplican sin acumulación (=) | 95% |
| **IA Enemiga** | Completa, 6 estados, Destroy arreglado | 95% |
| **Oleadas** | Wave clear sin duplicación | 95% |
| **Testing** | ✅ 62 tests pasan, asmdef configurado | 100% |
| **Deuda Técnica** | Namespace + editor scripts + FindObjectOfType eliminado | 95% |
| **Animaciones** | Fase 7planificada, 4 sprints definidos | 15% |

---

## 🚀 Estado General

**Veredicto**: El proyecto está en estado **avanzado de prototipo funcional con tests verdes**. Todas las Fases 1-4 y Bloques A-B están COMPLETADOS. Los 63 tests unitarios pasan correctamente. Bloque C (magic numbers), Fase 5 (nuevas habilidades), Fase 6 (balance final) y Fase 7 (animaciones) quedan como trabajo futuro.

**Próximo paso**: Bloque C (magic numbers), Fase 5 (nuevas habilidades), Fase 6 (balance) o Fase 7 (animaciones), según prioridad.

---

## 📝 Notas Técnicas

### Assembly Definitions (agregado Fase 4)

| Archivo | Assembly | Propósito |
|---------|----------|-----------|
| `_Project/Scripts/MobaGameplay.Runtime.asmdef` | MobaGameplay.Runtime | Todo el código del juego |
| `_Project/Scripts/Editor/MobaGameplay.Editor.asmdef` | MobaGameplay.Editor | Herramientas Editor (Editor-only) |
| `Tests/MobaGameplay.Tests.asmdef` | MobaGameplay.Tests | Tests unitarios (referencia Runtime + NUnit) |

### Versión del motor
- **Real**: Unity 6 (6000.3.11f1) con URP 17.3.0

### Estadísticas del proyecto
- **~88 archivos C#** en `_Project/Scripts/` (~389 KB)
- **4 archivos de test** en `Assets/Tests/` — **63 tests, todos pasando ✅**
- **42 ItemData** ScriptableObjects de equipo
- **3 WaveData** ScriptableObjects configurados

### Discoveries importantes (lecciones aprendidas)
- **Input System**: `else if (wasReleasedThisFrame)` rompe Quick Cast al hacer tap → usar `if` independientes
- **LayerMask**: `hitLayers` por defecto es `0` (Nothing) y falla silenciosamente → siempre inicializar con `~0` o bitmask
- **Projectiles**: Spawn a `Y = 1.0f`, dirección `dir.y = 0` para intersectar hurtboxes estándar
- **MCP bug**: `execute_code` recibe strings como null → workaround: ContextMenu o runtime
- **Equipment stats**: Usar `=` no `+=` para evitar acumulación al re-equipar
- **Critical damage**: Multiplicador ANTES de restar vida, una sola resta
- **Asmdef references**: Custom asmdef NO incluye paquetes automáticamente. Namespace C# ≠ assembly name (`UnityEngine.InputSystem` ≠ `Unity.InputSystem`)
- **EditMode tests**: `Start()` no ejecuta → flags como `manaInitialized` se quedan en `false` → usar reflection
- **EditMode tests**: `Destroy()` no funciona en Edit Mode → usar `LogAssert.Expect(LogType.Error, ...)`
- **EditMode tests**: `Awake()` puede no inicializar correctamente → forzar `_owner` y `OnEnable()` via reflection
- **EditMode tests**: `GameStateManager` singleton persiste entre tests → resetear via reflection en TearDown

---

*Última actualización: 2026-04-17 — Bloque B (Eliminar FindObjectOfType) completado ✅*