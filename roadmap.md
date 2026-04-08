# Roadmap - MobaGameplay

> Estado del proyecto y plan de trabajo

---

## 📊 Estado Actual

### ✅ Sistemas Completamente Funcionales (95%)

| Sistema | Estado | Archivos Clave |
|---------|--------|----------------|
| **Combate** | Completo | RangedCombat con charged attack, DamageInfo con tipos y críticos |
| **Entidades** | Completo | BaseEntity, HeroEntity, EnemyEntity con progresión de niveles |
| **Movimiento** | Completo | XZPlaneMovement (WASD, sprint, dash, gravedad) |
| **Input** | Completo | PlayerInputController completo (621 líneas) |
| **IA Enemiga** | Completo | EnemyAIController con 6 estados (638 líneas) |
| **Oleadas** | Completo | GameStateManager con sistema de oleadas Idle→Victory |
| **UI Floating Text** | Completo | FloatingDamageText con física, pop, críticos |
| **UI Status Bar** | Completo | FloatingStatusBar con shader de tick marks (536 líneas) |
| **Targeting** | Completo | Indicadores de círculo, línea, trail + HoverOutline |
| **Habilidades Base** | Completo | AbilityController con 4 slots, targeting y ejecución |
| **Shader Ticks** | Completo | UIHealthBarTick.shader funcional |

### ⚠️ Sistemas Parcialmente Implementados

| Sistema | Problema |
|---------|----------|
| **Habilidades Específicas** | Solo 4 implementadas (Fireball, Dash, GroundSmash, Trail) |
| **Recompensas** | `EnemyEntity.NotifyKillReward()` está **vacío** — no da oro/XP |
| **AoEZone** | Línea de daño comentada (`//hitEntity.TakeDamage(damage)`) |
| **Equipment** | Sistema desconectado — stats no aplican al héroe |
| **Colores HealthBar** | `GetHealthColor()` siempre retorna `healthyColor` |

### ❌ Bugs Críticos Encontrados

| Archivo | Línea | Problema | Impacto |
|---------|-------|----------|---------|
| `EnemyEntity.cs` | 105-114 | No otorga recompensas al matar | **Alto** — progresión rota |
| `AoEZone.cs` | 59-60 | Daño comentado | **Alto** — habilidades AoE no hacen daño |
| `AbilitySlotUI.cs` | 20 | Iconos no asignados | Medio — UI sin feedback visual |
| `EquipmentComponent` | Todo | Stats calculados pero no aplicados | Medio — sistema de items inútil |

---

## 🎯 Roadmap de Trabajo

### Fase 1: Arreglar lo Roto (Crítico)

**Tiempo estimado: 1-2 horas**

- [ ] **Conectar sistema de recompensas**
  - Archivo: `Assets/_Project/Scripts/Core/EnemyEntity.cs`
  - Activar `NotifyKillReward()` para dar oro/XP al matar enemigos
  - Conectar con `HeroEntity.AddGold()` y `HeroEntity.AddExp()`

- [ ] **Descomentar daño en AoE**
  - Archivo: `Assets/_Project/Scripts/Abilities/AreaEffects/AoEZone.cs` (línea 60)
  - Descomentar: `//hitEntity.TakeDamage(damage);`

- [ ] **Fix colores dinámicos del health bar**
  - Archivo: `Assets/_Project/Scripts/UI/FloatingStatusBar.cs` (línea 231-234)
  - Hacer que `GetHealthColor()` cambie según % de vida
  - <50% = warningColor, <25% = criticalColor

### Fase 2: Polish del UI

**Tiempo estimado: 30 minutos**

- [ ] **Probar y ajustar tick marks en escena**
  - Abrir escena principal
  - Seleccionar prefab `FloatingStatusBar` en enemigos
  - Ajustar parámetros en Inspector:
    - `Tick Interval`: cada cuántos % de vida sale una marca (ej: 10 = cada 10%)
    - `Tick Width`: grosor de las marcas (ej: 0.02)

- [ ] **Agregar iconos a habilidades**
  - Archivo: `Assets/_Project/Scripts/UI/AbilitySlotUI.cs`
  - Conectar íconos de ScriptableObjects a la UI
  - Mostrar en los 4 slots del HUD

### Fase 3: Sistema de Items (Opcional)

**Tiempo estimado: 2-3 horas**

- [ ] **Conectar EquipmentComponent**
  - Archivo: `Assets/_Project/Scripts/Inventory/EquipmentComponent.cs`
  - Hacer que items equipados modifiquen stats reales del héroe
  - Sincronizar con `BaseEntity` stats

- [ ] **Testear flujo completo de inventario**
  - Arrastrar items entre slots
  - Equipar y desequipar
  - Verificar que stats cambian

### Fase 4: Nuevas Habilidades (Mejora)

**Tiempo estimado: 2-4 horas**

- [ ] Crear 2-3 habilidades adicionales para demostrar versatilidad:
  - Heal/Shield (habilidad de soporte)
  - Multi-shot (ataque en cono)
  - Buff de velocidad/ataque

### Fase 5: Balance y Testing

**Tiempo estimado: 1-2 horas**

- [ ] **Ajustar números**
  - Daño del jugador y enemigos
  - Velocidades de movimiento
  - Cooldowns de habilidades

- [ ] **Testing completo**
  - Jugar de inicio a victoria/derrota
  - Verificar que todas las mecánicas funcionan
  - Buscar bugs de edge cases

---

## 📈 Métricas de Progreso

| Aspecto | Estado | Porcentaje |
|---------|--------|------------|
| **Sistemas Core** | Muy sólido | 95% |
| **UI/UX** | Excelente, faltan iconos | 90% |
| **Combate** | Funcional, recompensas rotas | 85% |
| **Progresión** | Estructura lista, conexiones faltantes | 70% |
| **Balance/Testing** | Necesita iteración | 40% |

---

## 🚀 Estado General

**Veredicto**: El proyecto está en estado **avanzado de desarrollo**. La arquitectura es sólida, la mayoría de los sistemas están completos y funcionales. Los problemas principales son conexiones faltantes entre sistemas (recompensas) y código comentado que debería estar activo.

**Con 1-2 sesiones de trabajo enfocadas en la Fase 1, el prototipo estaría completamente jugable de principio a fin.**

---

## 📝 Notas Técnicas

### Namespace inconsistente
- `MMORPG.Inventory` vs `MobaGameplay.*` — el sistema de inventario usa namespace diferente al resto del proyecto

### Código comentado pendiente
- Varios archivos tienen código comentado que debería estar activo:
  - `AoEZone.cs` línea 60: daño
  - `Projectile.cs` línea 59: daño

### Stats de equipo
- `EquipmentComponent` calcula stats pero no los aplica a `BaseEntity` — necesita conexión

---

*Última actualización: 2026-04-08*
