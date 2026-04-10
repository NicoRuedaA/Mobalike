using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using MobaGameplay.Core;
using MobaGameplay.Abilities;
using MobaGameplay.UI.Targeting;
using MobaGameplay.Combat;

namespace MobaGameplay.Controllers
{
    /// <summary>
    /// Main player input handler for MOBA gameplay.
    /// Processes all player input: movement, combat, abilities, targeting.
    /// Uses Input System for modern input handling.
    /// </summary>
    public class PlayerInputController : BaseController
    {
        #region Constants

        private const float MOVEMENT_DEADZONE = 0.01f;
        private const float CAMERA_CACHE_INTERVAL = 0.5f;
        private const float CHARGE_MIN_THRESHOLD = 0.1f;

        #endregion

        #region Fields

        // Cached references
        private Camera mainCamera;
        private Plane groundPlane;
        private HoverOutline currentHovered;
        
        // Movement cache
        private Vector3 lastValidMoveDir = Vector3.zero;
        
        // Camera direction cache
        private float cameraCacheTimer = 0f;
        private Vector3 cachedCameraForward = Vector3.forward;
        private Vector3 cachedCameraRight = Vector3.right;
        
// Cached combat reference
        private RangedCombat cachedRangedCombat;

        // Ability system routing — prefer new AbilitySystem over old AbilityController
        private bool usesNewAbilitySystem;
        private AbilitySystem cachedAbilitySystem;
        private bool abilitySystemResolved;

        private void Update()
        {
            // Early exit if invalid state
            if (!IsEntityValid) return;
            if (!ValidateInputDevices()) return;
            if (!ValidateMainCamera()) return;

            // Calculate common values
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray mouseRay = mainCamera.ScreenPointToRay(mousePosition);

            UpdateGroundPlane();
            UpdateCameraDirectionCache();

            // Process all input systems
            ProcessHover(mouseRay);
            ProcessAiming(mouseRay);
            ProcessMovement();
            ProcessAbilities();
            ProcessCombat();
            ProcessDash();
        }

        #endregion

        #region Ability System

        /// <summary>
        /// Resolves which ability system to use (new or old).
        /// Uses lazy initialization to avoid Awake execution order issues.
        /// </summary>
        private void ResolveAbilitySystem()
        {
            if (abilitySystemResolved) return;

            cachedAbilitySystem = entity?.AbilitySystem;
            usesNewAbilitySystem = cachedAbilitySystem != null;
            abilitySystemResolved = true;

            #if UNITY_EDITOR
            if (usesNewAbilitySystem)
                Debug.Log($"[PlayerInputController] Using NEW AbilitySystem (slots: {cachedAbilitySystem.SlotCount})");
            else if (entity?.Abilities != null)
                Debug.Log("[PlayerInputController] Using OLD AbilityController");
            else
                Debug.Log("[PlayerInputController] NO ability system found!");
            #endif
        }

        #endregion

        #region Validation

        private bool ValidateInputDevices()
        {
            if (Keyboard.current == null || Mouse.current == null) return false;
            return true;
        }

        private bool ValidateMainCamera()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            return mainCamera != null;
        }

        #endregion

        #region Camera Cache

        /// <summary>
        /// Updates the camera direction cache.
        /// Caches camera forward and right vectors every CAMERA_CACHE_INTERVAL seconds
        /// to avoid expensive calculations every frame.
        /// </summary>
        private void UpdateCameraDirectionCache()
        {
            cameraCacheTimer -= Time.deltaTime;
            
            if (cameraCacheTimer <= 0f)
            {
                cameraCacheTimer = CAMERA_CACHE_INTERVAL;
                
                // Cache camera forward (flattened to XZ plane)
                cachedCameraForward = mainCamera.transform.forward;
                cachedCameraForward.y = 0;
                cachedCameraForward.Normalize();
                
                // Cache camera right (flattened to XZ plane)
                cachedCameraRight = mainCamera.transform.right;
                cachedCameraRight.y = 0;
                cachedCameraRight.Normalize();
            }
        }

        /// <summary>
        /// Updates the ground plane used for mouse-to-world position calculations.
        /// The plane is positioned at the entity's Y position to handle
        /// different terrain heights correctly.
        /// </summary>
        private void UpdateGroundPlane()
        {
            if (entity != null)
            {
                groundPlane.SetNormalAndPosition(
                    Vector3.up, 
                    new Vector3(0, entity.transform.position.y, 0)
                );
            }
        }

