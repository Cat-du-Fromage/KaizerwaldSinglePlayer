using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public class ArmyPanel : MonoBehaviour, IOwnershipInformation
    {
        [field:SerializeField] public ulong OwnerPlayerID { get; private set; }
        [field:SerializeField] public short TeamID { get; private set; }

        public ArmyGrid ArmyGrid { get; private set; }
        
        public ArmySlot[] Slots => ArmyGrid.AvailableSlots;
        
        private void Awake()
        {
            ArmyGrid = GetComponentInChildren<ArmyGrid>();
        }

        public List<ArmyItem> GetArmyItems()
        {
            List<ArmyItem> items = new List<ArmyItem>();
            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].transform.childCount == 0) continue;
                if (!Slots[i].transform.GetChild(0).TryGetComponent(out ArmyItem item)) continue;
                items.Add(item);
            }
            return items;
        }
    }
}
