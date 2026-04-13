# ✅ Tests Unitarios

## Contexto del Proyecto
- **Namespace de tests:** No usan namespace específico
- **Assembly:** `MobaGameplay.Tests.asmdef` en `Assets/Tests/`
- **Referencia:** `MobaGameplace.Tests` → `MobaGameplay.Runtime`
- **Regla:** EditMode tests → `Start()` NO ejecuta → inicializar manualmente
- **Fixtures:** Usar `[SetUp]` para inicializar, `[TearDown]` para limpiar singletons

## Convenciones de Testing
```
Assets/Tests/
├── BaseEntityTests.cs          # 22 tests
├── HeroEntityTests.cs          # 16 tests
├── EquipmentComponentTests.cs  # 12 tests
├── GameStateManagerTests.cs    # 12 tests
└── MobaGameplay.Tests.asmdef
```

## Patrones Conocidos (Gotchas)
1. **EditMode no ejecuta Start()** → flags como `manaInitialized` quedan en `false`
2. **Destroy() loguea error** → usar `LogAssert.Expect(LogType.Error, ...)`
3. **Singletons persisten entre tests** → resetear via reflection en `[TearDown]`
4. **Custom asmdef** → no referenciar `Assembly-CSharp`, usar `Unity.InputSystem`
5. **Namespace** → `UnityEngine.InputSystem` ≠ `Unity.InputSystem` (paquete)

## Template de Test
```csharp
using NUnit.Framework;
using MobaGameplay.Core;

public class NuevoSystemTests
{
    private NuevoSystem _system;

    [SetUp]
    public void SetUp()
    {
        // Crear e inicializar manualmente (Start() no ejecuta en EditMode)
        _system = new GameObject().AddComponent<NuevoSystem>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_system != null) Object.DestroyImmediate(_system.gameObject);
    }

    [Test]
    public void NuevoSystem_Metodo_Esperado()
    {
        // Arrange
        var valor = 10;
        
        // Act
        _system.HacerAlgo(valor);
        
        // Assert
        Assert.AreEqual(valor, _system.Resultado);
    }
}
```

---

**Uso:**

```
Crear tests para [SISTEMA/CLASE]

Seguí la plantilla en Documentation/prompts/04-testing.md
```