using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaizerwald
{
    public class SelectionMenuUI : MonoBehaviour
    {
        public void OnPlayGame()
        {
            Debug.Log($"OnPlayGame");
            SceneController.Instance.LoadScene(EScene.GameScene);
        }
    }
}
