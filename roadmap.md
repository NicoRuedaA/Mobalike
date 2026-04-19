# 🗺️ Mobalike — Development Roadmap

**Última actualización:** 2026-04-19  
**Estado general:** ~60-70% completo (prototipo single-player)

---

## 📊 Estado de Sistemas

### ✅ SISTEMAS COMPLETADOS (100%)

| Sistema | Estado | Archivos Clave | Notas |
|---------|--------|----------------|-------|
| **Entity Framework** | ✅ | `BaseEntity.cs`, `HeroEntity.cs`, `EnemyEntity.cs` | Base sólida con eventos |
| **Ability System** | ✅ | `AbilitySystem.cs`, `AbilityData.cs`, 5 behaviors | Data-driven con ScriptableObjects |
| **Sistema de Munición** | ✅ | `RangedCombat.cs`, `AmmoUI.cs` | Con carga, recarga y cancelación |
| **Movimiento** | ✅ | `XZPlaneMovement.cs` | Walk, sprint, dash, jump |
| **Input** | ✅ | `PlayerInputController.cs` | Unity Input System moderno |
| **Targeting** | ✅ | `TargetingManager.cs` | Circle, Line, Trail indicators |
| **Proyectiles** | ✅ | `LinearProjectile`, `HomingProjectile`, `BasicAttackProjectile` | Sistema completo |
| **Enemy AI** | ✅ | `EnemyAIController.cs` | State machine de 636 líneas |
| **Wave System** | ✅ | `GameStateManager.cs`, `WaveData.cs` | Spawning por oleadas |
| **Inventario** | ✅ | `InventoryComponent.cs` | 20 slots con drag-and-drop |
| **Equipamiento** | ✅ | `EquipmentComponent.cs` | 6 slots con stats |
| **Items** | ✅ | `ItemData.cs`, `ItemDropSystem.cs` | Drop system estático |
| **Animaciones** | ✅ | `CharacterAnimator.cs` | AnimatorOverrideController dinámico |
| **UI** | ✅ | `PlayerHUD.cs`, `HUDManager.cs`, `AbilitySlotUI.cs` | HUD completo |
| **Hero Class System** | ✅ | `HeroClass.cs` | ScriptableObject por héroe |
| **Oro/XP** | ✅ | Integrado en entidades | Con equipamiento y oro |
| **Respawn** | ✅ | `GameStateManager.cs` | Sistema de respawn automático |

---

## 🔴 SISTEMAS FALTANTES (CRÍTICOS)

| Sistema | Prioridad | Impacto | Esfuerzo | Estado |
|---------|-----------|---------|----------|--------|
| **Melee Combat** | 🔴 ALTA | Sin héroes tipo guerrero | 2-3 días | ❌ No implementado |
| **Team System** (Blue vs Red) | 🔴 ALTA | No hay equipos en el MOBA | 3-5 días | ❌ No implementado |
| **Tower System** | 🔴 ALTA | Sin objetivos de juego | 3-5 días | ❌ No implementado |
| **Buff/Debuff System** | 🟡 MEDIA | TODO en `HealBuffBehavior.cs:37` | 2-3 días | 🟡 Parcial |
| **Shop System** | 🟡 MEDIA | No se pueden comprar items | 2-3 días | ❌ No implementado |
| **Minimap** | 🟡 MEDIA | Navegación limitada | 1-2 días | ❌ No implementado |
| **Creep Waves** | 🟡 MEDIA | Solo wave system básico | 2-3 días | ❌ No implementado |
| **Health Regen** | 🟡 MEDIA | Existe pero no tickea | 1 día | 🟡 Parcial |
| **Item Effects** | 🟡 MEDIA | Items sin efectos activos | 2-3 días | ❌ No implementado |
| **AGI → Attack Speed** | 🟢 BAJA | Stats de AGI sin usar | 1 día | 🟡 Parcial |

---

## 🐛 BUGS ACTIVOS

