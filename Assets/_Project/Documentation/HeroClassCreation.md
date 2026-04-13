# Cómo crear una nueva clase de héroe

Este documento explica cómo crear una nueva clase de héroe (HeroClass) para el sistema MOBA.

## Requisitos previos

- Unity 2022.3+
- Proyecto U.MobaGameplay abierto

## Paso 1: Crear el ScriptableObject

1. En el Project window, hacer **right-click**
2. Ir a **Create → MobaGameplay → Hero Class**
3. Nombrar según convención: `[NombreClase].asset` (ej: `Warrior.asset`, `Mage.asset`)

```
Assets/_Project/Resources/ScriptableObjects/Heroes/Mage.asset
```

## Paso 2: Configurar Identity

| Campo | Descripción | Ejemplo |
|-------|-------------|--------|
| className | Nombre de la clase (identificador único) | "Mage" |
| description | Descripción breve | "Wizard specializing in fire magic" |
| role | Rol principal | "Mage" |

## Paso 3: Configurar Combat Type

| Campo | Tipo | Descripción |
|------|-----|------------|
| Combat Type | Ranged / Melee | Cómo ataca |

- **Ranged**: Usa proyectiles (arquero, mago). Requiere `BasicAttackProjectilePrefab`.
- **Melee**: Ataque cuerpo a cuerpo (guerrero, assassin).

## Paso 4: Configurar Base Stats

| Campo | Descripción | Rango típico |
|-------|------------|-------------|
| baseHealth | Salud al nivel 1 | 400-700 |
| baseMana | Maná al nivel 1 | 200-400 |
| baseAttackDamage | Daño de ataque | 40-70 |
| baseMoveSpeed | Velocidad de movimiento | 5-6 |
| baseArmor | Armadura física | 15-30 |
| baseMagicResist | Resistencia mágica | 20-40 |
| healthRegen | Regeneración de vida | 0.5-2 |
| manaRegen | Regeneración de maná | 0.5-2 |

## Paso 5: Configurar Habilidades

Las 4 habilidades en orden: **Q, W, E, R**.

| Slot | Tecla | Uso |
|------|------|-----|
| 0 | Q | Habilidad principal |
| 1 | W | Utilidad |
| 2 | E | Mobility/Utility |
| 3 | R | Ultimate |

Para cada habilidad:
1. Crear **AbilityData** (Create → MobaGameplay → Ability)
2. Configurar sus properties
3. Asignar al slot correspondiente

## Paso 6: Configurar Ranged Combat (si combatType = Ranged)

| Campo | Descripción | Ejemplo |
|------|-------------|--------|
| basicAttackProjectilePrefab | Prefab del proyectil | Projectile prefab |
| projectileSpeed | Velocidad del proyectil | 25 |
| projectileMaxDistance | Distancia máxima | 20 |
| chargedDamageMultiplier | Multiplicador de daño cargado | 1.5 |
| chargedSpeedMultiplier | Multiplicador de velocidad cargado | 1.3 |
| chargedSizeMultiplier | Multiplicador de tamaño cargado | 1.5 |

## Paso 7: Configurar Visual Options

| Campo | Descripción |
|------|-------------|
| showAimLines | Mostrar líneas de aim (solo para Ranged) |

## Ejemplo completo: Mage.asset

```
className: "Mage"
role: "Mage"
combatType: Ranged
showAimLines: true

baseHealth: 450
baseMana: 350
baseAttackDamage: 45
baseMoveSpeed: 5.5
baseArmor: 18
baseMagicResist: 35
healthRegen: 1.2
manaRegen: 1.5

basicAttackProjectilePrefab: BasicAttackProjectile
projectileSpeed: 25
projectileMaxDistance: 20
chargedDamageMultiplier: 1.5
chargedSpeedMultiplier: 1.3
chargedSizeMultiplier: 1.5

abilities:
  0: Fireball (Q)
  1: Ground Smash (W)
  2: Dash (E)
  3: Ground Trail (R)
```

## Cómo funciona el sistema

```
HeroEntity.Awake()
    │
    ├─→ Load HeroClass from Resources
    │
    ├─→ ApplyClassConfiguration()
    │       │
    │       ├─→ Apply stats (health, mana, armor, etc.)
    │       │
    │       ├─→ SetupCombatComponent()
    │       │       │
    │       │       ├─→ Add RangedCombat / MeleeCombat
    │       │       │
    │       │       └─→ ConfigureFromHeroClass()
    │       │               │
    │       │               └─→ Setup LaserSight (if showAimLines)
    │       │
    │       └─→ Register abilities to AbilitySystem
    │
    └─→ PlayerInputController maps keys 1-4 to abilities
```

## Notas

- **No hardcodear componentes en Player.prefab** - todo se configura dinámicamente
- **Cada clase es un ScriptableObject independiente**
- **Combat type determina el Behaviour de ataque**
- **Habilidades se registran automáticamente**

## Issues comunes

| Error | Causa | Solución |
|-------|------|----------|
| IsValid = false | className vacío | Asignar className |
| No dispara | Falta projectilePrefab | Assignar en clase |
| No aparecen líneas | showAimLines = false | Activar en clase |