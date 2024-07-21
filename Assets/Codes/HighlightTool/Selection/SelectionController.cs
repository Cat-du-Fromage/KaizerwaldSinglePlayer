using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using static UnityEngine.Vector3;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;
using static PlayerControls;
using static UnityEngine.InputSystem.InputAction;

using Bounds = UnityEngine.Bounds;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

using Kaizerwald.Utilities.Core;

namespace Kaizerwald
{
    public sealed class SelectionController : HighlightController
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private const float SPHERE_RADIUS = 0.25f;
        private const float RAYCAST_DISTANCE = ushort.MaxValue;
        
        private readonly LayerMask SelectionLayer;
        private readonly RaycastHit[] Hits = new RaycastHit[2];
        
        private SelectionActions selectionControl;
        private bool ClickDragPerformed;
        private Vector2 StartLMouse, EndLMouse;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public SelectionSystem SelectionSystem { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private HighlightRegister PreselectionRegister => SelectionSystem.PreselectionRegister;
        private HighlightRegister SelectionRegister => SelectionSystem.SelectionRegister;
        private bool IsCtrlPressed => selectionControl.LockSelection.IsPressed();
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public SelectionController(HighlightSystem system, PlayerControls controls, LayerMask unitLayer)
        {
            SelectionSystem = (SelectionSystem)system;
            SelectionLayer = unitLayer;
            selectionControl = controls.Selection;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override void OnEnable()
        {
            if (!selectionControl.enabled) selectionControl.Enable();
            selectionControl.MouseMove.EnablePerformEvent(OnMouseHover);
            selectionControl.LeftMouseClickAndMove.EnableAllEvents(OnDragSelectionStart, OnDragSelectionPerformed, OnSelection);
        }

        public override void OnDisable()
        {
            selectionControl.MouseMove.DisablePerformEvent(OnMouseHover);
            selectionControl.LeftMouseClickAndMove.DisableAllEvents(OnDragSelectionStart, OnDragSelectionPerformed, OnSelection);
            selectionControl.Disable();
        }
        
        public override void OnUpdate() { return; }
        
        public override void OnFixedUpdate()
        {
            if (ClickDragPerformed)
            {
                GroupPreselectionRegiments();
            }
            else
            {
                CheckMouseHoverUnit();
            }
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PRESELECTION ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ EVENT FUNCTIONS ◈◈◈◈◈◈                                                                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Single Unit Preselection ◇◇◇◇◇◇                                                                    │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void OnMouseHover(CallbackContext context)
        {
            EndLMouse = context.ReadValue<Vector2>();
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Multiple Unit Preselection ◇◇◇◇◇◇                                                                  │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void OnDragSelectionStart(CallbackContext context)
        {
            StartLMouse = EndLMouse = context.ReadValue<Vector2>();
            ClickDragPerformed = false;
        }
        
        private void OnDragSelectionPerformed(CallbackContext context)
        {
            ClickDragPerformed = IsDragSelection();
            if (!ClickDragPerformed) return;
            GroupPreselectionRegiments();
        }
        
        private bool IsDragSelection() => Vector2.SqrMagnitude(EndLMouse - StartLMouse) >= 128;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Mouse Hover Check ◇◇◇◇◇◇                                                                           │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private bool NoHits(int numHits)
        {
            if (numHits != 0) return false;
            ClearPreselection();
            return true;
        }
        
        private void CheckMouseHoverUnit()
        {
            Ray singleRay = PlayerCamera.ScreenPointToRay(EndLMouse);
            int numHits = SphereCastNonAlloc(singleRay, SPHERE_RADIUS, Hits, RAYCAST_DISTANCE, SelectionLayer.value);
            if (NoHits(numHits)) return;
            
            MouseHoverSingleEntity(singleRay, numHits);
            if(Hits.Length == 0) return;
            
            Array.Clear(Hits, 0, Hits.Length);
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ PRESELECTION TYPE ◈◈◈◈◈◈                                                                                ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Single Preselection ◇◇◇◇◇◇                                                                         │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void MouseHoverSingleEntity(in Ray singleRay, int numHits)
        {
            if (!Hits[0].transform.TryGetComponent(out HighlightUnit unit)) return;
            
            HighlightRegiment candidate = GetPreselectionCandidate(singleRay, unit, numHits);
            if (candidate.IsPreselected) return;
            
            ClearPreselection();
            AddPreselection(candidate);
        }

        private HighlightRegiment GetPreselectionCandidate(in Ray singleRay, HighlightUnit unit, int numHits)
        {
            //sphere cast caught more than 1 target
            HighlightRegiment candidate = unit.HighlightRegimentAttach;
            if (numHits > 1 && !AreUnitsFromSameRegiment() && Raycast(singleRay, out RaycastHit unitHit, INFINITY, SelectionLayer.value))
            {
                //bool hit = Raycast(singleRay, out RaycastHit unitHit, INFINITY, SelectionLayer.value);
                bool hasComponent = unitHit.transform.TryGetComponent(out HighlightUnit highlightUnit);
                candidate = hasComponent ? highlightUnit.HighlightRegimentAttach : candidate;
            }
            return candidate;
            //┌▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁┐
            //▕  ◇◇◇◇◇◇ Internal Methods ◇◇◇◇◇◇                                                                        ▏
            //└▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔┘
            bool AreUnitsFromSameRegiment()
            {
                if (!Hits[1].transform.TryGetComponent(out HighlightUnit highlightUnit)) return false;
                return candidate.RegimentID == highlightUnit.HighlightRegimentAttach.RegimentID;
            }
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Group Preselection ◇◇◇◇◇◇                                                                          │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void GroupPreselectionRegiments()
        {
            Bounds selectionBounds = GetViewportBounds(StartLMouse, EndLMouse);
            foreach (HighlightRegiment regiment in SelectionSystem.Regiments)
            {
                if (regiment == null) continue;
                bool isInSelectionRectangle = CheckUnitsInRectangleBounds(regiment);
                if (!regiment.IsPreselected && isInSelectionRectangle) //NOT preselected but in rectangle
                {
                    AddPreselection(regiment);
                }
                else if (regiment.IsPreselected && !isInSelectionRectangle) //Preselected and NOT in Rectangle
                {
                    RemovePreselection(regiment);
                }
            }
            return;
            //┌▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁┐
            //▕  ◇◇◇◇◇◇ Internal Methods ◇◇◇◇◇◇                                                                        ▏
            //└▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔┘
            bool CheckUnitsInRectangleBounds(HighlightRegiment regiment)
            {
                foreach (Transform unitHighlightTransform in regiment.Transforms)
                {
                    if (unitHighlightTransform == null) continue;
                    Vector3 unitPositionInRect = PlayerCamera.WorldToViewportPoint(unitHighlightTransform.position);
                    if (!selectionBounds.Contains(unitPositionInRect)) continue;
                    return true;
                }
                return false;
            }
        }

        private Bounds GetViewportBounds(in Vector3 startPoint, in Vector3 endPoint)
        {
            Vector3 start = PlayerCamera.ScreenToViewportPoint(startPoint);
            Vector3 end = PlayerCamera.ScreenToViewportPoint(endPoint);
            
            (Vector3 min, Vector3 max) = (Min(start, end), Max(start, end));
            (min.z, max.z) = (PlayerCamera.nearClipPlane, PlayerCamera.farClipPlane);
            
            return new Bounds(min + (max - min) / 2f, max - min);
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Link To System ◈◈◈◈◈◈                                                                               ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void AddPreselection(HighlightRegiment selectableRegiment)
        {
            SelectionSystem.OnShow(selectableRegiment, (int)SelectionSystem.ESelectionRegister.Preselection);
        }

        private void RemovePreselection(HighlightRegiment selectableRegiment)
        {
            SelectionSystem.OnHide(selectableRegiment, (int)SelectionSystem.ESelectionRegister.Preselection);
        }

        private void ClearPreselection()
        {
            SelectionSystem.HideAll((int)SelectionSystem.ESelectionRegister.Preselection);
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ SELECTION ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Selection Methods ◈◈◈◈◈◈                                                                                ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void OnSelection(CallbackContext context)
        {
            DeselectNotPreselected();
            SelectPreselection();
            if (!ClickDragPerformed) return;
            CheckMouseHoverUnit();
            ClickDragPerformed = false;
        }
        
        private void SelectPreselection()
        {
            foreach (HighlightRegiment selectable in PreselectionRegister.ActiveHighlights)
            {
                if (selectable.IsSelected) continue;
                SelectionSystem.OnShow(selectable, (int)SelectionSystem.ESelectionRegister.Selection);
            }
        }
        
        private void DeselectNotPreselected()
        {
            // we remove element from list, by iterating reverse we stay inbounds
            if (IsCtrlPressed) return;
            for (int i = SelectionRegister.ActiveHighlights.Count - 1; i > -1; i--)
            {
                HighlightRegiment regiment = SelectionRegister.ActiveHighlights[i];
                if (regiment.IsPreselected) continue;
                SelectionSystem.OnHide(regiment, (int)SelectionSystem.ESelectionRegister.Selection);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Order Callback ◈◈◈◈◈◈                                                                                   ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
    }
}
