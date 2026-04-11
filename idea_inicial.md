# Idea del Proyecto

## ¿Qué quiero construir?

"MobaGameplay" es un prototipo de MOBA (Multiplayer Online Battle Arena) en Unity con mecánicas estilo League of Legends. Permite a un jugador controlar un héroe con ataque cargado (charged attack), 4 habilidades, y enfrentar oleadas de enemigos con IA automática. Incluye sistemas de progresión (nivel, oro, XP), UI estilo LoL (barras de vida flotantes, textos de daño animados), y efectos visuales (proyectiles, áreas de efecto, outlines).

**Estado actual:** Proyecto funcional con 62 tests unitarios passando ✅

## Usuarios objetivo

Desarrolladores de juegos que quieren un template base de MOBA, o usuarios que quieren probar mecánicas de combate MOBA en un entorno Unity.

## Requisitos funcionales

**Sistema de Combate:**
- Ataque básico cargado (hold LMB mientras se apunta con RMB)
- 4 slots de habilidades con cooldown y costo de mana
- Sistema de proyectiles (lineal, teledirigido, área de efecto)
- Sistema de daño con tipos (Físico/Mágico/True) y críticos
- Reducción de daño por-armadura y resistencia mágica

**Sistema de Progresión:**
- Héroe con stats que escalan por nivel (máx nivel 18)
- Sistema de oro y experiencia con recompensas por muerte de enemigos
- Sistema de oleadas con estados (Idle → Preparing → Spawning → Active → Cleared)

**Sistema de IA Enemiga:**
- Máquina de estados: Idle → Patrol → Chase → Attack → Retreat → Dead
- Detección de jugador por OverlapSphere
- Recompensas de oro y XP al morir

**Sistema de UI:**
- HUD con barras de HP/mana y slots de habilidades
- Floating damage text con animación pop y soporte para críticos
- Floating status bar estilo LoL con tick marks por shader
- Sistema de targeting con indicadores (círculo, línea, trail)
- Inventario toggleable con Tab

**Sistema de Movimiento:**
- Movimiento estilo MOBA (WASD) con sprint
- Dash con cooldown
- Cámara estilo hero shooter con zoom y edge panning

**Input:**
- Input System (New Input) para controls modernos
- WASD: movimiento | Shift: sprint | Space: dash
- RMB hold: apuntar | LMB hold/release: cargar y disparar
- 1-4: habilidades | Tab: inventario | Scroll: zoom

## Restricciones técnicas

- Motor: **Unity 6 (6000.3.11f1)** con URP 17.3.0
- Input: Input System Package 1.7.0+
- UI Text: TextMeshPro
- .NET Standard 2.1
- Testing: 62 tests unitarios pasando (EditMode)
- Assembly definitions: MobaGameplay.Runtime, Editor, Tests
- Sin multiplayer (prototipo single-player)
- Sin gestión de items persistente ni shop
- Sin sistema de runes o talentos
- Sin mapa completo con lanes (prototipo de combate)

## Lo que NO debe hacer

- No incluye networking ni modo multiplayer
- No incluye sistema de minions ni estructuras (torres, inhibidores)
- No incluye sistema de items comprables (inventario solo visual)
- No incluye chat,朋友 o sistemas sociales
- No incluye sonido o música
- No incluye guardado/carga de partida
- No incluye tutorial o campañas
