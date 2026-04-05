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

            // 1. Leer los inputs WASD (o flechas) del Input System nuevo
            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;

            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

            // Leer si el jugador está presionando Shift
            bool isSprinting = Keyboard.current.shiftKey.isPressed;
            entity.Movement.SetSprint(isSprinting);

            if (inputDirection.sqrMagnitude > 0.01f)
            {
                if (mainCamera == null) mainCamera = Camera.main;
                if (mainCamera == null) return;

                // 2. Hacer el movimiento relativo a la cámara isométrica
                // "W" va hacia donde la cámara está mirando (sin la inclinación Y)
                Vector3 forward = mainCamera.transform.forward;
                forward.y = 0;
                forward.Normalize();

                Vector3 right = mainCamera.transform.right;
                right.y = 0;
                right.Normalize();

                // Calcular la dirección final basada en la perspectiva de la cámara
                Vector3 moveDir = forward * inputDirection.z + right * inputDirection.x;

                // 3. El Cerebro ordena al Cuerpo que se mueva en esa dirección
                entity.Movement.MoveDirection(moveDir);
            }
            else
            {
                // Si no hay input, decirle que se detenga
                entity.Movement.MoveDirection(Vector3.zero);
            }
        }
    }
}