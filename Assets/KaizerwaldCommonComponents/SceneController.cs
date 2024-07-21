using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Kaizerwald.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

using Kaizerwald.Pattern;

namespace Kaizerwald
{
    public enum EScene : int
    {
        //BootstrapScene,
        //StartupScene,
        MainMenuScene,
        //LobbyRoomScene,
        GameScene
    }
    
   public sealed class SceneController : Singleton<SceneController>
    {
        //private EScene sceneActive = EScene.BootstrapScene;
        private EScene sceneActive = EScene.MainMenuScene;
        
        public EScene GetSceneActive()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName == sceneActive.ToString())
            {
                return sceneActive;
            }
            if (Enum.TryParse(currentSceneName, out EScene scene))
            {
                sceneActive = scene;
                return sceneActive;
            }
#if UNITY_EDITOR
                Debug.Log($"ERROR SceneController.GetSceneActive() : unable to parse sceneName : {currentSceneName} as EScene");
#endif
            return sceneActive;
        } 

        private bool IsValidScene(EScene scene)
        {
            return (int)scene < SceneManager.sceneCountInBuildSettings;
        }
        
        public void LoadScene(EScene targetScene, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (!IsValidScene(targetScene))
            {
                Debug.LogError($"Invalid Scene requested");
                return;
            }
            SceneManager.LoadScene(targetScene.ToString(), mode);
            sceneActive = targetScene;
        }
    } 
}




