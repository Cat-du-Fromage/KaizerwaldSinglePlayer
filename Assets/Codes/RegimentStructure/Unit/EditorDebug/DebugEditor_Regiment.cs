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
        public bool DebugShowRegimentPosition = false;
        
        public bool DebugShowUnitsAlive = false;
        
        public static bool FieldOfViewTriangle = false;
        public static bool FieldOfViewDebug = false;
        
        public static bool DebugDotProduct = false;
        public static bool FiringStateTest = false;
        public static bool ShowTargetsFiringStateTest = false;

        private void OnGUI()
        {
            int offset = 0;
            offset += FOVOnGUI(offset);
            offset += FindTargetOnGUI(offset);
        }
        
        private void OnDrawGizmos()
        {
            float2 intersect = FovGizmosDebug(FieldOfViewTriangle);
            if (DebugDotProduct)
            {
                TestDotProduct(intersect.x0y());
            }

            ShowRegimentPosition();
            ShowUnitsAlive();
        }

        private void ShowRegimentPosition()
        {
            if (!DebugShowRegimentPosition) return;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(Position, 0.5f);
        }

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
//║                                             ◆◆◆◆◆◆ DOT PRODUCT ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        
        private void TestDotProduct(Vector3 regimentBarycenter)
        {
            if (!GetIntersectionPointOnTerrain(out RaycastHit hit)) return;
            DrawDotProductInfo(regimentBarycenter, hit.point);
        }
        
        private bool GetIntersectionPointOnTerrain(out RaycastHit hit)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
            return Physics.Raycast(ray, out hit, Mathf.Infinity, 1<<8);
        }
        
        private void DrawDotProductInfo(float3 regimentBarycenter, float3 mousePositionOnTerrain)
        {
            const float areaRadius = 5f;
            
            float3 regimentPosition = transform.position.x1z();
            float3 positionBarycenter = regimentBarycenter.x1z();
            float3 positionMouse = mousePositionOnTerrain.x1z();
            
            float3 barycenterToLeaderDir = normalizesafe(regimentPosition - positionBarycenter);
            float3 barycenterToMouseToDir = normalizesafe(positionMouse - positionBarycenter);
            
            //Ray Direction Dot Check
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(positionBarycenter, barycenterToLeaderDir * areaRadius);
            
            //Ray center -> Mouse
            Gizmos.color = Color.magenta;
            Vector3 maxMousePosition = positionBarycenter + barycenterToMouseToDir * areaRadius;
            Gizmos.DrawSphere(maxMousePosition, 0.2f);
            Gizmos.DrawLine(positionBarycenter, maxMousePosition);
            
            //Dot Product Value
            float dotProduct = dot(barycenterToLeaderDir.xz, barycenterToMouseToDir.xz);
            Handles.Label(positionBarycenter, $"Dot:  {dotProduct}");
            
            float dotCZ = barycenterToLeaderDir.x * barycenterToMouseToDir.z - barycenterToLeaderDir.z * barycenterToMouseToDir.x;
            Handles.Label(positionBarycenter + back(), $"Dot Z-Component:  {dotCZ}");
            
            float angleRadian = acos(dotProduct / length(barycenterToLeaderDir.xz) * length(barycenterToMouseToDir.xz));
            Handles.Label(positionBarycenter + back()*2, $"Angle:  {degrees(angleRadian)}");
            
            //Circle
            Handles.DrawWireDisc(positionBarycenter, Vector3.up, areaRadius, 2f);
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

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Gizmos ◈◈◈◈◈◈                                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private float2 FovGizmosDebug(bool showTriangle)
        {
            if (!FieldOfViewDebug /*|| !IsSelected*/) return float2.zero;
            float2 regimentPosition = transform.position.xz();
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(regimentPosition.x1y(), 0.2f);

            float2 midWidthDistance = transform.right.xz() * (CurrentFormation.Width / 2f);
            float2 unit0 = regimentPosition - midWidthDistance;
            float2 unitWidth = regimentPosition + midWidthDistance;

            DrawStartingFOV(unit0, unitWidth);
            return DrawFieldOfView(regimentPosition, unit0, unitWidth, showTriangle);
        }

        private void DrawStartingFOV(in float2 unit0, in float2 unitWidth)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(unit0.x1y(), 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(unitWidth.x1y(), 0.2f);
        }

        private float2 DrawFieldOfView(float2 regimentPosition, float2 leftStart, float2 rightStart, bool showTriangle)
        {
            Gizmos.color = Color.white;
            float3 regimentForward = transform.forward;
            float2 directionLeft = mul(AngleAxis(-RegimentManager.RegimentFieldOfView, up()), regimentForward).xz;
            float2 directionRight = mul(AngleAxis(RegimentManager.RegimentFieldOfView, up()), regimentForward).xz;
            float2x2 leftRightDirection = float2x2(directionLeft, directionRight);

            float2x2 leftStartDirection = float2x2(leftStart, directionLeft);
            float2x2 rightStartDirection = float2x2(rightStart, directionRight);
            float2 intersection = GetIntersection(leftStartDirection, rightStartDirection);
            
            float distanceIntersection = distance(intersection, leftStart);
            Gizmos.DrawSphere(intersection.x1y(), 0.1f);
            
            (float arcAngle, float radius) = (RegimentManager.RegimentFieldOfView * 2, RegimentType.Range + distanceIntersection);
            //ATTENTION: From exprime une direction et non une position => directionLeft.xOy() si x1y() le trait sera plus haut
            Handles.DrawWireArc(intersection.x1y(), Vector3.up, directionLeft.x0y(), arcAngle, radius);

            Vector3[] lines = GetLines(intersection, leftStartDirection, rightStartDirection, RegimentType.Range);
            //Vector3[] lines = GetLines(leftStart, directionLeft, rightStart, directionRight, RegimentType.Range);
            Handles.DrawLines(lines);

            if (showTriangle)
            {
                GetBigTrianglePoints(leftRightDirection);
            }
            return intersection;
            
            
            void GetBigTrianglePoints(in float2x2 leftRightDir)
            {
                float2 topForwardDirection = normalizesafe(regimentPosition - intersection);
                float2 topForwardFov = intersection + topForwardDirection * radius;
                
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(topForwardFov.x1y(), 0.3f);
                
                float2 leftCross = topForwardDirection.normalccw();
                float2 intersectArcLeft = GetIntersection(topForwardFov, leftStart, leftCross, leftRightDir.c0);
                
                float2 rightCross = topForwardDirection.normalcw();
                float2 intersectArcRight = GetIntersection(topForwardFov, rightStart, rightCross, leftRightDir.c1);
                
                float2x2 leftRightCrossDir = float2x2(leftCross, rightCross);
                float2x2 leftRightIntersection = float2x2(intersectArcLeft, intersectArcRight);
                // DRAW Big Triangles
                DrawBigTriangle(topForwardFov, leftStartDirection, rightStartDirection, leftRightIntersection, leftRightCrossDir);
            }
        }

        private void DrawBigTriangle(float2 topForwardFov, in float2x2 leftStartDir, in float2x2 rightStartDir, in float2x2 leftRightIntersection, in float2x2 leftRightCrossDir)
        {
            
            Gizmos.color = Color.magenta;
            float dst = distance(topForwardFov, leftRightIntersection.c0);
            Gizmos.DrawRay(topForwardFov.x1y(), leftRightCrossDir.c0.x0y() * dst);
            Gizmos.DrawRay(topForwardFov.x1y(), leftRightCrossDir.c1.x0y() * dst);
            
            float dst2 = distance(leftStartDir.c0, leftRightIntersection.c0);
            Gizmos.DrawRay(leftStartDir.c0.x1y(), leftStartDir.c1.x0y() * dst2);
            Gizmos.DrawRay(rightStartDir.c0.x1y(), rightStartDir.c1.x0y() * dst2);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(leftRightIntersection.c0.x1y(), 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(leftRightIntersection.c1.x1y(), 0.2f);
        }

        private Vector3[] GetLines(float2 triangleTip, in float2x2 leftStartDir, in float2x2 rightStartDir, float range, bool allCone = false)
        {
            Vector3 leftEndPont = mad(leftStartDir.c1, range, leftStartDir.c0).x1y();
            Vector3 rightEndPont = mad(rightStartDir.c1, range, rightStartDir.c0).x1y();
            if (!allCone)
            {
                return new Vector3[] 
                { 
                    leftStartDir.c0.x1y(), leftEndPont, //left Cone
                    rightStartDir.c0.x1y(), rightEndPont, //right Cone
                    leftStartDir.c0.x1y(), rightStartDir.c0.x1y() //first row line
                };
            }
            else
            {
                return new Vector3[]
                {
                    triangleTip.x1y(), leftEndPont, //left Cone
                    triangleTip.x1y(), rightEndPont, //right Cone
                    leftStartDir.c0.x1y(), rightStartDir.c0.x1y() //first row line
                };
            }
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