using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine;

using Kaizerwald.Pattern;
using Kaizerwald.Utilities.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

using static Unity.Mathematics.math;
using static UnityEngine.Mesh;
using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static Unity.Collections.Allocator;
using static Unity.Collections.NativeArrayOptions;

using half4 = Unity.Mathematics.half4;
using UnityEngine.Rendering;

namespace Kaizerwald.TerrainBuilder
{
    [ExecutionOrder(64)]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    [RequireComponent(typeof(TerrainSettings), typeof(TerrainGridSystem))]
    public partial class SimpleTerrain : SingletonBehaviour<SimpleTerrain>
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                ◆◆◆◆◆◆ FIELD ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        private bool isInitialized;
        [SerializeField] private Material DefaultMaterial;
        [SerializeField] private LayerMask TerrainLayerMask;
        
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                               ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        public Transform TerrainTransform { get; private set; }
        [field:SerializeField] public TerrainSettings TerrainSettings { get; private set; }
        [field:SerializeField] public TerrainGridSystem TerrainGridSystem { get; private set; }
        [field:SerializeField] public SpawnerManager SpawnerManager { get; private set; }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Accessors ◈◈◈◈◈◈                                                                                        ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        
        public Mesh TerrainMesh => GetComponent<MeshFilter>().sharedMesh;
        
        //┌────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
        //│  ◇◇◇◇◇◇ Getters ◇◇◇◇◇◇                                                                                     │
        //└────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        public LayerMask TerrainLayer => TerrainLayerMask;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        protected override void OnAwake()
        {
            base.OnAwake();
            //if (isInitialized) return;
            //Initialize();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        

        protected override void Initialize()
        {
            TerrainTransform  = transform;
            meshFilter        = GetComponent<MeshFilter>();
            meshRenderer      = GetComponent<MeshRenderer>();
            meshCollider      = GetComponent<MeshCollider>();
            TerrainSettings   = GetComponent<TerrainSettings>();
            //for some reason we need to separate GetComponent from Initialize or we get a null ref!
            TerrainSettings.Initialize(); 
            
            GenerateTerrain();
            
            SpawnerManager    = SpawnerManager.Instance;
            TerrainGridSystem = TerrainGridSystem.Instance;
            //isInitialized = true;
        }

        public void GenerateTerrain(string terrainName = "SimpleTerrain")
        {
            name = terrainName;
            gameObject.layer = TerrainLayerMask.GetLayerIndex();
            
            // Mesh -> MeshData
            Mesh terrainMesh = new Mesh(){ name = "TerrainMesh" };
            MeshDataArray meshDataArray = AllocateWritableMeshData(1);
            meshDataArray[0].InitializeBufferParams(TerrainSettings.VerticesCount, TerrainSettings.TriangleIndicesCount);
            
            // MeshData Creation
            JMeshData.ScheduleParallel(meshDataArray[0], TerrainSettings.NumVerticesXY).Complete();
            meshDataArray[0].SetSubMesh(TerrainSettings.VerticesCount, TerrainSettings.TriangleIndicesCount);
            ApplyAndDisposeWritableMeshData(meshDataArray, terrainMesh);

            UpdateMeshProperties(terrainMesh);
            //TerrainGridSystem.Initialize(this);
        }

        private void UpdateMeshProperties(Mesh terrainMesh)
        {
            terrainMesh.RecalculateNormals();
            terrainMesh.RecalculateTangents();
            terrainMesh.RecalculateBounds();
            meshFilter.sharedMesh = terrainMesh;
            meshCollider.sharedMesh = terrainMesh;
            meshRenderer.sharedMaterial = DefaultMaterial;
            meshRenderer.ResetBounds();
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Grid System Accessor ◈◈◈◈◈◈                                                                             ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜

        public NativeArray<Cell> GetCellPathTo(float3 currentPosition, float3 targetPosition, Allocator allocator = Temp)
        {
            return TerrainGridSystem.GetCellPathTo(currentPosition, targetPosition, allocator);
        }
    }
    
    
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                                 ◆◆◆◆◆◆ JOBS ◆◆◆◆◆◆                                                 ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    internal struct JMeshData : IJobFor
    {
        [ReadOnly] public int2 NumVerticesXY;
        [ReadOnly, NativeDisableParallelForRestriction]
        public NativeArray<float> HeightMap;
        [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public NativeArray<ushort> Triangles;
        [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public NativeArray<half2> Uvs;
        [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> Vertices;

        private JMeshData(MeshData meshData, int2 numVerticesXY, NativeArray<float> heightMap)
        {
            NumVerticesXY = numVerticesXY;
            HeightMap = heightMap;
            Vertices = meshData.GetVertexData<float3>(stream: 0);
            Uvs = meshData.GetVertexData<half2>(stream: 3);
            Triangles = meshData.GetIndexData<ushort>();
        }

        public void Execute(int index)
        {
            int width = NumVerticesXY.x;
            int y = index / width;
            int x = index - y * width;
            int2 numQuadsXY = NumVerticesXY - 1;
            //Here we calculate the offset apply so the pivot is at the center of the mesh
            float2 halfSize = (float2)numQuadsXY / 2f;
            //DONT use math.select! select check BOTH elements regarding the condition (so NoiseMap[index] make the job crash)
            float height = HeightMap.Length == 1 ? HeightMap[0] : HeightMap[index];
            Vertices[index] = new float3(x - halfSize.x, height, y - halfSize.y);
            Uvs[index] = new half2(float2(x,y) / NumVerticesXY);
            //if (x < numQuadsXY.x && y < numQuadsXY.y)
            if ( all(int2(x,y) < numQuadsXY))
            {
                int baseTriIndex = (index - y) * 6;
                //(0,0)-(1,0)-(0,1)-(1,1) 
                int4 trianglesVertex = new int4(index, index + 1, index + width, index + width + 1);
                Triangles[baseTriIndex + 0] = (ushort)trianglesVertex.x; //(0,0)
                Triangles[baseTriIndex + 1] = (ushort)trianglesVertex.z; //(0,1)
                Triangles[baseTriIndex + 2] = (ushort)trianglesVertex.y; //(1,0)
                Triangles[baseTriIndex + 3] = (ushort)trianglesVertex.y; //(1,0)
                Triangles[baseTriIndex + 4] = (ushort)trianglesVertex.z; //(0,1)
                Triangles[baseTriIndex + 5] = (ushort)trianglesVertex.w; //(1,1)
            }
        }
        
        internal static JobHandle ScheduleParallel(MeshData meshData, int2 numVerticesXY, NativeArray<float> heightMap, JobHandle jobHandle = default)
        {
            JMeshData job = new JMeshData(meshData, numVerticesXY, heightMap);
            JobHandle jh = job.ScheduleParallel(numVerticesXY.x * numVerticesXY.y, JobWorkerCount - 1, jobHandle);
            return jh;
        }
        
        internal static JobHandle ScheduleParallel(MeshData meshData, int2 numVerticesXY, float height = 0, JobHandle jobHandle = default)
        {
            NativeArray<float> heightMap = new (1, TempJob, UninitializedMemory);
            heightMap[0] = height;
            JobHandle jh = ScheduleParallel(meshData, numVerticesXY, heightMap, jobHandle);
            heightMap.Dispose(jh);
            return jh;
        }
    }
}