        #endregion

        #region Hover

        /// <summary>
        /// Processes hover detection for targeting.
        /// Performs a raycast from the mouse position to detect objects with HoverOutline component.
        /// Automatically clears hover when moving over UI or when no valid target is hit.
        /// </summary>
        /// <param name="mouseRay">The ray from the mouse position.</param>
        private void ProcessHover(Ray mouseRay)
        {
            // Don't process hover when over UI
            if (IsPointerOverUI())
            {
                ClearHover();
                return;
            }

            if (Physics.Raycast(mouseRay, out RaycastHit hit))
            {
                HoverOutline outline = hit.collider.GetComponentInParent<HoverOutline>();
                
                if (outline != null && IsObjectValid(outline.gameObject))
                {
                    if (currentHovered != outline)
                    {
                        ClearHover();
                        currentHovered = outline;
                        currentHovered.SetHover(true);
                    }
                }
                else
                {
                    ClearHover();
                }
            }
            else
            {
                ClearHover();
            }
        }

        /// <summary>
        /// Clears the current hover state by disabling outline on the previously hovered object.
        /// Safely handles null checks to prevent errors during object destruction.
        /// </summary>
        private void ClearHover()
        {
            if (currentHovered != null && IsObjectValid(currentHovered.gameObject))
            {
                currentHovered.SetHover(false);
            }
            currentHovered = null;
        }

        /// <summary>
        /// Checks if the mouse cursor is currently over a UI element.
        /// Used to prevent world interactions when clicking on UI.
        /// </summary>
        /// <returns>True if pointer is over any UI element, false otherwise.</returns>
        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Validates that a Unity Object is not null or destroyed.
        /// Uses ReferenceEquals to detect destroyed objects (which == null returns false for).
        /// </summary>
        /// <param name="obj">The Object to validate.</param>
        /// <returns>True if object is valid, false if null or destroyed.</returns>
        private bool IsObjectValid(Object obj)
        {
            return obj != null && !ReferenceEquals(obj, null);
        }

        #endregion

        #region Aiming

        /// <summary>
        /// Processes aiming input and updates character rotation.
        /// The character always faces the mouse cursor position projected onto the ground plane.
        /// Also applies the "aiming" movement penalty when right mouse button is held.
        /// </summary>
        /// <param name="mouseRay">The ray from the mouse position.</param>
        private void ProcessAiming(Ray mouseRay)
        {
            Vector3 mouseHitPoint = GetGroundHitPoint(mouseRay);
            bool isAiming = Mouse.current.rightButton.isPressed;
            
            // Always look at mouse position
            entity.Movement.LookAtPoint(mouseHitPoint);
            
            // Apply aim movement penalty
            entity.Movement.SetAiming(isAiming);
        }

        /// <summary>
        /// Calculates the world position where the mouse ray intersects the ground plane.
        /// Used for character rotation and ability targeting.
        /// </summary>
        /// <param name="ray">The ray to intersect with the ground plane.</param>
        /// <returns>The world position on the ground plane, or Vector3.zero if no intersection.</returns>
        private Vector3 GetGroundHitPoint(Ray ray)
        {
            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            return Vector3.zero;
        }

        #endregion

        #region Movement

        /// <summary>
        /// Processes movement input from keyboard.
        /// Calculates world-space direction from camera-relative input and sends it to movement system.
        /// Sprint is disabled while aiming (right click held).
        /// </summary>
        private void ProcessMovement()
        {
            Vector2 input = GetMovementInput();
            Vector3 moveDir = CalculateWorldMoveDirection(input);
            
            bool isAiming = Mouse.current.rightButton.isPressed;
            bool isSprinting = Keyboard.current.shiftKey.isPressed;
            
            // Update sprint state (disabled while aiming)
            entity.Movement.SetSprint(isSprinting && !isAiming);

            if (moveDir.sqrMagnitude > MOVEMENT_DEADZONE)
            {
                lastValidMoveDir = moveDir;
                entity.Movement.MoveDirection(moveDir);
            }
            else
            {
                entity.Movement.MoveDirection(Vector3.zero);
            }
        }

