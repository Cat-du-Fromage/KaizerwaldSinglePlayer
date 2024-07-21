using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static System.Array;
using Object = UnityEngine.Object;

namespace Kaizerwald
{
    public class HighlightRegister
    {
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                              ◆◆◆◆◆◆ PROPERTIES ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        protected HighlightSystem System { get; private set; }
        protected GameObject Prefab { get; private set; }
        public Dictionary<int, HighlightBehaviour[]> Records { get; protected set; }
        public List<HighlightRegiment> ActiveHighlights { get; protected set; }
        
        public HighlightBehaviour[] this[int index]
        {
            get => !Records.TryGetValue(index, out _) ? Empty<HighlightBehaviour>() : Records[index];
            set => Records[index] = value;
        }

//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ CONSTRUCTOR ◆◆◆◆◆◆                                              ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public HighlightRegister(HighlightSystem system, GameObject highlightPrefab)
        {
            if (!highlightPrefab.TryGetComponent<HighlightBehaviour>(out _))
            {
#if UNITY_EDITOR
                Debug.LogError("Prefab Don't have component: HighlightBehaviour");
#endif
            }
            System = system;
            Prefab = highlightPrefab;
            Records = new Dictionary<int, HighlightBehaviour[]>();
            ActiveHighlights = new List<HighlightRegiment>();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
        
        public int CountAt(int regimentIndex)
        {
            return this[regimentIndex].Length;
        }

        public bool ContainsKey(int regimentIndex)
        {
            return Records.ContainsKey(regimentIndex);
        }

        public bool TryGetValue(int regimentIndex, out HighlightBehaviour[] values)
        {
            return Records.TryGetValue(regimentIndex, out values);
        }

    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Populate Records ◈◈◈◈◈◈                                                                                 ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        private void PopulateRecords(HighlightRegiment selectableRegiment, List<GameObject> units, GameObject prefab)
        {
            Records[selectableRegiment.RegimentID] ??= new HighlightBehaviour[units.Count];
            for (int i = 0; i < Records[selectableRegiment.RegimentID].Length; i++)
            {
                GameObject highlightObj = Object.Instantiate(prefab);
                Records[selectableRegiment.RegimentID][i] = highlightObj.GetComponent<HighlightBehaviour>();
                Records[selectableRegiment.RegimentID][i].InitializeHighlight(units[i]);
            }
        }
        
        private void PopulateRecords<T>(HighlightRegiment selectableRegiment, List<T> units, GameObject prefab)
        where T : MonoBehaviour
        {
            Records[selectableRegiment.RegimentID] ??= new HighlightBehaviour[units.Count];
            for (int i = 0; i < Records[selectableRegiment.RegimentID].Length; i++)
            {
                GameObject highlightObj = Object.Instantiate(prefab);
                Records[selectableRegiment.RegimentID][i] = highlightObj.GetComponent<HighlightBehaviour>();
                Records[selectableRegiment.RegimentID][i].InitializeHighlight(units[i].gameObject);
            }
        }
        
    //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
    //║ ◈◈◈◈◈◈ Register Methods ◈◈◈◈◈◈                                                                                 ║
    //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public void RegisterRegiment(HighlightRegiment selectableRegiment, List<GameObject> units, GameObject prefabOverride = null)
        {
            GameObject highlightPrefab = prefabOverride == null ? Prefab : prefabOverride;
            Records.TryAdd(selectableRegiment.RegimentID, new HighlightBehaviour[units.Count]);
            PopulateRecords(selectableRegiment, units, highlightPrefab);
        }
        
        public void RegisterRegiment<T>(HighlightRegiment selectableRegiment, List<T> units, GameObject prefabOverride = null)
        where T : MonoBehaviour
        {
            GameObject highlightPrefab = prefabOverride == null ? Prefab : prefabOverride;
            Records.TryAdd(selectableRegiment.RegimentID, new HighlightBehaviour[units.Count]);
            PopulateRecords(selectableRegiment, units, highlightPrefab);
        }
        
        public void UnregisterRegiment(HighlightRegiment selectableRegiment)
        {
            if (!Records.TryGetValue(selectableRegiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                Object.Destroy(highlight.gameObject);
            }
            Records.Remove(selectableRegiment.RegimentID);
            ActiveHighlights.Remove(selectableRegiment);
        }
    }
}
