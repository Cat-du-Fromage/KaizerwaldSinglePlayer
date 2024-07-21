using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kaizerwald.Utilities.Core;

namespace Kaizerwald
{
    [CreateAssetMenu(fileName = "NewRegimentPrefabsList", menuName = "Regiment/PrefabsList")]
    public class RegimentPrefabsList : ScriptableObject
    {
        private Dictionary<RegimentType, int> RegimentTypeKeyPairIndex;
        private Dictionary<Regiment, int> RegimentKeyPairIndex;
        
        public List<GameObject> RegimentPrefabs;
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                             ◆◆◆◆◆◆ UNITY EVENTS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

#if UNITY_EDITOR
        private void OnValidate()
        {
            RegimentKeyPairIndex = new Dictionary<Regiment, int>();
            RegimentTypeKeyPairIndex = new Dictionary<RegimentType, int>();
            if (RegimentPrefabs == null) return;
            Initialize();
        }
#endif
        
        private void OnEnable()
        {
            if (RegimentPrefabs == null) return;
            Initialize();
        }
        
//╔════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
//║                                            ◆◆◆◆◆◆ CLASS METHODS ◆◆◆◆◆◆                                             ║
//╚════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝

        public GameObject GetPrefabFromRegiment(Regiment regimentType)
        {
            return RegimentPrefabs[RegimentKeyPairIndex[regimentType]];
        }

        public GameObject GetPrefabFromRegimentType(RegimentType regimentType)
        {
            return RegimentPrefabs[RegimentTypeKeyPairIndex[regimentType]];
        }
        
        private void RewindInternalDictionaries()
        {
            RegimentKeyPairIndex.ClearSafe(RegimentPrefabs.Count);
            RegimentTypeKeyPairIndex.ClearSafe(RegimentPrefabs.Count);
        }

        private void Initialize()
        {
            RewindInternalDictionaries();
            Stack<GameObject> nonRegiments = new (RegimentPrefabs.Count);
            int indexOffset = 0;
            for (int i = 0; i < RegimentPrefabs.Count; i++)
            {
                if (RegimentPrefabs[i] == null)
                {
                    //avoid error when adding new empty element to the list on editor
#if !UNITY_EDITOR
                    nonRegiments.Push(RegimentPrefabs[i]);
                    indexOffset++;
#endif
                    continue;
                }
                
                if (!RegimentPrefabs[i].TryGetComponent(out Regiment regiment))
                {
                    nonRegiments.Push(RegimentPrefabs[i]);
                    indexOffset++;
                    continue;
                }

                if (regiment == null) continue;
                RegimentKeyPairIndex[regiment] = i - indexOffset;
                RegimentTypeKeyPairIndex[regiment.RegimentType] = i - indexOffset;
            }

            //not as elegant as a while it's safer
            for (int i = 0; i < nonRegiments.Count; i++)
            {
                GameObject nonRegiment = nonRegiments.Pop();
                RegimentPrefabs.Remove(nonRegiment);
            }
        }
    }
}
