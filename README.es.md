[English](README.md) | [Español](README.es.md)

# Mobalike — Prototipo MOBA en Unity 6

Un prototipo de MOBA estilo League of Legends/Dota 2/Supervive construido con **Unity 6 (6000.0.29f1)** y el New Input System. Sistema data-driven con arquitectura modular "Brain and Body".

> **Estado actual:** ~60-70% completo (single-player funcional)  
> **Última actualización:** 2026-04-25

---

## 1. ¿Qué es Mobalike?

Framework de gameplay MOBA que incluye:

- **Movimiento:** WASD con sprint, dash, jump — todo en plano XZ
- **Combate:** Ranged con munición + carga (charged attacks), melee en progreso
- **Habilidades:** Sistema data-driven con 5 behaviors (projectile, AoE, trail, buff, smash)
- **Entidades:** Heroes, enemigos, oleadas con arquitectura component-based
- **Inventario:** 20 slots + 6 slots de equipo con stats
- **UI:** HUD completo con barras, habilidades, munición, floating text

**Arquitectura:** "Brain and Body" — separación clara entre lógica (Brain) y representación (Body).

---

## 2. Arquitectura "Brain and Body"

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         HERO GAMEOBJECT                                  │
│                                                                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐            │
│  │   HeroEntity    │  │ XZPlaneMovement │  │  RangedCombat   │          │
│  │    (Brain)      │  │    (Body)       │  │    (Brain)      │          │
│  │                 │  │                 │  │                 │          │
│  │  - Health/Mana  │  │  - Walk/Sprint  │  │  - Ammo system  │          │
│  │  - Stats (STR/  │  │  - Dash/Jump    │  │  - Charged atk  │          │
│  │    AGI/INT)    │  │  - Character    │  │  - Reload       │          │
│  │  - Events       │  │    Controller   │  │  - Projectiles  │          │
│  └────────┬────────┘  └─────────────────┘  └────────┬────────┘          │
│           │                                         │                   │
│           └─────────────────────────────────────────┘                   │
│                              │                                         │
│  ┌───────────────────────────┴─────────────────────────────────┐       │
│  │                 PlayerInputController                        │       │
│  │                                                              │       │
│  │  - Procesa input (WASD, mouse, 1-4)                        │       │
│  │  - Coordina entre sistemas                                   │       │
│  │  - Gestiona estados (apuntando, cargando)                    │       │
│  └──────────────────────────────────────────────────────────────┘       │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────┐        │
│  │                  AbilitySystem (Data-driven)                 │        │
│  │                                                              │        │
│  │  - AbilityData ScriptableObjects                            │        │
│  │  - AbilityBehaviorFactory                                   │        │
│  │  - 5 tipos: Projectile, AoE, Trail, Buff, Smash            │        │
│  └─────────────────────────────────────────────────────────────┘        │
└─────────────────────────────────────────────────────────────────────────┘

FLUJO DE INPUT:
──────────────────────────────────────────────────────────────────────────

    Teclado/Mouse → Input System → PlayerInputController → Sistemas
                                                           (Movement,
                                                            Combat,
                                                            Abilities)
