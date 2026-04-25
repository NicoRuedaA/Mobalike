# Mobalike — MOBA Prototype in Unity 6

A League of Legends/Dota 2/Supervive-style MOBA prototype built with **Unity 6 (6000.0.29f1)** and the New Input System. Data-driven system with modular "Brain and Body" architecture.

> **Current Status:** ~60-70% complete (functional single-player)  
> **Last Updated:** 2026-04-25

---

## 1. What is Mobalike?

MOBA gameplay framework featuring:

- **Movement:** WASD with sprint, dash, jump — all on the XZ plane
- **Combat:** Ranged with ammo + charged attacks, melee in progress
- **Abilities:** Data-driven system with 5 behaviors (projectile, AoE, trail, buff, smash)
- **Entities:** Heroes, enemies, waves with component-based architecture
- **Inventory:** 20 slots + 6 equipment slots with stats
- **UI:** Complete HUD with bars, abilities, ammo, floating text

**Architecture:** "Brain and Body" — clear separation between logic (Brain) and representation (Body).

---

## 2. "Brain and Body" Architecture

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
│  │    AGI/INT)    │  │  - Character   │  │  - Reload       │          │
│  │  - Events      │  │    Controller   │  │  - Projectiles │          │
│  └────────┬────────┘  └─────────────────┘  └────────┬────────┘          │
│           │                                         │                   │
│           └─────────────────────────────────────────┘                   │
│                              │                                         │
│  ┌───────────────────────────┴─────────────────────────────────┐       │
│  │                 PlayerInputController                        │       │
│  │                                                              │       │
│  │  - Process input (WASD, mouse, 1-4)                       │       │
│  │  - Coordinate between systems                              │       │
│  │  - Manage states (aiming, charging)                     │       │
│  └──────────────────────────────────────────────────────────────┘       │
│                                                                          │
│  ┌──────────────────────────────────────────────────────���──────┐        │
│  │                  AbilitySystem (Data-driven)                 │        │
│  │                                                              │        │
│  │  - AbilityData ScriptableObjects                           │        │
│  │  - AbilityBehaviorFactory                                  │        │
│  │  - 5 types: Projectile, AoE, Trail, Buff, Smash             │        │
│  └─────────────────────────────────────────────────────────────┘        │
└─────────────────────────────────────────────────────────────────────────┘

INPUT FLOW:
─────────────────────────────────────────────────────────────────────────

    Keyboard/Mouse → Input System → PlayerInputController → Systems
                                                            (Movement,
                                                             Combat,
                                                             Abilities)
```

---

## 3. Implemented Systems

| System | Status | Description |
|--------|--------|-------------|
| **Entity Framework** | ✅ | BaseEntity, HeroEntity, EnemyEntity with events |
| **Ability System** | ✅ | Data-driven with AbilityData ScriptableObjects |
| **XZ Movement** | ✅ | Walk, sprint, dash, jump with CharacterController |
| **Ranged Combat** | ✅ | Ammo system with reload and charged attacks |
| **Melee Combat** | 🟡 | Skeleton created, implementation pending |
| **Input System** | ✅ | Modern Unity Input System |
| **Targeting** | ✅ | Circle, Line, Trail indicators |
| **Projectiles** | ✅ | Linear, Homing, BasicAttack with implicit pooling |
| **Enemy AI** | ✅ | State machine (636 lines) |
| **Wave System** | ✅ | Wave spawning |
| **Inventory** | ✅ | 20 slots with drag-and-drop |
| **Equipment** | ✅ | 6 slots with stats (STR/AGI/INT) |
| **Buff System** | 🟡 | Heal implemented, AttackSpeed/MoveSpeed pending |
| **Animations** | ✅ | CharacterAnimator with override controllers |
| **UI/HUD** | ✅ | Complete with bars, abilities, ammo |

---

## 4. Technical Requirements

| Technology | Version | Notes |
|------------|---------|-------|
| Unity Editor | **6000.0.29f1** | Unity 6 LTS |
| Input System | 1.11.2 | New Input System |
| URP | 17.0.3 | Universal Render Pipeline |
| C# | 10.0 | .NET Standard 2.1 |

### Required Packages

```bash
# Core (already included)
com.unity.inputsystem@1.11.2
com.unity.render-pipelines.universal@17.0.3
com.unity.cinemachine@3.1.3
```

### Input Configuration

```
Edit → Project Settings → Player → Active Input Handling → Both
```

---

## 5. Quick Installation

```bash
# 1. Clone repository
git clone https://github.com/NicoRuedaA/Mobalike.git
cd Mobalike

# 2. Open in Unity Hub
# Unity Hub → Open → Select Mobalike folder

# 3. Open main scene
Assets/_Project/Scenes/SampleScene.unity

