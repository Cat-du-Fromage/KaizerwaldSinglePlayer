using System;
using System.Collections.Generic;
using Unity.Mathematics;
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

        //VOIR pour une methods en interne ! qui permet de lancer des fonction APRES le resize!
        protected abstract void ResizeAndReformRegister(int registerIndex, HighlightRegiment regiment, int numHighlightToKeep);
        
        //TODO CORRIGER PROBLEME OU 1 Highlight survie a la mort de la dernière troupe
        protected virtual void CleanUnusedHighlights(int registerIndex, HighlightRegiment regiment, int numToKeep)
        {
            HighlightRegister register = Registers[registerIndex];
            if (!register.Records.TryGetValue(regiment.RegimentID, out HighlightBehaviour[] highlights)) return;
            if (highlights.Length == numToKeep) return;
            for (int i = numToKeep; i < highlights.Length; i++)
            {
                Object.Destroy(highlights[i].gameObject);
            }
            
            //BUG PLACEMENT! QUAND width change de manière à (numUnitsAlive < minWidth) alors "PlacementController.DynamicsTempWidth" n'est plus correct!
            
            if (numToKeep > 0) return;
            register.Records.Remove(regiment.RegimentID);
            register.ActiveHighlights.Remove(regiment);
        }
        
        public void ResizeRegister(HighlightRegiment regiment)
        {
            int numUnitsAlive = regiment.Count;
            for (int i = 0; i < Registers.Length; i++)
            {
                CleanUnusedHighlights(i, regiment, numUnitsAlive);
                ResizeAndReformRegister(i, regiment, numUnitsAlive);
            }
        }
    }
}