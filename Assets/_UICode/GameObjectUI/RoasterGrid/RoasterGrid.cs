using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using static UnityEngine.Mathf;
using static Unity.Mathematics.math;

namespace Kaizerwald
{
    public class RoasterGrid : BaseGrid<RoasterSlot>
    {
        [SerializeField] private GameObject SlotItemPrefab;
        [SerializeField] private List<CardDetails> RoasterCards;

        protected override void Awake()
        {
            base.Awake();
            CreateSlots(RoasterCards.Count);
            FillButtonSlots();
            AdjustGridSizeToSlots();
        }
        
        private void FillButtonSlots()
        {
            int size = min(RoasterCards.Count, AvailableSlots.Length);
            for (int i = 0; i < size; i++)
            {
                AvailableSlots[i].AddItem<RoasterItem>(SlotItemPrefab, RoasterCards[i]);
            }
        }
    }
}