        /// <summary>
        /// Reads raw movement input from keyboard (WASD and arrow keys).
        /// Supports both primary and alternative key bindings.
        /// </summary>
        /// <returns>Normalized 2D input vector (x = horizontal, y = vertical).</returns>
        private Vector2 GetMovementInput()
        {
            float horizontal = 0f;
            float vertical = 0f;

            // WASD
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;

            return new Vector2(horizontal, vertical).normalized;
        }

        /// <summary>
        /// Converts camera-relative input to world-space movement direction.
        /// Uses cached camera vectors for performance.
        /// </summary>
        /// <param name="input">Camera-relative input vector.</param>
        /// <returns>World-space movement direction.</returns>
        private Vector3 CalculateWorldMoveDirection(Vector2 input)
        {
            if (input.sqrMagnitude < MOVEMENT_DEADZONE)
            {
                return Vector3.zero;
            }

            return cachedCameraForward * input.y + cachedCameraRight * input.x;
        }

        #endregion

        #region Abilities

        /// <summary>
        /// Processes ability input for keys 1, 2, and 3.
        /// Handles both targeting (on key press) and executing (on key release).
        /// Also handles ability cancellation on right-click.
        /// Routes through AbilitySystem (new) if present, falls back to AbilityController (old).
        /// </summary>
        private void ProcessAbilities()
        {
            ResolveAbilitySystem();

            if (usesNewAbilitySystem)
            {
                ProcessAbilitiesNewSystem();
            }
            else
            {
                ProcessAbilitiesOldSystem();
            }
        }

        /// <summary>
        /// Processes ability input using the new data-driven AbilitySystem.
        /// Slot indices are 0-based internally.
        /// </summary>
        private void ProcessAbilitiesNewSystem()
        {
            if (cachedAbilitySystem == null) return;

            // Slot 1 → index 0, Slot 2 → index 1, etc.
            ProcessAbilityInputNewSystem(KeyCode.Alpha1, 0);
            ProcessAbilityInputNewSystem(KeyCode.Alpha2, 1);
            ProcessAbilityInputNewSystem(KeyCode.Alpha3, 2);
            ProcessAbilityInputNewSystem(KeyCode.Alpha4, 3);

            // Cancel targeting on right-click (if not over UI)
            if (Mouse.current.rightButton.wasPressedThisFrame && !IsPointerOverUI())
            {
                if (cachedAbilitySystem.HasActiveTargeting)
                {
                    cachedAbilitySystem.CancelTargeting();
                }
            }
        }

        /// <summary>
        /// Processes input for a single ability slot in the new system.
        /// Starts targeting on press, executes on release if targeting this slot.
        /// </summary>
        /// <param name="key">Legacy KeyCode for this ability slot.</param>
        /// <param name="slotIndex">0-based slot index in AbilitySystem.</param>
        private void ProcessAbilityInputNewSystem(KeyCode key, int slotIndex)
        {
            var instance = cachedAbilitySystem.GetAbilityInstance(slotIndex);
            if (instance == null) return;

            Key inputKey = ConvertToInputKey(key);
            if (inputKey == Key.None) return;

            bool isTargetingThis = cachedAbilitySystem.ActiveTargetingIndex == slotIndex;

            // Start targeting on key press
            if (Keyboard.current[inputKey].wasPressedThisFrame)
            {
                cachedAbilitySystem.TryStartTargeting(slotIndex);
            }

            // Execute on key release if targeting this slot
            if (Keyboard.current[inputKey].wasReleasedThisFrame && isTargetingThis)
            {
                ExecuteActiveAbility();
            }
        }

        /// <summary>
        /// Processes ability input using the old AbilityController.
        /// Kept for backward compatibility during migration.
        /// </summary>
        private void ProcessAbilitiesOldSystem()
        {
            if (entity.Abilities == null) return;

            // Process ability key presses
            ProcessAbilityInput(KeyCode.Alpha1, entity.Abilities.Ability1, 
                () => entity.Abilities.TryStartTargetingAbility1());
            
            ProcessAbilityInput(KeyCode.Alpha2, entity.Abilities.Ability2,
                () => entity.Abilities.TryStartTargetingAbility2());
            
            ProcessAbilityInput(KeyCode.Alpha3, entity.Abilities.Ability3,
                () => entity.Abilities.TryStartTargetingAbility3());

            ProcessAbilityInput(KeyCode.Alpha4, entity.Abilities.Ability4,
                () => entity.Abilities.TryStartTargetingAbility4());

            // Cancel targeting on right-click (if not over UI)
            if (Mouse.current.rightButton.wasPressedThisFrame && !IsPointerOverUI())
            {
                if (entity.Abilities.HasActiveTargeting)
                {
                    entity.Abilities.CancelTargeting();
                }
            }
        }