```

---

## 3. Sistemas Implementados

| Sistema | Estado | Descripción |
|---------|--------|-------------|
| **Entity Framework** | ✅ | BaseEntity, HeroEntity, EnemyEntity con eventos |
| **Ability System** | ✅ | Data-driven con AbilityData ScriptableObjects |
| **Movimiento XZ** | ✅ | Walk, sprint, dash, jump con CharacterController |
| **Combate Ranged** | ✅ | Sistema de munición con recarga y charged attacks |
| **Combate Melee** | 🟡 | Esqueleto creado, pendiente implementación |
| **Input System** | ✅ | Unity Input System moderno |
| **Targeting** | ✅ | Circle, Line, Trail indicators |
| **Proyectiles** | ✅ | Linear, Homing, BasicAttack con pooling implícito |
| **Enemy AI** | ✅ | State machine de 636 líneas |
| **Wave System** | ✅ | Spawning por oleadas |
| **Inventario** | ✅ | 20 slots con drag-and-drop |
| **Equipamiento** | ✅ | 6 slots con stats (STR/AGI/INT) |
| **Buff System** | 🟡 | Heal implementado, falta AttackSpeed/MoveSpeed |
| **Animaciones** | ✅ | CharacterAnimator con override controllers |
| **UI/HUD** | ✅ | Completo con barras, habilidades, munición |

---

## 4. Requisitos Técnicos

| Tecnología | Versión | Notas |
|------------|---------|-------|
| Unity Editor | **6000.0.29f1** | Unity 6 LTS |
| Input System | 1.11.2 | New Input System |
| URP | 17.0.3 | Universal Render Pipeline |
| C# | 10.0 | .NET Standard 2.1 |

### Paquetes Requeridos

```bash
# Core (ya incluidos)
com.unity.inputsystem@1.11.2
com.unity.render-pipelines.universal@17.0.3
com.unity.cinemachine@3.1.3
```

### Configuración de Input

```
Edit → Project Settings → Player → Active Input Handling → Both
```

---

## 5. Instalación Rápida

```bash
# 1. Clonar repositorio
git clone https://github.com/NicoRuedaA/Mobalike.git
cd Mobalike

# 2. Abrir en Unity Hub
# Unity Hub → Open → Seleccionar carpeta Mobalike

# 3. Abrir escena principal
Assets/_Project/Scenes/SampleScene.unity

# 4. Play (▶)
```

---

## 6. Controles

| Input | Acción | Notas |
|-------|--------|-------|
| `WASD` | Movimiento | Relativo a cámara |
| `Shift` (hold) | Sprint | Consume stamina |
| `Space` | Dash | Invulnerabilidad frames |
| `Right Click` (hold) | Apuntar | Activa laser sight |
| `Left Click` (hold) | Cargar ataque | Solo mientras apunta |
| `Left Click` (release) | Disparar | Básico o cargado |
| `R` | Recargar | Cancela carga si hay |
| `1, 2, 3, 4` | Habilidades | Q, W, E, R estilo MOBA |
| `Right Click` | Cancelar habilidad | En modo targeting |
| `B` | Inventario | Toggle UI |

---

## 7. Estructura del Proyecto

```
Assets/
├── _Project/                          # Código principal
│   ├── Art/
│   │   ├── Animations/               # Controllers, clips, masks
│   │   ├── Materials/                # URP materials
│   │   └── Shaders/                  # Outline, healthbar shaders
│   │
│   ├── Data/                         # ScriptableObjects
│   │   ├── Abilities/               # AbilityData (Fireball, Heal, etc.)
│   │   └── Classes/                # HeroClass (Mage, Warrior)
│   │
│   ├── Documentation/               # Roadmap, guías, prompts
│   │   ├── prompts/                # Plantillas para prompts
│   │   └── ANIMATION_SYSTEM_STATUS.md
│   │
│   ├── Prefabs/
│   │   ├── Abilities/              # VFX: zones, projectiles, trails
│   │   ├── Characters/             # Player, Enemy prefabs
│   │   ├── Environment/            # Walls, ramps
│   │   └── UI/                    # HUD elements, targeting indicators
│   │
│   ├── Scenes/                     # SampleScene principal
│   │
│   └── Scripts/                    # Código fuente (namespace MobaGameplay.*)
│       ├── Abilities/              # Core, Behaviors, Types, Projectiles
│       ├── AI/                     # EnemyAIController
│       ├── Animation/              # CharacterAnimator
│       ├── Combat/                 # RangedCombat, MeleeCombat (wip), DamageInfo
│       ├── Controllers/            # PlayerInputController
│       ├── Core/                   # BaseEntity, HeroEntity, GameStateManager
│       ├── Editor/                 # Tools (20+ scripts)
│       ├── Inventory/              # InventoryComponent, EquipmentComponent
│       ├── Movement/               # XZPlaneMovement
│       └── UI/                     # HUD, AbilitySlotUI, AmmoUI
│
├── Tests/                          # Tests unitarios (62 pasando)
│
└── .atl/                          # Agent Teams Lite config
    └── skill-registry.md
