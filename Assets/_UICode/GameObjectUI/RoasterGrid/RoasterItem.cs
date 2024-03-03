using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Kaizerwald
{
    public class RoasterItem : BaseItem, IPointerDownHandler, IPointerUpHandler, IDragHandler , IBeginDragHandler , IEndDragHandler
    {
        [SerializeField] private GameObject ArmyItemPrefabRelated;
        
        private ArmyItem instantiatedArmyItem; //script of the child to get the drag method
        
        public void OnPointerDown(PointerEventData data)
        {
            //Debug.Log($"OnPointerDown");
            instantiatedArmyItem = Instantiate(ArmyItemPrefabRelated, transform).GetComponent<ArmyItem>();
            instantiatedArmyItem.Initialize(CardItem.CardDetails);
        }

        public void OnBeginDrag(PointerEventData data)
        {
            //Debug.Log($"OnBeginDrag: isdrag = {data.dragging}, object {data.pointerDrag}");
            if (instantiatedArmyItem == null) return;
            instantiatedArmyItem.OnBeginDrag(data);
            //IDropHandle NEED those 2!
            data.pointerDrag = instantiatedArmyItem.gameObject;
            data.dragging = true;
        }
        
        public void OnDrag(PointerEventData data)
        {
            if (instantiatedArmyItem == null) return;
            instantiatedArmyItem.OnDrag(data);
        }
        
        public void OnEndDrag(PointerEventData data)
        {
            Debug.Log($"RoasterItem OnEndDrag");
            if (instantiatedArmyItem == null) return;
            instantiatedArmyItem.OnEndDrag(data);
            instantiatedArmyItem = null;
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (data.dragging) return;
            for (int i = ItemTransform.childCount - 1; i > -1; i--)
            {
                Destroy(ItemTransform.GetChild(i).gameObject);
            }
        }
    }
}
