using System.Collections;
using System.Collections.Generic;
using Kaizerwald.Utilities;
using UnityEngine;

using Kaizerwald.Pattern;

namespace Kaizerwald
{
    public class ApplicationController : Singleton<ApplicationController>
    {
        public SceneController SceneController { get; private set; }

        protected override void OnAwake()
        {
            base.OnAwake();
            SceneController = GetComponent<SceneController>();
        }

        private void Start()
        {
            SceneController.Instance.LoadScene(EScene.MainMenuScene);
        }
    }
}
