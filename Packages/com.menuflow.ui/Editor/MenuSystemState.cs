using UnityEngine;
using UnityEditor;
using MenuFlow.ScriptableObjects;
using System;

namespace MenuFlow.Editor
{
    /// <summary>
    /// Manages the state and operations of the MenuSystem asset.
    /// This class is responsible for loading, saving, and modifying MenuSystem data.
    /// </summary>
    public class MenuSystemState
    {
        private MenuSystem menuSystem;

        /// <summary>
        /// Event triggered when the MenuSystem state changes, including loading a new system or modifying the current one.
        /// </summary>
        public event Action OnMenuSystemChanged;
        
        /// <summary>
        /// Gets the current MenuSystem instance.
        /// </summary>
        public MenuSystem Current => menuSystem;

        /// <summary>
        /// Gets whether the current MenuSystem is valid and loaded.
        /// </summary>
        public bool IsValid => menuSystem != null;

        /// <summary>
        /// Initializes a new instance of MenuSystemState and attempts to load an existing MenuSystem.
        /// </summary>
        public MenuSystemState()
        {
            LoadExistingMenuSystem();
        }

        /// <summary>
        /// Attempts to find and load an existing MenuSystem asset from the project.
        /// </summary>
        public void LoadExistingMenuSystem()
        {
            var guids = AssetDatabase.FindAssets("t:MenuSystem");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                SetMenuSystem(AssetDatabase.LoadAssetAtPath<MenuSystem>(path));
            }
        }

        /// <summary>
        /// Sets a new MenuSystem instance and notifies listeners of the change.
        /// </summary>
        /// <param name="newMenuSystem">The new MenuSystem to set.</param>
        public void SetMenuSystem(MenuSystem newMenuSystem)
        {
            if (menuSystem != newMenuSystem)
            {
                menuSystem = newMenuSystem;
                OnMenuSystemChanged?.Invoke();
            }
        }

        /// <summary>
        /// Updates the initial panel of the current MenuSystem.
        /// </summary>
        /// <param name="newInitialPanel">The new initial panel to set.</param>
        public void UpdateInitialPanel(MenuDefinition newInitialPanel)
        {
            if (menuSystem != null && menuSystem.initialPanel != newInitialPanel)
            {
                menuSystem.initialPanel = newInitialPanel;
                EditorUtility.SetDirty(menuSystem);
                OnMenuSystemChanged?.Invoke();
            }
        }

        /// <summary>
        /// Updates the loading screen prefab of the current MenuSystem.
        /// </summary>
        /// <param name="prefab">The new loading screen prefab to set.</param>
        public void UpdateLoadingScreenPrefab(GameObject prefab)
        {
            if (Current == null) return;

            Current.loadingScreenPrefab = prefab;
            EditorUtility.SetDirty(Current);
        }
    }
}
