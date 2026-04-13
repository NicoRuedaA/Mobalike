using UnityEngine;
using MobaGameplay.Inventory;

namespace MobaGameplay.Testing
{
    /// <summary>
    /// Script de testing para el sistema de equipamiento.
    /// Agrega atajos de teclado para equipar/desequipar items rápidamente.
    /// Solo para desarrollo/testing.
    /// </summary>
    public class EquipmentTester : MonoBehaviour
    {
        [Header("Test Items")]
        [SerializeField] private ItemData testWeapon;
        [SerializeField] private ItemData testArmor;
        [SerializeField] private ItemData testBoots;
        [SerializeField] private ItemData testHelmet;

        private EquipmentComponent equipment;

        private void Start()
        {
            equipment = GetComponent<EquipmentComponent>();
            if (equipment == null)
            {
                Debug.LogError("[EquipmentTester] No EquipmentComponent found!");
                enabled = false;
            }
        }

        private void Update()
        {
            // Teclas para equipar items de prueba
            if (Input.GetKeyDown(KeyCode.F1) && testWeapon != null)
            {
                equipment.EquipItem(testWeapon, out var previous);
                Debug.Log($"[EquipmentTester] Equipped: {testWeapon.itemName}");
            }
            
            if (Input.GetKeyDown(KeyCode.F2) && testArmor != null)
            {
                equipment.EquipItem(testArmor, out var previous);
                Debug.Log($"[EquipmentTester] Equipped: {testArmor.itemName}");
            }
            
            if (Input.GetKeyDown(KeyCode.F3) && testBoots != null)
            {
                equipment.EquipItem(testBoots, out var previous);
                Debug.Log($"[EquipmentTester] Equipped: {testBoots.itemName}");
            }
            
            if (Input.GetKeyDown(KeyCode.F4) && testHelmet != null)
            {
                equipment.EquipItem(testHelmet, out var previous);
                Debug.Log($"[EquipmentTester] Equipped: {testHelmet.itemName}");
            }

            // Tecla para desequipar todo
            if (Input.GetKeyDown(KeyCode.F5))
            {
                UnequipAll();
                Debug.Log("[EquipmentTester] Unequipped all items");
            }

            // Tecla para mostrar stats
            if (Input.GetKeyDown(KeyCode.F6))
            {
                equipment.DebugShowEquippedItems();
                GetComponent<MobaGameplay.Core.BaseEntity>()?.DebugShowStats();
            }
        }

        private void UnequipAll()
        {
            foreach (EquipSlot slot in System.Enum.GetValues(typeof(EquipSlot)))
            {
                if (slot != EquipSlot.None)
                {
                    equipment.UnequipItem(slot, out var removed);
                }
            }
        }

        private void OnGUI()
        {
            // Mostrar instrucciones en pantalla
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("=== Equipment Tester ===");
            GUILayout.Label("F1: Equip Test Weapon");
            GUILayout.Label("F2: Equip Test Armor");
            GUILayout.Label("F3: Equip Test Boots");
            GUILayout.Label("F4: Equip Test Helmet");
            GUILayout.Label("F5: Unequip All");
            GUILayout.Label("F6: Show Stats");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
