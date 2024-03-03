using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using static UnityEngine.Mathf;
using static Unity.Mathematics.math;

namespace Kaizerwald
{
    public abstract class BaseGrid<T> : MonoBehaviour
    where T : BaseSlot
    {
        [SerializeField] protected GameObject SlotFramePrefab;

        protected RectTransform RectTransform;
        protected GridLayoutGroup GridLayoutGroup;
        public T[] AvailableSlots { get; private set; }

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            GridLayoutGroup = GetComponent<GridLayoutGroup>();
        }

        public void CreateSlots(int numSlots)
        {
            int numColumnConstraint = Max(1,GridLayoutGroup.constraintCount);
            AvailableSlots = new T[CeilToInt(Max(1,numSlots) / (float)numColumnConstraint) * numColumnConstraint];
            for (int i = 0; i < AvailableSlots.Length; i++)
            {
                AvailableSlots[i] = Instantiate(SlotFramePrefab, transform).GetComponent<T>();
            }
        }

        protected void AdjustGridSizeToSlots()
        {
            float2 size = CalculateGridSize();
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }
        
        protected float2 CalculateGridSize()
        {
            int2 numCellXY = int2(GridLayoutGroup.constraintCount, AvailableSlots.Length / GridLayoutGroup.constraintCount);
            float2 cellsBound = (float2)GridLayoutGroup.cellSize * numCellXY;
            float2 spacesBound = (numCellXY - 1) * (float2)GridLayoutGroup.spacing;
            return cellsBound + spacesBound + int2(GridLayoutGroup.padding.horizontal, GridLayoutGroup.padding.vertical);
        }
    }
}
