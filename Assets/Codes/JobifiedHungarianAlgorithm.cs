using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BurstLinq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

using static Unity.Mathematics.math;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using int2 = Unity.Mathematics.int2;
using Debug = UnityEngine.Debug;

using Kaizerwald;
using Kaizerwald.FormationModule;
using static Kaizerwald.Utilities.KzwMath;



namespace Kaizerwald
{
    public static class JobifiedHungarianAlgorithm
    {
        public static JobHandle FindAssignments(NativeArray<int> sortedIndex, NativeArray<float> costs, int width)
        {
            JHungarianAlgorithmSteps job = new(sortedIndex, costs, width);
            JobHandle jobHandle = job.Schedule();
            return jobHandle;
        }
        
        public static NativeArray<int> FindAssignments(NativeArray<float> costs, int width)
        {
            //NativeArray<byte> masks = new (costs.Length, TempJob, ClearMemory);
            //NativeArray<bool> rowsCovered = new (width, TempJob, ClearMemory);
            //NativeArray<bool> colsCovered = new (width, TempJob, ClearMemory);
            
            //JobHandle minDependency = SubtractRowsByMinValue(costs, width);
            //JobHandle maskJobHandle = JCreateMasksAndCoveredMatrix.Process(costs, masks, rowsCovered, colsCovered, width, minDependency);
            //JobHandle clearCoveredDependency = JClearCovers.Process(rowsCovered, colsCovered, maskJobHandle);
            
            NativeArray<int> agentTasks = new (width, TempJob, UninitializedMemory);
            JHungarianAlgorithmSteps job = new(agentTasks, costs, width);
            JobHandle jobHandle = job.Schedule();
            
            //masks.Dispose(jobHandle);
            //rowsCovered.Dispose(jobHandle);
            //colsCovered.Dispose(jobHandle);
            
            jobHandle.Complete();
            
            return agentTasks;
        }
        
