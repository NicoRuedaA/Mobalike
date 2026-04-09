# Roadmap - MobaGameplay

> Estado del proyecto y plan de trabajo — Basado en diagnóstico completo del 2026-04-09

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

### ⚠️ Sistemas con Problemas Conocidos

| Sistema | Problema | Severidad |
|---------|----------|-----------|
| **Daño Crítico** | `BaseEntity.TakeDamage()` aplica daño crítico 2 veces (doble resta) | 🔴 Crítico |
| **GoldDrop** | `OnCollected()` NO suma oro al héroe — solo se destruye | 🔴 Crítico |
| **Double Destroy** | `EnemyEntity.Die()` llama `Destroy(1f)` pero `BaseEntity.Die()` ya llama `Destroy(3f)` | 🔴 Crítico |
| **Wave Clear Duplicado** | `GameStateManager` dispara `OnWaveCleared` + score bonus 2 veces | 🔴 Crítico |
| **Equipment Stats** | `ApplyStatsToOwner()` usa `+=` en vez de `=`, acumula al re-equipar | 🔴 Crítico |
| **Equipment + Level-up** | `_baseMaxHealth` no se actualiza al subir nivel, pierde stats | 🟡 Alto |
| **Namespace MMORPG** | 13 archivos usan `MMORPG.*` en vez de `MobaGameplay.*` | 🟡 Alto |
| **Mana Regen sin evento** | `BaseEntity.Update()` modifica `currentMana` directo, sin disparar `OnManaChanged` | 🟡 Medio |
| **EnemyEntity.Start()** | Usa `new void Start()` que oculta el `Start()` de `BaseEntity` | 🟡 Medio |
| **GoldDrop rendimiento** | Cada `GoldDrop` llama `FindObjectOfType<HeroEntity>()` en Start | 🟡 Medio |
| **Projectile.cs base** | Línea de daño comentada — la clase base NO aplica daño | 🟡 Medio |
| **Editor Scripts** | ~15 de 26 scripts son one-time/duplicados | 🟢 Bajo |

---

## 🔴 Bugs Críticos (DIAGNÓSTICO 2026-04-09)

### ✅ ARREGLADOS (Fase 1)

| Bug | Archivo | Fix |
|-----|---------|-----|
| Daño crítico duplicado (~2.5x en vez de 1.5x) | `BaseEntity.cs` | Multiplicador antes de restar, una sola resta |
| GoldDrop no suma oro | `GoldDrop.cs` | Agregado `goldAmount` + `AddGold()` |
| Double Destroy (1s + 3s) | `EnemyEntity.cs` | Eliminado `Destroy(1f)`, solo base(3f) |
| Wave clear dispara 2 veces | `GameStateManager.cs` | Lógica duplicada eliminada de `UpdateActiveWave()` |
| Equipment stats acumulan | `EquipmentComponent.cs` | `+=` → `=` en `ApplyStatsToOwner()` |
| Mana regen sin evento | `BaseEntity.cs` | `currentMana` → `CurrentMana` (usa setter) |
| EnemyEntity.Start() oculta base | `EnemyEntity.cs` | `new void Start()` → `protected override void Start()` |

### ❌ PENDIENTES (Fase 2+)

| Bug | Archivo | Severidad |
|-----|---------|-----------|
| Equipment pierde stats al subir nivel | `EquipmentComponent.cs` `_baseMaxHealth` no se actualiza | 🟡 Alto |
| Projectile.cs base no aplica daño (comentado línea 58) | `Projectile.cs` | 🟡 Medio |
| Namespace MMORPG inconsistente (13 archivos) | Inventory + UI | 🟡 Alto |
| GoldDrop usa FindObjectOfType | `GoldDrop.cs` | 🟡 Medio |
| AbilityController usa reflexión para íconos | `AbilityController.cs` | 🟡 Medio |
| Editor scripts redundantes (~15 de 26) | `Editor/` | 🟢 Bajo |

---

## 🎯 Roadmap de Trabajo

### Fase 1: Bugs Críticos (URGENTE) ✅ COMPLETADO

**Tiempo estimado: 1-2 horas** | **Completado: 2026-04-09**

