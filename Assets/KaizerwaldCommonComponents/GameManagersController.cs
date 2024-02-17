using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.Utilities;

namespace Kaizerwald
{
    public interface IManagerInitialization
    {
        public bool Initialized { get; }
        public event Action OnManagerInitialized;
    }
    
    public class GameManagersController : Singleton<GameManagersController>
    {
        [SerializeField] private List<IManagerInitialization> Managers;

        protected override void OnAwake()
        {
            base.OnAwake();
            Managers = FindObjectsManagers();
        }
        
        public List<IManagerInitialization> FindObjectsManagers()
        {
            MonoBehaviour[] monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            List<IManagerInitialization> list = new List<IManagerInitialization>();
            foreach(MonoBehaviour behaviour in monoBehaviours)
            {
                IManagerInitialization[] components = behaviour.GetComponents<IManagerInitialization>();
                if (components == null || components.Length == 0) continue;
                list.AddRange(components);
            }
            return list;
        }
    }
}