        /// <summary>
        /// Processes input for a single ability slot.
        /// Starts targeting on press, executes on release if still targeting.
        /// </summary>
        /// <param name="key">The KeyCode for this ability slot.</param>
        /// <param name="ability">The ability to process.</param>
        /// <param name="executeIfActive">Callback to execute when ability is activated.</param>
        private void ProcessAbilityInput(KeyCode key, Object ability, System.Action executeIfActive)
        {
            if (ability == null) return;

            // Check if this specific ability is being targeted
            bool isTargetingThis = IsTargetingSpecificAbility(ability);
            
            // Convert KeyCode to InputSystem.Key
            Key inputKey = ConvertToInputKey(key);
            if (inputKey != Key.None && Keyboard.current[inputKey].wasPressedThisFrame)
            {
                executeIfActive();
            }
            
            if (inputKey != Key.None && Keyboard.current[inputKey].wasReleasedThisFrame && isTargetingThis)
            {
                ExecuteActiveAbility();
            }
        }

        /// <summary>
        /// Converts Unity's legacy KeyCode to Input System's Key enum.
        /// Required because PlayerInputController uses the new Input System.
        /// </summary>
        /// <param name="keyCode">Legacy KeyCode to convert.</param>
        /// <returns>Corresponding Input System Key, or Key.None if not supported.</returns>
        private Key ConvertToInputKey(KeyCode keyCode)
        {
            return keyCode switch
            {
                KeyCode.Alpha1 => Key.Digit1,
                KeyCode.Alpha2 => Key.Digit2,
                KeyCode.Alpha3 => Key.Digit3,
                KeyCode.Alpha4 => Key.Digit4,
                KeyCode.Alpha5 => Key.Digit5,
                KeyCode.Alpha6 => Key.Digit6,
                KeyCode.Alpha7 => Key.Digit7,
                KeyCode.Alpha8 => Key.Digit8,
                KeyCode.Alpha9 => Key.Digit9,
                KeyCode.Alpha0 => Key.Digit0,
                _ => Key.None
            };
        }

        /// <summary>
        /// Checks if the specified ability is currently being targeted.
        /// </summary>
        /// <param name="ability">The ability to check.</param>
        /// <returns>True if this ability is the active targeting ability.</returns>
        private bool IsTargetingSpecificAbility(Object ability)
        {
            return entity.Abilities?.ActiveTargetingAbility == ability;
        }

        #endregion

        #region Combat

        /// <summary>
        /// Processes combat input: charged attack mechanics.
        /// 
        /// Flow:
        /// 1. Right-click held → isAiming = true
        /// 2. Left-click held while aiming → Start charging
        /// 3. While charging → Update charge progress each frame
        /// 4. Right-click released → Cancel charge (attack becomes normal)
        /// 5. Left-click released → Fire attack (charged if was aiming)
        /// 
        /// Combat is blocked when:
        /// - An ability is in targeting mode
        /// - Pointer is over UI
        /// </summary>
        private void ProcessCombat()
        {
            bool isAiming = Mouse.current.rightButton.isPressed;
            bool overUI = IsPointerOverUI();

            // Cancel charge if we stopped aiming
            if (cachedRangedCombat != null && cachedRangedCombat.IsCharging && !isAiming)
            {
                cachedRangedCombat.ResetCharge();
            }
            
            // Update charge progress
            if (cachedRangedCombat != null && cachedRangedCombat.IsCharging)
            {
                cachedRangedCombat.UpdateCharge();
            }
            
            // Start charge on left-click while aiming
            if (Mouse.current.leftButton.isPressed && !overUI)
            {
                TryStartCharging(isAiming);
            }
            
            // Fire on left-click release
            if (Mouse.current.leftButton.wasReleasedThisFrame && !overUI)
            {
                TryFireAttack();
            }
        }