- [x] **Fix daño crítico duplicado** ✅
  - Archivo: `Assets/_Project/Scripts/Core/BaseEntity.cs`
  - Movido multiplicador de crítico ANTES de restar vida
  - Una sola resta: `currentHealth -= actualDamage;` después del cálculo
  - Bonus: Fix mana regen — cambiado `currentMana` por `CurrentMana` (dispara evento)

- [x] **Fix GoldDrop no suma oro** ✅
  - Archivo: `Assets/_Project/Scripts/Core/GoldDrop.cs`
  - Agregado campo `[SerializeField] private int goldAmount = 5;`
  - Agregado `_hero.AddGold(goldAmount)` en `OnCollected()`

- [x] **Fix double Destroy en EnemyEntity** ✅
  - Archivo: `Assets/_Project/Scripts/Core/EnemyEntity.cs`
  - Eliminado `Destroy(gameObject, 1f)` — base ya llama `Destroy(3f)`
  - Cambiado `new void Start()` por `protected override void Start()` con `base.Start()`

- [x] **Fix wave clear duplicado** ✅
  - Archivo: `Assets/_Project/Scripts/Game/GameStateManager.cs`
  - Eliminada lógica duplicada de `UpdateActiveWave()`
  - `HandleEnemyDeath()` ahora es el único punto de transición + timer

- [x] **Fix Equipment stats se acumulan** ✅
  - Archivo: `Assets/_Project/Scripts/Inventory/EquipmentComponent.cs`
  - Cambiado `+=` por `=` en `ApplyStatsToOwner()`
  - Ahora: `_owner.MaxHealth = _baseMaxHealth + (hp * 10f);`

### Fase 2: Bugs Medios y Deuda Técnica ✅ COMPLETADO

**Tiempo estimado: 2-3 horas** | **Completado: 2026-04-09**

- [x] **Fix Equipment + Level-up** ✅
  - `EquipmentComponent` ahora se suscribe a `HeroEntity.OnLevelUp`
  - Agregado `RefreshBaseStats()` que calcula valores base reales (stripping equipment)
  - Stats se recalculan correctamente al subir nivel

- [x] **Unificar namespace MMORPG → MobaGameplay** ✅
  - 20 archivos migrados
  - `MMORPG.Inventory` → `MobaGameplay.Inventory`
  - `MMORPG.UI` → `MobaGameplay.UI.Inventory`
  - `MMORPG.EditorScripts/Editor` → `MobaGameplay.Editor`
  - MenuItems actualizados de `Tools/MMORPG/` a `Tools/MobaGameplay/`

- [x] **Fix Projectile.cs daño comentado** ✅
  - Descomentado `TakeDamage` en clase base `Projectile`
  - Agregado `using MobaGameplay.Combat` y `DamageInfo`
  - Agregado check de `!hitEntity.IsDead`

### Fase 3: Polish y Limpieza ✅ COMPLETADO

**Tiempo estimado: 2-3 horas** | **Completado: 2026-04-09**

- [x] **Limpiar editor scripts redundantes** ✅
  - Eliminados: `FixAbilityIcons.cs`, `AttachMeleeCombat.cs`, `FixEventSystem.cs`, `GroundItemPrefabCreator.cs`, `CleanupPlayerAbilities.cs`

- [x] **Reemplazar reflexión en AbilityController** ✅
  - `FixAbilityIcon()` ahora usa propiedades públicas (`ability.AbilityIcon`, `ability.abilityName`)
  - Eliminada reflexión sobre campos privados `_abilityIcon` y `_abilityName`

- [x] **Eliminar código muerto** ✅
  - `MeleeCombat.cs`: Removido código comentado
  - `EnemyAIController.cs`: Eliminado `HandleDamageDealt()` vacío
  - `EquipmentComponent.cs`: AGI TODO documentado

- [x] **Limpiar archivos basura en raíz** ✅
  - Eliminados: `cookies.txt`, `mcp_raw.txt`, `mcp_req.txt`, `mcp_request.json`
  - Eliminados: `parse_importer.cs`, `parse_scene.py`, `download_real_pngs.py`
  - Eliminado: `idea_inicial_plantilla.md` (template Django REST de otro proyecto)

- [x] **Actualizar documentación desactualizada** ✅
  - `README.md`: Unity 2022.3 → Unity 6 (6000.3), URP 14 → 17.3

### Fase 4: Tests Unitarios (NUEVO)

**Tiempo estimado: 3-4 horas**

