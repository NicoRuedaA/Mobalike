using UnityEngine;

namespace MobaGameplay.UI
{
    public class Billboard : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera != null)
            {
                // Hace que el Canvas mire siempre en la misma dirección que la cámara
                transform.forward = mainCamera.transform.forward;
            }
            else
            {
                mainCamera = Camera.main;
            }
        }
    }
}
