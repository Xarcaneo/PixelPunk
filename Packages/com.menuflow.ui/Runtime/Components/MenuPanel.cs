using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
using MenuFlow.Interfaces;
using MenuFlow.ScriptableObjects;

namespace MenuFlow.Components
{
    /// <summary>
    /// Base component for menu panels in the MenuFlow system.
    /// Handles visibility, transitions, and menu definition linkage.
    /// Implements IMenuPanel for standardized menu operations.
    /// </summary>
    public class MenuPanel : MonoBehaviour, IMenuPanel
    {
        /// <summary>
        /// The menu definition that describes this panel's behavior and relationships.
        /// </summary>
        [SerializeField] private MenuDefinition menuDefinition;
        
        private CanvasGroup canvasGroup;
        private bool isTransitioning;
        
        /// <summary>
        /// Event invoked when the panel is shown.
        /// </summary>
        public UnityEvent onShow;

        /// <summary>
        /// Event invoked when the panel is hidden.
        /// </summary>
        public UnityEvent onHide;

        /// <summary>
        /// Gets the menu definition associated with this panel.
        /// </summary>
        public MenuDefinition MenuDefinition => menuDefinition;

        /// <summary>
        /// Initializes the panel component and sets up required components.
        /// </summary>
        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Initialize CanvasGroup properties
            SetVisible(false);

            // Register this instance with the MenuDefinition if assigned
            if (menuDefinition != null)
            {
                menuDefinition.CurrentInstance = gameObject;
            }   
        }

        /// <summary>
        /// Cleans up references when the panel is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (menuDefinition != null && menuDefinition.CurrentInstance == gameObject)
            {
                menuDefinition.CurrentInstance = null;
            }
        }

        /// <summary>
        /// Shows the panel by making it visible and interactive.
        /// Also shows all parent menus in the hierarchy.
        /// Can be overridden to add custom show animations or behavior.
        /// </summary>
        /// <returns>A task that completes when the show operation is finished.</returns>
        public virtual async Task Show()
        {
            if (isTransitioning)
            {
                Debug.Log($"[MenuPanel] Show skipped - already transitioning: {gameObject.name}");
                return;
            }
            
            isTransitioning = true;

            // First show parent menus if they exist
            if (menuDefinition != null && menuDefinition.ParentMenu != null)
            {
                var parentInstance = menuDefinition.ParentMenu.CurrentInstance;
                if (parentInstance != null)
                {
                    var parentPanel = parentInstance.GetComponent<MenuPanel>();
                    if (parentPanel != null)
                    {
                        await parentPanel.Show();
                    }
                }
            }
            
            onShow?.Invoke();
            SetVisible(true);
            isTransitioning = false;
        }

        /// <summary>
        /// Exits this menu panel and all parent menus in the hierarchy.
        /// Can be overridden to add custom exit animations or behavior.
        /// </summary>
        /// <returns>A task that completes when the exit operation is finished.</returns>
        public virtual async Task Exit()
        {
            if (isTransitioning)
            {
                Debug.Log($"[MenuPanel] Exit skipped - already transitioning: {gameObject.name}");
                return;
            }
            
            isTransitioning = true;

            // First exit parent menus if they exist
            if (menuDefinition != null && menuDefinition.ParentMenu != null)
            {
                var parentInstance = menuDefinition.ParentMenu.CurrentInstance;
                if (parentInstance != null)
                {
                    var parentPanel = parentInstance.GetComponent<MenuPanel>();
                    if (parentPanel != null)
                    {
                        await parentPanel.Exit();
                    }
                }
            }

            onHide?.Invoke();
            isTransitioning = false;
        }

        /// <summary>
        /// Sets the visibility state of the panel using CanvasGroup properties.
        /// </summary>
        /// <param name="visible">True to make the panel visible and interactive, false to hide it.</param>
        public void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        /// <summary>
        /// Updates the menu definition associated with this panel.
        /// </summary>
        /// <param name="definition">The new menu definition to associate with this panel.</param>
        public void SetMenuDefinition(MenuDefinition definition)
        {
            Debug.Log($"[MenuPanel] Updating menu definition for panel: {gameObject.name}, Old: {(menuDefinition != null ? menuDefinition.name : "none")}, New: {(definition != null ? definition.name : "none")}");
            
            if (menuDefinition != null && menuDefinition.CurrentInstance == gameObject)
            {
                menuDefinition.CurrentInstance = null;
            }

            menuDefinition = definition;

            if (menuDefinition != null)
            {
                menuDefinition.CurrentInstance = gameObject;
            }
        }
    }
}
