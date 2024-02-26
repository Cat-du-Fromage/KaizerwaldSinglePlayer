using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

using static UnityEngine.Quaternion;
using static Unity.Mathematics.math;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using Kaizerwald.FormationModule;
using Kaizerwald.StateMachine;
using Kaizerwald.Utilities;

namespace Kaizerwald
{
    public sealed partial class Regiment : OrderedFormationBehaviour<Unit>, IOwnershipInformation
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private Transform regimentTransform;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        // REGIMENT IDENTIFICATION (IOwnershipInformation)
        [field:SerializeField] public ulong OwnerPlayerID { get; private set; }
        [field:SerializeField] public short TeamID { get; private set; }
        [field:SerializeField] public int RegimentID { get; private set; }
        
        // REGIMENT STATS
        [field:SerializeField] public RegimentType RegimentType { get; private set; }
        [field:SerializeField] public RegimentBehaviourTree BehaviourTree { get; private set; }
        
        //"BlackBoard"
        public EnemyRegimentTargetData EnemyRegimentTargetData { get; private set; } = new EnemyRegimentTargetData();
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Formation Access ◇◇◇◇◇◇                                                                            │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public FormationData CurrentFormationData => CurrentFormation;
        public FormationData TargetFormationData => TargetFormation;
        
        //TODO: invalid as soon as the regiment move : use "Forward" instead or we need to update formation direction value?
        public float3 FormationForward => CurrentFormation.DirectionForward;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Getters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public float3 Position => regimentTransform.position;
        public float3 Forward  => regimentTransform.forward;
        public float3 Back     => -regimentTransform.forward;
        public float3 Right    => regimentTransform.right;
        public float3 Left     => -regimentTransform.right;
        public Quaternion Rotation => regimentTransform.rotation;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void Awake()
        {
            regimentTransform = transform;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Regiment Update Event ◈◈◈◈◈◈                                                                            ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public void OnFixedUpdate()
        {
            UpdateFormation();
        }
    
        public void OnUpdate()
        {
            BehaviourTree.OnUpdate();
            for (int i = 0; i < Count; i++)
            {
                Elements[i].UpdateUnit();
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Initialization Methods ◈◈◈◈◈◈                                                                           ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Static Constructor ◇◇◇◇◇◇                                                                          │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        // Meant to be instantiate this way AND NO OTHER WAY
        public static Regiment InstantiateAndInitialize(GameObject regimentPrefab, Vector3 position, Quaternion rotation, ulong playerId, int teamID, LayerMask terrainLayer)
        {
            GameObject newRegiment = Instantiate(regimentPrefab, position, rotation);
            Regiment regiment = newRegiment.GetOrAddComponent<Regiment>();
            //if (!newRegiment.TryGetComponent(out Regiment regiment)) { regiment = newRegiment.AddComponent<Regiment>(); }
            regiment.InitializeProperties(playerId, teamID, terrainLayer);
            return regiment;
        }
        
        public Regiment InitializeProperties(ulong playerId, int teamID, LayerMask terrainLayer)
        {
            return InitializeProperties(playerId, teamID, terrainLayer, Forward);
        }

        public Regiment InitializeProperties(ulong playerId, int teamID, LayerMask terrainLayer, float3 regimentDirection)
        {
            //Properties
            RegimentID = transform.GetInstanceID();
            OwnerPlayerID = playerId;
            TeamID = (short)teamID;
            name = $"Player({playerId})_Regiment({RegimentID})";
            
            //FormationMatrix
            Formation formation = RegimentType.GetFormation(regimentDirection);
            List<Unit> units = CreateRegimentsUnit();
            InitializeFormation(formation, units, Position);

            //BehaviourTree
            BehaviourTree = gameObject.GetOrAddComponent<RegimentBehaviourTree>();
            BehaviourTree.InitializeAndRegisterUnits(this);
            return this;
            //┌▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁┐
            //▕  ◇◇◇◇◇◇ Internal Methods ◇◇◇◇◇◇                                                                        ▏
            //└▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔┘
            List<Unit> CreateRegimentsUnit()
            {
                GameObject prefab = RegimentType.UnitPrefab;
                int unitLayerIndex = KaizerwaldGameManager.Instance.UnitLayerIndex;
                using NativeArray<float3> positions = formation.GetPositionsInFormationByRaycast(Position, terrainLayer);
                
                List<Unit> tmpUnits = new(formation.BaseNumUnits);
                for (int i = 0; i < positions.Length; i++)
                {
                    Unit unit = Unit.InstantiateUnit(i, unitLayerIndex, this, prefab, positions[i], Rotation);
                    tmpUnits.Add(unit);
                }
                return tmpUnits;
            }
        }
        
        private List<Unit> CreateRegimentsUnit2(Formation formation, LayerMask terrainLayer)
        {
            GameObject prefab = RegimentType.UnitPrefab;
            int unitLayerIndex = KaizerwaldGameManager.Instance.UnitLayerIndex;
            using NativeArray<float3> positions = formation.GetPositionsInFormationByRaycast(Position, terrainLayer);
                
            List<Unit> tmpUnits = new(formation.BaseNumUnits);
            for (int i = 0; i < positions.Length; i++)
            {
                Unit unit = Unit.InstantiateUnit(i, unitLayerIndex, this, prefab, positions[i], Rotation);
                tmpUnits.Add(unit);
            }
            return tmpUnits;
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Regiment Update Event ◈◈◈◈◈◈                                                                            ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        //CAREFUL: Event received from Fixed Update => Before Update
        public void OnDeadUnit(Unit unit)
        {
            RegisterInactiveElement(unit);
            //Remove(unit);
        }
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Player Orders ◇◇◇◇◇◇                                                                               │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public void OnOrderReceived(PlayerOrderData playerOrder)
        {
            EMoveType moveType = GetMoveType(playerOrder.MovePace);
            Order packedOrder = playerOrder.OrderType switch
            {
                EOrderType.Move => new MoveOrder(playerOrder.TargetFormation, playerOrder.LeaderDestination, moveType),
                _ => default
            };
            BehaviourTree.RequestChangeState(packedOrder);
        }

        private EMoveType GetMoveType(EMovePace movePace)
        {
            return movePace switch
            {
                EMovePace.March => EMoveType.March,
                EMovePace.Run => EMoveType.Run,
                _ => default
            };
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                    ◆◆◆◆◆◆ FORMATION BEHAVIOUR APPENDIX ◆◆◆◆◆◆                                      ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        
        protected override void SwapElementByIndex(int deadIndex, int swapIndex)
        {
            (Elements[deadIndex], Elements[swapIndex]) = (Elements[swapIndex], Elements[deadIndex]);
            Elements[deadIndex].OnRearrangement(deadIndex);
            Elements[swapIndex].OnRearrangement(swapIndex);
            HandleElementSwapped(deadIndex, swapIndex);
        }

        private void LastLineRearrangement(int numElementAfterResize)
        {
            if (CurrentFormation.IsLastLineComplete) return;
            MoveOrder moveOrder = new (CurrentFormationData, TargetPosition, EMoveType.March);
            for (int i = CurrentFormation.LastRowFirstIndex; i < numElementAfterResize; i++)
            {
                Elements[i].BehaviourTree.RequestChangeState(moveOrder);
            }
        }
        
        protected override void HandleFormationResized(int numElementAfterResize)
        {
            LastLineRearrangement(numElementAfterResize);
            base.HandleFormationResized(numElementAfterResize);
        }

        //TODO: Find a way without Memory allocation (Unit[] tmpUnits = new Unit[Elements.Count])
        //Here was the issue
        //We receive sorted Indices [2,0,3,1], Which translate to:
        // Elements[2] = (tmp cache) Elements[0]
        // Elements[0] = (tmp cache) Elements[1]
        // Elements[3] = (tmp cache) Elements[2]
        // Elements[1] = (tmp cache) Elements[3]
        // BUT when Element[i] is assigned it still had IndexInFormation it has ([0,1,2,3] case before)
        // We need to assign IndexInFormation to the sorted Index [2,0,3,1] too!
        public void ReorderElements(NativeArray<int> indices)
        {
            if (indices.Length != Elements.Count) return;
            List<Unit> tmpUnits = new List<Unit>(Elements);
            for (int i = 0; i < Elements.Count; i++)
            {
                int sortedIndex = indices[i];
                Elements[sortedIndex] = tmpUnits[i];
                Elements[sortedIndex].SetIndexInFormation(sortedIndex);
            }
            ResetTransformsIndicators();
        }
        
        public void ReorderElements(NativeArray<int> distanceSortedIndices, NativeArray<int> sortedIndices)
        {
            Unit[] tmpUnits = new Unit[Elements.Count];
            Elements.CopyTo(tmpUnits);
            for (int i = 0; i < Elements.Count; i++)
            {
                int realIndex = distanceSortedIndices[i];
                int sortedIndex = sortedIndices[i];
                Elements[sortedIndex] = tmpUnits[realIndex];
                Elements[sortedIndex].SetIndexInFormation(sortedIndex);
            }
            ResetTransformsIndicators();
        }
        
        public void ReorderElementsBySwap(NativeArray<int> indices)
        {
            if (indices.Length != Elements.Count) return;
            //Find Cpp's Iota equivalent
            //NativeArray<int> indicesPosition = new (Enumerable.Range(0, Elements.Count).ToArray(), Temp);
            NativeArray<int> indicesPosition = new (Elements.Count, Temp, UninitializedMemory);
            for (int i = 0; i < Elements.Count; i++) indicesPosition[i] = i;
            
            for (int i = 0; i < Elements.Count; i++)
            {
                int sortedIndex = indices[i];
                int indexElementToSwapWith = indicesPosition.IndexOf(i);
                //Debug.Log($"sortedIndex = {sortedIndex} i = {i} position i = {indexElementToSwapWith} test indicesPosition[i] = {indicesPosition[i]}");
                (Elements[sortedIndex], Elements[indexElementToSwapWith]) = (Elements[indexElementToSwapWith], Elements[sortedIndex]);
                (indicesPosition[sortedIndex], indicesPosition[indexElementToSwapWith]) = (indicesPosition[indexElementToSwapWith], indicesPosition[sortedIndex]);
                Elements[sortedIndex].SetIndexInFormation(sortedIndex);
            }
            //for (int i = 0; i < Elements.Count; i++){Elements[i].SetIndexInFormation(i);}
            ResetTransformsIndicators();
        }
    }
}
