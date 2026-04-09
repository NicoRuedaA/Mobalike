using UnityEngine;
using UnityEngine.EventSystems;

namespace MobaGameplay.UI.Inventory
{
    public class EquipmentSlotUI : MonoBehaviour, IDropHandler
    {
        public enum EquipmentType { Head, Chest, Weapon, Boots, Pants }
        public EquipmentType slotType;

        public void OnDrop(PointerEventData eventData)
        {
            // Comprobar si ya hay un item equipado buscando el componente DraggableItemUI
            if (GetComponentInChildren<DraggableItemUI>() == null)
            {
                GameObject dropped = eventData.pointerDrag;
                if (dropped != null)
                {
                    DraggableItemUI draggableItem = dropped.GetComponent<DraggableItemUI>();
                    if (draggableItem != null && draggableItem.itemData != null)
                    {
                        if (draggableItem.itemData.equipSlot.ToString() == slotType.ToString())
                        {
                            draggableItem.parentAfterDrag = transform;
                        }
                    }
                }
            }
        }
    }
}
