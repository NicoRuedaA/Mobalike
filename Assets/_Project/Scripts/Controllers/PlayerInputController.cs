using UnityEngine;
using UnityEngine.InputSystem;
using MobaGameplay.Core;

namespace MobaGameplay.Controllers
{
    public class PlayerInputController : BaseController
    {
        private Camera mainCamera;

        protected override void Awake()
        {
            base.Awake();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (entity.Movement == null) return;
            if (Keyboard.current == null) return;

            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;

            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
            bool isSprinting = Keyboard.current.shiftKey.isPressed;
            entity.Movement.SetSprint(isSprinting);

            if (inputDirection.sqrMagnitude > 0.01f)
            {
                if (mainCamera == null) mainCamera = Camera.main;
                if (mainCamera == null) return;

                Vector3 forward = mainCamera.transform.forward;
                forward.y = 0;
                forward.Normalize();

                Vector3 right = mainCamera.transform.right;
                right.y = 0;
                right.Normalize();

                Vector3 moveDir = forward * inputDirection.z + right * inputDirection.x;
                entity.Movement.MoveDirection(moveDir);
            }
            else
            {
                entity.Movement.MoveDirection(Vector3.zero);
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (entity.Combat != null) entity.Combat.BasicAttack();
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                entity.Movement.Jump();
            }
        }
    }
}
