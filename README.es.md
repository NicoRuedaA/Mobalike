[English](README.md) | [Español](README.es.md)

# Mobalike

Un prototipo modular y escalable de jugabilidad MOBA (Multiplayer Online Battle Arena) construido con Unity.

> [!NOTE]
> Este proyecto se encuentra actualmente en desarrollo temprano. El enfoque principal es establecer una base robusta y escalable para la locomoción de personajes, controles de cámara y la arquitectura de entidades.

## Características

* **Locomoción estilo MOBA:** Clásico movimiento de "hacer clic y caminar" con el botón derecho, restringido al plano XZ.
* **Cámara Isométrica Inteligente:** Sistema de cámara auto-configurable que incluye desplazamiento por los bordes (edge panning), amortiguación suave y centrado en el jugador (Barra espaciadora).
* **Arquitectura Modular:** Construido usando un patrón de componentes escalable de "Cerebro y Cuerpo" (Brain and Body), desacoplando la lógica de entrada de la ejecución para soportar de manera fluida tanto a personajes controlados por el jugador como entidades de IA.
* **Animaciones Fluidas:** Mezcla suave de animaciones y transiciones de estado utilizando los Starter Assets de Unity.

## Arquitectura

Para evitar los "Objetos Dios" monolíticos (como un script gigante `PlayerMovement`), el código está estrictamente separado en componentes especializados que se comunican mediante eventos e interfaces abstractas.

* **Core (`BaseEntity`)**: La identidad central de un personaje (ej. Héroe, Súbdito/Minion).
* **Controladores (Controllers)**: El "Cerebro". Clases como `PlayerInputController` manejan la entrada (ratón/teclado) o la lógica de IA, y envían comandos a la entidad.
* **Movimiento (Movement)**: El "Cuerpo". Clases como `XZPlaneMovement` ejecutan comandos de movimiento, manejan las matemáticas o la lógica del NavMesh, e informan de su velocidad actual.
* **Animación (Animation)**: Puros oyentes. El `CharacterAnimator` lee la velocidad de la entidad y actualiza el estado del Animator sin acoplarse fuertemente a los scripts de entrada o movimiento.

## Empezando

### Prerrequisitos

* **Unity Editor** (Recomendado: 2022.3 LTS o más reciente)
* Git

### Instalación

1. Clona el repositorio:
   ```bash
   git clone https://github.com/NicoRuedaA/Mobalike.git
   ```
2. Abre la carpeta clonada en **Unity Hub**.
3. Navega y abre la escena principal ubicada en `Assets/_Project/Scenes/SampleScene.unity`.
4. Pulsa **Play** en el Unity Editor para probar el prototipo.

> [!IMPORTANT]
> Este proyecto depende del **Nuevo Input System**. Asegúrate de que tus Player Settings de Unity tengan "Active Input Handling" configurado como `Input System Package (New)` o `Both`.

## Estructura del Proyecto

El código central está aislado dentro del directorio `Assets/_Project/` para separarlo de paquetes de terceros y assets importados.

```text
Assets/_Project/
├── Art/            # Modelos personalizados, materiales y recursos visuales
├── Scenes/         # Escenas de juego y pruebas
└── Scripts/        # Código fuente C# central
    ├── Animation/  # Controladores de mezcla de animación y receptores de eventos
    ├── Controllers/# Manejadores de entrada y "Cerebros" de IA
    ├── Core/       # Clases base abstractas e interfaces
    └── Movement/   # Lógica de movimiento concreta (Plano XZ, NavMesh)
```