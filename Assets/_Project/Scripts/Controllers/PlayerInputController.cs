using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using MobaGameplay.Core;
using MobaGameplay.UI.Targeting;

namespace MobaGameplay.Controllers
{
    public class PlayerInputController : BaseController
    {
        private Camera mainCamera;
        private Plane groundPlane;
        private HoverOutline currentHovered;

        protected override void Awake()
        {
            base.Awake();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (entity.Movement == null) return;
            if (Keyboard.current == null || Mouse.current == null) return;

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            // 0. HOVER (Raycast cada frame)
            Ray hoverRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                if (Physics.Raycast(hoverRay, out RaycastHit hoverHit))
                {
                    HoverOutline outline = hoverHit.collider.GetComponentInParent<HoverOutline>();
                    if (outline != null)
                    {
                        if (currentHovered != outline)
                        {
                            if (currentHovered != null) currentHovered.SetHover(false);
                            currentHovered = outline;
                            currentHovered.SetHover(true);
                        }
                    }
                    else if (currentHovered != null)
                    {
                        currentHovered.SetHover(false);
                        currentHovered = null;
                    }
                }
                else if (currentHovered != null)
                {
                    currentHovered.SetHover(false);
                    currentHovered = null;
                }
            }
            else if (currentHovered != null)
            {
                currentHovered.SetHover(false);
                currentHovered = null;
            }

            // 1. INPUT DE RATÓN Y ESTADO DE APUNTADO
            groundPlane = new Plane(Vector3.up, new Vector3(0, entity.transform.position.y, 0));
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Vector3 mouseHitPoint = Vector3.zero;
            if (groundPlane.Raycast(ray, out float enter))
            {
                mouseHitPoint = ray.GetPoint(enter);
            }

            bool isAiming = Mouse.current.rightButton.isPressed;
            bool isShooting = Mouse.current.leftButton.isPressed;
            
            // La rotación clásica: siempre mirar al ratón
            entity.Movement.LookAtPoint(mouseHitPoint);

            // Penalización de movimiento solo si apuntamos explícitamente con Click Derecho
            entity.Movement.SetAiming(isAiming);

            // 2. MOVIMIENTO (WASD)
            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;

            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
            bool isSprinting = Keyboard.current.shiftKey.isPressed;
            
            entity.Movement.SetSprint(isSprinting && !isAiming);

            Vector3 moveDir = Vector3.zero;

            if (inputDirection.sqrMagnitude > 0.01f)
            {
                Vector3 forward = mainCamera.transform.forward;
                forward.y = 0;
                forward.Normalize();

                Vector3 right = mainCamera.transform.right;
                right.y = 0;
                right.Normalize();

                moveDir = forward * inputDirection.z + right * inputDirection.x;
                entity.Movement.MoveDirection(moveDir);
            }
            else
            {
                entity.Movement.MoveDirection(Vector3.zero);
            }

            // 3. HABILIDADES (QUICK CAST: Mantener para apuntar, soltar para lanzar)
            if (entity.Abilities != null)
            {
                // Ability 1
                if (Keyboard.current.digit1Key.wasPressedThisFrame)
                    entity.Abilities.TryStartTargetingAbility1();
                
                if (Keyboard.current.digit1Key.wasReleasedThisFrame && entity.Abilities.ActiveTargetingAbility == entity.Abilities.Ability1)
                    ExecuteActiveAbility();

                // Ability 2
                if (Keyboard.current.digit2Key.wasPressedThisFrame)
                    entity.Abilities.TryStartTargetingAbility2();
                
                if (Keyboard.current.digit2Key.wasReleasedThisFrame && entity.Abilities.ActiveTargetingAbility == entity.Abilities.Ability2)
                    ExecuteActiveAbility();

                // Ability 3
                if (Keyboard.current.digit3Key.wasPressedThisFrame)
                    entity.Abilities.TryStartTargetingAbility3();
                
                if (Keyboard.current.digit3Key.wasReleasedThisFrame && entity.Abilities.ActiveTargetingAbility == entity.Abilities.Ability3)
                    ExecuteActiveAbility();
            }

            // 5. COMBATE - Ataque Cargado (Click Izquierdo)
            // Mantener click izq = cargar mientras apunta
            // Soltar click izq = disparar (si no hay habilidad activa)
            
            // Actualizar carga si estamos cargando
            var rangedCombat = entity.Combat as MobaGameplay.Combat.RangedCombat;
            if (rangedCombat != null && rangedCombat.IsCharging)
            {
                rangedCombat.UpdateCharge();
            }
            
            if (Mouse.current.leftButton.isPressed)
            {
                if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                {
                    // Si está apuntando (click derecho) y no hay habilidad activa, empezar a cargar
                    if (isAiming && entity.Combat != null && (entity.Abilities == null || entity.Abilities.ActiveTargetingAbility == null))
                    {
                        if (rangedCombat != null && !rangedCombat.IsCharging && rangedCombat.CanCharge)
                        {
                            rangedCombat.StartCharging();
                        }
                    }
                }
            }
            
            // Soltar click izq = disparar
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                {
                    // Si no hay habilidad activa y hay combate
                    if (entity.Combat != null && (entity.Abilities == null || entity.Abilities.ActiveTargetingAbility == null))
                    {
                        entity.Combat.BasicAttack();
                    }
                }
            }

            // 6. DASH / SALTO (Espacio)
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                // entity.Movement.Jump(); // Comentado por ahora si queremos Dash en Espacio
                Vector3 dashDir = moveDir != Vector3.zero ? moveDir.normalized : entity.transform.forward;
                entity.Movement.Dash(dashDir);
            }

            // 7. CANCELAR HABILIDAD (Click Derecho)
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                {
                    if (entity.Abilities != null && entity.Abilities.ActiveTargetingAbility != null)
                    {
                        entity.Abilities.CancelTargeting();
                    }
                }
            }
        }

        private void ExecuteActiveAbility()
        {
            if (entity.Abilities == null || entity.Abilities.ActiveTargetingAbility == null) return;
            
            // Comentamos la restricción de UI para asegurar que siempre se lance
            // if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            // {
            //     entity.Abilities.CancelTargeting();
            //     return;
            // }

            Ray aimRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Vector3 aimPoint = Vector3.zero;
            BaseEntity targetEnt = null;

            if (Physics.Raycast(aimRay, out RaycastHit hit))
            {
                aimPoint = hit.point;
                targetEnt = hit.collider.GetComponent<BaseEntity>();
            }
            else if (groundPlane.Raycast(aimRay, out float d))
            {
                aimPoint = aimRay.GetPoint(d);
            }

            entity.Abilities.ExecuteTargeting(aimPoint, targetEnt);
        }
    }
}