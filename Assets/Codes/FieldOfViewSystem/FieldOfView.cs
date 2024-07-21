using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Rendering;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.InputSystem;
using UnityEngine;

using static UnityEngine.Mesh;
using static Unity.Mathematics.math;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;
using static UnityEngine.Rendering.VertexAttribute;
using static UnityEngine.Rendering.VertexAttributeFormat;
using static Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;
using static Unity.Collections.CollectionHelper;

using Color = UnityEngine.Color;
using float3 = Unity.Mathematics.float3;
using quaternion = Unity.Mathematics.quaternion;
using VertexAttribute = UnityEngine.Rendering.VertexAttribute;
using Mesh = UnityEngine.Mesh;

namespace Kaizerwald.FieldOfView
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public partial class FieldOfView : MonoBehaviour
    {
        private const int ORIGIN_HEIGHT   = 32;
        private const int RAY_DISTANCE    = 64;
        
        public const float THICKNESS = 0.2f;
        
        private const float GROUND_OFFSET = 0.5f;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        private QueryParameters queryParameters;
        private Transform cachedTransform;
        
        // vertices
        private NativeArray<float3> verticesPositions;
        private bool isSchedule;
        private JobHandle verticesHeightsHandle;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        [field:SerializeField] public FieldOfViewController Controller { get; private set; }
        [field:SerializeField] public MeshFilter MeshFilter { get; private set; }
        [field:SerializeField] public MeshRenderer MeshRenderer { get; private set; }
        [field:SerializeField] public MeshInfos MeshInfos { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public FieldOfViewParams FovParams => Controller.FovParams;
        public Mesh FovMesh => MeshFilter.sharedMesh;
        
        public float3 Position => cachedTransform.position;
        public float3 Forward  => cachedTransform.forward;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        private void OnDisable()
        {
            if (isSchedule) verticesHeightsHandle.Complete();
        }

        private void OnDestroy()
        {
            if(isSchedule) verticesHeightsHandle.Complete();
            if(verticesPositions.IsCreated) verticesPositions.Dispose();
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Update Manager Events ◈◈◈◈◈◈                                                                            ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void OnFixedUpdate()
        {
            if (isSchedule) verticesHeightsHandle.Complete();
            
            verticesHeightsHandle = UpdateVerticesHeightsByJob();
            isSchedule = true;
        }
        
        public void OnLateUpdate()
        {
            if (!isSchedule || !verticesHeightsHandle.IsCompleted) return;
            
            verticesHeightsHandle.Complete();
            FovMesh.SetVertexBufferData(verticesPositions, 0, 0, verticesPositions.Length);
            isSchedule = false;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public void Show() => MeshRenderer.enabled = true;
        public void Hide() => MeshRenderer.enabled = false;
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Update Manager Events ◈◈◈◈◈◈                                                                            ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void BaseInitialize(float range, float sideAngleRadian, float widthLength, float resolution = 1)
        {
            queryParameters = new QueryParameters(FieldOfViewManager.Instance.TerrainLayer.value);
            
            cachedTransform = transform;
            MeshFilter      = GetComponent<MeshFilter>();
            MeshRenderer    = GetComponent<MeshRenderer>();
            
            MeshInfos = new MeshInfos(range, sideAngleRadian, widthLength, THICKNESS, resolution);
            verticesPositions.Dispose();
            verticesPositions = new NativeArray<float3>(MeshInfos.VerticesCount, Persistent, UninitializedMemory);
            
            CreateMesh();
        }
        
        public void Initialize(FieldOfViewController controller, float resolution = 1)
        {
            Controller = controller;
            BaseInitialize(controller.FovParams.Range, controller.FovParams.SideAngleRadian, controller.FovParams.WidthLength);
        }
        
        public JobHandle UpdateVerticesHeightsByJob()
        {
            NativeArray<RaycastHit> results = new (verticesPositions.Length, TempJob, UninitializedMemory);
            JobHandle raycastHandle = JRaycastsCommands.ScheduleParallel(results, verticesPositions, Position, Forward, queryParameters);
            JobHandle heightsHandle = JHeightVertices.Schedule(verticesPositions, results, -Position.y + GROUND_OFFSET, raycastHandle);
            results.Dispose(heightsHandle);
            return heightsHandle;
        }
        
        private void CreateMesh()
        {
            Mesh fovMesh = new Mesh { name = "FovMesh" };
            fovMesh.MarkDynamic();
            
            // Set Buffer Params
            fovMesh.SetVertexBufferParams(MeshInfos.VerticesCount, FovUtils.GetVertexAttribute());
            fovMesh.SetIndexBufferParams(MeshInfos.TriangleIndicesCount, IndexFormat.UInt16);

            // Build vertices and Triangles
            using NativeArray<ushort> triangleIndices = new(MeshInfos.TriangleIndicesCount, TempJob, UninitializedMemory);
            JobHandle triangleJh = JBuildTriangleIndices.ScheduleParallel(triangleIndices);
            JobHandle verticesJh = BuildVerticesByJob(verticesPositions);
            JobHandle.CompleteAll(ref triangleJh, ref verticesJh);
            
            // Set Buffer Data
            fovMesh.SetVertexBufferData(verticesPositions, 0, 0, MeshInfos.VerticesCount);
            fovMesh.SetIndexBufferData(triangleIndices, 0, 0, MeshInfos.TriangleIndicesCount);
            
            // Set SubMesh
            fovMesh.subMeshCount = 1;
            fovMesh.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndices.Length));
            
            fovMesh.Optimize();
            fovMesh.RecalculateNormals();
            fovMesh.RecalculateTangents();
            fovMesh.RecalculateBounds();
            
            //Carefull! fovMesh.Optimize() may change order of indices so copy after doing it!
            verticesPositions.CopyFrom(CreateNativeArray(fovMesh.vertices,Temp).Reinterpret<float3>());
            MeshFilter.sharedMesh = fovMesh;
        }
        
        private JobHandle BuildVerticesByJob(NativeArray<float3> vertices)
        {
            JobHandle borderHandle   = JBorderVertices.Schedule(vertices, MeshInfos.Border, FovParams);
            JobHandle arcHandle      = JArcVertices.Schedule(vertices, MeshInfos, FovParams);
            JobHandle frontHandle    = JFrontVertices.Schedule(vertices, MeshInfos, FovParams);
            JobHandle verticesHandle = JobHandle.CombineDependencies(borderHandle, arcHandle, frontHandle);
            
            NativeArray<RaycastHit> results = new (vertices.Length, TempJob, UninitializedMemory);
            JobHandle raycastHandle = JRaycastsCommands.ScheduleParallel(results, vertices, Position, Forward, queryParameters, verticesHandle);
            JobHandle heightsHandle = JHeightVertices.Schedule(vertices, results, -Position.y + GROUND_OFFSET, raycastHandle);
            results.Dispose(heightsHandle);
            return heightsHandle;
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ JOBS ◆◆◆◆◆◆                                                  ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Mesh Vertices Jobs ◈◈◈◈◈◈                                                                               ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        [BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
        public struct JBorderVertices : IJobFor
        {
            [ReadOnly] public float2 BorderDirection;
            [ReadOnly] public float2 OuterStart;
            [ReadOnly] public float2 InnerStart;
            [ReadOnly] public MeshSectionInfos BorderMeshInfos;
            
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeArray<float3> Vertices;

            public JBorderVertices(NativeArray<float3> vertices, MeshSectionInfos borderInfo, FieldOfViewParams fovParams)
            {
                BorderDirection = float2(cos(PI - fovParams.SideAngleRadian), sin(PI - fovParams.SideAngleRadian));
                OuterStart      = float2(-fovParams.WidthLength / 2, 0);
                InnerStart      = OuterStart + float2(BorderDirection.y, -BorderDirection.x) * THICKNESS;
                BorderMeshInfos = borderInfo;
                Vertices        = vertices;
            }
            
            public void Execute(int index)
            {
                float2 baseLeftOffset = index * BorderDirection;
                // -------------------------------------- LEFT SIDE ------------------------------
                // Outer
                float2 leftOuter = mad(baseLeftOffset, BorderMeshInfos.OuterStep, OuterStart);// OuterStart + baseLeftOffset * BorderMeshInfos.OuterStep;
                int outerLeftIndex = index * 2;
                Vertices[outerLeftIndex] = new float3(leftOuter.x, 0, leftOuter.y);
                // Inner
                float2 leftInner = mad(baseLeftOffset, BorderMeshInfos.InnerStep, InnerStart);// InnerStart + baseLeftOffset * BorderMeshInfos.InnerStep;
                int innerLeftIndex = outerLeftIndex + 1; //(i * 2) + 1
                Vertices[innerLeftIndex] = new float3(leftInner.x, 0, leftInner.y);

                // ------------------------------------- RIGHT SIDE -------------------------------
                // Inner
                float2 rightInner = leftInner - 2 * project(leftInner, right().xz);
                int innerRightIndex = Vertices.Length - (1 + index * 2);
                Vertices[innerRightIndex] = new float3(rightInner.x, 0, rightInner.y);
                // Outer
                float2 rightOuter = leftOuter - 2 * project(leftOuter, right().xz);
                int outerRightIndex = innerRightIndex - 1; //(vertices.Length - (1 + i * 2)) - 1;
                Vertices[outerRightIndex] = new float3(rightOuter.x, 0, rightOuter.y);
            }

            public static JobHandle Schedule(NativeArray<float3> vertices, MeshSectionInfos borderInfo, FieldOfViewParams fovParams, JobHandle dependency = default)
            {
                return new JBorderVertices(vertices, borderInfo, fovParams).Schedule(borderInfo.VertexCount / 4, dependency);
            }
        }
        
        [BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
        public struct JArcVertices : IJobFor
        {
            [ReadOnly] public int IndexOffset;
            
            [ReadOnly] public float Range;
            [ReadOnly] public float InnerRange;
            
            [ReadOnly] public float OuterArcStart;
            [ReadOnly] public float InnerArcStart;
            [ReadOnly] public float2 OuterBorderStart;
            
            [ReadOnly] public MeshSectionInfos ArcMeshInfos;
            
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeArray<float3> Vertices;

            public JArcVertices(NativeArray<float3> vertices, MeshInfos meshInfos, FieldOfViewParams fovParams)
            {
                IndexOffset      = meshInfos.Border.VertexCount / 2;
                Range            = fovParams.Range;
                InnerRange       = fovParams.Range - THICKNESS;
                OuterArcStart    = PI - fovParams.SideAngleRadian;
                InnerArcStart    = PIHALF + meshInfos.Arc.InnerStep * (int)(meshInfos.Arc.QuadCount / 2);
                OuterBorderStart = new float2(-fovParams.WidthLength / 2, 0);
                ArcMeshInfos     = meshInfos.Arc;
                Vertices         = vertices;
            }
            
            public void Execute(int index)
            {
                // ---------------------------------- LEFT SIDE  ------------------------------
                // Outer
                float currentAngleInRadian = OuterArcStart - (index + 1) * ArcMeshInfos.OuterStep;
                sincos(currentAngleInRadian, out float outerSin, out float outerCos);
                float2 outerLeft = mad(float2(outerCos, outerSin), Range, OuterBorderStart);
                Vertices[IndexOffset + index * 2] = new float3(outerLeft.x, 0, outerLeft.y);
                // Inner
                float innerAngleInRadian = InnerArcStart - (index + 1) * ArcMeshInfos.InnerStep;
                sincos(innerAngleInRadian, out float innerSin, out float innerCos);
                float2 innerLeft = mad(float2(innerCos, innerSin), InnerRange, OuterBorderStart);
                Vertices[IndexOffset + index * 2 + 1] = new float3(innerLeft.x, 0, innerLeft.y);
                // -------------------------------- RIGHT SIDE  -------------------------------
                // Inner
                float2 innerRight = innerLeft - 2 * project(innerLeft, right().xz);
                int innerRightIndex = Vertices.Length - (1 + index * 2) - IndexOffset;
                Vertices[innerRightIndex] = new float3(innerRight.x, 0, innerRight.y);
                // Outer
                float2 outerRight = outerLeft - 2 * project(outerLeft, right().xz);
                int outerRightIndex = innerRightIndex - 1;
                Vertices[outerRightIndex] = new float3(outerRight.x, 0, outerRight.y);
            }
            
            public static JobHandle Schedule(NativeArray<float3> vertices, MeshInfos meshInfos, FieldOfViewParams fovParams, JobHandle dependency = default)
            {
                return new JArcVertices(vertices, meshInfos, fovParams).Schedule(meshInfos.Arc.VertexCount / 4, dependency);
            }
        }
        
        [BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
        public struct JFrontVertices : IJobFor
        {
            [ReadOnly] public int IndexOffset;
            [ReadOnly] public float2 OuterFrontStart;
            [ReadOnly] public float2 InnerFrontStart;
            [ReadOnly] public MeshSectionInfos FrontMeshInfos;
            
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeArray<float3> Vertices;

            public JFrontVertices(NativeArray<float3> vertices, MeshInfos meshInfos, FieldOfViewParams fovParams)
            {
                IndexOffset     = meshInfos.Border.VertexCount / 2 + meshInfos.Arc.VertexCount / 2;
                OuterFrontStart = new float2(-fovParams.WidthLength / 2, fovParams.Range);
                InnerFrontStart = new float2(-fovParams.WidthLength / 2, fovParams.Range - THICKNESS);
                FrontMeshInfos  = meshInfos.Front;
                Vertices        = vertices;
            }
            
            public void Execute(int index)
            {
                int currentIndex = mad(index, 2, IndexOffset);
                float offset = index * FrontMeshInfos.OuterStep;
                Vertices[currentIndex]     = new float3(OuterFrontStart.x + offset, 0, OuterFrontStart.y);
                Vertices[currentIndex + 1] = new float3(InnerFrontStart.x + offset, 0, InnerFrontStart.y);
            }
            
            public static JobHandle Schedule(NativeArray<float3> vertices, MeshInfos meshInfos, FieldOfViewParams fovParams, JobHandle dependency = default)
            {
                return new JFrontVertices(vertices, meshInfos, fovParams).Schedule(meshInfos.Front.VertexCount / 2, dependency);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Height Vertices Jobs ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        [BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
        public struct JRaycastsCommands : IJobFor
        {
            [ReadOnly] public int OriginHeight;
            [ReadOnly] public int RayDistance;
            [ReadOnly] public QueryParameters QueryParams;
            
            [ReadOnly] public float2 Position;
            [ReadOnly] public float2x2 RotationMatrix;
            
            [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeArray<float3> Vertices;
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public NativeArray<RaycastCommand> Commands;

            public JRaycastsCommands(NativeArray<RaycastCommand> commands, NativeArray<float3> positions, float3 position, float3 forward, QueryParameters queryParams)
            {
                //float angle     = acos(Forward.z); // acos(dot(forward().xz, transformForward.xz)); ( forward.xz = (0,1) )
                //float signValue = sign(-Forward.x); // sign(forward().x * transformForward.y - forward().y * transformForward.x);
                sincos(acos(forward.z) * sign(-forward.x), out float sinA, out float cosA);
                OriginHeight   = ORIGIN_HEIGHT;
                RayDistance    = RAY_DISTANCE;
                QueryParams    = queryParams;
                Position       = position.xz;
                RotationMatrix = new float2x2(cosA, -sinA, sinA,  cosA);;
                Vertices       = positions;
                Commands       = commands;
            }
            
            public void Execute(int index)
            {
                float2 origin2D  = mul(RotationMatrix, Vertices[index].xz) + Position;
                Vector3 origin3D = new Vector3(origin2D.x, OriginHeight, origin2D.y);
                Commands[index]  = new RaycastCommand(origin3D, Vector3.down, QueryParams, RayDistance);
            }
            
            public static JobHandle ScheduleParallel(NativeArray<RaycastHit> results, NativeArray<float3> positions, float3 position, float3 forward, QueryParameters queryParams, JobHandle dependency = default)
            {
                NativeArray<RaycastCommand> commands = new (positions.Length, TempJob, UninitializedMemory);
                JRaycastsCommands job = new JRaycastsCommands(commands, positions, position, forward, queryParams);
                JobHandle rayCastCommandJh = job.ScheduleParallel(positions.Length, JobWorkerCount - 1, dependency);
                JobHandle rayCastHitJh = RaycastCommand.ScheduleBatch(commands, results, 1, 1, rayCastCommandJh);
                commands.Dispose(rayCastHitJh);
                return rayCastHitJh;
            }
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
        public struct JHeightVertices : IJobFor
        {
            [ReadOnly] public float Offset;
            
            [ReadOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeSlice<float> RaycastPointsSlice;
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction] 
            public NativeSlice<float> HeightsSlice;

            public JHeightVertices(NativeArray<float3> vertices, NativeArray<RaycastHit> results, float offset)
            {
                Offset             = offset;
                RaycastPointsSlice = results.Slice(0).SliceWithStride<float3>(0).SliceWithStride<float>(4);
                HeightsSlice       = vertices.Slice(0).SliceWithStride<float>(4);
            }
            
            public void Execute(int index)
            {
                HeightsSlice[index] = RaycastPointsSlice[index] + Offset;
            }
            
            public static JobHandle Schedule(NativeArray<float3> vertices, NativeArray<RaycastHit> results, float offset, JobHandle dependency = default)
            {
                return new JHeightVertices(vertices, results, offset).Schedule(vertices.Length, dependency);
            }
            public static JobHandle ScheduleParallel(NativeArray<float3> vertices, NativeArray<RaycastHit> results, float offset, JobHandle dependency = default)
            {
                return new JHeightVertices(vertices, results, offset).ScheduleParallel(vertices.Length, JobWorkerCount - 1, dependency);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Triangles Job ◈◈◈◈◈◈                                                                                    ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
    
        [BurstCompile(OptimizeFor = OptimizeFor.Performance, FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
        public struct JBuildTriangleIndices : IJobFor
        {
            [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
            public NativeArray<ushort> TriangleIndices;

            public JBuildTriangleIndices(NativeArray<ushort> triangleIndices)
            {
                TriangleIndices = triangleIndices;
            }

            public void Execute(int index)
            {
                int triangleIndicesStartIndex = index * 3;
                TriangleIndices[triangleIndicesStartIndex]     = (ushort)index;
                TriangleIndices[triangleIndicesStartIndex + 1] = (ushort)(index + 2 - (index & 1));
                TriangleIndices[triangleIndicesStartIndex + 2] = (ushort)(index + 1 + (index & 1));
            }
            
            public static JobHandle Schedule(NativeArray<ushort> triangleIndices, JobHandle dependency = default)
            {
                return new JBuildTriangleIndices(triangleIndices).Schedule(triangleIndices.Length / 3, dependency);
            }
            
            public static JobHandle ScheduleParallel(NativeArray<ushort> triangleIndices, JobHandle dependency = default)
            {
                return new JBuildTriangleIndices(triangleIndices).ScheduleParallel(triangleIndices.Length / 3, JobWorkerCount - 1, dependency);
            }
        }
    }
}