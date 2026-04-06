using UnityEngine;
using UnityEngine.InputSystem;

namespace MMORPG.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Tooltip("The parent UI panel representing the inventory or HUD")]
        public GameObject inventoryPanel;

        private void Start()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                if (inventoryPanel != null)
                {
                    inventoryPanel.SetActive(!inventoryPanel.activeSelf);
                }
            }
        }
    }
}