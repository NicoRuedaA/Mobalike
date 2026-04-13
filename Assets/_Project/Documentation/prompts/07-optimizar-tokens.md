# 🎯 Optimizar Tokens

## Contexto
El proyecto tiene un `.opencodeignore` que excluye archivos pesados.
Esta plantilla ayuda a reducir el consumo de tokens en sesiones.

## Estrategias de Ahorro

### 1. Archivos ya ignorados (.opencodeignore)
```
*.unity, *.prefab, *.meta, *.mat, *.asset
*.controller, *.anim, *.overrideController
Packages/, Library/, Builds/, Settings/
```

### 2. Reglas para la IA (rules.md)
- Sé conciso. No repitas código que no cambió
- Si solo cambias una línea, muestra solo el fragmento relevante
- Evita explicaciones extensas a menos que te las pida
- Para escenas/prefabs → SIEMPRE MCP, nunca leer YAML

### 3. Usar plantillas de prompts
En vez de explicar el contexto cada vez, usa las plantillas en
`Documentation/prompts/` que ya tienen el contexto pre-cargado.

### 4. Guardar contexto en Engram
Después de cada sesión significativa, el sistema guarda:
- Decisiones de arquitectura
- Bugs encontrados y fixes
- Patrones del proyecto

### 5. Leer solo lo necesario
Antes de leer un archivo, verificar si la info está en:
- `Documentation/memoria.md` — Estado general
- `Documentation/roadmap.md` — Progreso de fases
- `.atl/skill-registry.md` — Convenciones del proyecto

---

**Uso:**

```
Sigamos con la optimizacion de tokens

Seguí la plantilla en Documentation/prompts/07-optimizar-tokens.md
```