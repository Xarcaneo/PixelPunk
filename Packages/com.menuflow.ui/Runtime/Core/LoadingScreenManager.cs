using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using MenuFlow.Interfaces;
using MenuFlow.ScriptableObjects;
using UnityEngine.EventSystems;

namespace MenuFlow.Core
{
    /// <summary>
    /// Manages loading screen operations and scene transitions in the MenuFlow system.
    /// This component is responsible for instantiating, showing, and hiding loading screens
    /// during scene transitions. It coordinates with ILoadingScreenComponent implementations
    /// to provide customizable loading screen behaviors.
    /// </summary>
    /// <remarks>
    /// Key responsibilities:
    /// - Managing loading screen lifecycle (creation, showing, hiding)
    /// - Coordinating scene transitions with loading screens
    /// - Handling asynchronous scene loading operations
    /// - Managing loading screen state and visibility
    /// 
    /// The manager requires a MenuSystem reference to function properly, which should be
    /// assigned in the Unity Inspector.
    /// </remarks>
    public class LoadingScreenManager : MonoBehaviour, ILoadingScreenManager
    {
        /// <summary>
        /// Reference to the MenuSystem scriptable object that contains all menu configurations,
        /// including loading screen prefabs and settings.
        /// </summary>
        [Tooltip("Reference to the MenuSystem that contains loading screen configuration")]
        public MenuSystem menuSystem;
        
        // Internal state tracking
        private bool isLoadingScreenActive;
        private GameObject loadingScreenInstance;
        private ILoadingScreenComponent loadingScreenComponent;
        private GameObject eventSystemBackup;
        private AsyncOperation currentSceneLoad;

        /// <summary>
        /// Gets whether a loading screen is currently being displayed.
        /// </summary>
        /// <value>True if a loading screen is active, false otherwise.</value>
        public bool IsLoadingScreenActive => isLoadingScreenActive;

        /// <summary>
        /// Initializes the LoadingScreenManager on start.
        /// Validates required dependencies and sets up the loading screen if configured.
        /// </summary>
        private void Start()
        {
            if (menuSystem == null)
            {
                Debug.LogWarning($"LoadingScreenManager on {gameObject.name} requires a MenuSystem reference to function properly.");
                return;
            }

            InitializeLoadingScreen();
        }

        /// <summary>
        /// Creates and initializes the loading screen instance.
        /// The loading screen remains hidden until needed.
        /// </summary>
        private void InitializeLoadingScreen()
        {
            if (menuSystem.loadingScreenPrefab == null)
            {
                return;
            }

            loadingScreenInstance = Instantiate(menuSystem.loadingScreenPrefab, transform);
            loadingScreenComponent = loadingScreenInstance.GetComponent<ILoadingScreenComponent>();

            if (loadingScreenComponent == null)
            {
                Debug.LogError($"[LoadingManager] Loading screen prefab must have a component that implements {nameof(ILoadingScreenComponent)}");
                Destroy(loadingScreenInstance);
                loadingScreenInstance = null;
                return;
            }

            loadingScreenInstance.SetActive(false);
        }

        /// <summary>
        /// Validates a loading screen prefab to ensure it has the required component.
        /// </summary>
        /// <param name="prefab">The prefab to validate</param>
        /// <returns>True if the prefab is valid, false otherwise</returns>
        private bool ValidateLoadingScreen(GameObject prefab)
        {
            if (prefab == null) return false;
            return prefab.GetComponent<ILoadingScreenComponent>() != null;
        }

        /// <summary>
        /// Shows the loading screen if one is configured.
        /// </summary>
        /// <param name="targetSceneName">Name of the scene to load</param>
        /// <param name="loadMode">How to load the scene (Single or Additive)</param>
        /// <returns>A task that completes when the loading screen is fully visible</returns>
        public async Task ShowLoadingScreenAsync(string targetSceneName, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            if (menuSystem == null || loadingScreenComponent == null)
            {
                // Direct scene load without loading screen
                currentSceneLoad = SceneManager.LoadSceneAsync(targetSceneName, loadMode);
                currentSceneLoad.allowSceneActivation = true;

                // Wait for scene to be fully loaded and activated
                while (!currentSceneLoad.isDone)
                {
                    await Task.Yield();
                }
                currentSceneLoad = null;
                return;
            }

            try
            {
                isLoadingScreenActive = true;
                BackupEventSystem();

                // Show loading screen
                loadingScreenInstance.SetActive(true);
                await loadingScreenComponent.ShowAsync();

                // Let the loading screen handle pre-load customization
                bool customHandling = await loadingScreenComponent.OnBeforeSceneLoadAsync(
                    SceneManager.GetActiveScene(),
                    targetSceneName,
                    loadMode
                );

                // Load the scene but don't activate it yet
                currentSceneLoad = SceneManager.LoadSceneAsync(targetSceneName, loadMode);
                currentSceneLoad.allowSceneActivation = !customHandling;

                // Wait for scene to be loaded (but not necessarily activated)
                while (currentSceneLoad.progress < 0.9f)
                {
                    await Task.Yield();
                }

                if (customHandling)
                {
                    // Wait for the loading screen to signal it's ready to activate the scene
                    bool readyToActivate = await loadingScreenComponent.OnAfterSceneLoadAsync(SceneManager.GetSceneByName(targetSceneName));
                    
                    if (readyToActivate)
                    {
                        currentSceneLoad.allowSceneActivation = true;
                        
                        while (!currentSceneLoad.isDone)
                        {
                            await Task.Yield();
                        }
                    }
                }
                else
                {
                    // If not using custom handling, wait for scene to be fully loaded
                    while (!currentSceneLoad.isDone)
                    {
                        await Task.Yield();
                    }
                }

                // Wait a frame to ensure scene is fully set up
                await Task.Yield();

                // Hide the loading screen
                await HideLoadingScreenAsync();
            }
            finally
            {
                isLoadingScreenActive = false;
                currentSceneLoad = null;
                RestoreEventSystem();
            }
        }

        /// <summary>
        /// Hides the active loading screen if one is displayed.
        /// </summary>
        /// <returns>A task that completes when the loading screen is fully hidden</returns>
        public async Task HideLoadingScreenAsync()
        {
            if (loadingScreenComponent != null)
            {
                await loadingScreenComponent.HideAsync();
                loadingScreenInstance.SetActive(false);
            }
        }

        /// <summary>
        /// Backs up the EventSystem to prevent it from being destroyed during scene transitions.
        /// </summary>
        private void BackupEventSystem()
        {
            var currentEventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (currentEventSystem != null && currentEventSystem.gameObject.scene.name != "DontDestroyOnLoad")
            {
                eventSystemBackup = currentEventSystem.gameObject;
                DontDestroyOnLoad(eventSystemBackup);
            }
        }

        /// <summary>
        /// Restores the EventSystem after a scene transition.
        /// </summary>
        private void RestoreEventSystem()
        {
            if (eventSystemBackup != null)
            {
                var currentEventSystem = Object.FindFirstObjectByType<EventSystem>();
                if (currentEventSystem != null && currentEventSystem.gameObject != eventSystemBackup)
                {
                    Destroy(currentEventSystem.gameObject);
                }
                eventSystemBackup.transform.SetParent(null);
                SceneManager.MoveGameObjectToScene(eventSystemBackup, SceneManager.GetActiveScene());
                eventSystemBackup = null;
            }
        }

        /// <summary>
        /// Cleans up resources when the manager is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (loadingScreenInstance != null)
            {
                Destroy(loadingScreenInstance);
            }
            if (eventSystemBackup != null)
            {
                RestoreEventSystem();
            }
        }
    }
}
