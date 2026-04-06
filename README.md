[English](README.md) | [Español](README.es.md)

# Mobalike

A modular, scalable MOBA (Multiplayer Online Battle Arena) gameplay prototype built with Unity.

> [!NOTE]
> This project is currently in early development. The primary focus is establishing a robust, scalable foundation for character locomotion, camera controls, and entity architecture.

## Features

* **MOBA-Style Locomotion:** Classic right-click "click and go" movement constrained to the XZ plane.
* **Smart Isometric Camera:** Auto-configuring camera system featuring edge panning, smooth dampening, and player-centering (Spacebar).
* **Modular Architecture:** Built using a scalable "Brain and Body" component pattern, decoupling input logic from execution to seamlessly support both Player Characters and AI entities.
* **Fluid Animations:** Smooth animation blending and state transitions using Unity's Starter Assets.
* **MMORPG-style Inventory & Equipment System:** Features Drag & Drop functionality, a Paper-Doll equipment layout (Head, Chest, Weapon, Pants, Boots), and a scalable `ItemData` ScriptableObject backend.

## Tools & Editor Scripts

* **Wiki Mass Item Generator:** Fetches icons and stats from the Supervive wiki API to automatically generate ScriptableObject item data.

## Architecture

To avoid monolithic "God Objects" (like a massive `PlayerMovement` script), the codebase is strictly separated into specialized components that communicate via events and abstract interfaces. 

* **Core (`BaseEntity`)**: The central identity of a character (e.g., Hero, Minion).
* **Controllers**: The "Brain". Classes like `PlayerInputController` handle input (mouse/keyboard) or AI logic, and issue commands to the entity.
* **Movement**: The "Body". Classes like `XZPlaneMovement` execute movement commands, handle math or NavMesh logic, and report their current velocity.
* **Animation**: Pure listeners. The `CharacterAnimator` reads the entity's velocity and updates the Animator state without tightly coupling to the input or movement scripts.

## Getting Started

### Prerequisites

* **Unity Editor** (Recommended: 2022.3 LTS or newer)
* Git

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/NicoRuedaA/Mobalike.git
   ```
2. Open the cloned folder in **Unity Hub**.
3. Navigate to and open the main scene located at `Assets/_Project/Scenes/SampleScene.unity`.
4. Press **Play** in the Unity Editor to test the prototype.

> [!IMPORTANT]
> This project relies on the **New Input System**. Ensure your Unity Player Settings have "Active Input Handling" set to either `Input System Package (New)` or `Both`.

## Project Structure

The core codebase is isolated within the `Assets/_Project/` directory to separate it from third-party packages and imported assets.

```text
Assets/_Project/
├── Art/            # Custom models, materials, and visual assets
├── Scenes/         # Gameplay and testing scenes
└── Scripts/        # Core C# source code
    ├── Animation/  # Animation blend controllers and event receivers
    ├── Controllers/# Input handlers and AI "Brains"
    ├── Core/       # Abstract base classes and interfaces
    └── Movement/   # Concrete movement logic (XZ Plane, NavMesh)
```