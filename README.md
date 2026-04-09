# MobaGameplay - Sistema de Input

Sistema de input para un juego MOBA estilo League of Legends/Dota 2 construido con Unity 2022+ y el nuevo Input System.

## 1. Descripción

Framework de input para personajes MOBA que soporta movimiento, combate a distancia con carga (charged attacks), habilidades targeting, y sistemas de hover/outline. Diseñado para ser modular, testeable y extensible.

**Problema que resuelve:** Coordinar múltiples sistemas de input (teclado, ratón) con estado del personaje (vivo/muerto, apuntando, cargando ataque) de forma predecible y sin race conditions.

**Para quién es:** Desarrolladores de juegos MOBA en Unity que necesitan un sistema de input robusto y extensible.

---

## 2. Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         PLAYER GAMEOBJECT                                │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐      │
│  │  HeroEntity     │  │  XZPlaneMovement │  │  RangedCombat   │      │
│  │  (BaseEntity)   │  │  (BaseMovement)  │  │  (BaseCombat)   │      │
│  │                 │  │                 │  │                 │      │
│  │  - Health       │  │  - Walk/Sprint  │  │  - BasicAttack  │      │
│  │  - Mana        │  │  - Dash         │  │  - ChargedAtk   │      │
│  │  - Stats       │  │  - Aim          │  │  - Projectiles  │      │
│  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘      │
│           │                   │                   │                  │
│           └───────────────────┼───────────────────┘                  │
│                               │                                      │
│  ┌───────────────────────────┴─────────────────────────────────┐    │
│  │                    PLAYERINPUTCONTROLLER                     │    │
│  │                                                              │    │
│  │  Responsabilidades:                                           │    │
│  │  - Procesar input de teclado (WASD, Space, 1-3)             │    │
│  │  - Procesar input de ratón (Left/Right click)               │    │
│  │  - Coordinar estado entre sistemas                           │    │
│  │  - Cachear direcciones de cámara                             │    │
│  │                                                              │    │
│  │  Proceso por frame:                                          │    │
│  │  1. ValidateInput() → Early exit si inválido                │    │
│  │  2. UpdateCameraCache() → Actualiza cada 0.5s              │    │
│  │  3. ProcessHover() → Raycast para outline                   │    │
│  │  4. ProcessAiming() → Rotación hacia ratón                  │    │
│  │  5. ProcessMovement() → WASD → Movement                     │    │
│  │  6. ProcessAbilities() → 1-2-3 → Abilities                  │    │
│  │  7. ProcessCombat() → Charged attack logic                  │    │
│  │  8. ProcessDash() → Space → Dash                            │    │
│  └──────────────────────────────────────────────────────────────┘    │
│                               │                                      │
│  ┌───────────────────────────┴─────────────────────────────────┐    │
│  │                    ABILITYCONTROLLER                         │    │
│  │                                                              │    │
│  │  - Gestiona slots de habilidades (1, 2, 3)                  │    │
│  │  - Estado de targeting activo                               │    │
│  │  - Ejecución de habilidades                                 │    │
│  │                                                              │    │
│  │  Estados:                                                    │    │
│  │  None → Targeting → Executing → None                        │    │
│  └──────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘

FLUJO DE INPUT:
──────────────────────────────────────────────────────────────────────────

    ┌─────────┐     ┌──────────────┐     ┌──────────────────┐
    │ Teclado │────▶│Input System  │────▶│ PlayerInputCtrl │
    │ Mouse   │     │ (New Input)  │     │ .Update()       │
    └─────────┘     └──────────────┘     └────────┬─────────┘
                                                     │
         ┌────────────────────────────────────────────┼─────────────────┐
         │                                            │                 │
         ▼                                            ▼                 ▼
