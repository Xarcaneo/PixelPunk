using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MenuFlow.Interfaces;
using MenuFlow.ScriptableObjects;
using MenuFlow.Components;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace MenuFlow.Core
{
    /// <summary>
    /// Core manager class for the MenuFlow system.
    /// Handles menu navigation, scene transitions, and panel instantiation.
    /// Implements a singleton pattern for global access.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [Header("Configuration")]
        /// <summary>
        /// Reference to the MenuSystem configuration asset.
        /// </summary>
        [SerializeField] private MenuSystem menuSystem;

        /// <summary>
        /// Reference to the loading screen manager.
        /// </summary>
        [SerializeField] private LoadingScreenManager loadingScreenManager;

        // Navigation state
        private readonly Stack<MenuDefinition> menuStack = new();
        private MenuDefinition activeMenu;
        private bool isTransitioning;
        private string currentScene;

        // Events
        /// <summary>
        /// Invoked after a menu transition is complete.
        /// </summary>
        public UnityEvent onTransitionComplete = new UnityEvent();

        /// <summary>
        /// Gets the singleton instance of the MenuManager.
        /// </summary>
        public static MenuManager Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance and sets up initial configuration.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Load initial panel's scene if needed
                if (menuSystem.initialPanel != null)
                {
                    string targetScene = menuSystem.initialPanel.SceneName;
                    if (!string.IsNullOrEmpty(targetScene) && targetScene != SceneManager.GetActiveScene().name)
                    {
                        SceneManager.LoadScene(targetScene);
                    }
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Subscribes to scene loading events.
        /// </summary>
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Unsubscribes from scene loading events.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Handles scene loading completion.
        /// Opens the initial panel if the loaded scene matches its scene name.
        /// </summary>
        private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentScene = scene.name;

            // If we have an active menu for this scene, show it immediately
            if (activeMenu != null && activeMenu.SceneName == currentScene)
            {
                var panel = GetOrCreateMenuPanel(activeMenu);
                if (panel != null)
                {
                    isTransitioning = true;
                    try
                    {
                        await panel.Show();
                    }
                    finally
                    {
                        isTransitioning = false;
                        onTransitionComplete.Invoke();
                    }
                }
            }
            // Otherwise check if it's the initial panel's scene
            else if (menuSystem.initialPanel != null && menuSystem.initialPanel.SceneName == currentScene)
            {
                await TransitionTo(menuSystem.initialPanel.name);
            }
        }

        /// <summary>
        /// Opens the initial menu panel defined in the MenuSystem.
        /// Only opens if no menu is currently active.
        /// </summary>
        private void OpenInitialPanel()
        {
            if (menuSystem.initialPanel != null && activeMenu == null)
            {
                _ = TransitionTo(menuSystem.initialPanel.name);
            }
        }

        /// <summary>
        /// Opens a menu panel and handles scene transitions if needed.
        /// </summary>
        /// <param name="menuDefinition">The menu to open.</param>
        /// <param name="addToStack">Whether to add the current menu to the navigation stack.</param>
        /// <returns>A task that completes when the menu is fully opened.</returns>
        public async Task OpenMenu(MenuDefinition menuDefinition, bool addToStack = true)
        {
            if (menuDefinition == null || isTransitioning) return;
            isTransitioning = true;

            try
            {
                // Check if we need to change scenes
                bool needsSceneChange = !string.IsNullOrEmpty(menuDefinition.SceneName) && menuDefinition.SceneName != currentScene;

                // Only exit current menu if we're staying in the same scene
                if (activeMenu != null)
                {
                    var currentPanel = GetMenuPanel(activeMenu);
                    if (currentPanel != null)
                    {
                        await currentPanel.Exit();
                        currentPanel.SetVisible(needsSceneChange);
                    }
                }

                // Update navigation state before scene change
                if (addToStack && activeMenu != null)
                {
                    menuStack.Push(activeMenu);
                }
                activeMenu = menuDefinition;

                // Handle scene transition if needed
                if (needsSceneChange)
                {
                    // Use loading screen manager to handle scene transition
                    await loadingScreenManager.ShowLoadingScreenAsync(menuDefinition.SceneName);
                    currentScene = menuDefinition.SceneName;
                }
                else
                {
                    // If no scene change, show the menu immediately
                    var panel = GetOrCreateMenuPanel(menuDefinition);
                    if (panel != null)
                    {
                        await panel.Show();
                    }
                }
            }
            finally
            {
                isTransitioning = false;
                onTransitionComplete.Invoke();
            }
        }

        /// <summary>
        /// Returns to the previous menu in the navigation stack.
        /// </summary>
        /// <returns>A task that completes when the previous menu is shown.</returns>
        public async Task GoBack()
        {
            if (menuStack.Count == 0 || isTransitioning) return;
            var previousMenu = menuStack.Pop();
            await OpenMenu(previousMenu, false);
        }

        /// <summary>
        /// Transitions to a specified menu by its name.
        /// Uses the auto-generated MenuFlowConstants for type-safe menu references.
        /// </summary>
        /// <param name="menuName">Name of the menu to transition to. Use MenuFlowConstants.Menus.</param>
        /// <returns>A task that completes when the transition is finished.</returns>
        public async Task TransitionTo(string menuName)
        {
            if (isTransitioning)
            {
                Debug.LogWarning($"MenuFlow: Already transitioning to a menu. Ignoring transition to {menuName}");
                return;
            }

            // Find the menu definition
            MenuDefinition targetMenu = null;

            // First check current scene
            var currentSceneEntry = menuSystem.scenes
                .FirstOrDefault(entry => entry.sceneName == currentScene);

            if (currentSceneEntry != null)
            {
                targetMenu = currentSceneEntry.panels
                    .FirstOrDefault(panel => panel != null && panel.name == menuName);
            }

            // If not found in current scene, check all scenes
            if (targetMenu == null)
            {
                foreach (var entry in menuSystem.scenes)
                {
                    targetMenu = entry.panels
                        .FirstOrDefault(panel => panel != null && panel.name == menuName);
                    if (targetMenu != null) break;
                }
            }

            if (targetMenu == null)
            {
                Debug.LogError($"MenuFlow: Menu '{menuName}' not found in any scene");
                return;
            }

            // Open the menu
            await OpenMenu(targetMenu, true);
        }

        /// <summary>
        /// Gets the MenuPanel component for a given menu definition.
        /// </summary>
        /// <param name="menuDefinition">The menu definition to get the panel for.</param>
        /// <returns>The MenuPanel component if found, null otherwise.</returns>
        private MenuPanel GetMenuPanel(MenuDefinition menuDefinition)
        {
            return menuDefinition.CurrentInstance?.GetComponent<MenuPanel>();
        }

        /// <summary>
        /// Gets an existing menu panel or creates a new one if it doesn't exist.
        /// </summary>
        /// <param name="menuDefinition">The menu definition to get or create a panel for.</param>
        /// <returns>The MenuPanel component.</returns>
        private MenuPanel GetOrCreateMenuPanel(MenuDefinition menuDefinition)
        {
            var existingPanel = GetMenuPanel(menuDefinition);
            if (existingPanel != null)
            {
                return existingPanel;
            }

            GameObject menuObject;
            if (menuDefinition.SceneInstance != null)
            {
                menuObject = menuDefinition.SceneInstance;
            }
            else
            {
                menuObject = Instantiate(menuDefinition.MenuPrefab, transform);
            }

            var panel = menuObject.GetComponent<MenuPanel>();
            panel.SetMenuDefinition(menuDefinition);
            return panel;
        }
    }
}