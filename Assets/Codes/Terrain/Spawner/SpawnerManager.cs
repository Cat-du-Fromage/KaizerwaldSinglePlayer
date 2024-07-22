using System.Collections;
using System.Collections.Generic;
using Kaizerwald.Pattern;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kaizerwald.TerrainBuilder
{
    [RequireComponent(typeof(SimpleTerrain))]
    [ExecuteAfter(typeof(SimpleTerrain), OrderIncrease = 1)]
    public class SpawnerManager : SingletonBehaviour<SpawnerManager>
    {
        private const int SPAWNER_DEPTH_SIZE = 16;
        private const int BORDER_OFFSET = 2;
        private readonly Vector3 spawnerOffset = new Vector3(0.1f, 1, 0.1f);
        
        private TerrainSettings terrainSettings;
        private SimpleTerrain simpleTerrain;
        
        [SerializeField] private List<GameObject> PlayerSpawnerObjects;
        [SerializeField] private List<SpawnerComponent> PlayerSpawners;
        
        protected override void Initialize()
        {
            simpleTerrain = GetComponent<SimpleTerrain>();
            terrainSettings = GetComponent<TerrainSettings>();
            InitializeSpawners();
        }

        private void InitializeSpawners()
        {
            int spawnerCount = transform.childCount;
            PlayerSpawnerObjects = new List<GameObject>(spawnerCount);
            PlayerSpawners = new List<SpawnerComponent>(spawnerCount);
            for (int i = 0; i < spawnerCount; i++)
            {
                Transform child = transform.GetChild(i);
                //child.forward = -GetDirection((ECardinal)i); // Use inverse direction, because we wont dir to center
                //child.position = GetSpawnerPosition((ECardinal)i);
                //child.localScale = Vector3.Scale(spawnerOffset, new Vector3(terrainSettings.SizeXY.x - BORDER_OFFSET, 1, SPAWNER_DEPTH_SIZE));
                PlayerSpawnerObjects.Add(child.gameObject);
                PlayerSpawners.Add(child.GetComponent<SpawnerComponent>());
            }
        }
        
        public GameObject GetSpawnerForNumber(int index)
        {
            return index < 0 || index >= PlayerSpawnerObjects.Count ? null : PlayerSpawnerObjects[index];
        }
        
        public Transform GetSpawnerTransform(int spawnIndex)
        {
            return GetSpawnerForNumber(spawnIndex).transform;
        }
        
        public Vector3 GetPlayerFirstSpawnPosition(int spawnIndex)
        {
            Transform spawnerTransform = GetSpawnerTransform(spawnIndex);
            if(spawnerTransform == null) return Vector3.zero;

            Vector3 localRight = spawnerTransform.localRotation * transform.right;
            Vector3 localForward = spawnerTransform.localRotation * transform.forward;
            
            //float spawnHorizontalSize = (terrainSettings.SizeXY.x / 2f) - BORDER_OFFSET;
            float zOffset = (spawnerTransform.localScale.z * 10) / 2 - BORDER_OFFSET;
            float xOffset = (spawnerTransform.localScale.x * 10) / 2 - BORDER_OFFSET * 2;
            Vector3 firstSpawnPoint = spawnerTransform.localPosition - localRight * xOffset;
            
            //transform.localRotation
            firstSpawnPoint += localForward * zOffset;
            return Vector3.Scale(firstSpawnPoint, new Vector3(1f,0,1f));
        }
    }
}
