Sé conciso. No repitas código que no ha cambiado. Si solo cambias una línea, muestra solo el fragmento relevante, no el archivo completo. Evita explicaciones extensas a menos que te las pida.

## Archivos Prohibidos (ahorro de tokens)

El proyecto tiene un `.opencodeignore` que ignora automáticamente:
- `*.unity`, `*.prefab`, `*.meta`
- `*.mat`, `*.asset`, `*.controller`, `*.anim`
- `Packages/`, `Library/`, `Builds/`, `StarterAssets/`

**NUNCA leas estos archivos directamente.** Si necesitás info de una escena o prefab, usá las herramientas MCP de Unity (`manage_gameobject`, `manage_components`).
