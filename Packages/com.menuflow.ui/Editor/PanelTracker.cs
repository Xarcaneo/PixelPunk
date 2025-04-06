using UnityEngine;
using UnityEditor;
using MenuFlow.ScriptableObjects;
using MenuFlow.Components;
using System.Linq;
using System;

namespace MenuFlow.Editor
{
    /// <summary>
    /// Tracks and monitors the visibility state of menu panels in play mode.
    /// This class handles the detection and notification of panel state changes.
    /// </summary>
    public class PanelTracker
    {
        /// <summary>
        /// The interval in seconds between panel visibility checks.
        /// </summary>
        private const float CHECK_INTERVAL = 0.1f;

        /// <summary>
        /// The currently tracked menu system.
        /// </summary>
        private MenuSystem menuSystem;

        /// <summary>
        /// The currently opened panel's menu definition.
        /// </summary>
        private MenuDefinition currentlyOpenedPanel;

        /// <summary>
        /// Time of the last panel visibility check.
        /// </summary>
        private float lastCheckTime;

        /// <summary>
        /// Event triggered when the active panel changes.
        /// </summary>
        public event System.Action OnPanelChanged;

        /// <summary>
        /// Gets the currently active panel's menu definition.
        /// </summary>
        public MenuDefinition CurrentPanel => currentlyOpenedPanel;

        /// <summary>
        /// Initializes a new instance of the PanelTracker class.
        /// </summary>
        /// <param name="menuSystem">The MenuSystem to track panels for.</param>
        public PanelTracker(MenuSystem menuSystem)
        {
            this.menuSystem = menuSystem;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.update += OnEditorUpdate;
            lastCheckTime = 0; // Force immediate check
        }

        /// <summary>
        /// Cleans up subscribed events when the tracker is no longer needed.
        /// </summary>
        public void Dispose()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.update -= OnEditorUpdate;
        }

        /// <summary>
        /// Updates the MenuSystem being tracked.
        /// </summary>
        /// <param name="newMenuSystem">The new MenuSystem to track.</param>
        public void SetMenuSystem(MenuSystem newMenuSystem)
        {
            menuSystem = newMenuSystem;
            CheckForPanelChanges();
        }

        /// <summary>
        /// Handles the editor update event to check for panel changes at regular intervals.
        /// </summary>
        private void OnEditorUpdate()
        {
            if (Time.realtimeSinceStartup - lastCheckTime < CHECK_INTERVAL)
                return;

            lastCheckTime = Time.realtimeSinceStartup;
            CheckForPanelChanges();
        }

        /// <summary>
        /// Determines if a menu panel is currently visible based on its hierarchy and canvas group state.
        /// </summary>
        /// <param name="panel">The panel to check visibility for.</param>
        /// <returns>True if the panel is visible, false otherwise.</returns>
        private bool IsPanelVisible(MenuPanel panel)
        {
            if (!panel.gameObject.activeInHierarchy || !panel.enabled)
                return false;

            var canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                return panel.gameObject.activeSelf;

            // Check if this panel and all its parents are visible
            var currentTransform = panel.transform;
            while (currentTransform != null)
            {
                var currentCanvasGroup = currentTransform.GetComponent<CanvasGroup>();
                if (currentCanvasGroup != null)
                {
                    if (currentCanvasGroup.alpha <= 0 || !currentCanvasGroup.gameObject.activeInHierarchy)
                        return false;
                }
                currentTransform = currentTransform.parent;
            }

            return canvasGroup.alpha > 0;
        }

        /// <summary>
        /// Checks for changes in panel visibility and updates the current panel state.
        /// </summary>
        private void CheckForPanelChanges()
        {
            if (menuSystem == null)
                return;

            // Only track active panels in play mode
            if (!Application.isPlaying)
            {
                if (currentlyOpenedPanel != null)
                {
                    currentlyOpenedPanel = null;
                    OnPanelChanged?.Invoke();
                }
                return;
            }

            var panels = UnityEngine.Object.FindObjectsByType<MenuPanel>(FindObjectsSortMode.None);
            if (panels == null)
                return;

            var visiblePanels = panels.Where(p => p != null && IsPanelVisible(p)).ToList();
            
            // Get the most recently shown panel by checking sibling index and hierarchy depth
            var activePanel = visiblePanels
                .OrderByDescending(p => GetHierarchyDepth(p.transform))
                .ThenByDescending(p => p.transform.GetSiblingIndex())
                .FirstOrDefault();

            var newDefinition = activePanel?.MenuDefinition;

            if (currentlyOpenedPanel != newDefinition)
            {
                currentlyOpenedPanel = newDefinition;
                OnPanelChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets the depth of a transform in the hierarchy.
        /// </summary>
        /// <param name="transform">The transform to check.</param>
        /// <returns>The depth in the hierarchy.</returns>
        private int GetHierarchyDepth(Transform transform)
        {
            int depth = 0;
            while (transform.parent != null)
            {
                depth++;
                transform = transform.parent;
            }
            return depth;
        }

        /// <summary>
        /// Handles hierarchy changes by checking for panel visibility changes.
        /// </summary>
        private void OnHierarchyChanged()
        {
            CheckForPanelChanges();
        }
    }
}
