using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

using static PlayerControls;
using static UnityEngine.InputSystem.InputAction;
using static UnityEngine.Mathf;
using static UnityEngine.Vector3;
using static UnityEngine.Physics;
using static Unity.Mathematics.math;
using static Unity.Mathematics.quaternion;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;

using float3 = Unity.Mathematics.float3;

using Kaizerwald.Utilities;
using Kaizerwald.FormationModule;
using static Kaizerwald.Utilities.KzwMath;

namespace Kaizerwald
{
    public sealed class PlacementController : HighlightController
    {
        private const float DISTANCE_BETWEEN_REGIMENT = 1f;
        private const int ORIGIN_HEIGHT = 512;
        private const int RAY_DISTANCE = 1024;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private readonly LayerMask TerrainLayer;
        
        private PlacementActions PlacementControls;
        private bool PlacementsVisible;
        private bool MouseStartValid;
        
        private Vector3 MouseStart, MouseEnd;
        private bool PlacementCancel;
        
        private float mouseDistance;
        private int[] tempWidths;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public PlacementSystem PlacementSystem { get; private set; }

        public List<HighlightRegiment> SortedSelectedRegiments { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Getters ◈◈◈◈◈◈                                                                                          ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public int[] DynamicsTempWidth => tempWidths;

        private InputAction RightMouseClickAndMove => PlacementControls.RightMouseClickAndMove;
    
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        private List<HighlightRegiment> SelectedRegiments => PlacementSystem.SelectedRegiments;
        private int NumSelections => SelectedRegiments.Count;
        
        private HighlightRegister DynamicRegister => PlacementSystem.DynamicPlacementRegister;
        private HighlightRegister StaticRegister => PlacementSystem.StaticPlacementRegister;
        
        private float UpdateMouseDistance() => Magnitude(MouseEnd - MouseStart);
        private float3 LineDirection => normalizesafe(MouseEnd - MouseStart);
        private float3 DepthDirection => normalizesafe(cross(up(), LineDirection));

        //private SelectionInfos SelectionInfos => PlacementSystem.SelectionInfos;
        //private int TotalUnitsSelected => SelectionInfos.TotalUnitsSelected;
        private int TotalUnitsSelected => SortedSelectedRegiments.Sum(regiment => regiment.Count);
        //private float2 MinMaxSelectionWidth => SelectionInfos.MinMaxSelectionWidth + DISTANCE_BETWEEN_REGIMENT * (NumSelections-1);
        private float2 MinMaxSelectionWidth => SelectionUtils.GetMinMaxSelectionWidth(SortedSelectedRegiments) + DISTANCE_BETWEEN_REGIMENT * (NumSelections-1);
        //CANT USE THIS because we need the Sorted MinWidthsArray()
        //private int[] MinWidthsArray() => PlacementSystem.SelectionInfos.SelectionsMinWidth;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public PlacementController(HighlightSystem system, PlayerControls controls, LayerMask terrainLayer) : base()
        {
            PlacementSystem = (PlacementSystem)system;
            PlacementControls = controls.Placement;
            TerrainLayer = terrainLayer;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                       ◆◆◆◆◆◆ ABSTRACT METHODS ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public override void OnEnable()
        {
            PlacementControls.Enable();
            RightMouseClickAndMove.EnableAllEvents(OnRightMouseClickAndMoveStart, OnRightMouseClickAndMovePerform, OnRightMouseClickAndMoveCancel);
            PlacementControls.SpaceKey.EnableStartCancelEvent(OnSpaceKeyStart, OnSpaceKeyCancel);
            PlacementControls.LeftMouseCancel.EnableStartEvent(CancelPlacement);
        }

        public override void OnDisable()
        {
            RightMouseClickAndMove.DisableAllEvents(OnRightMouseClickAndMoveStart, OnRightMouseClickAndMovePerform, OnRightMouseClickAndMoveCancel);
            PlacementControls.SpaceKey.DisableStartCancelEvent(OnSpaceKeyStart, OnSpaceKeyCancel);
            PlacementControls.LeftMouseCancel.DisableStartEvent(CancelPlacement);
            PlacementControls.Disable();
        }

        //public override void OnUpdate() => OnCameraMoveFormation();
        
        public override void OnFixedUpdate()
        {
            OnCameraMoveFormation();
        }

        /// <summary>
        /// Trigger formation on camera movement
        /// </summary>
        private void OnCameraMoveFormation()
        {
            if (!PlacementsVisible) return;
            Vector3 lastPosition = MouseEnd;
            if (!TryUpdateMouseEnd(Mouse.current.position.value) || MouseEnd == lastPosition) return;
            mouseDistance = UpdateMouseDistance();
            tempWidths = PlaceRegiments();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                     ◆◆◆◆◆◆ EVENT BASED CONTROLS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private void OnRightMouseClickAndMoveStart(CallbackContext context)
        {
            if (SelectedRegiments.Count == 0) return;
            PlacementCancel = false;
            MouseStartValid = TryUpdateMouseStart(context.ReadValue<Vector2>());
        }

        private void OnRightMouseClickAndMovePerform(CallbackContext context)
        {
            if (SelectedRegiments.Count == 0 || !MouseStartValid || !TryUpdateMouseEnd(context.ReadValue<Vector2>())) return;
            mouseDistance = UpdateMouseDistance();
            tempWidths = PlaceRegiments();
        }
        
        private void OrdersCallbackChoice(int registerIndex)
        {
            bool marchOrdered = Keyboard.current.ctrlKey.isPressed;
            if (PlacementsVisible)
            {
                PlacementSystem.OnMoveOrderEvent(registerIndex, marchOrdered);
            }
            else
            {
                if (PlacementSystem.PreselectedRegiments.Count == 0)
                {
                    //TODO : Implement Move while keeping same Formation(Width)
                    //Carefull WAY More complicated than it looks.. How will it work when multiple selection?
                }
                else if(PlacementSystem.PreselectedRegiments[0].TeamID != HighlightRegimentManager.Instance.TeamID) //enemy is preselected
                {
                    //PlacementSystem.PreselectedRegiments[0].OwnerID
                    PlacementSystem.OnAttackOrderEvent(PlacementSystem.PreselectedRegiments[0].RegimentID, marchOrdered);
                }
            }
        }
        
        private void OnRightMouseClickAndMoveCancel(CallbackContext context)
        {
            if (SelectedRegiments.Count == 0 || PlacementCancel) return; //Means Left Click is pressed
            //Currently ONLY "drag-move" works
            OrdersCallbackChoice((int)PlacementSystem.EPlacementRegister.Dynamic);
            DisablePlacements();
            PlacementSystem.SwapDynamicToStatic();
        }
        private void OnSpaceKeyStart(CallbackContext context) => EnableAllStatic();
        private void OnSpaceKeyCancel(CallbackContext context) => DisableAllStatic();
        private void CancelPlacement(CallbackContext context) => DisablePlacements();

        private void DisablePlacements()
        {
            OnClearDynamicHighlight();
            mouseDistance = 0;
            PlacementCancel = true;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void OnHighlightRemoved(HighlightRegiment regiment)
        {
            if (!SortedSelectedRegiments.Contains(regiment)) return;
            tempWidths = PlaceRegiments();
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Sort Selections ◈◈◈◈◈◈                                                                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private NativeArray<float3> GetDestinationsPosition()
        {
            float distanceWidth = clamp(mouseDistance, MinMaxSelectionWidth[0], MinMaxSelectionWidth[1]);
            float singleWidthSpace = (distanceWidth / SortedSelectedRegiments.Count) * 1.5f;
            NativeArray<float3> mockedDestinationPoints = new (SortedSelectedRegiments.Count, Temp, UninitializedMemory);
            for (int i = 0; i < SortedSelectedRegiments.Count; i++)
            {
                float distance = i * singleWidthSpace;
                mockedDestinationPoints[i] = (float3)MouseStart + LineDirection * distance;
            }
            return mockedDestinationPoints;
        }
        
        private NativeArray<float> GetCostMatrix()
        {
            NativeArray<float3> destinations = GetDestinationsPosition();
            NativeArray<float> nativeCostMatrix = new (square(SortedSelectedRegiments.Count), Temp, UninitializedMemory);
            for (int i = 0; i < nativeCostMatrix.Length; i++)
            {
                (int x, int y) = GetXY(i, SortedSelectedRegiments.Count);
                float3 regimentPosition = SortedSelectedRegiments[y].CurrentPosition;
                nativeCostMatrix[i] = distancesq(regimentPosition, destinations[x]);
            }
            return nativeCostMatrix;
        }

        private void SortSelections()
        {
            NativeArray<float> costMatrix = GetCostMatrix();
            NativeArray<int> sortedIndex = HungarianAlgorithm.FindNativeAssignments(costMatrix, SelectedRegiments.Count);
            //CAREFULL sortedIndex express : At index  value = "sortedIndex[i]", I want current element index "i"!
            for (int i = 0; i < sortedIndex.Length; i++)
            {
                if(SelectedRegiments[i] == null) Debug.Log($"SortSelections: null Selected regiment at {i}");
                SortedSelectedRegiments[sortedIndex[i]] = SelectedRegiments[i];
            }
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Placement Logic ◈◈◈◈◈◈                                                                                  ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private float GetUnitsToAddLength()
        {
            // Règle le soucis de l'espacement, Pourquoi devoir commencer par 1? Pourquoi devoir retirer 1 unité?
            float unitsToAddLength = mouseDistance - MinMaxSelectionWidth.x;
            for (int i = 1; i < SortedSelectedRegiments.Count; i++)
            {
                unitsToAddLength -= SortedSelectedRegiments[i].CurrentFormation.DistanceUnitToUnitX;
            }
            return unitsToAddLength;
        }
    
        private int[] PlaceRegiments()
        {
            SortedSelectedRegiments = new List<HighlightRegiment>(SelectedRegiments);
            if (SelectedRegiments.Count == 0 || !IsVisibilityTrigger()) 
            {
                return Array.Empty<int>(); //Ordre => Garde la formation actuelle
            }
            if (SelectedRegiments.Count == 1 && SelectedRegiments[0].CurrentFormation.NumUnitsAlive == 1)
            {
                return PlaceSingleUnitRegiment();
            }
            
            SortSelections();
            float unitsToAddLength = GetUnitsToAddLength();
            NativeArray<int> newWidths = GetUpdatedFormationWidths(ref unitsToAddLength);
            NativeArray<float2> starts = GetStartsPosition(unitsToAddLength, newWidths);
            NativeList<JobHandle> jhs  = GetInitialTokensPosition(starts, newWidths, out NativeArray<float2> initialTokensPositions);
            using NativeArray<RaycastHit> results = GetPositionAndRotationOnTerrainByJob(ref initialTokensPositions, jhs);
            MoveHighlightsTokens(results);
            
            return newWidths.ToArray();
        }

        private int[] PlaceSingleUnitRegiment()
        {
            int regimentId = SelectedRegiments[0].RegimentID;
            Ray ray = new Ray(new Vector3(MouseStart.x, ORIGIN_HEIGHT, MouseStart.y), Vector3.down);
            if (!Raycast(ray, out RaycastHit hit, Infinity, TerrainLayer) || !DynamicRegister.ContainsKey(regimentId))
            {
                return Array.Empty<int>();
            }
            else
            {
                Vector3 hitPoint = MouseStart + hit.normal * 0.05f;
                Quaternion newRotation = LookRotationSafe(-DepthDirection, hit.normal);
                DynamicRegister[regimentId][0].transform.SetPositionAndRotation(hitPoint, newRotation);
                return new []{1};
            }
        }
        
        /// <summary>
        /// Initial Position from Start-Mouse to End-Mouse on the Map using only 2D coord
        /// </summary>
        /// <param name="starts">Starts calculated (Y component is incorrect at this point)</param>
        /// <param name="newWidths">Formations size foreach regiment allowed by the distance formed by Start-Mouse to End-Mouse</param>
        /// <param name="initialTokensPositions">Output: array of position calculated in 2D</param>
        /// <returns></returns>
        private NativeList<JobHandle> GetInitialTokensPosition(in NativeArray<float2> starts, in NativeArray<int> newWidths, out NativeArray<float2> initialTokensPositions)
        {
            // PHASE 1 : On récupère OU vont les tokens par rapport à la nouvelle formations
            half2 lineDirection2D = half2(LineDirection.xz);
            half2 depthDirection2D = half2(DepthDirection.xz);
            initialTokensPositions = new (TotalUnitsSelected, TempJob, UninitializedMemory);
            NativeList<JobHandle> jobHandles = new (SortedSelectedRegiments.Count, Temp);
            
            int numUnitsRegimentsBefore = 0;
            for (int i = 0; i < SortedSelectedRegiments.Count; i++)
            {
                FormationData regimentState = SortedSelectedRegiments[i].CurrentFormation;
                JGetInitialTokensPositions job = new()
                {
                    NewWidth           = newWidths[i],
                    NumUnitsAlive      = regimentState.NumUnitsAlive,
                    DistanceUnitToUnit = half2(regimentState.DistanceUnitToUnit),
                    LineDirection      = lineDirection2D,
                    DepthDirection     = depthDirection2D,
                    Start              = starts[i],
                    TokensPositions    = initialTokensPositions.Slice(numUnitsRegimentsBefore, regimentState.NumUnitsAlive)
                };
                jobHandles.Add(job.ScheduleParallel(regimentState.NumUnitsAlive, JobWorkerCount-1, default));
                numUnitsRegimentsBefore += regimentState.NumUnitsAlive;
            }
            return jobHandles;
        }
        
        private NativeArray<RaycastHit> GetPositionAndRotationOnTerrainByJob(ref NativeArray<float2> tokensPositions, NativeList<JobHandle> dependencies)
        {
            int totalUnitsSelected = TotalUnitsSelected;
            NativeArray<RaycastHit> results = new (totalUnitsSelected, TempJob, UninitializedMemory);
            using NativeArray<RaycastCommand> commands = new (totalUnitsSelected, TempJob, ClearMemory);
            // ---------------------------------------------------------------------------------------------------------
            // RAY CASTS
            NativeArray<JobHandle> jobHandles = new (NumSelections, Temp, UninitializedMemory);
            QueryParameters queryParams = new (TerrainLayer.value);
            int numUnitsRegimentBefore = 0;
            for (int i = 0; i < SortedSelectedRegiments.Count; i++)
            {
                int numToken = SortedSelectedRegiments[i].CurrentFormation.NumUnitsAlive;
                NativeSlice<float2> originSlice = tokensPositions.Slice(numUnitsRegimentBefore, numToken);
                NativeSlice<RaycastCommand> commandsSlice = commands.Slice(numUnitsRegimentBefore, numToken);
                jobHandles[i] = JRaycastsCommands.Process(commandsSlice, originSlice, queryParams, dependencies[i]);
                numUnitsRegimentBefore += numToken;
            }
            JobHandle combinedDependency = JobHandle.CombineDependencies(jobHandles);
            JobHandle rayCastCommandJh = RaycastCommand.ScheduleBatch(commands, results, 1, 1, combinedDependency);
            rayCastCommandJh.Complete();
            tokensPositions.Dispose();
            return results;
        }
        
        private void MoveHighlightsTokens(NativeArray<RaycastHit> results)
        {
            float3 depthDirection = DepthDirection;
            int numUnitsRegimentBefore = 0;
            foreach (HighlightRegiment regiment in SortedSelectedRegiments)
            {
                if(!DynamicRegister.Records.TryGetValue(regiment.RegimentID, out HighlightBehaviour[] tokens)) continue;
                int numToken = regiment.CurrentFormation.NumUnitsAlive;
                NativeSlice<RaycastHit> raycastHits = results.Slice(numUnitsRegimentBefore, numToken);
                for (int unitIndex = 0; unitIndex < numToken; unitIndex++)
                {
                    RaycastHit currentHit = raycastHits[unitIndex];
                    Vector3 hitPoint = currentHit.point + currentHit.normal * 0.05f;
                    Quaternion newRotation = LookRotationSafe(-depthDirection, currentHit.normal);
                    tokens[unitIndex].transform.SetPositionAndRotation(hitPoint, newRotation);
                }
                numUnitsRegimentBefore += numToken;
            }
        }
        
        private NativeArray<int> GetUpdatedFormationWidths(ref float unitsToAddLength)
        {
            NativeArray<int> newWidths = SelectionUtils.GetSelectionsMinWidth(SortedSelectedRegiments);
            int attempts = 0;
            while (unitsToAddLength > 0 && attempts < newWidths.Length)
            {
                attempts = 0;
                for (int i = 0; i < newWidths.Length; i++)
                {
                    FormationData currentState = SortedSelectedRegiments[i].CurrentFormation;
                    bool notEnoughSpace = unitsToAddLength < currentState.DistanceUnitToUnitX;
                    bool isWidthAtMax   = newWidths[i] == currentState.MaxRow;
                    bool failAttempt    = notEnoughSpace || isWidthAtMax;
                    attempts           += failAttempt ? 1 : 0;
                    newWidths[i]       += !failAttempt ? 1 : 0;
                    unitsToAddLength   -= !failAttempt ? currentState.DistanceUnitToUnit.x : 0;
                }
            }
            return newWidths;
        }

        private NativeArray<float2> GetStartsPosition(float unitsToAddLength, NativeArray<int> newWidths)
        {
            bool isMaxDistanceReach = mouseDistance < MinMaxSelectionWidth.y;
            float leftOver = isMaxDistanceReach ? unitsToAddLength / (SortedSelectedRegiments.Count - 1) : 0;
            NativeArray<float2> starts = new (SortedSelectedRegiments.Count, Temp, ClearMemory);
            if (SortedSelectedRegiments.Count == 0) return starts;
            
            starts[0] = ((float3)MouseStart).xz;
            for (int i = 1; i < SortedSelectedRegiments.Count; i++)
            {
                float currUnitSpace  = SortedSelectedRegiments[i].CurrentFormation.DistanceUnitToUnitX;
                float prevUnitSpace  = SortedSelectedRegiments[i - 1].CurrentFormation.DistanceUnitToUnitX;
                float previousLength = (newWidths[i - 1] - 1) * prevUnitSpace; // -1 because we us space, not units
                previousLength      += csum(float2(prevUnitSpace, currUnitSpace) * 0.5f);//arrive at edge of last Unit + 1/2 newUnitSize
                previousLength      += DISTANCE_BETWEEN_REGIMENT + max(0, leftOver); // add regiment space
                starts[i] = starts[i - 1] + LineDirection.xz * previousLength;
            }
            return starts;
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇ Visibility Trigger ◇◇◇◇◇                                                                            │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        /// <summary>
        /// Is Cursor far enough to trigger event?
        /// </summary>
        /// <returns>condition met to trigger placement visibility</returns>
        private bool IsVisibilityTrigger()
        {
            if (PlacementsVisible) return true;
            if (mouseDistance < SelectedRegiments[0].CurrentFormation.DistanceUnitToUnitX) return false;
            EnableAllDynamicSelected();
            return true;
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇ Mouses Positions ◇◇◇◇◇                                                                              │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        
        private bool TryUpdateMouseStart(in Vector2 mouseInput)
        {
            Ray singleRay = PlayerCamera.ScreenPointToRay(mouseInput);
            bool isHit = Raycast(singleRay, out RaycastHit hit, Infinity, TerrainLayer);
            MouseStart = isHit ? hit.point : MouseStart;
            return isHit;
        }

        private bool TryUpdateMouseEnd(in Vector2 mouseInput)
        {
            Ray singleRay = PlayerCamera.ScreenPointToRay(mouseInput);
            bool isHit = Raycast(singleRay, out RaycastHit hit, Infinity, TerrainLayer);
            MouseEnd = isHit ? hit.point : MouseEnd;
            return isHit;
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇ Toggle Methods ◇◇◇◇◇                                                                                │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        private void EnableAllDynamicSelected()
        {
            SelectedRegiments.ForEach(regiment => PlacementSystem.OnShow(regiment, (int)PlacementSystem.EPlacementRegister.Dynamic));
            PlacementsVisible = true;
        }
        
        private void OnClearDynamicHighlight()
        {
            PlacementSystem.HideAll((int)PlacementSystem.EPlacementRegister.Dynamic);
            PlacementsVisible = false;
        }
        
        private void EnableAllStatic()
        {
            foreach (HighlightBehaviour[] tokens in StaticRegister.Records.Values)
            {
                if (tokens[0].IsVisible()) continue;
                Array.ForEach(tokens, token => token.Show());
            }
        }
        
        private void DisableAllStatic()
        {
            foreach (HighlightBehaviour[] tokens in StaticRegister.Records.Values)
            {
                if (tokens[0].IsHidden()) continue;
                Array.ForEach(tokens, token => token.Hide());
            }
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ JOBS ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private struct JRaycastsCommands : IJobFor
        {
            [ReadOnly] public int OriginHeight;
            [ReadOnly] public int RayDistance;
            [ReadOnly] public QueryParameters QueryParams;
            
            [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeSlice<float2> Origins;
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public NativeSlice<RaycastCommand> Commands;

            public JRaycastsCommands(NativeSlice<RaycastCommand> commands, NativeSlice<float2> origins, QueryParameters queryParams)
            {
                OriginHeight = ORIGIN_HEIGHT;
                RayDistance = RAY_DISTANCE;
                QueryParams = queryParams;
                Origins = origins;
                Commands = commands;
            }

            public void Execute(int unitIndex)
            {
                float2 origin = Origins[unitIndex];
                Vector3 origin3D = new (origin.x, OriginHeight, origin.y);
                Commands[unitIndex] = new RaycastCommand(origin3D, Vector3.down, QueryParams, RayDistance);
            }

            public static JobHandle Process(NativeSlice<RaycastCommand> commands, NativeSlice<float2> origins, QueryParameters queryParams, JobHandle dependency = default)
            {
                JRaycastsCommands job = new JRaycastsCommands(commands, origins, queryParams);
                JobHandle jobHandle = job.ScheduleParallel(origins.Length, JobWorkerCount - 1, dependency);
                return jobHandle;
            }
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private struct JGetInitialTokensPositions : IJobFor
        {
            [ReadOnly] public int NewWidth;
            [ReadOnly] public int NumUnitsAlive;
            [ReadOnly] public half2 DistanceUnitToUnit;
            [ReadOnly] public half2 LineDirection;
            [ReadOnly] public half2 DepthDirection;
            [ReadOnly] public float2 Start;
            
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeSlice<float2> TokensPositions;

            public void Execute(int unitIndex)
            {
                int y = unitIndex / NewWidth;
                int x = unitIndex - (y * NewWidth);
                
                float2 yOffset = y * DistanceUnitToUnit.y * (float2)DepthDirection;
                float2 xOffset = x * DistanceUnitToUnit.x * (float2)LineDirection;
                
                int maxDepth = (int)ceil((float)NumUnitsAlive / NewWidth);
                int numUnitLastLine = NumUnitsAlive - NewWidth * (maxDepth - 1);
                int diffLastLineWidth = NewWidth - numUnitLastLine;
                float offset = (diffLastLineWidth * 0.5f) * DistanceUnitToUnit.x;
                bool isLastLine = y == maxDepth - 1;
                float2 offsetStart = select(0, (float2)LineDirection * offset, isLastLine);
                float2 position = Start + offsetStart + xOffset + yOffset;
                
                TokensPositions[unitIndex] = position;
            }
        }
    }
    
}

/*
private NativeArray<float3> GetDestinationsPosition2()
{
    float distanceWidth = clamp(mouseDistance, MinMaxSelectionWidth[0], MinMaxSelectionWidth[1]);
    float singleWidthSpace = distanceWidth / SortedSelectedRegiments.Count;
    float halfSingleWidthSpace = singleWidthSpace / 2f;
    NativeArray<float3> mockedDestinationPoints = new (SortedSelectedRegiments.Count, Temp, UninitializedMemory);
    for (int i = 0; i < SortedSelectedRegiments.Count; i++)
    {
        float distance = i * singleWidthSpace + halfSingleWidthSpace;
        mockedDestinationPoints[i] = (float3)MouseStart + LineDirection * distance;
    }
    return mockedDestinationPoints;
}
 
private NativeArray<RaycastHit> GetPositionAndRotationOnTerrain(ref NativeArray<float2> tokensPositions, NativeList<JobHandle> dependencies)
{
    int totalUnitsSelected = TotalUnitsSelected;
    JobHandle.CompleteAll(dependencies.AsArray());
    NativeArray<RaycastHit> results = new (totalUnitsSelected, Temp, UninitializedMemory);
    NativeArray<Ray> rays = new (totalUnitsSelected, Temp, ClearMemory);
    NativeHashMap<int, int> regimentKeyPairNumToken = new (SortedSelectedRegiments.Count, Temp);
    int numUnitsRegimentBefore = 0;
    for (int i = 0; i < SortedSelectedRegiments.Count; i++)
    {
        int numToken = SortedSelectedRegiments[i].CurrentFormation.NumUnitsAlive;
        regimentKeyPairNumToken.Add(i, numToken);
        NativeSlice<float2> originSlice = tokensPositions.Slice(numUnitsRegimentBefore, numToken);
        NativeSlice<Ray> raySlice = rays.Slice(numUnitsRegimentBefore, numToken);
        for (int j = 0; j < numToken; j++)
        {
            raySlice[j] = new Ray(originSlice[j].xay(ORIGIN_HEIGHT), Vector3.down);
        }
        numUnitsRegimentBefore += numToken;
    }
    tokensPositions.Dispose();

    int startIndex = 0;
    foreach (KVPair<int, int> kvPair in regimentKeyPairNumToken)
    {
        (int key, int value) = (kvPair.Key, kvPair.Value);
        for (int i = 0; i < value; i++)
        {
            int index = startIndex + i;
            bool hitTerrain = Raycast(rays[index], out RaycastHit hit, RAY_DISTANCE, TerrainLayer.value);
            results[index] = hitTerrain ? hit : results[index];
            if(!hitTerrain)
            {
                Debug.Log($"Sorted({key}): DONT HIT at {i} ray = {rays[index].origin}, Direction = {rays[index].direction} (num units = {SortedSelectedRegiments[key].Count} vs value used = {value})");
            }
        }
        startIndex += value;
    }
    //Erreur ou parfois TotalUnitsSelected a UN element en plus..
    if (results.Length != startIndex)
    {
        Debug.Log($"({results.Length}/{startIndex}) results.Length != startIndex => TotalUnitsSelected iw WRONG! ({TotalUnitsSelected})");
    }
    return results;
}
*/