# 4. Play (▶)
```

---

## 6. Controls

| Input | Action | Notes |
|-------|-------|-------|
| `WASD` | Movement | Camera-relative |
| `Shift` (hold) | Sprint | Consumes stamina |
| `Space` | Dash | Invulnerability frames |
| `Right Click` (hold) | Aim | Activates laser sight |
| `Left Click` (hold) | Charge attack | Only while aiming |
| `Left Click` (release) | Fire | Basic or charged |
| `R` | Reload | Cancels charge if any |
| `1, 2, 3, 4` | Abilities | Q, W, E, R MOBA-style |
| `Right Click` | Cancel ability | In targeting mode |
| `B` | Inventory | Toggle UI |

---

## 7. Project Structure

```
Assets/
├── _Project/                          # Main code
│   ├── Art/
│   │   ├── Animations/               # Controllers, clips, masks
│   │   ├── Materials/               # URP materials
│   │   └── Shaders/                 # Outline, healthbar shaders
│   │
│   ├── Data/                        # ScriptableObjects
│   │   ├── Abilities/               # AbilityData (Fireball, Heal, etc.)
│   │   └── Classes/                # HeroClass (Mage, Warrior)
│   │
│   ├── Documentation/              # Roadmap, guides, prompts
│   │   ├── prompts/                # Templates for prompts
│   │   └── ANIMATION_SYSTEM_STATUS.md
│   │
│   ├── Prefabs/
│   │   ├── Abilities/              # VFX: zones, projectiles, trails
│   │   ├── Characters/             # Player, Enemy prefabs
│   │   ├── Environment/           # Walls, ramps
│   │   └── UI/                    # HUD elements, targeting indicators
│   │
│   ├── Scenes/                     # Main SampleScene
│   │
│   └── Scripts/                    # Source code (namespace MobaGameplay.*)
│       ├── Abilities/              # Core, Behaviors, Types, Projectiles
│       ├── AI/                    # EnemyAIController
│       ├── Animation/             # CharacterAnimator
│       ├── Combat/               # RangedCombat, MeleeCombat (wip), DamageInfo
│       ├── Controllers/         # PlayerInputController
│       ├── Core/                # BaseEntity, HeroEntity, GameStateManager
│       ├── Editor/              # Tools (20+ scripts)
│       ├── Inventory/          # InventoryComponent, EquipmentComponent
│       ├── Movement/           # XZPlaneMovement
│       └── UI/                 # HUD, AbilitySlotUI, AmmoUI
│
├── Tests/                         # Unit tests (62 passing)
│
└── .atl/                        # Agent Teams Lite config
    └── skill-registry.md
```

---

## 8. Code Conventions

### Namespaces

```csharp
MobaGameplay.Core        // Entities, GameState
MobaGameplay.Movement    // XZPlaneMovement
MobaGameplay.Combat      // RangedCombat, DamageInfo
MobaGameplay.Abilities // AbilitySystem, behaviors
MobaGameplay.UI        // HUD, bars, inventory
MobaGameplay.Inventory // Items, equipment
MobaGameplay.Animation // CharacterAnimator
```

### Commits

Format: `<type>(<scope>): <description>`

```bash
feat(combat): add reload cancellation
fix(ammo): fix static 6/6 UI
refactor(abilities): migrate to data-driven system
docs(readme): update controls and architecture
```

---

## 9. Troubleshooting

### "Character won't move"

1. Verify `PlayerInputController` is enabled
2. Verify `CharacterController` is attached
3. Verify "Ground" layer on terrain

### "Animations snap-back"

Fixed in session 2026-04-19. Cause: Mixamo import settings with `loopBlendPositionXZ: 1`. Solution: disable in `.fbx.meta`.

### "Charged attack doesn't work"

1. Verify `RangedCombat` on the GameObject
2. Verify `BasicAttackProjectile` prefab assigned
3. Verify you're aiming (Right Click) while charging

### "Abilities don't appear"

1. Verify `AbilitySystem` has AbilityData assigned
2. Verify targetingType is not "None" in the asset
3. Verify sufficient mana

---

## 10. Changelog

### v0.5.0 (19-04-2026)
- ✅ Fix: Roll animation snap-back resolved
- ✅ Fix: Character sinking in walk/run resolved
- ✅ Fix: 4 ammo system bugs fixed
- ✅ Fix: AmmoUI now updates correctly
- ✅ Feat: HealBuffBehavior implemented
- ✅ Feat: Idle and Roll loop working correctly
- ✅ Docs: Complete roadmap created

### v0.4.0 (11-04-2026)
- ✅ Data-driven ability system consolidated
- ✅ 62 unit tests passing
- ✅ Assembly definitions configured
- ✅ AbilityData ScriptableObjects working
- ✅ Cooldown overlay functional

### v0.3.0 (10-04-2026)
- ✅ Charged attack system refactored
- ✅ Input raycasts optimization
- ✅ Hover outline without memory leaks
- ✅ Critical fixes: duplicates, GoldDrop, Equipment stats

### v0.2.0 (09-04-2026)
- ✅ Basic input system
- ✅ Dash implemented
- ✅ Abilities with targeting
- ✅ Laser sight visual

### v0.1.0 (08-04-2026)
- ✅ Initial Unity 6 project setup
- ✅ Basic WASD movement
- ✅ `_Project` folder structure

---

## 11. Roadmap

See [roadmap.md](./roadmap.md) for complete details.

**Phase 1: Complete Combat** (70%)
- ✅ Ranged combat
- 🟡 Melee combat (pending)
- 🟡 Buff system partial

**Phase 2: MOBA Systems** (0%)
- ❌ Team system (Blue vs Red)
- ❌ Tower system
- ❌ Creep waves
- ❌ Shop system

**Phase 3: Polish** (0%)
- ❌ Scoreboard
- ❌ Sound system
- ❌ Balance

**Phase 4: Multiplayer** (Optional)
- ❌ Networking
- ❌ Matchmaking

---

## 12. Resources

- **Repository:** https://github.com/NicoRuedaA/Mobalike
- **Unity MOBA Reference:** https://github.com/Michael032/Unity-MOBA
- **Documentation:** `Assets/_Project/Documentation/`
- **Roadmap:** [roadmap.md](./roadmap.md)

---

*Built with Unity 6 + passion + rioplatense vibes* 🇦🇷