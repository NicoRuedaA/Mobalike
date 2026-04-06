using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMORPG.Inventory
{
    public class EquipmentComponent : MonoBehaviour
    {
        private Dictionary<EquipSlot, ItemData> equippedItems = new Dictionary<EquipSlot, ItemData>();

        public event Action<EquipSlot, ItemData> OnEquipmentChanged;
        public event Action<int, int, int> OnStatsChanged; // HP, STR, AGI

        public int TotalHP { get; private set; }
        public int TotalSTR { get; private set; }
        public int TotalAGI { get; private set; }

        public void EquipItem(ItemData item, out ItemData previousItem)
        {
            previousItem = null;
            if (item == null || item.itemType != ItemType.Equipment || item.equipSlot == EquipSlot.None)
                return;

            if (equippedItems.ContainsKey(item.equipSlot))
            {
                previousItem = equippedItems[item.equipSlot];
            }

            equippedItems[item.equipSlot] = item;
            OnEquipmentChanged?.Invoke(item.equipSlot, item);
            RecalculateStats();
        }

        public void UnequipItem(EquipSlot slot, out ItemData removedItem)
        {
            removedItem = null;
            if (equippedItems.ContainsKey(slot))
            {
                removedItem = equippedItems[slot];
                equippedItems.Remove(slot);
                OnEquipmentChanged?.Invoke(slot, null);
                RecalculateStats();
            }
        }

        private void RecalculateStats()
        {
            TotalHP = 0;
            TotalSTR = 0;
            TotalAGI = 0;

            foreach (var item in equippedItems.Values)
            {
                if (item != null)
                {
                    TotalHP += item.hpBonus;
                    TotalSTR += item.strBonus;
                    TotalAGI += item.agiBonus;
                }
            }

            OnStatsChanged?.Invoke(TotalHP, TotalSTR, TotalAGI);
        }
    }
}