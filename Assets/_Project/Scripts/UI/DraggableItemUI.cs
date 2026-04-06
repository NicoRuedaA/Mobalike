using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MMORPG.UI
{
    public class DraggableItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public MMORPG.Inventory.ItemData itemData;
        public Transform parentAfterDrag;
        public Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void Start()
        {
            if (itemData != null && itemData.icon != null)
            {
                image.sprite = itemData.icon;
                image.color = Color.white;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
            image.raycastTarget = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.SetParent(parentAfterDrag);
            image.raycastTarget = true;
            
            // "Iman": Forzar que el icono encaje exactamente en el centro de su nuevo padre
            RectTransform rect = GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
