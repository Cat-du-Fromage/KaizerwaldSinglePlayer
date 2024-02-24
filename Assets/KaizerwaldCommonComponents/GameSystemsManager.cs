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
        
        //public void OnAwakeSystem();
        public void OnStartSystem();
        //
        //public void OnFixedUpdateSystem();
        //public void OnUpdateSystem();
        //public void OnLateUpdateSystem();

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
            //GameSystems = SearchAndSortGameSystems();
        }

        private void Start()
        {
            foreach (IGameSystem gameSystem in GameSystems)
            {
                gameSystem.OnStartSystem();
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
