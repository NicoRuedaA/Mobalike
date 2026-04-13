# 🔍 Investigar Escena / Prefab via MCP

## Contexto
- **MCP endpoint:** `http://127.0.0.1:9000/mcp`
- **Headers requeridos:** `Content-Type: application/json` + `Accept: application/json, text/event-stream`
- **Regla:** NUNCA leer archivos .unity, .prefab, .asset directamente — siempre MCP
- **Session ID:** El MCP requiere session tracking (ver `Documentation/prompts/`)

## Flujo MCP
1. Primero leer `mcpforunity://project/info` para detectar paquetes
2. Verificar estado con `mcpforunity://editor/state`
3. Usar `manage_gameobject` para inspeccionar jerarquía
4. Usar `manage_components` para leer/escribir componentes
5. Usar `batch_execute` para múltiplos operations (máx 25 por batch)
6. Usar `manage_camera(action="screenshot")` para verificar visualmente

## Comandos MCP comunes
| Acción | Tool | Parámetros |
|--------|------|-----------|
| Inspeccionar GameObject | `manage_gameobject` | `action="inspect"` |
| Crear GameObject | `manage_gameobject` | `action="create"` |
| Listar componentes | `manage_components` | `action="list"` |
| Leer propiedad | `manage_components` | `action="get_properties"` |
| Escribir propiedad | `manage_components` | `action="set_properties"` |
| Screenshot | `manage_camera` | `action="screenshot", include_image=True` |

---

**Uso:**

```
Investiga [OBJETO/ESCENA] usando MCP

Seguí la plantilla en Documentation/prompts/03-mcp-inspect.md
```