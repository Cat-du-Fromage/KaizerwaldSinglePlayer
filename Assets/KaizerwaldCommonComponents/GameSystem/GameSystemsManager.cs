using System.Collections.Generic;
using System.Linq;
using Kaizerwald.Utilities;
using UnityEngine;

namespace Kaizerwald
{
    public class GameSystemsManager : Singleton<GameSystemsManager>
    {
        protected List<IGameSystem> GameSystems;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            GameSystems = FindGameSystemInterface();
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
            MonoBehaviour[] monoBehaviours = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            List<IGameSystem> gameSystems = monoBehaviours
            .SelectMany(behaviour => behaviour.GetComponents<IGameSystem>())
            .Distinct()
            .OrderBy(system => system.ExecutionOrderWeight)
            .ToList();
            return gameSystems;
        }
    }
}
