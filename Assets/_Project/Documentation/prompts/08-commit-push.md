# 📦 Git Commit + Push

## Contexto
- **Convención:** Conventional commits (`feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`)
- **Regla:** Hacer commit por bloque lógico (no mega-commits)
- **Regla:** NUNCA agregar "Co-Authored-By" o atribución de IA
- **Regla:** NO pushear a menos que el usuario lo pida explícitamente

## Formato de Commit
```
tipo(alcance): descripción corta

-Detalle opcional 1
-Detalle opcional 2
```

## Tipos
| Tipo | Uso |
|------|-----|
| `feat` | Nueva funcionalidad |
| `fix` | Bug corregido |
| `refactor` | Refactor sin cambio funcional |
| `docs` | Documentación |
| `test` | Tests |
| `chore` | Mantenimiento, config |
| `perf` | Mejora de rendimiento |

## Alcances comunes
`core`, `combat`, `abilities`, `ui`, `inventory`, `ai`, `movement`, `editor`, `docs`, `tests`

---

**Uso:**

```
Commit y push de [LO QUE SE HIZO]

Seguí la plantilla en Documentation/prompts/08-commit-push.md
```