using UnityEngine;
using System.Threading.Tasks;
using MenuFlow.Interfaces;
using UnityEngine.SceneManagement;

namespace MenuFlow.Components
{
    /// <summary>
    /// Abstract base class for loading screen components that provides common functionality
    /// and enforces the contract for loading screen implementations.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseLoadingScreenComponent : MonoBehaviour, ILoadingScreenComponent
    {
        /// <summary>
        /// Reference to the CanvasGroup component used for controlling visibility and interactivity.
        /// </summary>
        protected CanvasGroup canvasGroup;

        /// <summary>
        /// The currently active scene when the loading screen is shown.
        /// </summary>
        protected Scene currentScene;

        /// <summary>
        /// The name of the target scene to be loaded.
        /// </summary>
        protected string targetSceneName;

        /// <summary>
        /// The loading mode for the target scene.
        /// </summary>
        protected LoadSceneMode loadMode;

        /// <summary>
        /// The async operation for loading the new scene.
        /// </summary>
        protected AsyncOperation sceneLoadOperation;

        /// <summary>
        /// Initializes the component by getting the CanvasGroup reference and setting initial state.
        /// </summary>
        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public abstract Task ShowAsync();
        public abstract Task HideAsync();

        /// <summary>
        /// Called before scene loading begins. Override to customize scene transition behavior.
        /// </summary>
        public virtual Task<bool> OnBeforeSceneLoadAsync(Scene currentScene, string targetScene, LoadSceneMode loadMode)
        {
            this.currentScene = currentScene;
            this.targetSceneName = targetScene;
            this.loadMode = loadMode;

            // By default, let the manager handle scene activation
            return Task.FromResult(false);
        }

        /// <summary>
        /// Called after scene is loaded but before activation. Override to customize scene transition behavior.
        /// </summary>
        public virtual Task<bool> OnAfterSceneLoadAsync(Scene loadedScene)
        {
            // By default, let the manager handle scene activation
            return Task.FromResult(false);
        }

        /// <summary>
        /// Helper method to set the canvas group state in a single call.
        /// </summary>
        protected void SetCanvasGroupState(float alpha, bool interactable)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.blocksRaycasts = interactable;
            canvasGroup.interactable = interactable;
        }

        /// <summary>
        /// Helper method to load a scene with control over activation.
        /// </summary>
        protected Task<AsyncOperation> LoadSceneAsync(bool allowActivation = false)
        {
            sceneLoadOperation = SceneManager.LoadSceneAsync(targetSceneName, loadMode);
            sceneLoadOperation.allowSceneActivation = allowActivation;
            return Task.FromResult(sceneLoadOperation);
        }

        /// <summary>
        /// Helper method to activate a loaded scene.
        /// </summary>
        protected void ActivateLoadedScene()
        {
            if (sceneLoadOperation != null)
            {
                sceneLoadOperation.allowSceneActivation = true;
            }
        }
    }
}