Actualmente el proyecto tiene **0 tests**. Esto es urgente para la sostenibilidad.

- [ ] **Configurar test framework**
  - Verificar que `com.unity.test-framework` esté instalado
  - Crear carpeta `Assets/_Project/Tests/`

- [ ] **Tests de BaseEntity**
  - `TakeDamage` aplica daño correctamente
  - `TakeDamage` con crítico aplica multiplicador (una sola vez)
  - `Die` reduce vida a 0 y deshabilita componentes
  - `Heal` no excede MaxHealth
  - `RestoreMana` dispara `OnManaChanged`

- [ ] **Tests de HeroEntity**
  - `AddExp` incrementa experiencia
  - `LevelUp` incrementa stats correctamente
  - `AddGold` acumula correctamente

- [ ] **Tests de EquipmentComponent**
  - Equipar item suma stats
  - Desequipar item resta stats
  - Re-equipar mismo item no acumula
  - Stats de nivel NO se pierden al equipar

- [ ] **Tests de GameStateManager**
  - WaveClear se disporta una sola vez (regresión del Bug 4)
  - Respawn después de muerte funciona
  - Game Over cuando vidas = 0

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

---

## 📈 Métricas de Progreso

| Aspecto | Estado | Porcentaje |
|---------|--------|------------|
| **Sistemas Core** | Sólido, bugs críticos arreglados | 90% |
| **UI/UX** | Completo, tick marks funcionales | 95% |
| **Combate** | Funcional, críticos arreglados | 95% |
| **Progresión** | GoldDrop suma oro, recompensas OK | 90% |
| **Inventario** | Stats se aplican correctamente (fix accumulation) | 80% |
| **IA Enemiga** | Completa, 6 estados, Destroy arreglado | 95% |
| **Oleadas** | Wave clear sin duplicación | 95% |
| **Testing** | Inexistente | 0% |
| **Deuda Técnica** | Namespace + editor scripts + archivos basura | 55% |

---

## 🚀 Estado General

**Veredicto**: El proyecto está en estado **avanzado de prototipo funcional**. La arquitectura base es sólida (herencia, eventos, state machines), la mayoría de los sistemas están completos y conectados. Los **5 bugs críticos de la Fase 1 están ARREGLADOS** (daño crítico, GoldDrop, double destroy, wave clear, equipment accumulation). 

**Fase 1 completada ✅. Próximo paso: Fase 2 (bugs medios + deuda técnica) o Fase 3 (limpieza).**

---

## 📝 Notas Técnicas

### Namespace inconsistencia (13 archivos)
Los siguientes archivos usan `MMORPG.*` en vez de `MobaGameplay.*`:
- `ItemData.cs`, `InventoryComponent.cs`, `EquipmentComponent.cs`
- `ItemDropSystem.cs`, `GroundItem.cs`, `ItemPickupDetector.cs`
- `DraggableItemUI.cs`, `InventorySlotUI.cs`, `EquipmentSlotUI.cs`, `HUDManager.cs`
- `ItemDropInitializer.cs`, `SetupMMORPGInventory.cs`, `FixEquipmentLayout.cs`
- `BubbleFrogCreator.cs`, `AssignBubbleFrogIcon.cs`, `AddBubbleFrogToInventory.cs`, `MassItemGenerator.cs`
- `Tools/AssignDummyItemData.cs`

### Editor scripts redundantes (eliminar o mover)
- `FixAbilityIcons.cs` → reemplazado por `ForceFixAbilityIcons.cs`
- `AttachMeleeCombat.cs` → reemplazado por `AttachCombatAuto.cs`
- `FixEventSystem.cs` → reemplazado por `EnsureEventSystemAuto.cs`
- `GroundItemPrefabCreator.cs` → reemplazado por `MobaGameplaySetup.cs`

### Versión del motor
- **Real**: Unity 6 (6000.3.11f1) con URP 17.3.0
- **Documentado**: Unity 2022.3 LTS con URP 14.0.10 (DESPAREJADO)

### Estadísticas del proyecto
- **88 archivos C#** en `_Project/Scripts/` (~389 KB)
- **26 editor scripts** (~15 son one-time o duplicados)
- **42 ItemData** ScriptableObjects de equipo
- **3 WaveData** ScriptableObjects configurados
- **0 archivos de test**

---

*Última actualización: 2026-04-09 — Diagnóstico completo*