```

---

## 8. Convenciones de Código

### Namespaces

```csharp
MobaGameplay.Core        // Entidades, GameState
MobaGameplay.Movement    // XZPlaneMovement
MobaGameplay.Combat      // RangedCombat, DamageInfo
MobaGameplay.Abilities   // AbilitySystem, behaviors
MobaGameplay.UI          // HUD, barras, inventario
MobaGameplay.Inventory   // Items, equipamiento
MobaGameplay.Animation   // CharacterAnimator
```

### Commits

Formato: `<tipo>(<scope>): <descripción>`

```bash
feat(combat): agregar cancelación de recarga
fix(ammo): corregir UI estática 6/6
refactor(abilities): migrar a data-driven system
docs(readme): actualizar controles y arquitectura
```

---

## 9. Solución de Problemas Comunes

### "El personaje no se mueve"

1. Verificar `PlayerInputController` habilitado
2. Verificar `CharacterController` adjunto
3. Verificar layer "Ground" en terrain

### "Las animaciones hacen snap-back"

Fixed en sesión 2026-04-19. Causa: import settings de Mixamo con `loopBlendPositionXZ: 1`. Solución: desactivar en `.fbx.meta`.

### "El ataque cargado no funciona"

1. Verificar `RangedCombat` en el GameObject
2. Verificar `BasicAttackProjectile` prefab asignado
3. Verificar que estás apuntando (Right Click) mientras cargas

### "Las habilidades no aparecen"

1. Verificar `AbilitySystem` tiene AbilityData asignados
2. Verificar targetingType no es "None" en el asset
3. Verificar mana suficiente

---

## 10. Registro de Cambios

### v0.5.0 (19-04-2026)
- ✅ Fix: Roll animation snap-back resuelto
- ✅ Fix: Character sinking en walk/run resuelto
- ✅ Fix: 4 bugs de sistema de munición corregidos
- ✅ Fix: AmmoUI ahora actualiza correctamente
- ✅ Feat: HealBuffBehavior implementado
- ✅ Feat: Idle y Roll loop funcionando correctamente
- ✅ Docs: Roadmap completo creado

### v0.4.0 (11-04-2026)
- ✅ Sistema de habilidades data-driven consolidado
- ✅ 62 tests unitarios pasando
- ✅ Assembly definitions configuradas
- ✅ AbilityData ScriptableObjects funcionando
- ✅ Cooldown overlay funcional

### v0.3.0 (10-04-2026)
- ✅ Sistema de charged attack refactorizado
- ✅ Optimización de raycasts en input
- ✅ Hover outline sin memory leaks
- ✅ Fixes: críticos duplicados, GoldDrop, Equipment stats

### v0.2.0 (09-04-2026)
- ✅ Sistema de input básico
- ✅ Dash implementado
- ✅ Habilidades con targeting
- ✅ Laser sight visual

### v0.1.0 (08-04-2026)
- ✅ Setup inicial proyecto Unity 6
- ✅ Movimiento WASD básico
- ✅ Estructura de carpetas `_Project`

---

## 11. Roadmap

Ver [roadmap.md](./roadmap.md) para detalle completo.

**Fase 1: Combate Completo** (70%)
- ✅ Ranged combat
- 🟡 Melee combat (pendiente)
- 🟡 Buff system parcial

**Fase 2: Sistemas MOBA** (0%)
- ❌ Team system (Blue vs Red)
- ❌ Tower system
- ❌ Creep waves
- ❌ Shop system

**Fase 3: Pulido** (0%)
- ❌ Scoreboard
- ❌ Sound system
- ❌ Balance

**Fase 4: Multiplayer** (Opcional)
- ❌ Networking
- ❌ Matchmaking

---

## 12. Recursos

- **Repositorio:** https://github.com/NicoRuedaA/Mobalike
- **Unity MOBA Reference:** https://github.com/Michael032/Unity-MOBA
- **Documentación:** `Assets/_Project/Documentation/`
- **Roadmap:** [roadmap.md](./roadmap.md)

---

*Built with Unity 6 + pasión + vibes rioplatenses* 🇦🇷