| Bug | Prioridad | Descripción | Archivos |
|-----|-----------|-------------|----------|
| **Recarga carga todos los disparos** | 🔴 CRÍTICA | "Al recargar hace que todos los disapros sean cargados" | `RangedCombat.cs` |
| **UI estática 6/6** | 🔴 CRÍTICA | AmmoUI no se actualiza correctamente | `AmmoUI.cs` |
| **Animación roll - snap back** | 🟡 MEDIA | Jugador vuelve hacia atrás al terminar roll | `roll.fbx.meta` |
| **Character sinking** | 🟡 MEDIA | Personaje se hunde al caminar/correr | `walk.fbx.meta`, `run.fbx.meta` |

---

## 📝 TODOs EXPLÍCITOS EN CÓDIGO

1. **`EquipmentComponent.cs:168`**
   ```csharp
   // TODO: AGI bonus reserved for future AttackSpeed implementation
   ```

2. **`HeroEntity.cs:190`**
   ```csharp
   // TODO: Implement MeleeCombat when available
   ```

3. **`HealBuffBehavior.cs:37`**
   ```csharp
   // TODO: Apply buff (AttackSpeed, MoveSpeed, etc.) when BuffSystem is implemented
   ```

---

## 📋 ROADMAP POR FASES

### **FASE 1: COMBATE COMPLETO** (1-2 semanas)

**Objetivo:** Tener ambos tipos de combate (ranged y melee) funcionales.

```
□ MeleeCombat.cs (vacío actualmente)
  └─ Implementar sistema similar a RangedCombat pero sin munición
  └─ Animaciones de ataque cuerpo a cuerpo
  └─ Hit detection en cone/frente del personaje

□ Completar BuffSystem
  └─ Implementar TODO en HealBuffBehavior.cs:37
  └─ Crear BuffComponent para manejar buffs activos
  └─ Soporte para AttackSpeed, MoveSpeed, Armor buffs

□ Health Regen
  └─ Agregar regen por segundo en BaseEntity
  └─ Configurable por ScriptableObject

□ Item Effects
  └─ Implementar efectos activos de items
  └─ Consumibles (potions)
  └─ Items con activables

□ AGI → Attack Speed
  └─ Conectar stat de AGI con velocidad de ataque
  └─ Implementar cooldown de ataques basado en AGI
```

**Criterio de aceptación:**
- ✅ Héroe ranged funciona (ya hecho)
- ✅ Héroe melee funciona
- ✅ Buffs se aplican correctamente
- ✅ Items tienen efectos utilizables

---

### **FASE 2: SISTEMAS MOBA** (3-4 semanas)

**Objetivo:** Tener los sistemas básicos de un MOBA.

```
□ Team System (Blue vs Red)
  └─ Enum Team { None, Blue, Red }
  └─ TeamComponent en BaseEntity
  └─ Check de amistad/enemistad en daño
  └─ Visual (color del outline/halo)

□ Tower System
  └─ TowerEntity con auto-attack
  └─ Targeting prioritario (minions vs heroes)
  └─ Stats de torres (daño, range, attack speed)
  └─ Torres destruíbles

□ Creep Waves
  └─ Lane system (Top, Mid, Bot)
  └─ Wave spawning por lane
  └─ Minion AI (mover hacia lane, atacar enemigos)
  └─ Wave types (melee, ranged, siege)

□ Shop System
  └─ UI de tienda (categorías: items, consumibles)
  └─ Compra con oro
  └─ Venta de items (50% valor)
  └─ Shop zones en base

□ Minimap
  └─ Render texture de cámara ortográfica
  └─ Icons de héroes, torres, minions
  └─ Fog of war (opcional)
  └─ Click para mover (opcional)
```

**Criterio de aceptación:**
- ✅ 2 equipos diferenciados
- ✅ Torres disparan a enemigos
- ✅ Waves de minions spawnean y caminan por lanes
- ✅ Se puede comprar items en shop
- ✅ Minimap muestra el estado del juego

---

### **FASE 3: PULIDO** (1-2 semanas)

**Objetivo:** Juego presentable y balanceado.

