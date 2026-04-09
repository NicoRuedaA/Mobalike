# Memoria del Proyecto - MobaGameplay

## Convenciones Generales

### Namespace
- **Principal**: `MobaGameplay.*` para todo el proyecto
- **Excepción**: `MMORPG.Inventory` para el sistema de inventario (inconsistencia conocida)

### Estructura de Carpetas
```
Assets/_Project/
  Scripts/
    Core/         # Entidades, combate, movimiento base
    Abilities/    # Habilidades y efectos de área
    Inventory/    # Sistema de inventario
    UI/           # Interfaces de usuario
  Prefabs/
    Characters/   # Prefabs de héroes/enemigos
    Abilities/    # Prefabs de proyectiles y efectos
    UI/           # Prefabs de UI
    Items/        # Prefabs de drops y items
```

## Entidades Principales

### BaseEntity
- Clase abstracta base para todos los personajes
- Maneja vida, maná, stats de combate
- Eventos: `OnTakeDamage`, `OnDeath`, `OnManaChanged`

### HeroEntity
- Extiende `BaseEntity`
- Sistema de progresión: nivel, experiencia, oro
- Métodos: `AddGold()`, `AddExp()`, `LevelUp()`

### EnemyEntity
- Extiende `BaseEntity`
- Configuración: `goldReward`, `experienceReward`
- Método `NotifyKillReward()` implementado - otorga oro/XP al héroe y spawnea GoldDrop
- Campo `goldDropPrefab` para el drop visual

## Sistemas de Combate

### AoEZone
- Prefab instanciado por `AreaOfEffectAbility`
- Delay configurable antes de explotar
- Daño activado - aplica daño mágico a entidades en el área

### EquipmentComponent
- Calcula stats totales de items equipados
- Aplica stats a `BaseEntity`: HP→MaxHealth, STR→AttackDamage
- Stats calculados: `TotalHP`, `TotalSTR`, `TotalAGI`
- Evento `OnStatsChanged` conectado al owner

### GoldDrop
- Prefab visual que spawnea al morir un enemigo
- Se mueve hacia el jugador cuando está en rango (5 unidades)
- Se recoge automáticamente al estar cerca (1.5 unidades)
- Auto-destroy después de 3 segundos

## Habilidades del Jugador

### Slot 1 - FireballAbility
- Proyectil lineal con daño mágico (AP ratio)
- Targeting: Line (IndicatorType: 2)

### Slot 2 - GroundSmashAbility
- Daño en área instantáneo (AD ratio)
- Targeting: Circle (IndicatorType: 1)

### Slot 3 - DashAbility
- Dash rápido en dirección
- Targeting: None (IndicatorType: 0)

### Slot 4 - GroundTrailAbility
- Trail de fuego que aplica DoT
- Targeting: Trail (IndicatorType: 3)
- Usa prefab TrailZone con BoxCollider trigger

## Bugs Críticos Arreglados (2026-04-08)
1. ✅ `EnemyEntity.NotifyKillReward()` - implementado, da oro/XP y spawnea drop
2. ✅ `AoEZone.cs` - daño descomentado y funcional
3. ✅ `EquipmentComponent` - stats aplicados correctamente al héroe
4. ✅ `GoldDrop` - recolección funciona por distancia (sin depender de collider del jugador)
5. ✅ `Habilidad 4` - GroundTrailAbility asignada al prefab del jugador

## Notas Técnicas
- El jugador no tiene Collider en su prefab base - GoldDrop usa detección por distancia
- TrailZone requiere BoxCollider con isTrigger=true
- GroundTrailAbility.Initialize() configura automáticamente el targeting type

## Última actualización
2026-04-08