        /// <summary>
        /// Attempts to start a charged attack.
        /// Requires: aiming (right-click), combat available.
        /// </summary>
        /// <param name="isAiming">Whether the player is currently aiming.</param>
        private void TryStartCharging(bool isAiming)
        {
            if (!isAiming) return;
            if (!CanCombat()) return;
            
            if (cachedRangedCombat != null && !cachedRangedCombat.IsCharging)
            {
                cachedRangedCombat.StartCharging();
            }
        }

        /// <summary>
        /// Fires a basic attack.
        /// If was charging while aiming, fires a charged attack.
        /// Uses cachedRangedCombat if available to ensure correct attack type.
        /// </summary>
        private void TryFireAttack()
        {
            if (!CanCombat()) return;
            
            // Use ranged combat specifically if available
            if (cachedRangedCombat != null)
            {
                cachedRangedCombat.BasicAttack();
            }
            else if (entity.Combat != null)
            {
                entity.Combat.BasicAttack();
            }
        }

        /// <summary>
        /// Checks if combat actions are currently allowed.
        /// Combat is blocked when targeting an ability or combat component is missing.
        /// Checks both new AbilitySystem and old AbilityController for targeting state.
        /// </summary>
        /// <returns>True if combat is allowed, false otherwise.</returns>
        private bool CanCombat()
        {
            if (entity.Combat == null) return false;

            ResolveAbilitySystem();

            // Block combat if either ability system has active targeting
            if (usesNewAbilitySystem)
            {
                if (cachedAbilitySystem != null && cachedAbilitySystem.HasActiveTargeting) return false;
            }
            else
            {
                if (entity.Abilities?.HasActiveTargeting == true) return false;
            }

            return true;
        }

        #endregion

        #region Dash

        /// <summary>
        /// Processes dash input (Space key).
        /// Calculates dash direction based on current input, last movement, or facing direction.
        /// </summary>
        private void ProcessDash()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Vector3 dashDir = GetDashDirection();
                entity.Movement.Dash(dashDir);
            }
        }

        /// <summary>
        /// Determines the dash direction with fallback priority:
        /// 1. Current WASD input direction
        /// 2. Last valid movement direction (for dash after stopping)
        /// 3. Current facing direction (fallback)
        /// </summary>
        /// <returns>Normalized dash direction vector.</returns>
        private Vector3 GetDashDirection()
        {
            Vector2 input = GetMovementInput();
            Vector3 currentDir = CalculateWorldMoveDirection(input);
            
            // Priority 1: Current input direction
            if (currentDir.sqrMagnitude > MOVEMENT_DEADZONE)
            {
                return currentDir.normalized;
            }
            
            // Priority 2: Last valid movement direction
            if (lastValidMoveDir.sqrMagnitude > MOVEMENT_DEADZONE)
            {
                return lastValidMoveDir.normalized;
            }
            
            // Priority 3: Current facing direction
            return entity.transform.forward;
        }

        #endregion

        #region Ability Execution

        /// <summary>
        /// Executes the currently targeted ability at the mouse position.
        /// Performs a raycast to determine world position and any target entity.
        /// Called when releasing an ability key while targeting.
        /// Routes through AbilitySystem (new) if present, falls back to AbilityController (old).
        /// </summary>
        private void ExecuteActiveAbility()
        {
            ResolveAbilitySystem();

            // Determine which system is active
            bool hasTargeting = usesNewAbilitySystem
                ? (cachedAbilitySystem != null && cachedAbilitySystem.HasActiveTargeting)
                : (entity.Abilities != null && entity.Abilities.HasActiveTargeting);

            if (!hasTargeting) return;
            
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray aimRay = mainCamera.ScreenPointToRay(mousePosition);
            
            Vector3 aimPoint = Vector3.zero;
            BaseEntity targetEntity = null;

            if (Physics.Raycast(aimRay, out RaycastHit hit))
            {
                aimPoint = hit.point;
                targetEntity = hit.collider.GetComponent<BaseEntity>();
            }
            else if (groundPlane.Raycast(aimRay, out float distance))
            {
                aimPoint = aimRay.GetPoint(distance);
            }

            // Route execution to the active system
            if (usesNewAbilitySystem)
            {
                cachedAbilitySystem.ExecuteTargeting(aimPoint, targetEntity);
            }
            else
            {
                entity.Abilities.ExecuteTargeting(aimPoint, targetEntity);
            }
        }

        #endregion

        #region Cleanup

        private void OnDisable()
        {
            ClearHover();
        }

        #endregion
    }
}
