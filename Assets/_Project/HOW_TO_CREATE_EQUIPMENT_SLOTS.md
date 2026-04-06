# How to Create and Remove Equipment Slots

## Introduction & Rule
This guide explains how to add new equipment slots (like Rings, Amulets) and how to safely remove them without causing compilation errors. 

**CRITICAL RULE:** All scripts and project files MUST be located within the `Assets/_Project/` directory.

---

## Adding a New Equipment Slot

### Step 1: Backend Update
To define the item at the data level, you must update the core data structure.
1. Open `Assets/_Project/Scripts/Inventory/ItemData.cs`.
2. Locate the `EquipSlot` enum.
3. Add your new slot name (e.g., `Ring`, `Amulet`) to the enum list.

### Step 2: Frontend Logic Update
We use an MVC (Model-View-Controller) architecture, meaning the frontend UI is decoupled from the backend data to keep the codebase modular and clean. Therefore, the UI needs its own reference.
1. Open `Assets/_Project/Scripts/UI/EquipmentSlotUI.cs`.
2. Locate the `EquipmentType` enum.
3. Add the exact same slot name to this enum.

### Step 3: Visual UI Creation (Manual)
Because the UI is visually driven, you must set up the slot in the Unity Editor:
1. Open your main UI Scene and locate the `EquipmentPanel` in the Canvas.
2. Duplicate an existing slot GameObject (e.g., `EquipSlot_Head`).
3. Rename the duplicated GameObject appropriately (e.g., `EquipSlot_Ring`).
4. Select the new GameObject and look at the Inspector. On the `EquipmentSlotUI` component, change the **Slot Type** dropdown to your new enum value.
5. Expand the GameObject in the hierarchy and change its child Text label to match the new slot name.
6. **Layout Gotcha:** The `EquipmentPanel` uses **absolute positioning** (`anchoredPosition`) to create a "paper-doll" cross layout, *not* automatic Layout Groups. You must manually drag the new slot in the Scene view to its desired visual position.

---

## How to SAFELY Remove a Slot (The CS0117 Bug)

Removing an equipment slot can easily break the game if not done in the correct order. We previously encountered the **CS0117 Bug** when removing "Gloves" and "Pants". If you delete an enum value first, Unity will throw compilation errors (e.g., `error CS0117: 'EquipSlot' does not contain a definition for 'Gloves'`) because scene objects or Editor scripts are still referencing the deleted value.

**Follow these exact steps to safely remove a slot:**

1. **Step 1: Delete the UI GameObject.** Go into the Unity Editor, find the slot in the `EquipmentPanel` (e.g., `EquipSlot_Gloves`), and delete it from the scene. Save the scene.
2. **Step 2: Clean up Scripts.** Search the codebase for any custom Editor scripts, test scripts, or hardcoded logic that references the specific enum value and delete or update those references.
3. **Step 3: Remove the Enum Values.** Finally, open `ItemData.cs` and `EquipmentSlotUI.cs` and remove the values from the `EquipSlot` and `EquipmentType` enums.

Following this order guarantees you won't get locked out by Unity's compiler!