        public static JobHandle FindAssignments(NativeSlice<int> sliceSortedIndex, NativeSlice<float> costs, int width, int offsetIndex)
        {
            JChunkedHungarianAlgorithmSteps job = new(sliceSortedIndex, costs, width, offsetIndex);
            JobHandle jobHandle = job.Schedule();
            return jobHandle;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                               ◆◆◆◆◆◆ METHODS ◆◆◆◆◆◆                                                ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static JobHandle SubtractRowsByMinValue(NativeArray<float> costs, int width, JobHandle dependency = default)
        {
            NativeArray<float> minValues = new (width, TempJob);
            JobHandle minRowsJobHandle = JFindMinValue.Process(costs, minValues, width, dependency);
            JobHandle subtractRowsJobHandle = JSubtractMinValue.Process(costs, minValues, width, minRowsJobHandle);
            minValues.Dispose(subtractRowsJobHandle);
            return subtractRowsJobHandle;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ SUB -JOBS ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private struct JFindMinValue : IJob
        {
            [ReadOnly] public int Index;
        
            [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeSlice<float> Row;
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeArray<float> Mins;
        
            public void Execute()
            {
                Mins[Index] = Row.Min();;
            }
            
            public static JobHandle Process(NativeArray<float> costs, NativeArray<float> minValues, int width, JobHandle dependency = default)
            {
                NativeArray<JobHandle> minJobHandles = new (width, Temp, UninitializedMemory);
                for (int y = 0; y < width; y++)
                {
                    int minIndex = y * width;
                    JFindMinValue job = new JFindMinValue
                    {
                        Index = y,
                        Row = costs.Slice(minIndex, width),
                        Mins = minValues
                    };
                    minJobHandles[y] = job.Schedule(dependency);
                }
                JobHandle.ScheduleBatchedJobs();
                return JobHandle.CombineDependencies(minJobHandles);
            }
        }
        
        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private struct JSubtractMinValue : IJobFor
        {
            [ReadOnly] public int Width;
            [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeArray<float> Mins;
            [NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
            public NativeArray<float> Costs;
        
            public void Execute(int index)
            {
                int y = index / Width;
                Costs[index] -= Mins[y];
            }
        
            public static JobHandle Process(NativeArray<float> costs, NativeArray<float> minValues, int width, JobHandle minDependency = default)
            {
                JSubtractMinValue job = new JSubtractMinValue
                {
                    Width = width,
                    Mins = minValues,
                    Costs = costs
                };
                JobHandle jobHandle = job.ScheduleParallel(costs.Length, JobsUtility.JobWorkerCount - 1, minDependency);
                return jobHandle;
            }
        }
        
        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private struct JCreateMasksAndCoveredMatrix : IJob
        {
            [ReadOnly] public int Width;
        
            [ReadOnly, NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
            public NativeArray<float> Costs;
        
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeArray<byte> Masks;
            [NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
            public NativeArray<bool> YCovered;
            [NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
            public NativeArray<bool> XCovered;
        
            public void Execute()
            {
                for (int i = 0; i < Costs.Length; i++)
                {
                    (int x, int y) = GetXY(i, Width);
                    if (!(abs(Costs[i]) < EPSILON) || YCovered[y] || XCovered[x]) return;
                    Masks[i] = 1;
                    YCovered[y] = true;
                    XCovered[x] = true;
                }
            }
        
            public static JobHandle Process(NativeArray<float> costs, NativeArray<byte> masks, NativeArray<bool> yCovered, NativeArray<bool> xCovered, int width, JobHandle dependency = default)
            {
                JCreateMasksAndCoveredMatrix job = new JCreateMasksAndCoveredMatrix
                {
                    Width = width,
                    Costs = costs,
                    Masks = masks,
                    YCovered = yCovered,
                    XCovered = xCovered
                };
                JobHandle jobHandle = job.Schedule(dependency);
                return jobHandle;
            }
        }
        
        private struct JClearCovers : IJobFor
        {
            [WriteOnly, NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
            public NativeArray<bool> RowCovered;
            [WriteOnly, NativeDisableContainerSafetyRestriction, NativeDisableParallelForRestriction] 
            public NativeArray<bool> ColumnCovered;
        
            public void Execute(int index)
            {
                RowCovered[index] = false;
                ColumnCovered[index] = false;
            }

            public static JobHandle Process(NativeArray<bool> yCovered, NativeArray<bool> xCovered, JobHandle dependency = default)
            {
                JClearCovers job = new JClearCovers
                {
                    RowCovered = yCovered,
                    ColumnCovered = xCovered
                };
                JobHandle jobHandle = job.ScheduleParallel(yCovered.Length, JobsUtility.JobWorkerCount -1, dependency);
                return jobHandle;
            }
        }
    }
    
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ JOBS ◆◆◆◆◆◆                                                    ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public struct JHungarianAlgorithmSteps : IJob
        {
            [ReadOnly] public int Width;
            
            public NativeArray<float> Costs;
            public NativeArray<int>  AgentsTasks;
            
            private int2 PathStart;
            [DeallocateOnJobCompletion] private NativeArray<byte> Masks;
            [DeallocateOnJobCompletion] private NativeArray<bool> RowsCovered;
            [DeallocateOnJobCompletion] private NativeArray<bool> ColsCovered;
            [DeallocateOnJobCompletion] private NativeArray<int2> Path;

            public JHungarianAlgorithmSteps(NativeArray<int> agentsTasks, NativeArray<float> costs, int width)
            {
                Width = width;
                PathStart = int2.zero;
                Path = new (costs.Length, TempJob, ClearMemory);
                AgentsTasks = agentsTasks;
                Costs = costs;
                Masks = new NativeArray<byte>(costs.Length, TempJob, ClearMemory);
                RowsCovered = new NativeArray<bool>(width, TempJob, ClearMemory);
                ColsCovered = new NativeArray<bool>(width, TempJob, ClearMemory);
            }
            
            public void Execute()
            {
                for (int i = 0; i < Costs.Length; i++)
                {
                    (int x, int y) = GetXY(i, Width);
                    bool assignValue = Approximately(Costs[i], 0) && !RowsCovered[y] && !ColsCovered[x];
                    Masks[i] = (byte)select(Masks[i], 1, assignValue);
                    RowsCovered[y] = assignValue || RowsCovered[y];
                    ColsCovered[x] = assignValue || ColsCovered[x];
                }
                
                int step = 1;
                while (step != -1)// <-- THIS WHILE is ne one crashing
                {
                    step = step switch
                    {
                        1 => RunStep1(),
                        2 => RunStep2(),
                        3 => RunStep3(),
                        4 => RunStep4(),
                        _ => step
                    };
                }

                for (int i = 0; i < Costs.Length; i++)
                {
                    int y = i / Width;
                    int x = i - y * Width;
                    AgentsTasks[y] = select(AgentsTasks[y],x,Masks[i] == 1);
                    i += select(0,Width - x - 1,Masks[i] == 1);
                }
            }
            
            //TODO Check if Mathf.Approximately is still bugged for burst!
            private bool Approximately(float a, float b)
            {
                return abs(b - a) < max(0.000001f * max(abs(a), abs(b)), EPSILON * 8);
            }
            
            private int2 FindZero()
            {
                for (int i = 0; i < Costs.Length; i++)
                {
                    int y = i / Width;
                    int x = i - y * Width;
                    if (Approximately(Costs[i], 0) && !RowsCovered[y] && !ColsCovered[x] ) return new int2(x,y);
                }
                return new int2(-1, -1);
            }
            
            private int RunStep1()
            {
                for (int i = 0; i < Masks.Length; i++)
                {
                    int x = (int)fmod(i, Width);
                    ColsCovered[x] = Masks[i] == 1 || ColsCovered[x];
                }
                int colsCoveredCount = 0;
                for (int x = 0; x < Width; x++)
                {
                    colsCoveredCount += select(0,1,ColsCovered[x]);
                }
                return select(2, -1, colsCoveredCount == Width);
            }
            
            //EXPENSIVE!
            private int RunStep2()
            {
                while (true)
                {
                    int2 location = FindZero();
                    if (location.y == -1) return 4;
                    
                    int index = location.y * Width + location.x;
                    Masks[index] = 2;

                    int starColumn = FindStarInRow(location.y);
                    if (starColumn != -1)
                    {
                        RowsCovered[location.y] = true;
                        ColsCovered[starColumn] = false;
                    }
                    else
                    {
                        PathStart = location;
                        return 3;
                    }
                }
            }
            
            //CHEAP!
            private int RunStep3()
            {
                int pathIndex = 0;
                Path[0] = PathStart;
                while (true)
                {
                    int y = FindStarInColumn(Path[pathIndex].x);
                    if (y == -1) break;
                    Path[++pathIndex] = new int2(Path[pathIndex - 1].x, y);
                    int x = FindPrimeInRow(Path[pathIndex].y);
                    Path[++pathIndex] = new int2(x, Path[pathIndex - 1].y);
                }
                ConvertPath(pathIndex + 1);
                ClearCovers();
                ClearPrimes();
                return 1;
            }
            
            //Expensive!
            private int RunStep4()
            {
                float minValue = float.MaxValue;
                for (int i = 0; i < Costs.Length; i++)
                {
                    int y = i / Width;
                    int x = i - y * Width;
                    bool isNotCovered = !RowsCovered[y] && !ColsCovered[x];
                    minValue = select(minValue, min(minValue, Costs[i]), isNotCovered);
                }
                
                for (int i = 0; i < Costs.Length; i++)
                {
                    int y = i / Width;
                    int x = i - y * Width;
                    Costs[i] += select(0,minValue,RowsCovered[y]);
                    Costs[i] -= select(0,minValue,!ColsCovered[x]);
                }
                return 2;
            }
            
            private void ConvertPath(int pathLength)
            {
                for (int i = 0; i < pathLength; i++)
                {
                    int index = Path[i].y * Width + Path[i].x;
                    Masks[index] = Masks[index] switch
                    {
                        1 => 0,
                        2 => 1,
                        _ => Masks[index]
                    };
                }
            }
            
            private void ClearCovers()
            {
                for (int i = 0; i < Width; i++)
                {
                    RowsCovered[i] = false;
                    ColsCovered[i] = false;
                }
            }
            
            private void ClearPrimes()
            {
                for (int i = 0; i < Masks.Length; i++)
                {
                    if (Masks[i] == 2) Masks[i] = 0;
                }
            }
            
            private int FindStarInRow(int y)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    if (Masks[index] == 1) return x;
                }
                return -1;
            }
            
            private int FindPrimeInRow(int y)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    if (Masks[index] == 2) return x;
                }
                return -1;
            }
            
            private int FindStarInColumn(int x)
            {
                for (int y = 0; y < Width; y++)
                {
                    int index = y * Width + x;
                    if (Masks[index] == 1) return y;
                }
                return -1;
            }
        }
        
        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public struct JChunkedHungarianAlgorithmSteps : IJob
        {
            [ReadOnly] public int OffsetIndex;
            [ReadOnly] public int Width;
            
            [NativeDisableContainerSafetyRestriction, NativeDisableUnsafePtrRestriction] 
            public NativeSlice<float> Costs;
            [NativeDisableContainerSafetyRestriction, NativeDisableUnsafePtrRestriction] 
            public NativeSlice<int> AgentsTasks;
            
            private int2 PathStart;
            [DeallocateOnJobCompletion] private NativeArray<int2> Path;
            
            [DeallocateOnJobCompletion] private NativeArray<byte> Masks;
            [DeallocateOnJobCompletion] private NativeArray<bool> RowsCovered;
            [DeallocateOnJobCompletion] private NativeArray<bool> ColsCovered;
            
            public JChunkedHungarianAlgorithmSteps(NativeSlice<int> agentsTasks, NativeSlice<float> costs, int width, int offsetIndex)
            {
                OffsetIndex = offsetIndex;
                Width = width;
                AgentsTasks = agentsTasks;
                Costs = costs;
                PathStart = int2.zero;
                Path = new (costs.Length, TempJob, ClearMemory);
                Masks = new NativeArray<byte>(costs.Length, TempJob, ClearMemory);
                RowsCovered = new NativeArray<bool>(width, TempJob, ClearMemory);
                ColsCovered = new NativeArray<bool>(width, TempJob, ClearMemory);
            }
            
            public void Execute()
            {
                for (int i = 0; i < Costs.Length; i++)
                {
                    (int x, int y) = GetXY(i, Width);
                    bool assignValue = Approximately(Costs[i], 0) && !RowsCovered[y] && !ColsCovered[x];
                    Masks[i] = (byte)select(Masks[i], 1, assignValue);
                    RowsCovered[y] = assignValue || RowsCovered[y];
                    ColsCovered[x] = assignValue || ColsCovered[x];
                }
                
                int step = 1;
                while (step != -1)// <-- THIS WHILE is ne one crashing
                {
                    step = step switch
                    {
                        1 => RunStep1(),
                        2 => RunStep2(),
                        3 => RunStep3(),
                        4 => RunStep4(),
                        _ => step
                    };
                }

                for (int i = 0; i < Costs.Length; i++)
                {
                    int y = i / Width;
                    int x = i - y * Width;
                    AgentsTasks[y] = select(AgentsTasks[y],OffsetIndex + x,Masks[i] == 1);
                    i += select(0,Width - x - 1,Masks[i] == 1);
                }
            }
            
            //TODO Check if Mathf.Approximately is still bugged for burst!
            private bool Approximately(float a, float b)
            {
                return abs(b - a) < max(0.000001f * max(abs(a), abs(b)), EPSILON * 8);
            }
            
            private int2 FindZero()
            {
                for (int i = 0; i < Costs.Length; i++)
                {
                    int y = i / Width;
                    int x = i - y * Width;
                    if (Approximately(Costs[i], 0) && !RowsCovered[y] && !ColsCovered[x] ) return new int2(x,y);
                }
                return new int2(-1, -1);
            }
            
            private int RunStep1()
            {
                for (int i = 0; i < Masks.Length; i++)
                {
                    int x = (int)fmod(i, Width);
                    ColsCovered[x] = Masks[i] == 1 || ColsCovered[x];
                }
                int colsCoveredCount = 0;
                for (int x = 0; x < Width; x++)
                {
                    colsCoveredCount += select(0,1,ColsCovered[x]);
                }
                return select(2, -1, colsCoveredCount == Width);
            }
            
            //EXPENSIVE!
            private int RunStep2()
            {
                while (true)
                {
                    int2 location = FindZero();
                    if (location.y == -1) return 4;
                    
                    int index = location.y * Width + location.x;
                    Masks[index] = 2;

                    int starColumn = FindStarInRow(location.y);
                    if (starColumn != -1)
                    {
                        RowsCovered[location.y] = true;
                        ColsCovered[starColumn] = false;
                    }
                    else
                    {
                        PathStart = location;
                        return 3;
                    }
                }
            }
            
            //CHEAP!
            private int RunStep3()
            {
                int pathIndex = 0;
                Path[0] = PathStart;
                while (true)
                {
                    int y = FindStarInColumn(Path[pathIndex].x);
                    if (y == -1) break;
                    Path[++pathIndex] = new int2(Path[pathIndex - 1].x, y);
                    int x = FindPrimeInRow(Path[pathIndex].y);
                    Path[++pathIndex] = new int2(x, Path[pathIndex - 1].y);
                }
                ConvertPath(pathIndex + 1);
                ClearCovers();
                ClearPrimes();
                return 1;
            }
            
            //Expensive!
            private int RunStep4()
            {
                float minValue = float.MaxValue;
                for (int i = 0; i < Costs.Length; i++)
                {
                    int y = i / Width;
                    int x = i - y * Width;
                    bool isNotCovered = !RowsCovered[y] && !ColsCovered[x];
                    minValue = select(minValue, min(minValue, Costs[i]), isNotCovered);
                }
                
                for (int i = 0; i < Costs.Length; i++)
                {
                    int y = i / Width;
                    int x = i - y * Width;
                    Costs[i] += select(0,minValue,RowsCovered[y]);
                    Costs[i] -= select(0,minValue,!ColsCovered[x]);
                }
                return 2;
            }
            
            private void ConvertPath(int pathLength)
            {
                for (int i = 0; i < pathLength; i++)
                {
                    int index = Path[i].y * Width + Path[i].x;
                    Masks[index] = Masks[index] switch
                    {
                        1 => 0,
                        2 => 1,
                        _ => Masks[index]
                    };
                }
            }
            
            private void ClearCovers()
            {
                for (int i = 0; i < Width; i++)
                {
                    RowsCovered[i] = false;
                    ColsCovered[i] = false;
                }
            }
            
            private void ClearPrimes()
            {
                for (int i = 0; i < Masks.Length; i++)
                {
                    if (Masks[i] == 2) Masks[i] = 0;
                }
            }
            
            private int FindStarInRow(int y)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    if (Masks[index] == 1) return x;
                }
                return -1;
            }
            
            private int FindPrimeInRow(int y)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    if (Masks[index] == 2) return x;
                }
                return -1;
            }
            
            private int FindStarInColumn(int x)
            {
                for (int y = 0; y < Width; y++)
                {
                    int index = y * Width + x;
                    if (Masks[index] == 1) return y;
                }
                return -1;
            }
        }
}
