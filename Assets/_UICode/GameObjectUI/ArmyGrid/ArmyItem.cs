using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Kaizerwald
{
    public class ArmyItem : BaseItem, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public void OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log($"ArmyItem: OnBeginDrag");
            ParentAfterDrag = ItemTransform.parent;
            ItemTransform.SetParent(ItemTransform.root);
            ItemTransform.SetAsLastSibling();
            ItemRaycastImage.raycastTarget = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            //Debug.Log($"ArmyItem: OnDrag");
            ItemTransform.position = Mouse.current.position.value;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            //Debug.Log($"ArmyItem: OnEndDrag");
            if (ShouldBeDestroyed()) return;
            ItemTransform.SetParent(ParentAfterDrag);
            ItemRaycastImage.raycastTarget = true;
        }
    }
}
