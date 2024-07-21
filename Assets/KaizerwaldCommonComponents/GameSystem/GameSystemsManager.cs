using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Kaizerwald.Pattern;

namespace Kaizerwald
{
    public class GameSystemsManager : Singleton<GameSystemsManager>
    {
        [SerializeField] private int NumSystems;
        protected List<IGameSystem> GameSystems;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            GameSystems = FindGameSystemInterface();
            enabled = NumSystems > 0;
            //if(NumSystems == 0) enabled = false;
        }

        private void Start()
        {
            for (int i = 0; i < GameSystems.Count; i++)
            {
                GameSystems[i].OnStart();
            }
        }
        
        private void FixedUpdate()
        {
            for (int i = 0; i < GameSystems.Count; i++)
            {
                GameSystems[i].OnFixedUpdate();
            }
        }

        private void Update()
        {
            for (int i = 0; i < GameSystems.Count; i++)
            {
                GameSystems[i].OnUpdate();
            }
        }
        
        private void LateUpdate()
        {
            for (int i = 0; i < GameSystems.Count; i++)
            {
                GameSystems[i].OnLateUpdate();
            }
        }

        private List<IGameSystem> FindGameSystemInterface()
        {
            NumSystems = 0;
            MonoBehaviour[] monoBehaviours = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            List<IGameSystem> gameSystems = monoBehaviours
            .SelectMany(behaviour => behaviour.GetComponents<IGameSystem>())
            .Distinct()
            .OrderBy(system => system.ExecutionOrderWeight)
            .ToList();
            NumSystems = gameSystems.Count;
            return gameSystems;
        }
    }
}
