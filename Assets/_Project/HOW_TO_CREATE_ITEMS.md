# ⚔️ How to Create Items (MMORPG MVC Architecture)

This guide serves as a comprehensive reference for creating items in the project, grounded in the architectural decisions and past problem-solving experiences. 

**IMPORTANT RULE:** All files, scripts, and assets MUST be placed inside the `Assets/_Project/` directory to maintain project organization.

---

## 🏗️ 1. Architecture Concept (MVC)

The inventory and item system is strictly separated using the **Model-View-Controller (MVC)** architectural pattern:

*   **Model (Backend - `ItemData`):** This represents the raw, immutable data of the item. It is implemented as a Unity `ScriptableObject`. It knows nothing about the UI, the screen resolution, or how it is being dragged. It only contains stats and logic properties.
*   **View/Controller (Frontend - `DraggableItemUI` + `Image`):** This is the visual representation of the item on the HUD and the logic to interact with it. It reads from the `ItemData` to know what icon to display and what stats to show, but it handles the actual Unity UI components, dragging logic, and raycasting.

---

## 🛠️ Step 1: Creating the Backend (`ItemData`)

The backend data defines what the item actually *is*.

1.  Navigate to your intended data folder within `Assets/_Project/` (e.g., `Assets/_Project/Data/Items/`).
2.  Right-click in the Project window and select: **Create > MMORPG > Item Data**.
3.  Name the newly created ScriptableObject appropriately (e.g., `SwordOfThousandTruths`).
4.  Configure the item's properties in the Unity Inspector:
    *   **ItemType:** Is it a weapon, armor, consumable, or material?
    *   **EquipSlot:** If equippable, where does it go (Head, Chest, MainHand, etc.)?
    *   **Stats:** Set the base stats like HP, STR, AGI, etc.

---

## 🖼️ Step 2: The Icon (Sprite) & Image Gotchas

The visual icon requires careful handling due to past bugs.

1.  **Downloading the Asset:** Ensure you are downloading a raw image file. **BUG WARNING:** We previously encountered an issue where HTML pages were downloaded instead of actual `.png` files. Always verify the file is a valid image format.
2.  **Importing to Unity:** Place the `.png` file inside `Assets/_Project/Sprites/Items/`.
3.  **Texture Type (CRITICAL):** Select the imported image in the Project window. In the Inspector, you **MUST** change the `Texture Type` to `Sprite (2D and UI)`. Otherwise, the UI `Image` component will not accept it. Click "Apply".

---

## 🖱️ Step 3: Creating the Frontend (UI)

Now we link the backend data to a visual UI element on the HUD.

1.  In your UI Canvas, create a new UI Image (`GameObject > UI > Image`) or instantiate your Item Prefab.
2.  Add the `DraggableItemUI` component to this GameObject.
3.  **Link the Data:** Drag the `ItemData` ScriptableObject you created in Step 1 into the designated field on the `DraggableItemUI` component in the Inspector.

### 🚨 Crucial UI Bugs to Watch Out For

*   **The `EventSystem` Bug (No Clicks/Drags Registering):**
    If you cannot click or drag the item, the scene is likely missing an EventSystem or using the wrong input module.
    *   **Fix:** Ensure there is an `EventSystem` GameObject in the scene. 
    *   **Input Module:** It MUST be using the `InputSystemUIInputModule` component (from the new Input System), not the legacy `StandaloneInputModule`.
*   **The Raycast Bug (Items Getting Stuck During Drag):**
    When dragging an item, the item itself can block the mouse raycast, preventing the system from detecting what UI element is underneath it (like an inventory slot).
    *   **Fix:** The `Image` component has a `Raycast Target` boolean. The `DraggableItemUI` script must dynamically disable `raycastTarget` on the Image when the drag begins (`OnBeginDrag`), and re-enable it when the drag ends (`OnEndDrag` or `OnDrop`). Ensure this logic remains intact.
---

## ⚙️ Editor Scripting Gotchas (Mass Importing & Scraping)

When writing C# Editor tools to generate items or mass import assets, keep these crucial bugs in mind:

1. **The WebP Thumbnail Bug:** When scraping wikis (like MediaWiki), never use the `src` of `<img>` tags directly. They often serve compressed `.webp` thumbnails but disguise them with a `.png` extension, which causes Unity's `TextureImporter` to fail silently or corrupt the asset. **Fix:** Always use the site's API (e.g., `api.php?action=query&prop=imageinfo`) to get the true, original `.png` binary url.
2. **The AssetDatabase Sprite Cache Bug:** If you import a Texture and convert it to a Sprite (`TextureImporterType.Sprite`) via code, calling `AssetDatabase.LoadAssetAtPath<Sprite>(path)` in the exact same frame will often return `null` because Unity hasn't cached the sub-asset yet.
   *   **Fix:** You must use `Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);` and loop through the array checking `if (asset is Sprite)` to extract it reliably.
