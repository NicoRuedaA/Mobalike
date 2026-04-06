using System;
using UnityEngine;

namespace MMORPG.Inventory
{
    public class InventoryComponent : MonoBehaviour
    {
        public const int InventorySize = 20;
        public ItemData[] items = new ItemData[InventorySize];

        public event Action<int, ItemData> OnSlotChanged;

        public bool AddItem(ItemData item)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null)
                {
                    items[i] = item;
                    OnSlotChanged?.Invoke(i, item);
                    return true;
                }
            }
            return false;
        }

        public void RemoveItem(int index)
        {
            if (index >= 0 && index < items.Length)
            {
                items[index] = null;
                OnSlotChanged?.Invoke(index, null);
            }
        }

        public void SwapItems(int indexA, int indexB)
        {
            if (indexA >= 0 && indexA < items.Length && indexB >= 0 && indexB < items.Length)
            {
                ItemData temp = items[indexA];
                items[indexA] = items[indexB];
                items[indexB] = temp;
                
                OnSlotChanged?.Invoke(indexA, items[indexA]);
                OnSlotChanged?.Invoke(indexB, items[indexB]);
            }
        }
    }
}