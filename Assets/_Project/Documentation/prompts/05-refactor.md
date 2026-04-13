# 📝 Refactorización

## Contexto del Proyecto
- **Namespace:** `MobaGameplay.*`
- **Regla de encapsulación:** `[SerializeField] private` + properties públicas
- **Regla:** NO campos públicos innecesarios
- **Testing:** 62 tests existentes — refactor NO debe romperlos

## Proceso
1. **Identificar** el scope del refactor (archivos afectados)
2. **Diseñar** la solución antes de codear
3. **Implementar** paso a paso, un bloque a la vez
4. **Verificar** que los tests siguen pasando
5. **Commitear** por bloque lógico

## Convenciones de Código
```csharp
// ❌ MAL — campo público
public float moveSpeed = 5f;

// ✅ BIEN — serializado privado + property
[SerializeField] private float moveSpeed = 5f;
public float MoveSpeed => moveSpeed;
```

```csharp
// ❌ MAL — MonoBehaviour acoplado
public class Ability : MonoBehaviour {
    public GameObject projectilePrefab;
}

// ✅ BIEN — data-driven con SO
[CreateAssetMenu(fileName = "NewAbility", menuName = "MobaGameplay/Ability")]
public class AbilityData : ScriptableObject {
    [SerializeField] private float damage;
    public float Damage => damage;
}
```

---

**Uso:**

```
Refactorizar [SISTEMA] — [DESCRIPCIÓN]

Seguí la plantilla en Documentation/prompts/05-refactor.md
```