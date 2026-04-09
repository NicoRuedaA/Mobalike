using UnityEngine;
using UnityEngine.EventSystems;

namespace MobaGameplay.UI.Inventory
{
    public class InventorySlotUI : MonoBehaviour, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            // Comprobar si el slot de inventario está vacío buscando hijos con el componente DraggableItemUI
            if (GetComponentInChildren<DraggableItemUI>() == null)
            {
                GameObject dropped = eventData.pointerDrag;
                if (dropped != null)
                {
                    DraggableItemUI draggableItem = dropped.GetComponent<DraggableItemUI>();
                    if (draggableItem != null)
                    {
                        draggableItem.parentAfterDrag = transform;
                    }
                }
            }
        }
    }
}
