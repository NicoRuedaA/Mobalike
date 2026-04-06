# How to Manually Create and Add Items in the MMORPG System

**ALL project files must reside inside the `Assets/_Project/` folder.**

## 1. Creating a New Item Data Asset
1. In the Project window, navigate to the folder where you want to store your new item (e.g., `Assets/_Project/ScriptableObjects/Items`).
2. Right-click in the Project window and select **Create > MMORPG > Item Data**.
3. Name your new Scriptable Object file appropriately (e.g., `ItemData_MyNewItem`).

## 2. Assigning Stats to the Item
1. Select the newly created Item Data asset in the Project window.
2. In the Inspector window, you will see the fields for your item. Fill them in:
   - **Item ID:** Assign a unique integer ID (must be distinct from other items).
   - **Item Name:** The display name of the item.
   - **Description:** A short description of the item.
   - **Icon:** Drag and drop an appropriate Sprite to serve as the item's visual representation.
   - **Stackable:** Check this if multiple copies of this item can be placed in a single slot.
   - **Max Stack:** Set the maximum number of items per stack (if stackable is true).

## 3. Adding the Item to an Inventory Slot UI
To manually place this item into an inventory slot in the UI (bypassing any automated Editor tools), follow these steps:
1. Locate your main HUD/Canvas object that contains the Inventory Grid in your scene or prefab.
2. Expand the Inventory Grid in the Hierarchy to find the individual slot GameObjects (usually named like `Slot_0`, `Slot_1`, etc.).
3. Select an empty or desired slot GameObject.
4. Expand the slot to find its child GameObject responsible for the visual representation (often named `Icon` or `ItemImage`).
   - Find the `Image` component on this object.
   - Assign your item's Sprite to the **Source Image** field.
   - Ensure the `Image` component is enabled and its Color alpha is set to fully visible (e.g., `255`).
5. Find the `DraggableItemUI` component (this will be attached to either the slot itself or its `Icon` child).
   - Drag and drop your newly created Item Data Scriptable Object from the Project window into the **Item Data** field of the `DraggableItemUI` component.
6. Save your scene or prefab. The item is now fully configured and will appear in the UI!