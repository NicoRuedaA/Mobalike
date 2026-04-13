# 🏗️ Nueva Feature / Feature Request

## Contexto del Proyecto
- **Namespace:** `MobaGameplay.*`
- **Arquitectura:** MVC — Model (ScriptableObjects) / View (UI) / Controller (MonoBehaviours)
- **Sistema de habilidades:** Data-driven (`AbilityData` SO → `AbilitySystem` → `AbilityInstance`)
- **Sistema de entidades:** `BaseEntity` → `HeroEntity` / `EnemyEntity`
- **UI:** `PlayerHUD`, `FloatingStatusBar`, `AbilitySlotUI`, `TargetingManager`
- **Regla:** NO leer .unity, .prefab, .meta — usar MCP para escenas/prefabs

## Instrucciones
1. **Explorar** antes de codear — revisar qué existe y qué se puede reutilizar
2. **Diseñar** la solución antes de implementar — indicar archivos nuevos y modificados
3. **Implementar** siguiendo convenciones del proyecto:
   - `[SerializeField] private` + properties (no campos públicos)
   - Namespace `MobaGameplay.*`
   - ScriptableObjects para datos configurables
4. **Verificar** — indicar cómo probar la feature

## Estructura de carpetas
```
Assets/_Project/Scripts/[Categoría]/NuevoScript.cs
Assets/_Project/Prefabs/[Categoría]/       ← usar MCP
Assets/Resources/ScriptableObjects/         ← para .asset SO
```

---

**Uso:**

```
Feature: [DESCRIPCION DE LA FEATURE]

Seguí la plantilla en Documentation/prompts/02-feature-request.md
```