┌─────────────────┐                    ┌──────────────────┐   ┌────────────────┐
│ Hover Outline   │                    │ AbilityController │   │ RangedCombat  │
│                 │                    │                  │   │                │
│ - GetComponent  │                    │ - TryStartTarget │   │ - StartCharge │
│ - SetHover(true)│                    │ - ExecuteTarget  │   │ - UpdateCharge│
│ - SetHover(false│                    │ - CancelTarget   │   │ - ResetCharge │
└─────────────────┘                    └──────────────────┘   └────────────────┘

COMBAT FLOW (Charged Attack):
──────────────────────────────────────────────────────────────────────────

    ┌───────────┐     ┌───────────┐     ┌───────────┐     ┌───────────┐
    │ R-Click   │────▶│ isAiming  │────▶│ L-Hold    │────▶│Charging...│
    │ (Press)   │     │ = true    │     │ (Start)   │     │           │
    └───────────┘     └───────────┘     └───────────┘     └─────┬─────┘
                                                                 │
                                                                 ▼
    ┌───────────┐     ┌───────────┐     ┌───────────┐     ┌───────────┐
    │ R-Click   │◀────│ isAiming  │◀────│ L-Release │◀────│ Progress  │
    │ (Release) │     │ = false   │     │ (Stop)    │     │ = 1.0     │
    └─────┬─────┘     └───────────┘     └─────┬─────┘     └─────┬─────┘
          │                                       │                 │
          │                                       ▼                 ▼
          │                              ┌───────────────────────────┐
          │                              │ ResetCharge()              │
          │                              │ BasicAttack()              │
          │                              │ FireProjectile(isCharged)  │
          │                              └───────────────────────────┘
          │
          ▼
    ┌─────────────────┐
    │ Charged cooldown │
    │ = 2.0s          │
    └─────────────────┘
```

---

## 3. Requisitos Previos

| Tecnología | Versión Mínima | Versión Recomendada | Notas |
|------------|-----------------|---------------------|-------|
| Unity Editor | 2022.3 LTS | 6000.3 (Unity 6) | LTS preferred; project uses Unity 6 |
| Input System Package | 1.6.0 | 1.19.0 | New Input System |
| URP | 14.0.0 | 17.3.0 | Universal Render Pipeline |
| C# | 9.0 | 10.0 | .NET Standard 2.1 |
| IDE | VS 2022 | Rider 2024.1 | Recomendado para debugging |

### Paquetes de Unity Requeridos

```bash
# Core
com.unity.inputsystem@1.7.0
com.unity.render-pipelines.universal@14.0.10

# Testing (opcional pero recomendado)
com.unity.test-framework@1.1.33
```

### Configuración de Input System

```
1. Edit → Project Settings → Player → Active Input Handling → Both
2. Reiniciar Unity cuando se cambie esta configuración
```

---

## 4. Instalación

### Paso 1: Abrir el Proyecto

```bash
# Usando Unity Hub
# 1. Abrir Unity Hub
# 2. Click "Open" → Seleccionar carpeta "MobaGameplay"
# 3. Esperar importación de assets
```

### Paso 2: Configurar Input System (si no está instalado)

```bash
# Window → Package Manager → Unity Registry
# Buscar "Input System" → Install
```

### Paso 3: Configurar Layers

```
1. Edit → Project Settings → Tags and Layers
2. Crear/verificar las siguientes capas:
   - Ground (capa 0 por defecto)
   - Player (capa 8)
   - Enemy (capa 9)
```

### Paso 4: Configurar API Compatibility

```
1. Edit → Project Settings → Player → Other Settings
2. API Compatibility Level: .NET Standard 2.1
```

### Paso 5: Abrir la Escena de Prueba

```
1. En Project window: Assets/_Project/Scenes/
2. Double-click en SampleScene
3. Play (▶)
```

---

## 5. Variables de Entorno (Project Settings)

| Variable | Descripción | Valor por Defecto | Ubicación |
|----------|-------------|-------------------|-----------|
| Active Input Handling | Sistema de input activo | Both | Player Settings |
| API Compatibility | Versión .NET | .NET Standard 2.1 | Player Settings |
| Default Isometric Angle | Ángulo de cámara | 30° | CameraController |

---

## 6. Estructura del Proyecto

```
Assets/
├── _Project/                          # Código principal del proyecto
│   ├── Scripts/
│   │   ├── Core/                     # Clases base abstractas
│   │   │   ├── BaseController.cs     # Controller base (input)
│   │   │   ├── BaseEntity.cs        # Entidad base (health, mana, stats)
│   │   │   ├── BaseMovement.cs      # Movimiento base abstracto
│   │   │   ├── BaseCombat.cs        # Combate base abstracto
│   │   │   ├── HeroEntity.cs        # Jugador (leveling, gold)
│   │   │   └── EnemyEntity.cs       # Enemigo con hover outline
│   │   │
│   │   ├── Controllers/             # Controladores de input
│   │   │   └── PlayerInputController.cs  # MAIN INPUT HANDLER
│   │   │
│   │   ├── Movement/                 # Sistemas de movimiento
│   │   │   └── XZPlaneMovement.cs   # Movimiento estilo MOBA
│   │   │
│   │   ├── Combat/                  # Sistemas de combate
│   │   │   ├── RangedCombat.cs      # Combate a distancia
│   │   │   ├── MeleeCombat.cs       # Combate cuerpo a cuerpo
│   │   │   └── DamageInfo.cs        # Struct de daño
│   │   │
│   │   ├── Abilities/               # Sistema de habilidades
│   │   │   ├── BaseAbility.cs       # Habilidad base abstracta
│   │   │   ├── AbilityController.cs # Gestor de abilities
│   │   │   ├── DashAbility.cs      # Ability de dash
│   │   │   ├── FireballAbility.cs  # Proyectil de fuego
│   │   │   ├── GroundSmashAbility.cs # AoE melee
│   │   │   ├── Types/              # Tipos de abilities
│   │   │   │   ├── ProjectileAbility.cs
│   │   │   │   ├── AreaOfEffectAbility.cs
│   │   │   │   └── TargetedProjectileAbility.cs
│   │   │   └── Projectiles/         # Proyectiles
│   │   │       ├── Projectile.cs
│   │   │       ├── BasicAttackProjectile.cs
│   │   │       ├── LinearProjectile.cs
│   │   │       └── HomingProjectile.cs
│   │   │
│   │   ├── Visuals/                 # Efectos visuales
│   │   │   └── LaserSight.cs        # Laser de apuntar
│   │   │
│   │   ├── UI/                      # UI del juego
│   │   │   ├── Targeting/          # Sistema de targeting UI
│   │   │   │   ├── HoverOutline.cs     # Outline en hover
│   │   │   │   ├── TargetingManager.cs  # Gestor de indicators
│   │   │   │   ├── CircleIndicator.cs
│   │   │   │   ├── LineIndicator.cs
│   │   │   │   └── IndicatorType.cs     # Enum de tipos
│   │   │   ├── FloatingTextManager.cs
│   │   │   ├── FloatingDamageText.cs
│   │   │   └── ...
│   │   │
│   │   └── VFX/                     # Efectos de partículas
│   │       └── SimpleVFX.cs
│   │
│   ├── Prefabs/                     # Prefabs del proyecto
│   │   ├── Abilities/
│   │   │   ├── BasicAttackProjectile.prefab
│   │   │   ├── Fireball.prefab
│   │   │   └── GroundSmashVFX.prefab
│   │   └── ...
│   │
│   └── Scenes/
│       └── SampleScene.unity
│
├── StarterAssets/                   # Assets base de Unity
│   ├── InputSystem/
│   └── ThirdPersonController/
│
└── docs/                           # Documentación
    └── DEPLOYMENT.md
```

---

## 7. Guía de Contribución

### 7.1 Configurar Git Flow

```bash
# Clonar repositorio
git clone https://github.com/tu-usuario/MobaGameplay.git
cd MobaGameplay

# Crear branch para feature
git checkout -b feature/nueva-habilidad

# O para bugfix
git checkout -b fix/bug-de-carga
```

### 7.2 Convenciones de Commits

```
formato: <tipo>(<alcance>): <descripción>

tipos:
  - feat: nueva funcionalidad
  - fix: corrección de bug
  - refactor: refactorización sin cambio de funcionalidad
  - docs: documentación
  - perf: mejora de rendimiento
  - test: agregar tests
  - chore: mantenimiento

ejemplos:
  feat(combat): agregar charged attack para ranged combat
  fix(input): cancelar carga al dejar de apuntar
  refactor(movement): extraer dash logic a método separado
  docs(readme): actualizar guía de contribución
```

### 7.3 Checklist Antes de PR

```bash
# 1. Verificar que el código compila
# (En Unity: Ctrl+Shift+C o File → Save Project)

# 2. Verificar que no hay errores en Console
# (Window → General → Console)

# 3. Probar manualmente:
#    - Movimiento WASD
#    - Apuntar con click derecho
#    - Ataque cargado (mantener L-click mientras apunta)
#    - Habilidades (1, 2, 3)
#    - Dash (Espacio)
#    - Morir y verificar que input para

# 4. Verificar Scene sigue guardando
# (File → Save Scene)
```

### 7.4 Pull Request Template

```markdown
## Descripción
[Descripción breve de los cambios]

## Tipo de Cambio
- [ ] Bug fix
- [ ] Nueva feature
- [ ] Breaking change
- [ ] Documentación

## Testing
[ ] Compila sin errores
[ ] Tests manuales completados:
- [ ] Movimiento
- [ ] Combate
- [ ] Habilidades
- [ ] Muerte

## Screenshots (si aplica)
[Agregar screenshots de cambios visuales]
```

---

## 8. Sistema de Input - Referencia Rápida

### Controles

| Input | Acción | Sistema |
|-------|--------|---------|
| WASD / Flechas | Movimiento | XZPlaneMovement |
| Shift (hold) | Sprint | XZPlaneMovement |
| Right Click (hold) | Apuntar | RangedCombat, LaserSight |
| Left Click (hold) | Cargar ataque | RangedCombat |
| Left Click (release) | Disparar | RangedCombat |
| 1, 2, 3 | Usar habilidad | AbilityController |
| Space | Dash | XZPlaneMovement |
| Right Click | Cancelar habilidad | AbilityController |

### Estados del Jugador

```
┌────────────────────────────────────────────────────────────┐
│                     INPUT VALIDO                           │
│                                                              │
│  isAlive = true                                             │
│  isAiming = (right click pressed)                          │
│  isCharging = (left click pressed AND isAiming)            │
│  hasActiveAbility = (ability in targeting mode)            │
└────────────────────────────────────────────────────────────┘

Ejemplo de lógica:
─────────────────────────────────────────────────────────────

if (isAlive && isAiming && hasActiveAbility == false)
{
    // PUEDE cargar ataque
    if (leftClickPressed && canCharge)
        StartCharging()
}
else if (leftClickReleased)
{
    // DISPARAR
    BasicAttack() // Si isCharging → charged attack
}
```

---

## 9. Extendiendo el Sistema

### 9.1 Crear una Nueva Habilidad

```csharp
using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace TuNamespace
{
    public class TuHabilidad : BaseAbility
    {
        [Header("Tu Habilidad")]
        public float damage = 100f;
        public GameObject effectPrefab;

        protected override void OnExecute(Vector3 targetPosition, BaseEntity targetEntity)
        {
            // Tu lógica aquí
            float finalDamage = damage + ownerEntity.AbilityPower * 0.5f;
            
            // Ejemplo: aplicar daño en área
            Collider[] hits = Physics.OverlapSphere(targetPosition, Range);
            foreach (var hit in hits)
            {
                var entity = hit.GetComponentInParent<BaseEntity>();
                if (entity != null && entity != ownerEntity)
                {
                    entity.TakeDamage(new DamageInfo(finalDamage, DamageType.Magical, ownerEntity));
                }
            }

            // Instanciar VFX
            if (effectPrefab != null)
            {
                Instantiate(effectPrefab, targetPosition, Quaternion.identity);
            }
        }
    }
}
```

### 9.2 Crear un Nuevo Tipo de Proyectil

```csharp
using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace TuNamespace
{
    public class TuProyectil : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float damage;
        private DamageType damageType;
        private BaseEntity caster;
        private LayerMask hitLayers;

        public void Initialize(Vector3 dir, float dmg, DamageType type, BaseEntity source, float spd, LayerMask layers)
        {
            direction = dir.normalized;
            speed = spd;
            damage = dmg;
            damageType = type;
            caster = source;
            hitLayers = layers;
        }

        private void Update()
        {
            // Tu lógica de movimiento
            transform.position += direction * speed * Time.deltaTime;
            
            // Tu lógica de detección de impacto
            // ...
        }
    }
}
```

---

## 10. Solución de Problemas Comunes

### "El personaje no se mueve"

1. Verificar que `PlayerInputController` está habilitado en el GameObject
2. Verificar que el `CharacterController` está adjunto
3. Verificar que `XZPlaneMovement` está en el mismo GameObject
4. Revisar Console por errores de null reference

### "El ataque cargado no funciona"

1. Verificar que `RangedCombat` está en el GameObject del jugador
2. Verificar que `BasicAttackProjectile` prefab está asignado en RangedCombat
3. Verificar que el laser sight aparece (significa que está apuntando)
4. Revisar `IsOnChargedCooldown` en RangedCombat

### "Las habilidades no aparecen"

1. Verificar que `AbilityController` tiene las 3 habilidades asignadas
2. Verificar que los prefabs de abilities están en las ranuras correctas
3. Verificar que el personaje tiene suficiente mana

### "El outline de hover no funciona"

1. Verificar que el shader `Custom/Outline` existe en el proyecto
2. Verificar que `HoverOutline` está adjunto al prefab del enemigo
3. Revisar que el collider tiene la capa correcta

---

## 11. Changelog

### v0.2.0 (2024)
- Sistema de charged attack refactorizado
- Input system optimizado (raycasts reducidos)
- Mejor manejo de estados y edge cases
- Hover outline con memory leak fix

### v0.1.0 (2024)
- Sistema de input básico implementado
- Charged attack con cooldown
- Habilidades con targeting
- Dash implementado
- Hover outline funcional
- Laser sight con feedback visual
