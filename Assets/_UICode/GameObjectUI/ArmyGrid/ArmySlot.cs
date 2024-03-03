using UnityEngine;
using UnityEngine.EventSystems;

namespace Kaizerwald
{
    public class ArmySlot : BaseSlot, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            //Debug.Log($"ArmySlot.OnDrop: From {name}: OnDrop: num child = {SlotTransform.childCount}");
            if (SlotTransform.childCount > 0 || !eventData.pointerDrag.TryGetComponent(out BaseItem item)) return;
            item.SetParentAfterDrag(SlotTransform);
        }
    }
}