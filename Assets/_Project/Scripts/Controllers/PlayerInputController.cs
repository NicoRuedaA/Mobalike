using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using MobaGameplay.Core;

namespace MobaGameplay.Controllers
{
    public class PlayerInputController : BaseController
    {
        private Camera mainCamera;
        private Plane groundPlane;

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

            // 1. APUNTADO (Rotación hacia el ratón)
            groundPlane = new Plane(Vector3.up, new Vector3(0, entity.transform.position.y, 0));
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                entity.Movement.LookAtPoint(hitPoint);
            }

            // 2. MOVIMIENTO (WASD)
            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;

            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
            bool isSprinting = Keyboard.current.shiftKey.isPressed;
            entity.Movement.SetSprint(isSprinting);

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

            // 3. HABILIDADES (1, 2, 3)
            if (Keyboard.current.digit1Key.wasPressedThisFrame && entity.Abilities != null)
                entity.Abilities.TryStartTargetingAbility1();
            if (Keyboard.current.digit2Key.wasPressedThisFrame && entity.Abilities != null)
                entity.Abilities.TryStartTargetingAbility2();
            if (Keyboard.current.digit3Key.wasPressedThisFrame && entity.Abilities != null)
                entity.Abilities.TryStartTargetingAbility3();

            // 4. COMBATE O CONFIRMAR HABILIDAD (Click Izquierdo)
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Evitar atacar si estamos haciendo click en la interfaz (UI)
                if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                {
                    if (entity.Abilities != null && entity.Abilities.ActiveTargetingAbility != null)
                    {
                        // Estamos apuntando una habilidad, así que el click la lanza
                        Ray aimRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                        Vector3 aimPoint = Vector3.zero;
                        BaseEntity targetEnt = null;

                        // Raycast para encontrar un objetivo válido
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
                    else if (entity.Combat != null) 
                    {
                        // No estamos apuntando, lanzar ataque básico normal
                        entity.Combat.BasicAttack();
                    }
                }
            }

            // 5. SALTO
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                entity.Movement.Jump();
            }

            // 6. DASH Y CANCELAR HABILIDAD (Click Derecho)
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                {
                    if (entity.Abilities != null && entity.Abilities.ActiveTargetingAbility != null)
                    {
                        // Si estamos apuntando, el click derecho cancela la habilidad en lugar de hacer un dash
                        entity.Abilities.CancelTargeting();
                    }
                    else
                    {
                        // Dash en la dirección del movimiento, o si está quieto, hacia donde mira
                        Vector3 dashDir = moveDir != Vector3.zero ? moveDir.normalized : entity.transform.forward;
                        entity.Movement.Dash(dashDir);
                    }
                }
            }
        }
    }
}
