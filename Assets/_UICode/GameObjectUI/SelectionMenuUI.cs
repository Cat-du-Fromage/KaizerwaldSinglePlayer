using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public class SelectionMenuUI : MonoBehaviour
    {
        [SerializeField] private List<ArmyPanel> ArmyPanels;
        
        public void OnPlayGame()
        {
            Debug.Log($"OnPlayGame");
            GatherPlayersArmy();
            
            SceneController.Instance.LoadScene(EScene.GameScene);
        }

        public void GatherPlayersArmy()
        {
            foreach (ArmyPanel army in ArmyPanels)
            {
                ulong playerId = army.OwnerPlayerID;
                int teamId = army.TeamID;
                List<ArmyItem> items = army.GetArmyItems();
                foreach (ArmyItem item in items)
                {
                    SpawnCommandManager.Instance.AddRegimentSpawner(army.OwnerPlayerID, army.TeamID, item.CardItem.RegimentType);
                }
                SpawnCommandManager.Instance.Initialize();
            }
        }
    }
}
