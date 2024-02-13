using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kaizerwald
{
    /// <summary>
    /// HIGHLIGHT SYSTEM
    /// </summary>
    public abstract class HighlightSystem
    {
        protected HighlightRegimentManager Manager { get; private set; }
        
        protected HighlightRegister[] Registers = new HighlightRegister[2];
        public HighlightController Controller { get; protected set; }

        protected HighlightSystem(HighlightRegimentManager manager)
        {
            Manager = manager;
        }

        protected abstract void InitializeController();
        protected abstract void InitializeRegisters();
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Add REGIMENT ◈◈◈◈◈◈                                                                                 ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public virtual void AddRegiment(HighlightRegiment regiment, List<GameObject> units)
        {
            Array.ForEach(Registers, register => register.RegisterRegiment(regiment, units));
        }
        
        public virtual void AddRegiment<T>(HighlightRegiment regiment, List<T> units) 
        where T : MonoBehaviour
        {
            Array.ForEach(Registers, register => register.RegisterRegiment(regiment, units));
        }
        
        //╓────────────────────────────────────────────────────────────────────────────────────────────────────────────╖
        //║ ◈◈◈◈◈◈ Remove REGIMENT ◈◈◈◈◈◈                                                                              ║
        //╙────────────────────────────────────────────────────────────────────────────────────────────────────────────╜
        public virtual void RemoveRegiment(HighlightRegiment regiment)
        {
            Array.ForEach(Registers, register => register.UnregisterRegiment(regiment));
        }
        
        public virtual void OnShow(HighlightRegiment regiment, int registerIndex)
        {
            if (regiment == null) return;
            if (!Registers[registerIndex].Records.TryGetValue(regiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                highlight.Show();
            }
            Registers[registerIndex].ActiveHighlights.Add(regiment);
        }
        
        public virtual void OnHide(HighlightRegiment regiment, int registerIndex)
        {
            if (regiment == null) return;
            if (!Registers[registerIndex].Records.TryGetValue(regiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            foreach (HighlightBehaviour highlight in highlights)
            {
                if (highlight == null) continue;
                highlight.Hide();
            }
            Registers[registerIndex].ActiveHighlights.Remove(regiment);
        }

        public virtual void HideAll(int registerIndex)
        {
            for (int i = Registers[registerIndex].ActiveHighlights.Count - 1; i > -1; i--)
            {
                OnHide(Registers[registerIndex].ActiveHighlights[i], registerIndex);
            }
        }
        
        //TODO CORRIGER PROBLEME OU 1 Highlight survie a la mort de la dernière troupe
        protected virtual void CleanUnusedHighlights(int registerIndex, int regimentIndex, int numToKeep)
        {
            bool exist = Registers[registerIndex].Records.TryGetValue(regimentIndex, out HighlightBehaviour[] highlights);
            if (!exist) return;
            //if (!Registers[registerIndex].Records.ContainsKey(regimentIndex)) return;
            //int registerLength = Registers[registerIndex][regimentIndex].Length;
            Debug.Log($"CleanUnusedHighlights: highlights.Length == numToKeep = {highlights.Length == numToKeep}");
            if (highlights.Length == numToKeep) return;
            for (int i = numToKeep; i < highlights.Length; i++)
            {
                Debug.Log($"CleanUnusedHighlights, destroy{i} from {highlights[i].UnitAttach.name}");
                Object.Destroy(highlights[i].gameObject);
            }
            if (numToKeep > 0) return;
            Registers[registerIndex].Records.Remove(regimentIndex);
        }
/*
        protected void DestroyAllHighlights(int regimentIndex)
        {
            foreach (HighlightRegister register in Registers)
            {
                if (!register.Records.ContainsKey(regimentIndex)) continue;
                foreach (HighlightBehaviour highlight in register.Records[regimentIndex])
                {
                    Destroy(highlight.gameObject);
                }
            }
        }
        */
    }
}