```
□ Scoreboard
  └─ K/D/A por jugador
  └─ Oro, CS, items
  └─ Tiempos de respawn

□ Sound System
  └─ SFX de habilidades
  └─ SFX de ataques
  └─ Música de fondo
  └─ Voice lines (opcional)

□ VFX Polish
  └─ Mejorar VFX de habilidades
  └─ Hit feedback visual
  └─ Death VFX
  └─ Level up VFX

□ Balance de Gameplay
  └─ Ajustar daños de habilidades
  └─ Balance de items
  └─ Tuning de stats de héroes
  └─ Playtesting iterativo
```

**Criterio de aceptación:**
- ✅ Juego se siente "terminado"
- ✅ Balance razonable (sin builds rotas)
- ✅ Feedback visual y sonoro claro

---

### **FASE 4: MULTIPLAYER** (Opcional — 6+ semanas)

**Objetivo:** Juego online multijugador.

```
□ Networking
  └─ Elegir solución (Photon, Mirror, Netcode for GameObjects)
  └─ Autoridad del servidor
  └─ Replicación de estado
  └─ Predicción de movimiento

□ Matchmaking
  └─ Cola de búsqueda
  └─ Balance de teams (MMR opcional)
  └─ Lobby system

□ Server Infrastructure
  └─ Dedicated server build
  └─ Hosting (AWS, Azure, etc.)
  └─ Anti-cheat básico

□ Ranked System (opcional)
  └─ MMR/LP system
  └─ Tiers (Bronze, Silver, Gold...)
  └─ Seasonal rewards
```

**Criterio de aceptación:**
- ✅ 2 jugadores pueden jugar online
- ✅ Conexión estable (sin lag excesivo)
- ✅ Sistema de partidas funcional

---

## 🎯 PRÓXIMOS PASOS INMEDIATOS

### Sprint Actual (Semana 1)

1. **Fix bugs críticos**
   - [ ] Bug de recarga (carga todos los disparos)
   - [ ] UI estática 6/6 (AmmoUI no actualiza)
   - [x] ~~Animación roll (snap back)~~ — FIXEADO 2026-04-19

2. **Implementar MeleeCombat**
   - [ ] Crear clase `MeleeCombat.cs`
   - [ ] Hit detection en cone frontal
   - [ ] Animaciones de ataque
   - [ ] Testear con héroe warrior

3. **Completar BuffSystem**
   - [ ] Implementar TODO en `HealBuffBehavior.cs`
   - [ ] Crear `BuffComponent.cs`
   - [ ] Testear con habilidad de heal/buff

---

## 📈 MÉTRICAS DE PROGRESO

| Fase | Progreso | Bloques | Tareas Completadas |
|------|----------|---------|-------------------|
| **Fase 1** | 70% | 5/7 completados | 17/24 |
| **Fase 2** | 0% | 0/5 completados | 0/20 |
| **Fase 3** | 0% | 0/4 completados | 0/15 |
| **Fase 4** | 0% | 0/4 completados | 0/12 |

**Total general:** 17/71 tareas (24%)

---

## 📚 RECURSOS Y REFERENCIAS

### Arquitectura
- **Patrón:** Component-Based con ScriptableObjects
- **Namespaces:** `MobaGameplay.Core`, `MobaGameplay.Abilities`, `MobaGameplay.Combat`, etc.
- **Convenciones:** Ver `.agent/skills/_shared/` y `Documentation/`

### Herramientas
- **Unity:** 6 (6000.0.29f1)
- **Input:** Unity Input System
- **Version Control:** Git + GitHub

### Scripts de Editor (20+)
- `Assets/_Project/Scripts/Editor/` — Herramientas de debug y utilidad
- Auto-generación de meta files
- Test walls para dash collision

---

## 🔗 ENLACES RELACIONADOS

- **Repositorio:** https://github.com/NicoRuedaA/Mobalike
- **Unity MOBA Reference:** https://github.com/Michael032/Unity-MOBA
- **Documentación interna:** `Documentation/` folder

---

*Documento generado automáticamente basado en análisis de código — 2026-04-19*
