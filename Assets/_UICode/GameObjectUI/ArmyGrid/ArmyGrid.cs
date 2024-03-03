using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using static Unity.Mathematics.math;

namespace Kaizerwald
{
    public class ArmyGrid : BaseGrid<ArmySlot>
    {
        [SerializeField] private GameObject SlotItemPrefab;
        [SerializeField] private int NumSlots;
        [SerializeField] private List<CardDetails> DefaultItems;

        protected override void Awake()
        {
            base.Awake();
            CreateSlots(NumSlots);
            FillDefaultItems();
            AdjustGridSizeToSlots();
        }

        private void FillDefaultItems()
        {
            int size = min(DefaultItems.Count, AvailableSlots.Length);
            for (int i = 0; i < size; i++)
            {
                AvailableSlots[i].AddItem<ArmyItem>(SlotItemPrefab, DefaultItems[i]);
            }
        }
    }
}