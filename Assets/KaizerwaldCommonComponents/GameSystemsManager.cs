using System;
using System.Collections.Generic;
using System.Linq;
using Kaizerwald.Utilities;
using UnityEngine;

namespace Kaizerwald
{
    public interface IGameSystem : IComparable<IGameSystem>
    {
        public int PriorityOrder { get; }
        
        public void OnStart();
        
        public void OnFixedUpdate();
        public void OnUpdate();
        public void OnLateUpdate();

        int IComparable<IGameSystem>.CompareTo(IGameSystem other)
        {
            return PriorityOrder.CompareTo(other.PriorityOrder);
        }
    }
    
    
    public class GameSystemsManager : Singleton<GameSystemsManager>
    {
        private List<IGameSystem> GameSystems;
        protected override void OnAwake()
        {
            base.OnAwake();
            GameSystems = FindGameSystemInterface();
        }

        private void Start()
        {
            foreach (IGameSystem gameSystem in GameSystems)
            {
                gameSystem.OnStart();
            }
        }
        
        private void FixedUpdate()
        {
            foreach (IGameSystem gameSystem in GameSystems)
            {
                gameSystem.OnFixedUpdate();
            }
        }

        private void Update()
        {
            foreach (IGameSystem gameSystem in GameSystems)
            {
                gameSystem.OnUpdate();
            }
        }
        
        private void LateUpdate()
        {
            foreach (IGameSystem gameSystem in GameSystems)
            {
                gameSystem.OnLateUpdate();
            }
        }

        public List<IGameSystem> FindGameSystemInterface()
        {
            MonoBehaviour[] monoBehaviours = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            List<IGameSystem> gameSystems = monoBehaviours
            .SelectMany(behaviour => behaviour.GetComponents<IGameSystem>())
            .Distinct()
            .OrderBy(system => system.PriorityOrder)
            .ToList();
            return gameSystems;
        }
    }
}
