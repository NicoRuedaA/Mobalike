# 📋 Plantillas de Prompts — Mobalike

Plantillas pre-cargadas con contexto del proyecto para ahorrar tokens.
Copiar el bloque de "Uso" al inicio de una sesión nueva.

| # | Plantilla | Uso |
|---|-----------|-----|
| 01 | `bug-fix.md` | Investigar y corregir bugs |
| 02 | `feature-request.md` | Implementar nueva funcionalidad |
| 03 | `mcp-inspect.md` | Inspeccionar escenas/prefabs via MCP |
| 04 | `testing.md` | Crear tests unitarios |
| 05 | `refactor.md` | Refactorizar código existente |
| 06 | `recuperar-contexto.md` | Iniciar sesión recuperando estado |
| 07 | `optimizar-tokens.md` | Estrategias de ahorro de tokens |
| 08 | `commit-push.md` | Commit con conventional commits |

## Cómo usar

1. Abrir la plantilla correspondiente
2. Reemplazar los marcadores `[DESCRIPCION]` con lo que necesitás
3. Copiar el bloque de "Uso" y pegarlo como prompt

## Contexto del Proyecto (resumen)

- **Motor:** Unity 6 (6000.3.11f1) + URP 17.3.0
- **Namespace:** `MobaGameplay.*`
- **Tests:** 62 EditMode passing
- **MCP:** Puerto 9000
- **Regla de oro:** NO leer .unity, .prefab, .meta — usar MCP
- **Regla de encapsulación:** `[SerializeField] private` + properties