#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

using static Unity.Mathematics.math;
using static UnityEngine.Vector3;
using static UnityEngine.Quaternion;

using float2 = Unity.Mathematics.float2;
using float2x2 = Unity.Mathematics.float2x2;

using Kaizerwald.FormationModule;
using Kaizerwald.Utilities;

using static Kaizerwald.Utilities.Core.KzwMath;
using static Kaizerwald.Utilities.Core.KzwGeometry;

namespace Kaizerwald
{
    public sealed partial class Regiment : OrderedFormationBehaviour<Unit>, IOwnershipInformation
    {
        
        
        public bool DebugShowUnitsAlive = false;
        
        public static bool FieldOfViewTriangle = false;
        public static bool FieldOfViewDebug = false;
        
        public static bool FiringStateTest = false;
        public static bool ShowTargetsFiringStateTest = false;



        private void ShowUnitsAlive()
        {
            if (!DebugShowUnitsAlive) return;
            Vector3 size = Vector3.one / 2f;
            Gizmos.color = Color.yellow;
            foreach (Transform unitTransform in Transforms)
            {
                Gizmos.DrawCube(Position + up() * 2, size);
            }
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ FieldOfView ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ GUI ◈◈◈◈◈◈                                                                                              ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private const int BoxWidth = 150;
        private const int ButtonHeight = 25;
        private const int ButtonWidth = 125;
        private const int DefaultSpace = 4;
        private const int BoxBaseVerticalOffset = 30;
        

        private int FOVOnGUI(int previousBoxWidth)
        {
            int offset = BoxBaseVerticalOffset;
            offset += FieldOfViewGUI(offset, DefaultSpace);
            offset += FieldOfViewTriangleGUI(offset, DefaultSpace);
            FOVBox(offset, previousBoxWidth);
            return BoxWidth;
        }

        private void FOVBox(int previousHeightOffset, int previousBoxWidth)
        {
            (int width, int height) = (BoxWidth + previousBoxWidth, 10 + previousHeightOffset);
            GUI.Box(new Rect(2, 2, width, height), "Debug Field Of View");
        }
        
        private int FieldOfViewGUI(int previousOffset, int space = 2)
        {
            int offset = previousOffset + space;
            bool onFovButton = GUI.Button(new Rect(15, offset, ButtonWidth, ButtonHeight), FieldOfViewDebug ? "Deactivate" : "Activate");
            if (onFovButton)
            {
                FieldOfViewDebug = !FieldOfViewDebug;
            }
            return ButtonHeight + space;
        }
        
        private int FieldOfViewTriangleGUI(int previousOffset, int space = 2)
        {
            int offset = previousOffset + space;
            if (!FieldOfViewDebug) return 0;
            bool toggleTriangle = GUI.Button(new Rect(15, offset, ButtonWidth, ButtonHeight), !FieldOfViewTriangle ? "Show Triangle" : "Hide Triangle");
            if (toggleTriangle)
            {
                FieldOfViewTriangle = !FieldOfViewTriangle;
            }
            return ButtonHeight + space;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                         ◆◆◆◆◆◆ Get Target Attack ◆◆◆◆◆◆                                            ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private int FindTargetOnGUI(int previousBoxWidth)
        {
            int offset = BoxBaseVerticalOffset;
            offset += FindTargetGUI(offset, previousBoxWidth, DefaultSpace);
            offset += ShowUnitsTargetGUI(offset, previousBoxWidth, DefaultSpace);
            FindTargetBox(offset, previousBoxWidth, DefaultSpace);
            return BoxWidth;
        }

        private void FindTargetBox(int previousOffset, int previousBoxWidth, int space = 2)
        {
            (int width, int height) = (150, 10 + previousOffset);
            //GUI.Box(new Rect(2, 2, width, height), "Debug Field Of View");
            GUI.Box(new Rect(previousBoxWidth + space, 2, width, height), "Find Target");
        }

        private int FindTargetGUI(int previousOffset, int previousBoxWidth, int space = 2)
        {
            int offset = previousOffset + space;
            bool onFindTarget = GUI.Button(new Rect(previousBoxWidth + space * 4, offset, ButtonWidth, ButtonHeight), FiringStateTest ? "Deactivate" : "Activate");
            if (onFindTarget)
            {
                FiringStateTest = !FiringStateTest;
            }
            return ButtonHeight + space;
        }
        
        private int ShowUnitsTargetGUI(int previousOffset, int previousBoxWidth, int space = 2)
        {
            if (!FiringStateTest) return 0;
            int offset = previousOffset + space;
            bool onFindTarget = GUI.Button(new Rect(previousBoxWidth + space * 4, offset, ButtonWidth, ButtonHeight), ShowTargetsFiringStateTest ? "HideTargets" : "ShowTargets");
            if (onFindTarget)
            {
                ShowTargetsFiringStateTest = !ShowTargetsFiringStateTest;
            }
            return ButtonHeight + space;
        }
/*
        private void Debug_FiringStateTargetDetection()
        {
            if (!FiringStateTest && BehaviourTree.State == EStates.Idle)
            {
                IdleRegimentState idleState = (IdleRegimentState)StateMachine.CurrentRegimentState;
                if (!idleState.AutoFire) return;
                idleState.AutoFireOff();
            }
            else if (FiringStateTest && BehaviourTree.State == EStates.Idle)
            {
                IdleRegimentState idleState = (IdleRegimentState)StateMachine.CurrentRegimentState;
                if (idleState.AutoFire) return;
                idleState.AutoFireOn();
            }
        }
        */
    }
}
#endif