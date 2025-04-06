using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MenuFlow.Interfaces;

namespace MenuFlow.Core
{
    /// <summary>
    /// Default implementation of scene loading operation.
    /// Handles the core scene loading logic while allowing for extension through the interface.
    /// </summary>
    public class DefaultSceneLoadingOperation : ISceneLoadingOperation
    {
        private readonly string sceneName;
        private readonly LoadSceneMode loadMode;
        private AsyncOperation loadOperation;
        private bool isDone;

        public string SceneName => sceneName;
        public bool IsDone => isDone;
        public float Progress => loadOperation?.progress ?? 0f;

        public event Action<ISceneLoadingOperation> OnLoadingComplete;

        public DefaultSceneLoadingOperation(string sceneName, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            this.sceneName = sceneName;
            this.loadMode = loadMode;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                loadOperation = SceneManager.LoadSceneAsync(sceneName, loadMode);
                loadOperation.allowSceneActivation = true;

                while (!loadOperation.isDone)
                {
                    await Task.Yield();
                }

                isDone = true;
                OnLoadingComplete?.Invoke(this);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error loading scene {sceneName}: {ex.Message}");
                throw;
            }
        }
    }
}
