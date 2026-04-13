# 📋 Recuperar Contexto + Continuar

## Instrucciones para la IA
1. Ejecutar `mem_context` y `mem_search` con keywords del tema
2. Leer `Documentation/memoria.md` para estado general
3. Leer `Documentation/roadmap.md` para progreso
4. Leer `Documentation/rules.md` para reglas de tokens
5. Resumir estado y preguntar por dónde seguir

## Contexto del Proyecto (resumen rápido)
- **Motor:** Unity 6 (6000.3.11f1) + URP 17.3.0
- **Namespace:** `MobaGameplay.*`
- **Tests:** 62 pasando en EditMode
- **MCP:** Puerto 9000 (para escenas/prefabs)
- **Reglas:** NO leer .unity, .prefab, .meta, .mat, .asset, .controller

## Estructura de Carpetas
```
Assets/_Project/
├── Art/Animations, Icons, Shaders, Materials
├── Data/Abilities, Classes
├── Documentation/        ← .md del proyecto
│   └── prompts/          ← ESTAS PLANTILLAS
├── Prefabs/Abilities, Characters, Environment, Items, UI
├── Scenes/SampleScene
├── Scripts/
│   ├── Abilities/Core, Behaviors, Types, Projectiles, AreaEffects
│   ├── AI, Animation, Combat, Controllers, Core, Editor, Game
│   ├── Inventory, Movement, UI/Targeting, VFX, Visuals
└── Shaders/

Assets/Resources/ScriptableObjects/
├── Items/Equipment/      ← 80+ items
├── Heroes/Mage, Warrior  ← HeroClass assets
└── Game/Waves/           ← WaveData
```

---

**Uso directo (copiar y pegar):**

```
recupera el contexto de la ultima sesion. Sigamos con [TEMA]

Seguí la plantilla en Documentation/prompts/06-recuperar-contexto.md
```